using System.Collections;
using DigitalSalmon.Fade;
using UnityEngine;
using UnityEngine.UI;

public class EffectNameView : MonoBehaviour {
	//-----------------------------------------------------------------------------------------
	// Constants:
	//-----------------------------------------------------------------------------------------

	private const float TEXT_FADE_DURATION = 0.2f;
	private const float CANVAS_SIZE_SENS = 8f;

	//-----------------------------------------------------------------------------------------
	// Inspector Variables:
	//-----------------------------------------------------------------------------------------

	[SerializeField]
	protected FadePostProcess fadePostProcess;

	[SerializeField]
	protected Text effectName;

	[SerializeField]
	protected RectTransform canvasTransform;

	//-----------------------------------------------------------------------------------------
	// Unity Lifecycle:
	//-----------------------------------------------------------------------------------------

	protected void OnEnable() { fadePostProcess.EffectChanged += FadePostProcess_EffectChanged; }

	protected void Update() {
		canvasTransform.sizeDelta = new Vector2(Mathf.Lerp(canvasTransform.sizeDelta.x, 430 + 60 + effectName.preferredWidth, Time.deltaTime * CANVAS_SIZE_SENS), canvasTransform.sizeDelta.y);
	}

	protected void OnDisable() { fadePostProcess.EffectChanged -= FadePostProcess_EffectChanged; }

	//-----------------------------------------------------------------------------------------
	// Event Handlers:
	//-----------------------------------------------------------------------------------------

	private void FadePostProcess_EffectChanged(FadeEffect effect) {
		StartCoroutine(FadeTextCoroutine(effect.Name));
	}



	IEnumerator FadeTextCoroutine(string newName) {
		float alpha = 1;
		bool forward = false;

		while (true) {
			alpha += (forward ? 1 : -1) * Time.deltaTime / TEXT_FADE_DURATION;

			if (alpha >= 1) {
				alpha = 1;
			}

			if (alpha == 1) {
				break;
			}

			if (alpha <= 0) {
				alpha = 0;
				forward = true;
				effectName.text = newName;
			}

			effectName.color = WithAlpha(effectName.color, alpha);

			yield return null;
		}
	}

	private Color WithAlpha(Color color, float alpha) {
		return new Color(color.r, color.g, color.b, alpha);
	}
}