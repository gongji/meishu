using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System;

[RequireComponent(typeof(Camera))]
public class EnviroSkyRendering : MonoBehaviour
{
	[HideInInspector]public Material material;

	private Camera myCam;
	private RenderTexture spSatTex;
	private Camera spSatCam;
	//private RenderingPath currentUsedRenderingPath;
	private Material mat;

	private Material blitMat;
	private Material weatherMapMat;
	private Material curlMat;
	private RenderTexture curlMap;


	private Texture3D noiseTexture = null;
	private Texture3D detailNoiseTexture = null;
	private Texture3D detailNoiseTextureHigh = null;

    //Cloud Rendering
	private Matrix4x4 projection;
	private Matrix4x4 inverseRotation;
    private Matrix4x4 inverseRotationSPVR;

    public RenderTexture cloudsMap;
    public Material cloudsMaterial;



    /// <summary>
    /// ///////////////////
    /// </summary>
    /// 
    public enum VolumtericResolution
    {
        Full,
        Half,
        Quarter
    };

    public static event Action<EnviroSkyRendering, Matrix4x4, Matrix4x4> PreRenderEvent;

    private static Mesh _pointLightMesh;
    private static Mesh _spotLightMesh;
    private static Material _lightMaterial;

    private Camera _camera;
    private CommandBuffer _preLightPass;
    public CommandBuffer _afterLightPass;

    private Matrix4x4 _viewProj;
    private Matrix4x4 _viewProjSP;
    [HideInInspector]
    public Material _volumeRenderingMaterial;
    private Material _bilateralBlurMaterial;

    private RenderTexture _volumeLightTexture;
    private RenderTexture _halfVolumeLightTexture;
    private RenderTexture _quarterVolumeLightTexture;
    private static Texture _defaultSpotCookie;

    private RenderTexture _halfDepthBuffer;
    private RenderTexture _quarterDepthBuffer;
    private Texture2D _ditheringTexture;
    private Texture3D _noiseTexture;

    private VolumtericResolution _currentResolution = VolumtericResolution.Quarter;
    
    [HideInInspector]
    public Texture DefaultSpotCookie;

    private Material _material;

    public CommandBuffer GlobalCommandBuffer { get { return _preLightPass; } }
    public CommandBuffer GlobalCommandBufferForward { get { return _afterLightPass; } }


    [HideInInspector]
    public bool volumeLighting = true;
    [HideInInspector]
    public bool dirVolumeLighting = true;
    [HideInInspector]
    public bool distanceFog = true;
    [HideInInspector]
    public bool useRadialDistance = false;
    [HideInInspector]
    public bool heightFog = true;
    [HideInInspector]
    public float height = 1.0f;
    [Range(0.001f, 10.0f)]
    [HideInInspector]
    public float heightDensity = 2.0f;
    [HideInInspector]
    public float startDistance = 0.0f;

 
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static Material GetLightMaterial()
    {
        return _lightMaterial;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static Mesh GetPointLightMesh()
    {
        return _pointLightMesh;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static Mesh GetSpotLightMesh()
    {
        return _spotLightMesh;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public RenderTexture GetVolumeLightBuffer()
    {
        if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Quarter)
            return _quarterVolumeLightTexture;
        else if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Half)
            return _halfVolumeLightTexture;
        else
            return _volumeLightTexture;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public RenderTexture GetVolumeLightDepthBuffer()
    {
        if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Quarter)
            return _quarterDepthBuffer;
        else if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Half)
            return _halfDepthBuffer;
        else
            return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static Texture GetDefaultSpotCookie()
    {
        return _defaultSpotCookie;
    }

    /// <summary>
    /// //////////////////////////
    /// </summary>

    void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera.actualRenderingPath == RenderingPath.Forward)
            _camera.depthTextureMode = DepthTextureMode.Depth;

        _currentResolution = EnviroSky.instance.volumeLightSettings.Resolution;

        Shader shader = Shader.Find("Enviro/EnviroVolumeRendering");
        if (shader == null)
            throw new Exception("Critical Error: \"Enviro/EnviroVolumeRendering\" shader is missing.");
        _volumeRenderingMaterial = new Material(shader);

        _material = new Material(Shader.Find("Enviro/VolumeLight"));

        shader = Shader.Find("Hidden/BilateralBlur");
        if (shader == null)
            throw new Exception("Critical Error: \"Hidden/BilateralBlur\" shader is missing.");
        _bilateralBlurMaterial = new Material(shader);

        _preLightPass = new CommandBuffer();
        _preLightPass.name = "PreLight";

        _afterLightPass = new CommandBuffer();
        _afterLightPass.name = "AfterLight";

        ChangeResolution();

