using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnviroVolumeLight))]
[CanEditMultipleObjects]
public class EnviroVolumeLightEditor : Editor {

	private GUIStyle boxStyle;
	private GUIStyle wrapStyle;
	private GUIStyle headerStyle;

	SerializedObject serializedObj;
	private EnviroVolumeLight myTarget;
	SerializedProperty SampleCount, ScatteringCoef, ExtinctionCoef, Anistropy, Noise, scaleWithTime;

	void OnEnable()
	{
		myTarget = (EnviroVolumeLight)target;
		serializedObj = new SerializedObject (myTarget);
		SampleCount = serializedObj.FindProperty ("SampleCount");
		scaleWithTime= serializedObj.FindProperty ("scaleWithTime");
		ScatteringCoef = serializedObj.FindProperty ("ScatteringCoef");
		ExtinctionCoef = serializedObj.FindProperty ("ExtinctionCoef");
		Anistropy = serializedObj.FindProperty ("Anistropy");
		Noise = serializedObj.FindProperty ("Noise");

	}
	
	public override void OnInspectorGUI ()
	{
		if (boxStyle == null)
		{
			boxStyle = new GUIStyle(GUI.skin.box);
			boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
			boxStyle.fontStyle = FontStyle.Bold;
			boxStyle.alignment = TextAnchor.UpperLeft;
		}

		if (wrapStyle == null)
		{
			wrapStyle = new GUIStyle(GUI.skin.label);
			wrapStyle.fontStyle = FontStyle.Normal;
			wrapStyle.wordWrap = true;
		}

		if (headerStyle == null)
		{
			headerStyle = new GUIStyle(GUI.skin.label);
			headerStyle.fontStyle = FontStyle.Bold;
			headerStyle.wordWrap = true;
		}

		#if UNITY_5_6_OR_NEWER
		serializedObj.UpdateIfRequiredOrScript ();
		#else
		serializedObj.UpdateIfDirtyOrScript ();
		#endif
		EditorGUI.BeginChangeCheck ();
		GUILayout.BeginVertical("Enviro - Volume Light", boxStyle);
		GUILayout.Space(20);
		EditorGUILayout.LabelField("This component adds volume lighting effects to your scene lights!", wrapStyle);
	
		GUILayout.EndVertical ();
		if (myTarget.GetComponent<Light> () == null) {
			EditorGUILayout.LabelField("No Light found on this gameobject. Please add Point or Spot Light!", wrapStyle);
		} else if (myTarget.GetComponent<Light> ().type == LightType.Directional) {
			GUILayout.BeginVertical("", boxStyle);
			EditorGUILayout.LabelField("Please control directional light directly in EnviroSky Manager -> Lighting category!", wrapStyle);
			GUILayout.EndVertical ();
		} else {
			GUILayout.BeginVertical("Settings", boxStyle);
			GUILayout.Space(20);
			EditorGUILayout.PropertyField (SampleCount, true, null);
			GUILayout.Space(10);
			EditorGUILayout.LabelField("Light Settings", headerStyle);
			EditorGUILayout.PropertyField (scaleWithTime, true, null);
			EditorGUILayout.PropertyField (ScatteringCoef, true, null);
			EditorGUILayout.PropertyField (ExtinctionCoef, true, null);
			EditorGUILayout.PropertyField (Anistropy, true, null);
			GUILayout.Space(10);
			EditorGUILayout.PropertyField (Noise, true, null);
			GUILayout.EndVertical ();
		}
			
		if (EditorGUI.EndChangeCheck ()) {
			serializedObj.ApplyModifiedProperties ();
		}
	}
}

//[Header("Quality")]
//[Range(1, 64)]
//public int SampleCount = 8;
//[Header("Light Settings")]
//[Range(0.0f, 1.0f)]
//public float ScatteringCoef = 0.5f;
//[Range(0.0f, 0.1f)]
//public float ExtinctionCoef = 0.01f;
//[Range(0.0f, 0.999f)]
//public float Anistropy = 0.1f;

//[Header("3D Noise")]
//public bool Noise = false;
