using System.Reflection;
using DigitalSalmon.Fade;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FadeEffect))]
public class FadeEffectEditor : Editor {
	private Editor _materialEditor;

	private FadeEffect FadeEffect { get { return target as FadeEffect; } }

	private Editor MaterialEditor {
		get {
			if (_materialEditor == null) {
				if (FadeEffect.BaseMaterial != null) _materialEditor = CreateEditor(FadeEffect.BaseMaterial, typeof(MaterialEditor));
			}
			return _materialEditor;
		}
	}

	private Shader previousShader;

	public override void OnInspectorGUI() {
		if (previousShader != FadeEffect.BaseShader) {
			RegenerateMaterialAssets();
			_materialEditor = null;
			previousShader = FadeEffect.BaseShader;
		}

		GUILayout.Label("Effect Info", EditorStyles.boldLabel);
		base.OnInspectorGUI();

		if (MaterialEditor != null) {
			GUILayout.Label("Effect Material", EditorStyles.boldLabel);

			ForceMaterialEditorVisible();
			MaterialEditor.DrawHeader();
			MaterialEditor.OnInspectorGUI();
		}
	}

	private void ForceMaterialEditorVisible() {
		PropertyInfo forceVisiblePropertyInfo = typeof(MaterialEditor).GetProperty("forceVisible", BindingFlags.Instance | BindingFlags.NonPublic);
			if (forceVisiblePropertyInfo != null) forceVisiblePropertyInfo.SetValue(MaterialEditor, true, null);
	}

	private void RegenerateMaterialAssets() {
		if (FadeEffect.BaseMaterial != null && FadeEffect.BaseShader == FadeEffect.BaseMaterial.shader) return;
		if (FadeEffect.BaseShader != null)
		{
			Material mat = new Material(FadeEffect.BaseShader);
			Object[] subObjects = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(FadeEffect));
			for (int i = 0; i < subObjects.Length; i++)
			{
				if (subObjects[i] != FadeEffect) DestroyImmediate(subObjects[i], true);
			}
			AssetDatabase.AddObjectToAsset(mat, FadeEffect);
			FadeEffect.EditorAssignMaterial(mat);
			AssetDatabase.SaveAssets();
		}
	}
}