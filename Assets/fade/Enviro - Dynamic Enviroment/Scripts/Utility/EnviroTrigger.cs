using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnviroTrigger : MonoBehaviour {

	public EnviroInterior myZone;
	public string Name;

	private bool entered = false;

	void Start () 
	{
		
	}
	

	void Update () 
	{
		
	}


	void OnTriggerEnter (Collider col)
	{
		if (!entered)
			return;

		entered = false;

		if (EnviroSky.instance.weatherSettings.useTag) {
			if (col.gameObject.tag == EnviroSky.instance.gameObject.tag) {
				EnterExit ();
			}
		} else {
			if (col.gameObject.GetComponent<EnviroSky> ()) {
				EnterExit ();
			}
		}
	}

	void OnTriggerExit (Collider col)
	{
		if (entered)
			return;

		entered = true;


		if (EnviroSky.instance.weatherSettings.useTag) {
			if (col.gameObject.tag == EnviroSky.instance.gameObject.tag) {
				EnterExit ();
			}
		} else {
			if (col.gameObject.GetComponent<EnviroSky> ()) {
				EnterExit ();
			}
		}
	}
		



	void EnterExit ()
	{
		if (!EnviroSky.instance.interiorMode)
			myZone.Enter ();
		else
			myZone.Exit ();
	}

	void OnDrawGizmos () 
	{
		Gizmos.matrix = transform.worldToLocalMatrix;
		Gizmos.color = Color.blue;
		Gizmos.DrawCube (Vector3.zero,Vector3.one);
	}
}
