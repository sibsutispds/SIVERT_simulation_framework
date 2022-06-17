/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Opal;

public class ConstantStepMoverAndTransmitter : MonoBehaviour
{
	

	public float stepDistance = 1f;
	public Vector3 direction;
	//Assume it is normalized
	public int maxSteps = -1;
	public bool exitOnArrival = false;
	protected int steps;
	public Transmitter t;


	void Awake() {
		if (t == null) {
			t = GetComponent<Transmitter> ();
		}
		if (!OpalManager.isInitialized) {
			OpalManager.Instance.RegisterOpalInitializedListener (OnOpalManagerInitialized);
			enabled = false;
		}
	
	}

	public void OnOpalManagerInitialized() {
		if (OpalManager.isInitialized) {
			enabled = true;
		}
	}
		// Use this for initialization
		void Start ()
	{
		steps = 0;
	}

	void OnEnable ()
	{
		steps = 0;
	}
	// Update is called once per frame
	void FixedUpdate ()
	{
		
		if (steps > maxSteps) {

			if (exitOnArrival) {
				#if UNITY_EDITOR 
				UnityEditor.EditorApplication.isPaused = true;

				#else
				Application.Quit();

				#endif
			}

			return;
		}
		Debug.Log (Time.time+"\t. Transmit:"+transform.position );
		t.Transmit ();
		transform.Translate (direction * stepDistance);
		steps++;


	}

}

