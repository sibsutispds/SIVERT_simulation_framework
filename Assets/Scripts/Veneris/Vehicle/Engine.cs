/******************************************************************************/
// 
// Copyright (c) 2019 Fernando Losilla 
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;

namespace Veneris.Vehicle {

	public class Engine: MonoBehaviour {

		private CarController carController;

		public float rpm{
			get {
//				return Mathf.Clamp (_rpmUnclampled, minRpm, maxRpm + 1000f);
				return _rpmUnclampled;
			}
			set{
				//_rpmUnclampled = value < maxRpm + 1000f ? value : maxRpm + 1000f ;
				_rpmUnclampled = value;
			}
		}
		//private float _rpmUnclampled; // approxiamtion used for engine braking at minimum rpm (at other revs rpm = _rpmUncampled)
		public float _rpmUnclampled; // approxiamtion used for engine braking at minimum rpm (at other revs rpm = _rpmUncampled)
		public float torque;
		public float brakingCoefficient = 0.15f; // engine braking
		public float brakingAtMinRpm = 20f;
		public float engineRotInertia = 0.211f; 
		public float rotAccel;
		public float prevRpm;
		public float clutchTorque;

		public bool ignoreThrottle = false;

		public AnimationCurve torqueRpmCurve;
		// Corvette C5
		//public float maxEngineTorque = 450.0f; 

		//standard car
		public float maxEngineTorque = 150.0f; 

		private float[] nextTorque = new float[3];


		public float rpmAtMaxTorque = 4000f;
		public float maxRpm = 6000.0f;

		//public float minRpm = 1000.0f;
		public float minRpm = 850.0f;


		void Start (){
			//carControler = controller;
			carController = GetComponent<CarController> ();
			if (torqueRpmCurve == null) {
				//torqueRpmCurve = new AnimationCurve (new Keyframe (minRpm, 0.8f), new Keyframe ((minRpm + maxRpm) / 2.0f, 1.0f), new Keyframe (maxRpm, 0.75f)); 
				torqueRpmCurve = new AnimationCurve (new Keyframe (minRpm/2, 0f), new Keyframe (minRpm * 1.2f, 0.76f), new Keyframe (rpmAtMaxTorque, 1.0f), new Keyframe (maxRpm, 0.82f), new Keyframe (maxRpm + 1000f, 0f)); 
//				Keyframe key = torqueRpmCurve [2];
//				key.inTangent = 1f;
//				key.outTangent = 1f;
//				torqueRpmCurve.MoveKey (1, key);
				rotAccel = 0f;
				prevRpm = 0f;
				clutchTorque = 0f;
				_rpmUnclampled = minRpm;
				nextTorque = new float[3];
				nextTorque[0] = 0f;
				nextTorque[1] = 0f;
				nextTorque[2] = 0f;
			}
		}

		void FixedUpdate(){
			// TODO move to other place
			if (_rpmUnclampled > minRpm) {
				carController.input.clutch = 0f;
			} else {
				carController.input.clutch = Mathf.Lerp(1f, 0f, Mathf.Clamp01((minRpm - _rpmUnclampled)/minRpm) );
			}

			//torque = nextTorque[2];
//			nextTorque [2] = nextTorque [1];
//			nextTorque [1] = nextTorque [0];
//			nextTorque[0] = calcTorque ();// - carController.input.clutch * clutchTorque;
//			torque = (nextTorque[0] + nextTorque[1] + nextTorque[2])/3f;
			torque = calcTorque ();
				
			_rpmUnclampled += (torque - clutchTorque) / engineRotInertia * Time.fixedDeltaTime * 60f / Mathf.PI /2f;

			if (_rpmUnclampled < minRpm)
				_rpmUnclampled = minRpm;

			//rotAccel = (_rpmUnclampled - prevRpm) / 60f / Time.fixedDeltaTime * 2f * Mathf.PI;
			//prevRpm = _rpmUnclampled;
		}

		public float calcTorque(){
			//			float brakeTorque = 0f;
//			if (rpm < maxRpm)
//				torque = torqueRpmCurve.Evaluate (rpm) * maxEngineTorque * carController.input.throttle;
//			else {
//				torque = -brakingCoefficient * _rpmUnclampled / 60.0f;
//				return torque;
//			}
			//torque = torqueRpmCurve.Evaluate (rpm) * maxEngineTorque * carController.input.throttle;

			// engine braking, disble for very low speeds to avoid undesired effects
			if (!ignoreThrottle) {
				if (float.IsNaN (rpm)) {
					throw new UnityException ();
				}
				torque = torqueRpmCurve.Evaluate (rpm) * maxEngineTorque * carController.input.throttle;
			}	else
				torque = 0f;
			if (_rpmUnclampled > 100f){
				//torque -= brakingCoefficient * _rpmUnclampled / 60.0f;  // engine torque, includes engine braking
				//torque -= 20 + 0.15f * _rpmUnclampled / 60f; //(testing)
				torque -= brakingAtMinRpm + brakingCoefficient * _rpmUnclampled / 60f; //(testing)
			}
			return torque;

//			if (_rpmUnclampled > minRpm)
//				return torque - engineRotInertia * rotAccel;
//			else
//				return torque;
			//			if (rpm < maxRpm)
			//				torque = torqueRpmCurve.Evaluate (rpm) * maxEngineTorque * carControler.input.throttle - brakingCoefficient * _rpmUnclampled / 60.0f;  // engine torque, includes engine braking
			//			else
			//				torque = - brakingCoefficient * _rpmUnclampled / 60.0f; // enigne can't spin faster
			//			
			//			return torque; 
		}
	}
}
