using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Enviro/Interior Zone")]
public class EnviroInterior : MonoBehaviour {
	//Feature Controls
	public bool directLighting;
	public bool ambientLighting;
	public bool weatherAudio;
	public bool ambientAudio;
	public bool fog;
	public bool weatherEffects;

	//Lighting
	public Color directLightingMod = Color.black;
	public Color ambientLightingMod = Color.black;
	public Color ambientEQLightingMod = Color.black;
	public Color ambientGRLightingMod = Color.black;
	private Color curDirectLightingMod;
	private Color curAmbientLightingMod;
	private Color curAmbientEQLightingMod;
	private Color curAmbientGRLightingMod;
	private bool fadeInDirectLight = false;
	private bool fadeOutDirectLight = false;
	private bool fadeInAmbientLight = false;
	private bool fadeOutAmbientLight = false;

    //Volume
    public float ambientVolume = 0f;
	public float weatherVolume = 0f;

    //Fog
    public float fogFadeSpeed = 2f;
    public float minFogMod = 0f;
    private bool fadeInFog = false;
    private bool fadeOutFog = false;

    //Weather
    public float weatherFadeSpeed = 2f;
    private bool fadeInWeather = false;
    private bool fadeOutWeather = false;

    public List<EnviroTrigger> triggers = new List<EnviroTrigger>();

	private Color fadeOutColor = new Color (0,0,0,0);

	void Start () 
	{
		
	}
	

	public void CreateNewTrigger ()
	{
		GameObject t = new GameObject ();
		t.name = "Trigger " + triggers.Count.ToString ();
		t.transform.SetParent (transform,false);
		t.AddComponent<BoxCollider> ().isTrigger = true;
		EnviroTrigger trig = t.AddComponent<EnviroTrigger> ();
		trig.myZone = this;
		trig.name = t.name;
		triggers.Add(trig);

		#if UNITY_EDITOR
		UnityEditor.Selection.activeObject = t;
		#endif
	}

	public void RemoveTrigger (EnviroTrigger id)
	{
			DestroyImmediate (id.gameObject);
			triggers.Remove (id);
	}

	public void Enter ()
	{
		EnviroSky.instance.interiorMode = true;

		if (directLighting) {
			fadeOutDirectLight = false;
			fadeInDirectLight = true;
		}

		if (ambientLighting) {
			fadeOutAmbientLight = false;
			fadeInAmbientLight = true;
		}

		if(ambientAudio)
			EnviroSky.instance.Audio.ambientSFXVolumeMod = ambientVolume;
		if(weatherAudio)
			EnviroSky.instance.Audio.weatherSFXVolumeMod = weatherVolume;

        if(fog)
        {
            fadeOutFog = false;
            fadeInFog = true;
        }

        if (weatherEffects)
        {
            fadeOutWeather = false;
            fadeInWeather = true;
        }

    }


	public void Exit ()
	{
		EnviroSky.instance.interiorMode = false;

		if (directLighting) {
			fadeInDirectLight = false;
			fadeOutDirectLight = true;
		}
		if (ambientLighting) {
			fadeOutAmbientLight = true;
			fadeInAmbientLight = false;
		}

		if(ambientAudio)
			EnviroSky.instance.Audio.ambientSFXVolumeMod = 0f;
		if(weatherAudio)
			EnviroSky.instance.Audio.weatherSFXVolumeMod = 0f;

        if (fog)
        {
            fadeOutFog = true;
            fadeInFog = false;
        }

        if (weatherEffects)
        {
            fadeOutWeather = true;
            fadeInWeather = false;
        }
    }


