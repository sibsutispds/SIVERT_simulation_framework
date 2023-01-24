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
	[System.Serializable]
	public class AxleInfo 
	{
		public Wheel leftWheel;
		public Wheel rightWheel;

		public bool isPowered;
		public bool isSteerable;

	}

	public class Axle: MonoBehaviour{
		public AxleInfo axleInfo;
		[HideInInspector]
		CarController carController;

		//[HideInInspector]
		public List<Wheel> wheels = new List<Wheel>();

		public float driveTorque;

		//[HideInInspector]
		public float angSpeed; // not used
		public float rpm;
	
		private float _slipSmoothFactor = 0.2f;
		public float _estimatedSlip = 0.0f;
		//[HideInInspector]
		public bool isSlippingTooMuch {
			get {
				return _estimatedSlip > 0.2f;
			}
		}

			// FUNCTIONS MUST BE CALLED FROM OTHER COMPONENTS, THIS CLASS THAT DOES NOT INHERIT FROM MONOBEHAVIOUR
		void Awake(){
			carController = GetComponent<CarController> ();
		}

		void Start(){
			//next lines commented: done in CarController.Awake()
			//axleInfo must have been set in CarController.Awake()
//			if (axleInfo.leftWheel != null)
//				wheels.Add (axleInfo.leftWheel);
//
//			if (axleInfo.rightWheel != null)
//				wheels.Add (axleInfo.rightWheel);

			if (wheels.Count == 0) {
				Debug.LogError ("There is an axle without any wheels");
			} else {
				if (wheels.Count != 2) {
					Debug.LogWarning ("There is an axle that does not have two wheels");
				}
			}
			_estimatedSlip = 0f;
		}

		void FixedUpdate(){
			// update rpm and apply torque to wheels
			rpm = 0f;
			foreach (Wheel wheel in wheels) {
				rpm += wheel.rpm;
				//wheel.motorTorque = driveTorque / wheels.Count;
				wheel.motorTorque = driveTorque;
			}
			rpm = rpm / wheels.Count;

			float prevSlip =  _estimatedSlip;
			if (carController.vLong != 0) {
				_estimatedSlip = (rpm / 60f * 2f * Mathf.PI * wheels [0].radius - carController.vLong) / carController.vLong;
			} else
				_estimatedSlip = 0f;

			// obtain a rough estimation of wheel slipage (can be used to avoid gear shifthing if too much slip)
			_estimatedSlip = (prevSlip < _estimatedSlip) ? _estimatedSlip : (prevSlip * (1 - _slipSmoothFactor) * _slipSmoothFactor * _estimatedSlip); 
			_estimatedSlip = Mathf.Clamp (_estimatedSlip, 0.0f, 1.0f);

		}
	}
}