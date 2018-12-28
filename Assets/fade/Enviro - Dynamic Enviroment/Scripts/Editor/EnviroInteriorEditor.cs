using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(EnviroInterior))]
public class EnviroInteriorEditor : Editor {

	GUIStyle boxStyle;
	GUIStyle wrapStyle;
	EnviroInterior myTarget;


	void OnEnable()
	{
		myTarget = (EnviroInterior)target;
	}
	
	public override void OnInspectorGUI ()
	{

		myTarget = (EnviroInterior)target;

		if (boxStyle == null) {
			boxStyle = new GUIStyle (GUI.skin.box);
			boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
			boxStyle.fontStyle = FontStyle.Bold;
			boxStyle.alignment = TextAnchor.UpperLeft;
		}

		if (wrapStyle == null)
		{
			wrapStyle = new GUIStyle(GUI.skin.label);
			wrapStyle.fontStyle = FontStyle.Normal;
			wrapStyle.wordWrap = true;
			wrapStyle.alignment = TextAnchor.UpperLeft;
		}

		GUILayout.BeginVertical("Enviro - Interior Zone", boxStyle);
		GUILayout.Space(20);
		EditorGUILayout.LabelField("Welcome to the Interior Zone for Enviro - Sky and Weather!", wrapStyle);
		GUILayout.EndVertical ();

		GUILayout.BeginVertical("Setup", boxStyle);
		GUILayout.Space(20);
		if (GUILayout.Button ("Create New Trigger")) {
			myTarget.CreateNewTrigger ();
		} 

		for (int i = 0; i < myTarget.triggers.Count; i++) {
			GUILayout.BeginVertical ("", boxStyle);
			GUILayout.Space (10);
			myTarget.triggers[i].Name = EditorGUILayout.TextField ("Name", myTarget.triggers[i].Name);
			GUILayout.Space (10);
			if (GUILayout.Button ("Select")) 
			{
				Selection.activeObject = myTarget.triggers[i].gameObject;
			}
			if (GUILayout.Button ("Remove")) 
			{
				myTarget.RemoveTrigger (myTarget.triggers[i]);
			}
			GUILayout.EndVertical ();
		}


		GUILayout.EndVertical ();
		GUILayout.BeginVertical("Lighting", boxStyle);
		GUILayout.Space(20);
		myTarget.directLighting = EditorGUILayout.BeginToggleGroup("Direct Light Modifications", myTarget.directLighting);
		myTarget.directLightingMod = EditorGUILayout.ColorField ("Direct Lighting Mod", myTarget.directLightingMod);
		EditorGUILayout.EndToggleGroup ();
	
		myTarget.ambientLighting = EditorGUILayout.BeginToggleGroup("Ambient Light Modifications", myTarget.ambientLighting);
		myTarget.ambientLightingMod = EditorGUILayout.ColorField ("Ambient Sky Lighting Mod", myTarget.ambientLightingMod);
		if (EnviroSky.instance.lightSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Trilight) {
			myTarget.ambientEQLightingMod = EditorGUILayout.ColorField ("Ambient Equator Lighting Mod", myTarget.ambientEQLightingMod);
			myTarget.ambientGRLightingMod = EditorGUILayout.ColorField ("Ambient Ground Lighting Mod", myTarget.ambientGRLightingMod);
		}
		EditorGUILayout.EndToggleGroup ();
		GUILayout.EndVertical ();
		GUILayout.BeginVertical("Audio", boxStyle);
		GUILayout.Space(20);
		myTarget.ambientAudio = EditorGUILayout.BeginToggleGroup("Ambient Audio Modifications", myTarget.ambientAudio);
		myTarget.ambientVolume = EditorGUILayout.Slider ("Ambient Audio Mod", myTarget.ambientVolume,-1f,0f);
		EditorGUILayout.EndToggleGroup ();
		myTarget.weatherAudio = EditorGUILayout.BeginToggleGroup("Weather Audio Modifications", myTarget.weatherAudio);
		myTarget.weatherVolume = EditorGUILayout.Slider ("Weather Audio Mod", myTarget.weatherVolume,-1f,0f);
		EditorGUILayout.EndToggleGroup ();
		GUILayout.EndVertical ();
        GUILayout.BeginVertical("Fog", boxStyle);
        GUILayout.Space(20);
        myTarget.fog = EditorGUILayout.BeginToggleGroup("Fog Modifications", myTarget.fog);
        myTarget.fogFadeSpeed = EditorGUILayout.Slider("Fog Fading Speed", myTarget.fogFadeSpeed, 0f, 100f);
        myTarget.minFogMod = EditorGUILayout.Slider("Fog Min Value", myTarget.minFogMod, 0f, 1f);
        EditorGUILayout.EndToggleGroup();
        GUILayout.EndVertical();

        GUILayout.BeginVertical("Weather Effects", boxStyle);
        GUILayout.Space(20);
        myTarget.weatherEffects = EditorGUILayout.BeginToggleGroup("Weather Effects Modifications", myTarget.weatherEffects);
        myTarget.weatherFadeSpeed = EditorGUILayout.Slider("Weather Effects Fading Speed", myTarget.weatherFadeSpeed, 0f, 100f);
        EditorGUILayout.EndToggleGroup();
        GUILayout.EndVertical();
    }
}
