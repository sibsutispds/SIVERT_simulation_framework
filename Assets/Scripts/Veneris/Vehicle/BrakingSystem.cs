/******************************************************************************/
// 
// Copyright (c) 2019 Fernando Losilla 
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;


namespace Veneris.Vehicle
{
	public class BrakingSystem : MonoBehaviour
	{
		private CarController _carController;
		private BaseCarInputController input;

//		private Brake _brakeFL;
//
//		public Brake brakeFL {
//			set {
//				_brakeFL = value;
//				brakes.Add (_brakeFL);
//			}
//			get {
//				return _brakeFL;
//			}
//		}
//
//		private Brake _brakeFR;
//
//		public Brake brakeFR {
//			set {
//				_brakeFR = value;
//				brakes.Add (_brakeFR);
//			}
//			get {
//				return _brakeFR;
//			}
//
//		}
//
//		private Brake _brakeRL;
//
//		public Brake brakeRL {
//			set {
//				_brakeRL = value;
//				brakes.Add (_brakeRL);
//			}
//			get {
//				return _brakeRL;
//			}
//
//		}
//
//		private Brake _brakeRR;
//
//		public Brake brakeRR {
//			set {
//				_brakeRR = value;
//				brakes.Add (_brakeRR);
//			}
//			get {
//				return _brakeRR;
//			}
//
//		}

		public List<Brake> brakes = new List<Brake> ();

		//public float brakeInput = 0.0f;
		private float pedalForce;
		public float maxPedalForce = 650f; //400f;
		public float pedalGain = 9.0f; //typical 4, adjusted to match car to compare to
		// brake pedal force mechanical gain / leverage
		public float masterCylinderArea = 0.00028f; // 0.0006f;
		// used to get ratio for all brakes (in conjunction with slave cylinder area)
		public float hydraulicPressure;
		public Keyframe[] rearPressurePercent; // will be changed according to pedalForce
		public AnimationCurve rearPressureCurve;



		void Awake ()
		{
			_carController = GetComponent<CarController> ();
			brakes = new List<Brake> ();
		}

		// Use this for initialization
		void Start ()
		{
			Wheel[] wheelsArray = _carController.GetComponentsInChildren<Wheel> ();

			foreach (Wheel wheel in wheelsArray) {
				brakes.Add (wheel.brake);
			}

			input = _carController.input;

			if (rearPressurePercent == null) {
				//rearPressurePercent = new [] {new Keyframe(0f, 0.8f), new Keyframe(0.5f, 0.8f), new Keyframe(1f, 0.7f)};
				rearPressurePercent = new [] {new Keyframe(0f, 1f), new Keyframe(0.5f, 1f), new Keyframe(1f, 1f)};
				rearPressureCurve = new AnimationCurve ();
				for (int i = 0; i < rearPressurePercent.Length; i++) {
					rearPressureCurve.AddKey (rearPressurePercent[i]);
					//AnimationUtility.SetKeyLeftTangentMode (rearPressureCurve, i, AnimationUtility.TangentMode.Linear);
					//AnimationUtility.SetKeyRightTangentMode (rearPressureCurve, i, AnimationUtility.TangentMode.Linear);
				}
			}

			//new AnimationCurve(new Keyframe(rearPressurePercent[0]
//			if (!_carController.isStarted)
//				yield return new WaitForSeconds (1.0f);
//			brakeInput = 0.0f;

			//		foreach (Wheel wheel in _carController.wheels) {
			//			brakes.Add (wheel.gameObject.GetComponent<Brake>());
			//		}

		}

		void FixedUpdate ()
		{
			if (input.brake > 0.00f) {
				pedalForce = input.brake * maxPedalForce;
				hydraulicPressure = pedalForce * pedalGain / masterCylinderArea;
				//			foreach (Brake brake in brakes) {
				//				brake.hydraulicPreassure = hydraulicPressure;
				//			}
			} else {
				pedalForce = 0.0f;
				hydraulicPressure = 0.0f;
			}
			foreach (Brake brake in brakes) {
				if (brake.wheel.inAxle == Wheel.axleEnum.REAR) {
					//EBD (Electronic Brake-force Distribution System

					brake.hydraulicPreassure = hydraulicPressure * rearPressureCurve.Evaluate (input.brake); 
				} else {
					brake.hydraulicPreassure = hydraulicPressure;
				}
				//brake.BrakeFixedUpdate ();
			}

		}

		// Update is called once per frame
	/*	void Update ()
		{
//			brakeInput = _carController.brake;
		}
		*/
	}
}