using DigitalSalmon.Fade;
using UnityEngine;

[AddComponentMenu("Fade/Fade On Start")]
public class FadeUpOnStart : MonoBehaviour {
	//-----------------------------------------------------------------------------------------
	// Inspector Variables:
	//-----------------------------------------------------------------------------------------

	[SerializeField]
	protected FadePostProcess fadePostProcess;

	[SerializeField]
	protected FadeEffect[] randomEffects;

	//-----------------------------------------------------------------------------------------
	// Unity Lifecycle:
	//-----------------------------------------------------------------------------------------

	protected void Start() {
		if (randomEffects.Length > 0) {
			int effectIndex = Random.Range(0, randomEffects.Length);
			fadePostProcess.AssignEffect(randomEffects[effectIndex]);
		}
		fadePostProcess.FadeUp();
	}
}