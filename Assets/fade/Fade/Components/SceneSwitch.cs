using DigitalSalmon.Fade;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitch : MonoBehaviour {
	//-----------------------------------------------------------------------------------------
	// Inspector Variables:
	//-----------------------------------------------------------------------------------------

	[SerializeField]
	protected FadePostProcess fadePostProcess;

	[SerializeField]
	protected KeyCode keyCode;

	[SerializeField]
	protected string sceneToLoad;

	//-----------------------------------------------------------------------------------------
	// Unity Lifecycle:
	//-----------------------------------------------------------------------------------------

	protected void Update() {
		if (Input.GetKeyDown(keyCode)) {
			fadePostProcess.FadeDown(false, null);
		}
	}
}