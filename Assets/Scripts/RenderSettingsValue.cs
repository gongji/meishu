using UnityEngine;
using System.Collections;

/// <summary>
/// 渲染设置参数
/// </summary>
public class RenderSettingsValue : MonoBehaviour
{
    //天空盒
    [SerializeField]
    public Material skyMaterial;
    //环境光模式
    [SerializeField]
    public UnityEngine.Rendering.AmbientMode ambientMode;

    //环境光的强度
    [SerializeField]
    public float ambientIntensity;

    //环境光颜色
    [SerializeField]
    public Color ambientLight;

    //环境光的天空盒颜色
    [SerializeField]
    public Color ambientSkyColor;

    //赤道颜色
    [SerializeField]
    public Color ambientEquatorColor;
    //地面颜色
    [SerializeField]
    public Color ambientGroundColor;

    //是否启用雾效

    [SerializeField]
    public bool isfog;

    [SerializeField]
    public Color fogColor;

    [SerializeField]
    public FogMode fogmode;

    [SerializeField]
    public float fogStartDistance;


    [SerializeField]
    public float fogEndDistance;

     [SerializeField]
    public float fogDensityValue;


    /// <summary>
    /// 设置渲染参数
    /// </summary>
    public void SetRenderSettings()
    {
        //Skybox skybox = SceneController.currentCamera.GetComponent<Skybox>();
        //if (skybox !=null)
        //{
        //    Debug.Log("SetRenderSettings:"+SceneController.currentCamera.name);
        //    skybox = SceneController.currentCamera.gameObject.AddComponent<Skybox>();
        //    Material currentMat = SkyDataManeger.Instance.currentMode ? skyMaterial : SkyDataManeger.Instance.NightDayMat;
        //    skybox.material = currentMat;
        //}
        //Material currentMat = SkyDataManeger.Instance.currentMode ? skyMaterial : SkyDataManeger.Instance.NightDayMat;
        //RenderSettings.skybox = currentMat;
        //RenderSettings.ambientMode = ambientMode;
        //if (SkyDataManeger.Instance.currentMode == false) return;
        //if (ambientMode == UnityEngine.Rendering.AmbientMode.Trilight)
            
        //{
        //    RenderSettings.ambientSkyColor = ambientSkyColor;
        //    RenderSettings.ambientEquatorColor = ambientEquatorColor;
        //    RenderSettings.ambientGroundColor = ambientGroundColor;

        //}
        //else if(ambientMode == UnityEngine.Rendering.AmbientMode.Flat)
        //{
        //    RenderSettings.ambientLight = ambientLight;
        //}

        //RenderSettings.ambientIntensity = ambientIntensity;
        //RenderSettings.fog = isfog;

        //if (isfog)
        //{
        //    RenderSettings.fogColor = fogColor;
        //    RenderSettings.fogMode = fogmode;
        //    if (fogmode == FogMode.Linear)
        //    {
        //         RenderSettings.fogStartDistance =fogStartDistance;
        //         RenderSettings.fogEndDistance =fogEndDistance;
        //    }
        //    else
        //    {
        //        RenderSettings.fogDensity = fogDensityValue;
        //    }

        //}
        
    }

    /// <summary>
    /// 是否切换到层模式
    /// </summary>
    public void SwitchFloorMode(bool flag)
    {
        if (flag)
        {
            RenderSettings.skybox = null;
        }
        else
        {
            RenderSettings.skybox = skyMaterial;
        }
    }

}
