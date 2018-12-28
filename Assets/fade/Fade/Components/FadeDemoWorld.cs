using System.Linq;
using DigitalSalmon.Fade;
using UnityEngine;

public class FadeDemoWorld : MonoBehaviour {

	//-----------------------------------------------------------------------------------------
	// Inspector Variables:
	//-----------------------------------------------------------------------------------------

	[SerializeField]
	protected FadePostProcess fadePostProcess;

	//-----------------------------------------------------------------------------------------
	// Private Fields:
	//-----------------------------------------------------------------------------------------

	private Transform[] children;

	//-----------------------------------------------------------------------------------------
	// Unity Lifecycle:
	//-----------------------------------------------------------------------------------------

	protected void Awake() { children = GetComponentsInChildren<Transform>().Where(t => t != transform).ToArray(); }

	protected void OnEnable() {
		if (!fadePostProcess) return;
		fadePostProcess.FadedDown -= FadePostProcess_FadedDown;
		fadePostProcess.FadedDown += FadePostProcess_FadedDown;
	}

	protected void Start() { RandomiseChildren(); }

	protected void OnDisable() { fadePostProcess.FadedDown -= FadePostProcess_FadedDown; }

	//-----------------------------------------------------------------------------------------
	// Event Handlers:
	//-----------------------------------------------------------------------------------------

	private void FadePostProcess_FadedDown() { RandomiseChildren(); }

	//-----------------------------------------------------------------------------------------
	// Private Methods:
	//-----------------------------------------------------------------------------------------

	private void RandomiseChildren() {
		foreach (Transform child in children) {
			child.transform.position = GetRandomPosition();
			child.transform.rotation = GetRandomRotation();
			child.transform.localScale = GetRandomScale();
		}
	}

	private Vector3 GetRandomPosition() { return new Vector3(Random.Range(-12, 12), Random.Range(-8, 8), Random.Range(10, 20)); }

	private Quaternion GetRandomRotation() { return Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)); }

	private Vector3 GetRandomScale() {
		float scale = Random.Range(0.5f, 2f);
		return new Vector3(scale, scale, scale);
	}
}