        if (_pointLightMesh == null)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _pointLightMesh = go.GetComponent<MeshFilter>().sharedMesh;
            Destroy(go);
        }

        if (_spotLightMesh == null)
        {
            _spotLightMesh = CreateSpotLightMesh();
        }

        if (_lightMaterial == null)
        {
            shader = Shader.Find("Enviro/VolumeLight");
            if (shader == null)
                throw new Exception("Critical Error: \"Enviro/VolumeLight\" shader is missing.");
            _lightMaterial = new Material(shader);
        }

        if (_defaultSpotCookie == null)
        {
            _defaultSpotCookie = DefaultSpotCookie;
        }

        LoadNoise3dTexture();
        GenerateDitherTexture();
    }


    private void Start()
    {
        myCam = GetComponent<Camera>();
        CreateMaterialsAndTextures();
        //RefreshCameraCommand ();
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        if (_camera.actualRenderingPath == RenderingPath.Forward)
        {
            _camera.AddCommandBuffer(CameraEvent.AfterDepthTexture, _preLightPass);
            _camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, _afterLightPass);
        }
        else
        {
            _camera.AddCommandBuffer(CameraEvent.BeforeLighting, _preLightPass);
            //	
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        if (_camera.actualRenderingPath == RenderingPath.Forward)
        {
            _camera.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, _preLightPass);
            _camera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, _afterLightPass);
        }
        else
        {
            _camera.RemoveCommandBuffer(CameraEvent.BeforeLighting, _preLightPass);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void ChangeResolution()
    {
        int width = _camera.pixelWidth;
        int height = _camera.pixelHeight;

        if (_volumeLightTexture != null)
            Destroy(_volumeLightTexture);

        _volumeLightTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf);
        _volumeLightTexture.name = "VolumeLightBuffer";
        _volumeLightTexture.filterMode = FilterMode.Bilinear;
#if UNITY_2017_2_OR_NEWER
        if (EnviroSky.instance.singlePassVR)
            _volumeLightTexture.vrUsage = VRTextureUsage.TwoEyes;
#endif


        if (_halfDepthBuffer != null)
            Destroy(_halfDepthBuffer);
        if (_halfVolumeLightTexture != null)
            Destroy(_halfVolumeLightTexture);

        if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Half || EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Quarter)
        {
            _halfVolumeLightTexture = new RenderTexture(width / 2, height / 2, 0, RenderTextureFormat.ARGBHalf);
            _halfVolumeLightTexture.name = "VolumeLightBufferHalf";
            _halfVolumeLightTexture.filterMode = FilterMode.Bilinear;

            _halfDepthBuffer = new RenderTexture(width / 2, height / 2, 0, RenderTextureFormat.RFloat);
            _halfDepthBuffer.name = "VolumeLightHalfDepth";
            _halfDepthBuffer.Create();
            _halfDepthBuffer.filterMode = FilterMode.Point;
        }

        if (_quarterVolumeLightTexture != null)
            Destroy(_quarterVolumeLightTexture);
        if (_quarterDepthBuffer != null)
            Destroy(_quarterDepthBuffer);

        if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Quarter)
        {
            _quarterVolumeLightTexture = new RenderTexture(width / 4, height / 4, 0, RenderTextureFormat.ARGBHalf);
            _quarterVolumeLightTexture.name = "VolumeLightBufferQuarter";
            _quarterVolumeLightTexture.filterMode = FilterMode.Bilinear;

            _quarterDepthBuffer = new RenderTexture(width / 4, height / 4, 0, RenderTextureFormat.RFloat);
            _quarterDepthBuffer.name = "VolumeLightQuarterDepth";
            _quarterDepthBuffer.Create();
            _quarterDepthBuffer.filterMode = FilterMode.Point;
        }
    }


    private void CreateMaterialsAndTextures ()
    {

        if (mat == null)
            mat = new Material(Shader.Find("Enviro/RaymarchClouds"));

        if (blitMat == null)
            blitMat = new Material(Shader.Find("Enviro/Blit"));

        if (weatherMapMat == null)
            weatherMapMat = new Material(Shader.Find("Enviro/WeatherMap"));

        if (curlMat == null)
            curlMat = new Material(Shader.Find("Enviro/CurlNoise"));

        if (curlMap == null)
            curlMap = new RenderTexture(512, 512, 0, RenderTextureFormat.Default);

        //Load Noise Textures
        if (noiseTexture == null)
            noiseTexture = Resources.Load("enviro_clouds_base") as Texture3D;

        if (detailNoiseTexture == null)
            detailNoiseTexture = Resources.Load("enviro_clouds_detail_low") as Texture3D;


        if (detailNoiseTextureHigh == null)
            detailNoiseTextureHigh = Resources.Load("enviro_clouds_detail_high") as Texture3D;

        RenderCurlNoise();
    }

    /*void CreateSinglePassCameras ()
	{
		var format = EnviroSky.instance.GetCameraHDR(EnviroSky.instance.PlayerCamera) ? RenderTextureFormat.DefaultHDR: RenderTextureFormat.Default;

		if (spSkyCam == null) {
			spSkytex = new RenderTexture (Screen.currentResolution.width, Screen.currentResolution.height, 16, format);
			GameObject s = new GameObject ();
			s.name = "Enviro Sky SinglePass Camera";
			s.hideFlags = HideFlags.HideAndDontSave;
			spSkyCam = s.AddComponent<Camera> ();
			EnviroSky.instance.SetCameraHDR (spSkyCam, EnviroSky.instance.HDR);
			spSkyCam.renderingPath = RenderingPath.Forward;
			spSkyCam.enabled = false;
			spSkyCam.cullingMask = (1 << EnviroSky.instance.satelliteRenderingLayer);
			spSkyCam.targetTexture = spSkytex;
			spSkyCam.useOcclusionCulling = false;
		}
	}
*/

    /*public void Apply()
	{
		myCam = GetComponent<Camera> ();
		currentUsedRenderingPath = myCam.actualRenderingPath;
		if (EnviroSky.instance.singlePassVR == true) {
			CreateSinglePassCameras ();
		}
		//RefreshCameraCommand ();
	}*/

    /*void Update ()
	{
		if (myCam != null) {
			if(currentUsedRenderingPath != myCam.actualRenderingPath)
				RefreshCameraCommand ();
		}
	}*/

    /// <summary>
    /// Refreshs the camera command buffers. Usefull when switching rendering path in runtime!
    /// </summary>
    /*public void RefreshCameraCommand ()
	{
		// Remove old Command Buffer
		CommandBuffer[] cbs;
		cbs = myCam.GetCommandBuffers (CameraEvent.BeforeGBuffer);

		for (int i = 0; i < cbs.Length; i++) {

			if (cbs [i].name == "Enviro Sky Rendering")
				myCam.RemoveCommandBuffer (CameraEvent.BeforeGBuffer, cbs [i]);
		}

		cbs = myCam.GetCommandBuffers (CameraEvent.BeforeForwardOpaque);
		for (int i = 0; i < cbs.Length; i++) {

			if (cbs [i].name == "Enviro Sky Rendering")
				myCam.RemoveCommandBuffer (CameraEvent.BeforeForwardOpaque, cbs [i]);
		}
		// Add new Command Buffer
		currentUsedRenderingPath = myCam.actualRenderingPath;
		CommandBuffer cb = new CommandBuffer();
		cb.name = "Enviro Sky Rendering";
		cb.SetGlobalTexture("_CloudsTex", EnviroSky.instance.cloudsRenderTarget);
		cb.SetGlobalTexture("_SkyTex", BuiltinRenderTextureType.CameraTarget);
		cb.Blit(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget, blitMat);


		if (myCam.actualRenderingPath == RenderingPath.DeferredShading) 
			myCam.AddCommandBuffer (CameraEvent.BeforeGBuffer, cb);
		else
			myCam.AddCommandBuffer (CameraEvent.BeforeForwardOpaque, cb);
	}*/

    void OnPreRender ()
	{
        //Volume Lighting
        if (volumeLighting)
        {
            Matrix4x4 projLeft = Matrix4x4.Perspective(_camera.fieldOfView, _camera.aspect, 0.01f, _camera.farClipPlane);
            Matrix4x4 projRight = Matrix4x4.Perspective(_camera.fieldOfView, _camera.aspect, 0.01f, _camera.farClipPlane);

            if (UnityEngine.XR.XRSettings.enabled)
            {
                projLeft = _camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                projLeft = GL.GetGPUProjectionMatrix(projLeft, true);
                projRight = _camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                projRight = GL.GetGPUProjectionMatrix(projRight, true);
            }
            else
            {
                projLeft = Matrix4x4.Perspective(_camera.fieldOfView, _camera.aspect, 0.01f, _camera.farClipPlane);
                projLeft = GL.GetGPUProjectionMatrix(projLeft, true);
            }

            // use very low value for near clip plane to simplify cone/frustum intersection 
            if (UnityEngine.XR.XRSettings.enabled)
            {
                _viewProj = projLeft * _camera.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
                _viewProjSP = projRight * _camera.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
            }
            else
            {
                _viewProj = projLeft * _camera.worldToCameraMatrix;
                _viewProjSP = projRight * _camera.worldToCameraMatrix;
            }

            _preLightPass.Clear();
            _afterLightPass.Clear();

            bool dx11 = SystemInfo.graphicsShaderLevel > 40;

            if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Quarter)
            {
                Texture nullTexture = null;
                // down sample depth to half res
                _preLightPass.Blit(nullTexture, _halfDepthBuffer, _bilateralBlurMaterial, dx11 ? 4 : 10);
                // down sample depth to quarter res
                _preLightPass.Blit(nullTexture, _quarterDepthBuffer, _bilateralBlurMaterial, dx11 ? 6 : 11);

                _preLightPass.SetRenderTarget(_quarterVolumeLightTexture);
            }
            else if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Half)
            {
                Texture nullTexture = null;
                // down sample depth to half res
                _preLightPass.Blit(nullTexture, _halfDepthBuffer, _bilateralBlurMaterial, dx11 ? 4 : 10);

                _preLightPass.SetRenderTarget(_halfVolumeLightTexture);
            }
            else
            {
                _preLightPass.SetRenderTarget(_volumeLightTexture);
            }

            _preLightPass.ClearRenderTarget(false, true, new Color(0, 0, 0, 1));

            UpdateMaterialParameters();

            if (PreRenderEvent != null)
                PreRenderEvent(this, _viewProj, _viewProjSP);
        }


        //Satellites
        if (myCam != null) {
			switch (myCam.stereoActiveEye) {
			case Camera.MonoOrStereoscopicEye.Mono:
				if (EnviroSky.instance.satCamera != null)
					RenderCamera (EnviroSky.instance.satCamera, Camera.MonoOrStereoscopicEye.Mono);
				break;

			case Camera.MonoOrStereoscopicEye.Left:
				if (EnviroSky.instance.satCamera != null)
					RenderCamera (EnviroSky.instance.satCamera, Camera.MonoOrStereoscopicEye.Left);
				break;

			case Camera.MonoOrStereoscopicEye.Right:
				if (EnviroSky.instance.satCamera != null)
					RenderCamera (EnviroSky.instance.satCamera, Camera.MonoOrStereoscopicEye.Right);
				break;
			}
				
			if (EnviroSky.instance.satCamera != null)
				RenderSettings.skybox.SetTexture ("_SatTex", EnviroSky.instance.satCamera.targetTexture);
		}
	}

    void RenderCamera(Camera targetCam, Camera.MonoOrStereoscopicEye eye)
	{
		targetCam.fieldOfView = EnviroSky.instance.PlayerCamera.fieldOfView;	
		targetCam.aspect = EnviroSky.instance.PlayerCamera.aspect;

		switch (eye) 
		{
		case Camera.MonoOrStereoscopicEye.Mono:
			targetCam.transform.position = EnviroSky.instance.PlayerCamera.transform.position;
			targetCam.transform.rotation = EnviroSky.instance.PlayerCamera.transform.rotation;
			targetCam.worldToCameraMatrix = EnviroSky.instance.PlayerCamera.worldToCameraMatrix;
			targetCam.Render ();
			break;

		case Camera.MonoOrStereoscopicEye.Left:

			targetCam.transform.position = EnviroSky.instance.PlayerCamera.transform.position;
			targetCam.transform.rotation = EnviroSky.instance.PlayerCamera.transform.rotation;
			targetCam.projectionMatrix = EnviroSky.instance.PlayerCamera.GetStereoProjectionMatrix (Camera.StereoscopicEye.Left);
			targetCam.worldToCameraMatrix = EnviroSky.instance.PlayerCamera.GetStereoViewMatrix (Camera.StereoscopicEye.Left);
			targetCam.Render ();

		/*	if (EnviroSky.instance.singlePassVR == true) 
			{
			   if (targetCam == EnviroSky.instance.skyCamera && spSkyCam != null) {
					spSkyCam.fieldOfView = EnviroSky.instance.PlayerCamera.fieldOfView;	
					spSkyCam.aspect = EnviroSky.instance.PlayerCamera.aspect;
					spSkyCam.projectionMatrix = EnviroSky.instance.PlayerCamera.GetStereoProjectionMatrix (Camera.StereoscopicEye.Right);
					spSkyCam.worldToCameraMatrix = EnviroSky.instance.PlayerCamera.GetStereoViewMatrix (Camera.StereoscopicEye.Right);
					spSkyCam.Render ();
					material.SetTexture ("_SkySPSR", spSkytex);
				}
			}*/
			break;

		case Camera.MonoOrStereoscopicEye.Right:
			targetCam.transform.position = EnviroSky.instance.PlayerCamera.transform.position;
			targetCam.transform.rotation = EnviroSky.instance.PlayerCamera.transform.rotation;
			targetCam.projectionMatrix = EnviroSky.instance.PlayerCamera.GetStereoProjectionMatrix (Camera.StereoscopicEye.Right);
			targetCam.worldToCameraMatrix = EnviroSky.instance.PlayerCamera.GetStereoViewMatrix (Camera.StereoscopicEye.Right);
			targetCam.Render ();
			break;
		}
	}
    void RenderWeatherMap()
    {
        if (EnviroSky.instance.cloudsSettings.customWeatherMap == null)
        {
        weatherMapMat.SetVector("_WindDir", EnviroSky.instance.cloudAnimNonScaled);//new Vector2(EnviroSky.instance.cloudsSettings.cloudsWindDirectionX, EnviroSky.instance.cloudsSettings.cloudsWindDirectionY));
        weatherMapMat.SetFloat("_AnimSpeedScale", EnviroSky.instance.cloudsSettings.weatherAnimSpeedScale);
        //  weatherMapMat.SetFloat("_WindSpeed", EnviroSky.instance.cloudsSettings.cloudsWindStrengthModificator);
        weatherMapMat.SetInt("_Tiling", EnviroSky.instance.cloudsSettings.weatherMapTiling);
        weatherMapMat.SetVector("_Location", EnviroSky.instance.cloudsSettings.locationOffset);
        double cov = EnviroSky.instance.cloudsConfig.coverage * EnviroSky.instance.cloudsSettings.globalCloudCoverage;
        weatherMapMat.SetFloat("_Coverage", (float)System.Math.Round(cov, 4));
    
        Graphics.Blit(null, EnviroSky.instance.weatherMap, weatherMapMat);
        mat.SetTexture("_WeatherMap", EnviroSky.instance.weatherMap);
    }
    else
    {
         mat.SetTexture("_WeatherMap", EnviroSky.instance.cloudsSettings.customWeatherMap);
    }
	}

	void RenderCurlNoise ()
	{
		Graphics.Blit (null, curlMap, curlMat);
		mat.SetTexture("_CurlNoise", curlMap);
	}

    void Update()
    {
        RenderWeatherMap();

        //#if UNITY_EDITOR
        if (_currentResolution != EnviroSky.instance.volumeLightSettings.Resolution)
        {
            _currentResolution = EnviroSky.instance.volumeLightSettings.Resolution;
            ChangeResolution();
        }

        if ((_volumeLightTexture.width != _camera.pixelWidth || _volumeLightTexture.height != _camera.pixelHeight))
            ChangeResolution();

        if (_volumeRenderingMaterial == null)
            return;
        //#endif
    }


    private void SetCloudProperties ()
    {
        mat.SetTexture("_Noise", noiseTexture);

        if (EnviroSky.instance.cloudsSettings.detailQuality == EnviroCloudSettings.CloudDetailQuality.Low)
            mat.SetTexture("_DetailNoise", detailNoiseTexture);
        else
            mat.SetTexture("_DetailNoise", detailNoiseTextureHigh);

        switch (myCam.stereoActiveEye)
        {
            case Camera.MonoOrStereoscopicEye.Mono:
                projection = myCam.projectionMatrix;
                Matrix4x4 inverseProjection = projection.inverse;
                mat.SetMatrix("_InverseProjection", inverseProjection);
                inverseRotation = myCam.cameraToWorldMatrix;
                mat.SetMatrix("_InverseRotation", inverseRotation);
                break;

            case Camera.MonoOrStereoscopicEye.Left:
                projection = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                Matrix4x4 inverseProjectionLeft = projection.inverse;
                mat.SetMatrix("_InverseProjection", inverseProjectionLeft);
                inverseRotation = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Left).inverse;
                mat.SetMatrix("_InverseRotation", inverseRotation);

                if (EnviroSky.instance.singlePassVR)
                {
                    Matrix4x4 inverseProjectionRightSP = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right).inverse;
                    mat.SetMatrix("_InverseProjection_SP", inverseProjectionRightSP);

                    inverseRotationSPVR = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Right).inverse;
                    mat.SetMatrix("_InverseRotation_SP", inverseRotationSPVR);
                }
                break;

            case Camera.MonoOrStereoscopicEye.Right:
                projection = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                Matrix4x4 inverseProjectionRight = projection.inverse;
                mat.SetMatrix("_InverseProjection", inverseProjectionRight);
                inverseRotation = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Right).inverse;
                mat.SetMatrix("_InverseRotation", inverseRotation);
                break;
        }

        mat.SetVector("_Steps", new Vector4(EnviroSky.instance.cloudsSettings.raymarchSteps* EnviroSky.instance.cloudsConfig.raymarchingScale, EnviroSky.instance.cloudsSettings.raymarchSteps * EnviroSky.instance.cloudsConfig.raymarchingScale, 0.0f, 0.0f));
        mat.SetFloat("_BaseNoiseUV", EnviroSky.instance.cloudsSettings.baseNoiseUV);
        mat.SetFloat("_DetailNoiseUV", EnviroSky.instance.cloudsSettings.detailNoiseUV);
        mat.SetFloat("_PrimAtt", EnviroSky.instance.cloudsSettings.primaryAttenuation);
        mat.SetFloat("_SecAtt", EnviroSky.instance.cloudsSettings.secondaryAttenuation);
        mat.SetFloat("_SkyBlending", EnviroSky.instance.cloudsConfig.skyBlending);
        mat.SetFloat("_HgPhaseFactor", EnviroSky.instance.cloudsSettings.hgPhase);
        mat.SetVector("_CloudsParameter", new Vector4(EnviroSky.instance.cloudsSettings.bottomCloudHeight, EnviroSky.instance.cloudsSettings.topCloudHeight, EnviroSky.instance.cloudsSettings.topCloudHeight - EnviroSky.instance.cloudsSettings.bottomCloudHeight, EnviroSky.instance.cloudsSettings.cloudsWorldScale*10));
        mat.SetFloat("_AmbientLightIntensity", EnviroSky.instance.cloudsSettings.ambientLightIntensity.Evaluate(EnviroSky.instance.GameTime.solarTime));
        mat.SetFloat("_SunLightIntensity", EnviroSky.instance.cloudsSettings.directLightIntensity.Evaluate(EnviroSky.instance.GameTime.solarTime));
        mat.SetFloat("_AlphaCoef", EnviroSky.instance.cloudsConfig.alphaCoef);
        mat.SetFloat("_ExtinctionCoef", EnviroSky.instance.cloudsConfig.scatteringCoef);
        mat.SetFloat("_CloudDensityScale", EnviroSky.instance.cloudsConfig.density);
        mat.SetColor("_CloudBaseColor", EnviroSky.instance.cloudsConfig.bottomColor);
        mat.SetColor("_CloudTopColor", EnviroSky.instance.cloudsConfig.topColor);
        mat.SetFloat("_CloudsType", EnviroSky.instance.cloudsConfig.cloudType);
        mat.SetFloat("_CloudsCoverage", EnviroSky.instance.cloudsConfig.coverageHeight);
        mat.SetVector("_CloudsAnimation", new Vector4(EnviroSky.instance.cloudAnim.x, EnviroSky.instance.cloudAnim.y, 0f, 0f));
        mat.SetFloat("_CloudsExposure", EnviroSky.instance.cloudsSettings.cloudsExposure);
        mat.SetFloat("_GlobalCoverage", EnviroSky.instance.cloudsConfig.coverage * EnviroSky.instance.cloudsSettings.globalCloudCoverage);
        mat.SetColor("_LightColor", EnviroSky.instance.cloudsSettings.volumeCloudsColor.Evaluate(EnviroSky.instance.GameTime.solarTime));
        mat.SetColor("_MoonLightColor", EnviroSky.instance.cloudsSettings.volumeCloudsMoonColor.Evaluate(EnviroSky.instance.GameTime.lunarTime));
        mat.SetFloat("_Tonemapping", EnviroSky.instance.cloudsSettings.tonemapping ? 0f : 1f);
    }

    public void RenderClouds ()
	{
        SetCloudProperties();
        //Render Clouds with downsampling tex
		CustomGraphicsBlit(null,EnviroSky.instance.cloudsRenderTarget, mat, 0);
	}

	[ImageEffectOpaque]
	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
        RenderTexture clouds = RenderTexture.GetTemporary(Screen.currentResolution.width, Screen.currentResolution.height, 16, RenderTextureFormat.DefaultHDR);

