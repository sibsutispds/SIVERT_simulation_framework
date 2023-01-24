/******************************************************************************/
// 
// Copyright (c) 2019 Fernando Losilla 
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;
using System;

namespace Veneris.Vehicle
{
	public class KeyboardCarInputController : BaseCarInputController
	{
//		public float timeToFullThrottle = 0.5f;
//		public float timeToMaxSteer = 0.5f;
//		public float timeToMaxBrake = 0.5f;

		//private CarController cc = null;
		//private float lastSpeed = 0f;

		// Use this for initialization
		void Start ()
		{
			//cc = GetComponent<CarController> ();

		}

		// Update is called once per frame
		void Update ()
		{
			readInput ();
		}

		public  void readInput ()
		{
			float hor = Input.GetAxis ("Horizontal");
			float ver = Input.GetAxis ("Vertical");
			float signThrottle;
			float signBrake;
			float signSteer;

			//ver = 0.3f;

			throttle = ver * (ver > 0 ? 1 : 0);
			//float speed = Mathf.Sqrt ((cc.vLat * cc.vLat) + (cc.vLong * cc.vLong));
			//Debug.Log ("Acceleration:=" + ((speed - lastSpeed) / Time.deltaTime));
			//lastSpeed = speed;


			steeringWheelRotation = hor * maxSteerWheelRotation;

			brake = ver * (ver < 0 ? -1 : 0);

			requestReverseGear = Input.GetButton ("Reverse");
			gearUp = Input.GetButton ("Gear Up");
			gearDown = Input.GetButton ("Gear Down");



		}
	}
}