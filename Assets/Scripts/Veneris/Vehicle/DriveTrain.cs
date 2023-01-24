/******************************************************************************/
// 
// Copyright (c) 2019 Fernando Losilla 
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Veneris.Vehicle {



	public class DriveTrain: MonoBehaviour {

		private CarController carControler;
		private Engine engine;
		//[HideInInspector]
		public Axle mainAxle;

		// Corvette C5 gear ratios
		// public  float[] gearRatios = {-2.9f, 0, 2.66f, 1.78f, 1.3f, 1.0f, 0.74f, 0.5f }; // {Reverse, Neutral, 1st, 2nd ... }
		// public float finalDriveRatio = 3.42f;
		// public float transissionEfficiency = 0.7f;

		// Standard car
		public  float[] gearRatios = {-4.058f, 0, 3.58f, 2.04f, 1.41f, 1.11f, 0.88f}; // {Reverse, Neutral, 1st, 2nd ... }
		public float finalDriveRatio = 4.06f;
		public float transissionEfficiency = 0.8f;

		public float clutch; // state of clutch plates, not pedal
		public float rpmClutchEngage = 400f;
		public float rpmClutchDisengage = 1100f; //2000f;
//		[HideInInspector]
		public float clutchStrength = 0.2f;  // caution! controls torque transfer from engine to driveTrain. Too low -> loss of power, too high -> oscillations (unless Time.fixedDeltaTime is changed)
//		[HideInInspector]
		public float maxClutchTorque = 0f; //150f; // be careful with these 2 parameters! Too much clutch torque may cause oscillations at steep accelerations if Time.fixedDeltaTime is the default
		public float engineTorque;
		public float clutchTorque;



		private int _numGears;

		public int _gear;
		public int gear {
			get{
				return _gear - 1;
			}
		}

		private int nextGear;

		public bool hasAutomaticGearShift = true;
		public float rpmToShiftDown = 1100f; //2700.0f;
		//public float rpmToShiftDown = 1200.0f;
		// maximum acceleration shift
		public float rpmToShiftUp = 2000f; //5900.0f;
		//public float rpmToShiftUp = 2000.0f;
		[HideInInspector]
		public bool isShifting = false;
		private float _shiftDuration = 0.5f;
		private float shiftDurationShort=0.0f;
		private float shiftDurationLong=0.0f;
		public float shiftDuration {
			get { return _shiftDuration; }
			set {
				_shiftDuration = value;
				shiftWaitForSecondsShort = new WaitForSeconds (shiftDuration * 0.3f);
				shiftWaitForSecondsLong = new WaitForSeconds (shiftDuration * 0.7f);
				shiftDurationShort = shiftDuration * 0.3f;
				shiftDurationLong = shiftDuration * 0.7f;
			}
		}
		protected WaitForSeconds shiftWaitForSecondsShort = null;
		protected WaitForSeconds shiftWaitForSecondsLong = null;
		public float startShifting;
		public float percentShifting {
			get { 
				if (isShifting)
					return Mathf.Lerp (0f, carControler.input.throttle, (Time.time - startShifting) / shiftDuration);
				else
					return 1f;
			}

		}




		void Start(){
			//carControler = controller;
			carControler = GetComponent<CarController> ();
			engine = GetComponent<Engine> ();

			_gear = 2; // 1st gear
			_numGears = gearRatios.Length;

			// set main axle (powered)
			foreach (var axle in carControler.axles) {
				if (axle.axleInfo.isPowered){
					//					if (mainAxle != null)
					//						Debug.Log ("4WD not supported. Set 'isPowered' in only one axle");
					//					else {
					mainAxle = axle;
					carControler.mainAxle = axle;
					//					}
				}
			}
			if (mainAxle == null) {
				Debug.LogError ("None of the vehicles axles is powered");
			}
			if (maxClutchTorque == 0f) {
				maxClutchTorque = engine.maxEngineTorque;
			}
			shiftWaitForSecondsShort= new WaitForSeconds (shiftDuration * 0.3f);
			shiftWaitForSecondsLong = new WaitForSeconds (shiftDuration * 0.7f);
			shiftDurationShort = shiftDuration * 0.3f;
			shiftDurationLong = shiftDuration * 0.7f;
			//clutch = 1f; 
		}

		void FixedUpdate(){
//			if (engineTorque < maxClutchTorque) {
//				clutchTorque = engineTorque;
//			} else {
//				clutchTorque = maxClutchTorque;
//			}
			//float gearGain = calcTransmissionTorqueMultiplier();

//			clutch = Mathf.Lerp(1f, 0f, carControler.input.clutch);

			if (hasAutomaticGearShift) {
				//				if (mainAxle != null && (!mainAxle.isSlippingTooMuch || _gear <= 1))
				if (mainAxle != null)
					automaticShift ();
			}
			if (carControler.input.requestReverseGear && carControler.vLong < 40.0f) {
				setGear (0);
			}

			clutchControl ();


			float rpmDiff = engine.rpm - mainAxle.rpm * calcRpmInvMultiplier();
//			Debug.Log ("rpmDiff= " + rpmDiff);
			//float clutchStrength = Mathf.Lerp (0.1f, 0.03f, rpmDiff / 7000f);

				//rpmDiff > 500f ? 0.03f : 0.1f;
			clutchTorque = clutch * (engine.rpm - mainAxle.rpm * calcRpmInvMultiplier() ) * clutchStrength;
			clutchTorque = Mathf.Clamp (clutchTorque, -maxClutchTorque, maxClutchTorque);

			mainAxle.driveTorque = clutchTorque * calcTransmissionTorqueMultiplier();
			engine.clutchTorque = clutchTorque;

		}

		void Update () {
			if (carControler.input.requestNeutralGear) {
				carControler.input.requestNeutralGear = false;
				setNeutral ();
			}
				
			if (hasAutomaticGearShift) {
				//Moved to FixedUpdate
			//	if (mainAxle != null)
			//		automaticShift (); 
			} else {
				if (carControler.input.gearUp)
					gearUp ();
				else if (carControler.input.gearDown)
					gearDown ();
			}


			if (carControler.input.requestReverseGear && carControler.vLong < 40.0f) {
				setGear (0);
			}

			clutchControl ();
		}


		public float calcEngineRpm(){ // doing in axle

			return mainAxle.rpm * gearRatios[_gear] * finalDriveRatio;
		}

		public float calcTransmissionTorqueMultiplier(){
			return gearRatios [_gear] * finalDriveRatio * transissionEfficiency;
		}

		public float calcRpmInvMultiplier(){
			return gearRatios [_gear] * finalDriveRatio;
		}


		public void setNeutral(){
			_gear = 1;
		}

		public void gearUp(){
			if (_gear < _numGears && !isShifting){
				_gear = _gear + 1;
				if (_gear == 1) // skip neutral
					_gear = 2;
				StartCoroutine(doShift ());
			}
		}

		public void gearDown(){
			if (_gear > 2 && !isShifting){
				_gear = _gear -1;
				//StartCoroutine(doShift ());
				doShiftNoCoroutine();
			}
		}

		public void setGear(int requestedGear){
			if (!isShifting){
				_gear = requestedGear;
				//StartCoroutine (doShift ());
				doShiftNoCoroutine();
			}
		}

		public void automaticShift(){
			if (isShifting) {
				if ((Time.time - startShifting) >= shiftDurationShort) {
					engine.ignoreThrottle = false;
				}
				if ((Time.time - startShifting) >= shiftDurationLong) {
					isShifting = false;
				}
			}
			if (_gear > 1) {
				if (carControler.input.requestReverseGear)
					setGear (0);
				else if (engine.rpm < rpmToShiftDown) {
					gearDown ();
				} else if (engine.rpm > rpmToShiftUp && _gear < _numGears - 1) {
					gearUp ();
				}

			} else {
				if (carControler.input.gearUp)
					setGear (2);
			}

		}

		public void doShiftNoCoroutine() {
			isShifting = true;
			startShifting = Time.time;
			engine.ignoreThrottle = true;

		}

		public IEnumerator doShift(){
			isShifting = true;
			startShifting = Time.time;
			engine.ignoreThrottle = true;
			yield return  shiftWaitForSecondsShort;
			engine.ignoreThrottle = false;
			yield return shiftWaitForSecondsLong ;
			isShifting = false;
		}

		public void clutchControl(){
			if (_gear == 1) {
				clutch = 0f;
				return;
			}
			if (!isShifting || engine.rpm < rpmClutchEngage) {
				clutch = Mathf.Clamp01 (Mathf.Lerp (0f, 1f, (engine.rpm - rpmClutchEngage) / rpmClutchDisengage));
			} else {
				//float t = percentShifting;
				clutch = percentShifting;
//				clutch = Mathf.Abs(t - 0.5f) * 2f;
//				if (t < 0.9f) {
//					engine.ignoreThrottle = true;
//				} else {
//					engine.ignoreThrottle = false;
//				}
			}
		}

	}

}