#if UNITY_2017_2_OR_NEWER
        if (EnviroSky.instance.singlePassVR)
            clouds.vrUsage = VRTextureUsage.TwoEyes;
#endif

        // Clouds
        // Workaround! Not sure why we need to blit trough in deferred?!! Unity Bug? If we don't blit we get wrong cloud blending on edges...
        if (myCam.actualRenderingPath == RenderingPath.Forward)
			myCam.depthTextureMode |= DepthTextureMode.Depth;
		//else
		//	Graphics.Blit (source, destination);

		if (EnviroSky.instance.cloudsMode == EnviroSky.EnviroCloudsMode.Volume || EnviroSky.instance.cloudsMode == EnviroSky.EnviroCloudsMode.Both) {
            EnviroSky.instance.cloudsRenderTarget = RenderTexture.GetTemporary(Screen.currentResolution.width / EnviroSky.instance.cloudsSettings.cloudsRenderResolution, Screen.currentResolution.height / EnviroSky.instance.cloudsSettings.cloudsRenderResolution, 0, RenderTextureFormat.DefaultHDR);
            RenderClouds ();
            //Blit clouds to final image
            blitMat.SetTexture ("_MainTex", source);
			blitMat.SetTexture ("_CloudsTex", EnviroSky.instance.cloudsRenderTarget);
			Graphics.Blit (source, clouds, blitMat);
            RenderTexture.ReleaseTemporary(EnviroSky.instance.cloudsRenderTarget);
        } else {
			Graphics.Blit (source, clouds);
		}

        // Volume Lighting and Fog
        Transform camtr = myCam.transform;
        float camNear = myCam.nearClipPlane;
        float camFar = myCam.farClipPlane;
        float camFov = myCam.fieldOfView;
        float camAspect = myCam.aspect;

        float fovWHalf = camFov * 0.5f;

        Vector3 toRight = camtr.right * camNear * Mathf.Tan(fovWHalf * Mathf.Deg2Rad) * camAspect;
        Vector3 toTop = camtr.up * camNear * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

        Vector3 topLeft = (camtr.forward * camNear - toRight + toTop);
        float camScale = topLeft.magnitude * camFar / camNear;

        topLeft.Normalize();
        topLeft *= camScale;

        Vector3 topRight = (camtr.forward * camNear + toRight + toTop);
        topRight.Normalize();
        topRight *= camScale;

        Vector3 bottomRight = (camtr.forward * camNear + toRight - toTop);
        bottomRight.Normalize();
        bottomRight *= camScale;

        Vector3 bottomLeft = (camtr.forward * camNear - toRight - toTop);
        bottomLeft.Normalize();
        bottomLeft *= camScale;

        Matrix4x4 frustumCorners = Matrix4x4.identity;
        frustumCorners.SetRow(0, topLeft);
        frustumCorners.SetRow(1, topRight);
        frustumCorners.SetRow(2, bottomRight);
        frustumCorners.SetRow(3, bottomLeft);

        float FdotC = myCam.transform.position.y - height;
        float paramK = (FdotC <= 0.0f ? 1.0f : 0.0f);
        var sceneMode = RenderSettings.fogMode;
        var sceneDensity = RenderSettings.fogDensity;
        var sceneStart = RenderSettings.fogStartDistance;
        var sceneEnd = RenderSettings.fogEndDistance;
        Vector4 sceneParams;
        bool linear = (sceneMode == FogMode.Linear);
        float diff = linear ? sceneEnd - sceneStart : 0.0f;
        float invDiff = Mathf.Abs(diff) > 0.0001f ? 1.0f / diff : 0.0f;
        sceneParams.x = sceneDensity * 1.2011224087f; // density / sqrt(ln(2)), used by Exp2 fog mode
        sceneParams.y = sceneDensity * 1.4426950408f; // density / ln(2), used by Exp fog mode
        sceneParams.z = linear ? -invDiff : 0.0f;
        sceneParams.w = linear ? sceneEnd * invDiff : 0.0f;

        if (volumeLighting)
        {
            //Dir volume
            if (dirVolumeLighting)
            {

                Light _light = EnviroSky.instance.Components.DirectLight.GetComponent<Light>();
                int pass = 4;

                _material.SetPass(pass);

                if (EnviroSky.instance.volumeLightSettings.directLightNoise)
                    _material.EnableKeyword("NOISE");
                else
                    _material.DisableKeyword("NOISE");

                _material.SetVector("_LightDir", new Vector4(_light.transform.forward.x, _light.transform.forward.y, _light.transform.forward.z, 1.0f / (_light.range * _light.range)));
                _material.SetVector("_LightColor", _light.color * _light.intensity);
                _material.SetFloat("_MaxRayLength", EnviroSky.instance.volumeLightSettings.MaxRayLength);

                if (_light.cookie == null)
                {
                    _material.EnableKeyword("DIRECTIONAL");
                    _material.DisableKeyword("DIRECTIONAL_COOKIE");
                }
                else
                {
                    _material.EnableKeyword("DIRECTIONAL_COOKIE");
                    _material.DisableKeyword("DIRECTIONAL");
                    _material.SetTexture("_LightTexture0", _light.cookie);
                }

                _material.SetInt("_SampleCount", EnviroSky.instance.volumeLightSettings.SampleCount);
                _material.SetVector("_NoiseVelocity", new Vector4(EnviroSky.instance.volumeLightSettings.noiseVelocity.x, EnviroSky.instance.volumeLightSettings.noiseVelocity.y) * EnviroSky.instance.volumeLightSettings.noiseScale);
                _material.SetVector("_NoiseData", new Vector4(EnviroSky.instance.volumeLightSettings.noiseScale, EnviroSky.instance.volumeLightSettings.noiseIntensity, EnviroSky.instance.volumeLightSettings.noiseIntensityOffset));
                _material.SetVector("_MieG", new Vector4(1 - (EnviroSky.instance.volumeLightSettings.Anistropy * EnviroSky.instance.volumeLightSettings.Anistropy), 1 + (EnviroSky.instance.volumeLightSettings.Anistropy * EnviroSky.instance.volumeLightSettings.Anistropy), 2 * EnviroSky.instance.volumeLightSettings.Anistropy, 1.0f / (4.0f * Mathf.PI)));
                _material.SetVector("_VolumetricLight", new Vector4(EnviroSky.instance.volumeLightSettings.ScatteringCoef, EnviroSky.instance.volumeLightSettings.ExtinctionCoef, _light.range, 1.0f));// - SkyboxExtinctionCoef));
                _material.SetTexture("_CameraDepthTexture", GetVolumeLightDepthBuffer());
                _material.SetMatrix("_FrustumCornersWS", frustumCorners);

                //Texture tex = null;
                if (_light.shadows != LightShadows.None)
                {
                    _material.EnableKeyword("SHADOWS_DEPTH");
                    CustomGraphicsBlitFog(GetVolumeLightBuffer(), _material, pass);
                }
                else
                {
                    _material.DisableKeyword("SHADOWS_DEPTH");
                    CustomGraphicsBlitFog(GetVolumeLightBuffer(), _material, pass);
                }
            }

            if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Quarter)
            {
                RenderTexture temp = RenderTexture.GetTemporary(_quarterDepthBuffer.width, _quarterDepthBuffer.height, 0, RenderTextureFormat.ARGBHalf);
                temp.filterMode = FilterMode.Bilinear;

                // horizontal bilateral blur at quarter res
                Graphics.Blit(_quarterVolumeLightTexture, temp, _bilateralBlurMaterial, 8);
                // vertical bilateral blur at quarter res
                Graphics.Blit(temp, _quarterVolumeLightTexture, _bilateralBlurMaterial, 9);

                // upscale to full res
                Graphics.Blit(_quarterVolumeLightTexture, _volumeLightTexture, _bilateralBlurMaterial, 7);

                RenderTexture.ReleaseTemporary(temp);
            }
            else if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Half)
            {
                RenderTexture temp = RenderTexture.GetTemporary(_halfVolumeLightTexture.width, _halfVolumeLightTexture.height, 0, RenderTextureFormat.ARGBHalf);
                temp.filterMode = FilterMode.Bilinear;

                // horizontal bilateral blur at half res
                Graphics.Blit(_halfVolumeLightTexture, temp, _bilateralBlurMaterial, 2);

                // vertical bilateral blur at half res
                Graphics.Blit(temp, _halfVolumeLightTexture, _bilateralBlurMaterial, 3);

                // upscale to full res
                Graphics.Blit(_halfVolumeLightTexture, _volumeLightTexture, _bilateralBlurMaterial, 5);
                RenderTexture.ReleaseTemporary(temp);
            }
            else
            {
                RenderTexture temp = RenderTexture.GetTemporary(_volumeLightTexture.width, _volumeLightTexture.height, 0, RenderTextureFormat.ARGBHalf);
                temp.filterMode = FilterMode.Bilinear;

                // horizontal bilateral blur at full res
                Graphics.Blit(_volumeLightTexture, temp, _bilateralBlurMaterial, 0);
                // vertical bilateral blur at full res
                Graphics.Blit(temp, _volumeLightTexture, _bilateralBlurMaterial, 1);
                RenderTexture.ReleaseTemporary(temp);
            }
            _volumeRenderingMaterial.EnableKeyword("ENVIROVOLUMELIGHT");
        }
        else
            _volumeRenderingMaterial.DisableKeyword("ENVIROVOLUMELIGHT");

        Shader.SetGlobalFloat("_EnviroVolumeDensity", EnviroSky.instance.globalVolumeLightIntensity);
        Shader.SetGlobalVector("_SceneFogParams", sceneParams);
        Shader.SetGlobalVector("_SceneFogMode", new Vector4((int)sceneMode, useRadialDistance ? 1 : 0, 0, 0));
        Shader.SetGlobalMatrix("_FrustumCornersWS", frustumCorners);
        Shader.SetGlobalVector("_HeightParams", new Vector4(height, FdotC, paramK, heightDensity * 0.5f));
        Shader.SetGlobalVector("_DistanceParams", new Vector4(-Mathf.Max(startDistance, 0.0f), 0, 0, 0));

        //Scene Image
        _volumeRenderingMaterial.SetTexture("_Source", clouds);

        if (volumeLighting)
            Shader.SetGlobalTexture("_EnviroVolumeLightingTex", _volumeLightTexture);
        else
            Shader.SetGlobalTexture("_EnviroVolumeLightingTex", null);

        CustomGraphicsBlitFog(destination, _volumeRenderingMaterial, 0);

        RenderTexture.ReleaseTemporary(clouds);
    }

    private void UpdateMaterialParameters()
    {
        _bilateralBlurMaterial.SetTexture("_HalfResDepthBuffer", _halfDepthBuffer);
        _bilateralBlurMaterial.SetTexture("_HalfResColor", _halfVolumeLightTexture);
        _bilateralBlurMaterial.SetTexture("_QuarterResDepthBuffer", _quarterDepthBuffer);
        _bilateralBlurMaterial.SetTexture("_QuarterResColor", _quarterVolumeLightTexture);

        Shader.SetGlobalTexture("_DitherTexture", _ditheringTexture);
        Shader.SetGlobalTexture("_NoiseTexture", _noiseTexture);
    }


    void LoadNoise3dTexture()
    {
        // basic dds loader for 3d texture - !not very robust!

        TextAsset data = Resources.Load("NoiseVolume") as TextAsset;

        byte[] bytes = data.bytes;

        uint height = BitConverter.ToUInt32(data.bytes, 12);
        uint width = BitConverter.ToUInt32(data.bytes, 16);
        uint pitch = BitConverter.ToUInt32(data.bytes, 20);
        uint depth = BitConverter.ToUInt32(data.bytes, 24);
        uint formatFlags = BitConverter.ToUInt32(data.bytes, 20 * 4);
        //uint fourCC = BitConverter.ToUInt32(data.bytes, 21 * 4);
        uint bitdepth = BitConverter.ToUInt32(data.bytes, 22 * 4);
        if (bitdepth == 0)
            bitdepth = pitch / width * 8;


        // doesn't work with TextureFormat.Alpha8 for some reason
        _noiseTexture = new Texture3D((int)width, (int)height, (int)depth, TextureFormat.RGBA32, false);
        _noiseTexture.name = "3D Noise";

        Color[] c = new Color[width * height * depth];

        uint index = 128;
        if (data.bytes[21 * 4] == 'D' && data.bytes[21 * 4 + 1] == 'X' && data.bytes[21 * 4 + 2] == '1' && data.bytes[21 * 4 + 3] == '0' &&
            (formatFlags & 0x4) != 0)
        {
            uint format = BitConverter.ToUInt32(data.bytes, (int)index);
            if (format >= 60 && format <= 65)
                bitdepth = 8;
            else if (format >= 48 && format <= 52)
                bitdepth = 16;
            else if (format >= 27 && format <= 32)
                bitdepth = 32;

            //Debug.Log("DXGI format: " + format);
            // dx10 format, skip dx10 header
            //Debug.Log("DX10 format");
            index += 20;
        }

        uint byteDepth = bitdepth / 8;
        pitch = (width * bitdepth + 7) / 8;

        for (int d = 0; d < depth; ++d)
        {
            //index = 128;
            for (int h = 0; h < height; ++h)
            {
                for (int w = 0; w < width; ++w)
                {
                    float v = (bytes[index + w * byteDepth] / 255.0f);
                    c[w + h * width + d * width * height] = new Color(v, v, v, v);
                }

                index += pitch;
            }
        }

        _noiseTexture.SetPixels(c);
        _noiseTexture.Apply();
    }


    /// <summary>
    /// 
    /// </summary>
    private void GenerateDitherTexture()
    {
        if (_ditheringTexture != null)
        {
            return;
        }

        int size = 8;
#if DITHER_4_4
        size = 4;
#endif
        // again, I couldn't make it work with Alpha8
        _ditheringTexture = new Texture2D(size, size, TextureFormat.Alpha8, false, true);
        _ditheringTexture.filterMode = FilterMode.Point;
        Color32[] c = new Color32[size * size];

        byte b;
#if DITHER_4_4
        b = (byte)(0.0f / 16.0f * 255); c[0] = new Color32(b, b, b, b);
        b = (byte)(8.0f / 16.0f * 255); c[1] = new Color32(b, b, b, b);
        b = (byte)(2.0f / 16.0f * 255); c[2] = new Color32(b, b, b, b);
        b = (byte)(10.0f / 16.0f * 255); c[3] = new Color32(b, b, b, b);

        b = (byte)(12.0f / 16.0f * 255); c[4] = new Color32(b, b, b, b);
        b = (byte)(4.0f / 16.0f * 255); c[5] = new Color32(b, b, b, b);
        b = (byte)(14.0f / 16.0f * 255); c[6] = new Color32(b, b, b, b);
        b = (byte)(6.0f / 16.0f * 255); c[7] = new Color32(b, b, b, b);

        b = (byte)(3.0f / 16.0f * 255); c[8] = new Color32(b, b, b, b);
        b = (byte)(11.0f / 16.0f * 255); c[9] = new Color32(b, b, b, b);
        b = (byte)(1.0f / 16.0f * 255); c[10] = new Color32(b, b, b, b);
        b = (byte)(9.0f / 16.0f * 255); c[11] = new Color32(b, b, b, b);

        b = (byte)(15.0f / 16.0f * 255); c[12] = new Color32(b, b, b, b);
        b = (byte)(7.0f / 16.0f * 255); c[13] = new Color32(b, b, b, b);
        b = (byte)(13.0f / 16.0f * 255); c[14] = new Color32(b, b, b, b);
        b = (byte)(5.0f / 16.0f * 255); c[15] = new Color32(b, b, b, b);
#else
        int i = 0;
        b = (byte)(1.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(49.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(13.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(61.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(4.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(52.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(16.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(64.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

        b = (byte)(33.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(17.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(45.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(29.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(36.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(20.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(48.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(32.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

        b = (byte)(9.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(57.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(5.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(53.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(12.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(60.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(8.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(56.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

        b = (byte)(41.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(25.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(37.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(21.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(44.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(28.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(40.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(24.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

        b = (byte)(3.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(51.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(15.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(63.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(2.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(50.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(14.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(62.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

        b = (byte)(35.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(19.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(47.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(31.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(34.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(18.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(46.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(30.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

        b = (byte)(11.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(59.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(7.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(55.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(10.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(58.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(6.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(54.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

        b = (byte)(43.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(27.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(39.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(23.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(42.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(26.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(38.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(22.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
#endif

        _ditheringTexture.SetPixels32(c);
        _ditheringTexture.Apply();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private Mesh CreateSpotLightMesh()
    {
        // copy & pasted from other project, the geometry is too complex, should be simplified
        Mesh mesh = new Mesh();

        const int segmentCount = 16;
        Vector3[] vertices = new Vector3[2 + segmentCount * 3];
        Color32[] colors = new Color32[2 + segmentCount * 3];

        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(0, 0, 1);

        float angle = 0;
        float step = Mathf.PI * 2.0f / segmentCount;
        float ratio = 0.9f;

        for (int i = 0; i < segmentCount; ++i)
        {
            vertices[i + 2] = new Vector3(-Mathf.Cos(angle) * ratio, Mathf.Sin(angle) * ratio, ratio);
            colors[i + 2] = new Color32(255, 255, 255, 255);
            vertices[i + 2 + segmentCount] = new Vector3(-Mathf.Cos(angle), Mathf.Sin(angle), 1);
            colors[i + 2 + segmentCount] = new Color32(255, 255, 255, 0);
            vertices[i + 2 + segmentCount * 2] = new Vector3(-Mathf.Cos(angle) * ratio, Mathf.Sin(angle) * ratio, 1);
            colors[i + 2 + segmentCount * 2] = new Color32(255, 255, 255, 255);
            angle += step;
        }

        mesh.vertices = vertices;
        mesh.colors32 = colors;

        int[] indices = new int[segmentCount * 3 * 2 + segmentCount * 6 * 2];
        int index = 0;

        for (int i = 2; i < segmentCount + 1; ++i)
        {
            indices[index++] = 0;
            indices[index++] = i;
            indices[index++] = i + 1;
        }

        indices[index++] = 0;
        indices[index++] = segmentCount + 1;
        indices[index++] = 2;

        for (int i = 2; i < segmentCount + 1; ++i)
        {
            indices[index++] = i;
            indices[index++] = i + segmentCount;
            indices[index++] = i + 1;

            indices[index++] = i + 1;
            indices[index++] = i + segmentCount;
            indices[index++] = i + segmentCount + 1;
        }

        indices[index++] = 2;
        indices[index++] = 1 + segmentCount;
        indices[index++] = 2 + segmentCount;

        indices[index++] = 2 + segmentCount;
        indices[index++] = 1 + segmentCount;
        indices[index++] = 1 + segmentCount + segmentCount;

        //------------
        for (int i = 2 + segmentCount; i < segmentCount + 1 + segmentCount; ++i)
        {
            indices[index++] = i;
            indices[index++] = i + segmentCount;
            indices[index++] = i + 1;

            indices[index++] = i + 1;
            indices[index++] = i + segmentCount;
            indices[index++] = i + segmentCount + 1;
        }

        indices[index++] = 2 + segmentCount;
        indices[index++] = 1 + segmentCount * 2;
        indices[index++] = 2 + segmentCount * 2;

        indices[index++] = 2 + segmentCount * 2;
        indices[index++] = 1 + segmentCount * 2;
        indices[index++] = 1 + segmentCount * 3;

        ////-------------------------------------
        for (int i = 2 + segmentCount * 2; i < segmentCount * 3 + 1; ++i)
        {
            indices[index++] = 1;
            indices[index++] = i + 1;
            indices[index++] = i;
        }

        indices[index++] = 1;
        indices[index++] = 2 + segmentCount * 2;
        indices[index++] = segmentCount * 3 + 1;

        mesh.triangles = indices;
        mesh.RecalculateBounds();

        return mesh;
    }


    static void CustomGraphicsBlit (RenderTexture source, RenderTexture dest, Material mat, int passNr)
	{
		RenderTexture.active = dest;

		//mat.SetTexture ("_MainTex", source);

		GL.PushMatrix ();
		GL.LoadOrtho ();

		mat.SetPass (0);

		GL.Begin (GL.QUADS);

		GL.TexCoord2(0, 0);
		GL.Vertex3(0.0F, 0.0F, 0);
		GL.TexCoord2(0, 1);
		GL.Vertex3(0.0F, 1.0F, 0);
		GL.TexCoord2(1, 1);
		GL.Vertex3(1.0F, 1.0F, 0);
		GL.TexCoord2(1, 0);
		GL.Vertex3(1.0F, 0.0F, 0);

		GL.End ();
		GL.PopMatrix ();
	}

    static void CustomGraphicsBlitFog(RenderTexture dest, Material fxMaterial, int passNr)
    {
        RenderTexture.active = dest;

        GL.PushMatrix();
        GL.LoadOrtho();

        fxMaterial.SetPass(passNr);

        GL.Begin(GL.QUADS);

        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f); // BL

        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f); // BR

        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f); // TR

        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f); // TL

        GL.End();
        GL.PopMatrix();
    }
}