	void Update () 
	{
		if (directLighting) 
		{
			if (fadeInDirectLight) 
			{
				curDirectLightingMod = Color.Lerp (curDirectLightingMod, directLightingMod, 2f * Time.deltaTime);
				EnviroSky.instance.currentInteriorDirectLightMod = curDirectLightingMod;
				if (curDirectLightingMod == directLightingMod)
					fadeInDirectLight = false;
			} 
			else if (fadeOutDirectLight) 
			{
				curDirectLightingMod = Color.Lerp (curDirectLightingMod, fadeOutColor, 2f * Time.deltaTime);
				EnviroSky.instance.currentInteriorDirectLightMod = curDirectLightingMod;
				if (curDirectLightingMod == fadeOutColor)
					fadeOutDirectLight = false;
			}
		}

		if (ambientLighting) 
		{
			if (fadeInAmbientLight) 
			{
				curAmbientLightingMod = Color.Lerp (curAmbientLightingMod, ambientLightingMod, 2f * Time.deltaTime);
				EnviroSky.instance.currentInteriorAmbientLightMod = curAmbientLightingMod;

				if (EnviroSky.instance.lightSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Trilight) {
					curAmbientEQLightingMod = Color.Lerp (curAmbientEQLightingMod, ambientEQLightingMod, 2f * Time.deltaTime);
					EnviroSky.instance.currentInteriorAmbientEQLightMod = curAmbientEQLightingMod;

					curAmbientGRLightingMod = Color.Lerp (curAmbientGRLightingMod, ambientGRLightingMod, 2f * Time.deltaTime);
					EnviroSky.instance.currentInteriorAmbientGRLightMod = curAmbientGRLightingMod;
				}

				if (curAmbientLightingMod == ambientLightingMod)
					fadeInAmbientLight = false;
			} 
			else if (fadeOutAmbientLight) 
			{
				curAmbientLightingMod = Color.Lerp (curAmbientLightingMod, fadeOutColor, 2f * Time.deltaTime);
				EnviroSky.instance.currentInteriorAmbientLightMod = curAmbientLightingMod;

				if (EnviroSky.instance.lightSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Trilight) {
					curAmbientEQLightingMod = Color.Lerp (curAmbientEQLightingMod, fadeOutColor, 2f * Time.deltaTime);
					EnviroSky.instance.currentInteriorAmbientEQLightMod = curAmbientEQLightingMod;

					curAmbientGRLightingMod = Color.Lerp (curAmbientGRLightingMod, fadeOutColor, 2f * Time.deltaTime);
					EnviroSky.instance.currentInteriorAmbientGRLightMod = curAmbientGRLightingMod;
				}

				if (curAmbientLightingMod == fadeOutColor)
					fadeOutAmbientLight = false;
			}
        }

         if (fog)
            {
                if (fadeInFog)
                {
                    EnviroSky.instance.currentInteriorFogMod = Mathf.Lerp(EnviroSky.instance.currentInteriorFogMod, minFogMod, fogFadeSpeed * Time.deltaTime);
                    if (EnviroSky.instance.currentInteriorFogMod <= minFogMod + 0.001)
                       fadeInFog = false;
                }
                else if (fadeOutFog)
                {
                    EnviroSky.instance.currentInteriorFogMod = Mathf.Lerp(EnviroSky.instance.currentInteriorFogMod, 1, (fogFadeSpeed * 2) * Time.deltaTime);
                   if (EnviroSky.instance.currentInteriorFogMod >= 0.999)
                       fadeOutFog = false;
                }
            }


        if(weatherEffects)
        {
            if (fadeInWeather)
            {
                EnviroSky.instance.currentInteriorWeatherEffectMod = Mathf.Lerp(EnviroSky.instance.currentInteriorWeatherEffectMod, 0, weatherFadeSpeed * Time.deltaTime);
                if (EnviroSky.instance.currentInteriorWeatherEffectMod <= 0.001)
                    fadeInWeather = false;
            }
            else if (fadeOutWeather)
            {
                EnviroSky.instance.currentInteriorWeatherEffectMod = Mathf.Lerp(EnviroSky.instance.currentInteriorWeatherEffectMod, 1, (weatherFadeSpeed * 2) * Time.deltaTime);
                if (EnviroSky.instance.currentInteriorWeatherEffectMod >= 0.999)
                    fadeOutWeather = false;
            }
        }
	}
}
