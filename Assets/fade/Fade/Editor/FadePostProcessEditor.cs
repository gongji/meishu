using UnityEditor;
using UnityEngine;

namespace DigitalSalmon.Fade {
	[CustomEditor(typeof(FadePostProcess))]
	public class FadePostProcessEditor : Editor {
		private Editor _effectEditor;

		private FadeEffect previousEffect;

		private FadePostProcess FadePostProcess { get { return target as FadePostProcess; } }
		private FadeEffect FadeEffect { get { return FadePostProcess.CurrentEffect; } } 

		private Editor EffectEditor {
			get {
				if (_effectEditor == null) {
					if (FadeEffect != null) {
						_effectEditor = CreateEditor(FadeEffect);
					}
				}
				return _effectEditor;
			}
		}

		public override void OnInspectorGUI() {
			if (previousEffect != FadeEffect) {
				RegenerateEditor();
				previousEffect = FadeEffect;
			}

			base.OnInspectorGUI();
			if (EffectEditor != null) {
				GUILayout.Label("Fade Effect Editor (Scriptable Object)", EditorStyles.boldLabel);
				EffectEditor.OnInspectorGUI();
			}
		}

		private void RegenerateEditor() { _effectEditor = null; }
	}
}