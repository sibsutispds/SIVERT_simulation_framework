/******************************************************************************/
// 
// Copyright (c) 2019 Fernando Losilla 
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Veneris.Vehicle
{
	public class Differential : MonoBehaviour
	{
		private CarController _carController;
		public Axle axle;

		private Wheel _leftWheel;
		private Wheel _rightWheel;

		public float preloadTorque = 50.0f;

		private float _rpmDiffference = 0f;
		public float _torqueDifference = 0f;



		// Use this for initialization
		void Start ()
		{
			if (axle == null) {
				Debug.LogError ("Differentials must be assined to an axle");
			}
			_leftWheel = axle.wheels[0];
			_rightWheel = axle.wheels[1];


		}
	
		// Update is called once per frame
		void FixedUpdate ()
		{

			_rpmDiffference = _leftWheel.rpm - _rightWheel.rpm;

			//_torqueDifference = Mathf.Abs(_leftWheel.netTorque - _rightWheel.netTorque);
			_torqueDifference = Mathf.Abs(_rpmDiffference);

			if (Mathf.Abs (axle.rpm) > 1f) {
				_leftWheel.diffTorque = -Mathf.Sign (_rpmDiffference) * Mathf.Min (preloadTorque, _torqueDifference) * Mathf.Sign(axle.rpm) / 2f;
				_rightWheel.diffTorque = Mathf.Sign (_rpmDiffference) * Mathf.Min (preloadTorque, _torqueDifference) * Mathf.Sign(axle.rpm) / 2f;
			}

		}
	}
}