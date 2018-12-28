using UnityEngine;

namespace DigitalSalmon.Fade {
	[CreateAssetMenu(fileName = "FadeEffect",menuName = "FadeEffect", order = 1000000)]
	public class FadeEffect : ScriptableObject {
		//-----------------------------------------------------------------------------------------
		// Inspector Variables:
		//-----------------------------------------------------------------------------------------

		[SerializeField]
		protected string effectName;

		[SerializeField]
		protected Shader shader;

		[HideInInspector]
		[SerializeField]
		protected Material baseMaterial;

		//-----------------------------------------------------------------------------------------
		// Public Properties:
		//-----------------------------------------------------------------------------------------
		
		public string Name { get { return effectName; } }

		public Material BaseMaterial {
			get {
				return baseMaterial;
			}
		}

		public Shader BaseShader {get { return shader; }}

		public Material GenerateMaterial() { return new Material(BaseMaterial); }

		public void EditorAssignMaterial(Material material) { baseMaterial = material; }
	}
}