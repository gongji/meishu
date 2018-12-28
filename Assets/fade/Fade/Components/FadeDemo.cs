using System;
using System.Collections;
using DigitalSalmon.Fade;
using UnityEngine;
using Random = UnityEngine.Random;

[AddComponentMenu("Fade/Fade Demo")]
public class FadeDemo : MonoBehaviour {
	//-----------------------------------------------------------------------------------------
	// Constants:
	//-----------------------------------------------------------------------------------------

	private int DEMO_DELAY = 1;

	//-----------------------------------------------------------------------------------------
	// Inspector Variables:
	//-----------------------------------------------------------------------------------------

	[SerializeField]
	protected FadePostProcess fadePostProcess;

	[SerializeField]
	protected int defaultIndex;

	[SerializeField]
	protected FadeEffect[] demoEffects;

	[SerializeField]
	protected Material skyMaterial;

	[SerializeField]
	protected Light skyLight;

	[SerializeField]
	protected Light fillLight;

	//-----------------------------------------------------------------------------------------
	// Private Fields:
	//-----------------------------------------------------------------------------------------

	private int effectIndex;

	//-----------------------------------------------------------------------------------------
	// Unity Lifecycle:
	//-----------------------------------------------------------------------------------------

	protected IEnumerator Start() {
		yield return new WaitForSeconds(DEMO_DELAY);
		effectIndex = defaultIndex;
		StartDemoMode();
	}

	//-----------------------------------------------------------------------------------------
	// Event Handlers:
	//-----------------------------------------------------------------------------------------

	private void FadePostProcess_FadedUp() {
		SwitchFadeEffect();
		StartCoroutine(DelayCoroutine(DEMO_DELAY, () => fadePostProcess.FadeDown()));
	}

	private void FadePostProcess_FadedDown() {
		SwitchFadeEffect();
		ChangeSky();
		StartCoroutine(DelayCoroutine(DEMO_DELAY, () => fadePostProcess.FadeUp()));
	}

	//-----------------------------------------------------------------------------------------
	// Public Methods:
	//-----------------------------------------------------------------------------------------

	public void StartDemoMode() {
		// When we finish fading up, switch effect and fade back down.
		fadePostProcess.FadedUp -= FadePostProcess_FadedUp;
		fadePostProcess.FadedUp += FadePostProcess_FadedUp;

		// When we finish fading down, switch effect and fade up again.
		fadePostProcess.FadedDown -= FadePostProcess_FadedDown;
		fadePostProcess.FadedDown += FadePostProcess_FadedDown;

		// Begin the demo by starting the loop.
		SwitchFadeEffect();
		ChangeSky();
		fadePostProcess.FadeUp();
	}

	public void StopDemoMode() {
		fadePostProcess.FadedUp -= FadePostProcess_FadedUp;
		fadePostProcess.FadedDown -= FadePostProcess_FadedDown;

		fadePostProcess.FadeUp(true);
	}

	//-----------------------------------------------------------------------------------------
	// Private Methods:
	//-----------------------------------------------------------------------------------------

	private void SwitchFadeEffect() {
		effectIndex++;
		if (effectIndex >= demoEffects.Length) effectIndex = 0;

		fadePostProcess.AssignEffect(demoEffects[effectIndex]);
	}

	private void ChangeSky() {
		float hue = Random.Range(0f, 1f);

		Color zenith = Random.ColorHSV(hue, hue, 0.5f, 1f, 0.7f, 1);
		skyMaterial.SetColor("_SkyZenithColor", zenith);
		skyMaterial.SetColor("_SkyAzimuthColor", Random.ColorHSV(hue, hue, 0.1f, 0.2f, 0.9f, 1));
		skyMaterial.SetColor("_SkyNadirColor", Random.ColorHSV(hue, hue, 0.5f, 1f, 0.1f, 0.2f));

		skyLight.color = zenith;
		fillLight.color = zenith;
	}

	private IEnumerator DelayCoroutine(float seconds, Action onComplete) {
		yield return new WaitForSeconds(seconds);
		if (onComplete != null) onComplete();
	}
}