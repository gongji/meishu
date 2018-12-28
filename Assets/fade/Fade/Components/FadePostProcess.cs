using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace DigitalSalmon.Fade {
	[ExecuteInEditMode]
	[ImageEffectAllowedInSceneView]
	[AddComponentMenu("Fade/Fade Post Process")]
	public class FadePostProcess : MonoBehaviour {
		//-----------------------------------------------------------------------------------------
		// Constants:
		//-----------------------------------------------------------------------------------------

		private const string ALPHA = "_Alpha";
		private const string DELTA = "_Delta";

		//-----------------------------------------------------------------------------------------
		// Delegates:
		//-----------------------------------------------------------------------------------------

		public delegate void FadeEvent();
		public delegate void EffectEvent(FadeEffect effect);

		//-----------------------------------------------------------------------------------------
		// Events:
		//-----------------------------------------------------------------------------------------

		public event FadeEvent FadedUp;
		public event FadeEvent FadedDown;
		public event EffectEvent EffectChanged;

		//-----------------------------------------------------------------------------------------
		// Inspector Variables:
		//-----------------------------------------------------------------------------------------

		[Header("Effect")]
		[SerializeField]
		protected FadeEffect defaultFadeEffect;

		[SerializeField]
		protected float effectDuration = 1;

		[SerializeField]
		protected AnimationCurve effectEasing;

		[Header("Events")]
		[SerializeField]
		protected UnityEvent fadedUpUnityEvent;

		[SerializeField]
		protected UnityEvent fadedDownUnityEvent;

		[Header("Developer")]
		[SerializeField]
		protected bool preview;

		[SerializeField]
		protected float previewAlphaPercentage;

		//-----------------------------------------------------------------------------------------
		// Backing Fields:
		//-----------------------------------------------------------------------------------------

		private FadeEffect _currentEffect;

		//-----------------------------------------------------------------------------------------
		// Private Fields:
		//-----------------------------------------------------------------------------------------

		private Material currentEffectMaterial;

		//-----------------------------------------------------------------------------------------
		// Public Properties:
		//-----------------------------------------------------------------------------------------

		public FadeEffect CurrentEffect {
			get { return _currentEffect; }
			private set {
				if (_currentEffect == value) return;
				_currentEffect = value;
				if (EffectChanged != null) EffectChanged(_currentEffect);
				if (_currentEffect == null) {
					currentEffectMaterial = null;
					return;
				}
				currentEffectMaterial = _currentEffect.BaseMaterial;
			}
		}

		//-----------------------------------------------------------------------------------------
		// Unity Lifecycle:
		//-----------------------------------------------------------------------------------------

		protected void Awake() {
			if (defaultFadeEffect != null) AssignEffect(defaultFadeEffect);
		}

		protected void OnRenderImage(RenderTexture source, RenderTexture destination) {
			if (currentEffectMaterial == null || (!Application.isPlaying && !preview) || GetEffectAlpha() == 0) {
				Graphics.Blit(source, destination);
				return;
			}

			Graphics.Blit(source, destination, currentEffectMaterial);
		}

		protected void Update() {
			if (!Application.isPlaying) {
				if (CurrentEffect != defaultFadeEffect) AssignEffect(defaultFadeEffect);
				if (preview) {
					SetEffectAlpha(previewAlphaPercentage / 100f);
				}
			}
		}

		public void AssignEffect(FadeEffect fadeEffect) {
			float currentAlpha = GetEffectAlpha();
			CurrentEffect = fadeEffect;
			SetEffectAlpha(currentAlpha);
		}

		//-----------------------------------------------------------------------------------------
		// Public Methods:
		//-----------------------------------------------------------------------------------------

		/// <summary>
		/// Fade down then back up again immediately.
		/// </summary>
		public void Dip() {
			StopAllCoroutines();
			StartCoroutine(FadeUpCoroutine(() => FadeUp()));
		}

		/// <summary>
		/// Fades 'Down' (Displays the fade effect).
		/// </summary>
		public void FadeDown(bool instant = false, Action onComplete=null) {
          
			StopAllCoroutines();
			if (instant) {
				SetEffectAlpha(1);
				if (onComplete != null) onComplete();
				return;
			}
			StartCoroutine(FadeDownCoroutine(onComplete));
		}

		/// <summary>
		/// Fades 'Up' (Hides the fade effect).
		/// </summary>
		public void FadeUp(bool instant = false, Action onComplete = null) {
			StopAllCoroutines();
			if (instant) {
				SetEffectAlpha(0);
				if (onComplete != null) onComplete();
				return;
			}
			StartCoroutine(FadeUpCoroutine(onComplete));
		}

		//-----------------------------------------------------------------------------------------
		// Protected Methods:
		//-----------------------------------------------------------------------------------------

		protected void InvokeFadedUp() {
			if (FadedUp != null) FadedUp.Invoke();
			fadedUpUnityEvent.Invoke();
		}

		protected void InvokeFadedDown() {
			if (FadedDown != null) FadedDown.Invoke();
			fadedDownUnityEvent.Invoke();
		}

		protected float GetEffectAlpha() {
			if (currentEffectMaterial == null) return 0;
			return currentEffectMaterial.GetFloat(ALPHA);
		}

		protected void SetEffectAlpha(float alpha) {
			if (currentEffectMaterial == null) return;
			currentEffectMaterial.SetFloat(ALPHA, alpha);
			float delta = effectEasing == null ? alpha : effectEasing.Evaluate(alpha);
			currentEffectMaterial.SetFloat(DELTA, delta);
		}

		//-----------------------------------------------------------------------------------------
		// Private Methods:
		//-----------------------------------------------------------------------------------------

		private IEnumerator FadeDownCoroutine(Action onComplete) {
			float alpha = 0; //GetEffectAlpha();
			
			while (true) {
				alpha += Time.unscaledDeltaTime / effectDuration;

				if (alpha >= 1) {
					alpha = 1;
				}
				SetEffectAlpha(alpha);

				if (alpha == 1) {
					yield return new WaitForEndOfFrame();

					InvokeFadedDown();
					if (onComplete != null) onComplete.Invoke();
					break;
				}

				yield return null;
			}
		}

		private IEnumerator FadeUpCoroutine(Action onComplete) {
			float alpha = 1; //GetEffectAlpha();

			while (true) {
				alpha -= Time.unscaledDeltaTime / effectDuration;

				if (alpha <= 0) {
					alpha = 0;
				}

				SetEffectAlpha(alpha);

				if (alpha == 0) {
					yield return new WaitForEndOfFrame();

					InvokeFadedUp();
					if (onComplete != null) onComplete.Invoke();
					break;
				}

				yield return null;
			}
		}
	}
}