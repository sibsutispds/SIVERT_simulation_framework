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


//	[RequireComponent (typeof(DriveTrain))]
//	[RequireComponent (typeof(Engine))]
	public class PowerTrain : MonoBehaviour
	{
		//input
		// public BaseInput... // should be set in awake in CarController
		public CarController carController;
		public BaseCarInputController input;
		public DriveTrain driveTrain;
		public Engine engine;

		public float driveTorque;
		// Use this for initialization
		void Awake(){
			if (GetComponent<DriveTrain> () == null)
				gameObject.AddComponent<Engine> ();
			if (GetComponent<DriveTrain> () == null)
				gameObject.AddComponent<DriveTrain> ();
		}
		void Start ()
		{
			carController = GetComponent<CarController> ();
			if (input == null) {
				input = GetComponent<BaseCarInputController> ();
			}

			if (driveTrain == null)
				driveTrain = GetComponent<DriveTrain> ();
//				driveTrain = new DriveTrain(carController);
			if (engine == null)
//				engine = new Engine (carController);
				engine = GetComponent<Engine> ();
		}
	
		// Update is called once per frame
	/*	void Update ()
		{
		
		}
		*/

		void FixedUpdate(){
//			engine.rpm = driveTrain.calcEngineRpm ();
//			engine.torque = engine.calcTorque ();
//			driveTrain.mainAxle.driveTorque = engine.torque * driveTrain.calcTransmissionTorqueMultiplier();

			//			driveTrain.engineTorque = engine.torque;
//			engine.clutchTorque = driveTrain.clutchTorque;
			if (driveTrain.isShifting){
				//driveTrain.mainAxle.driveTorque *= 0.8f;
				//driveTrain.mainAxle.driveTorque *= (engine.maxRpm + engine.rpm)/ (engine.maxRpm + engine.maxRpm) / (1f + carController.input.throttle/2f); // simplification of clutch effect 
//				driveTrain.mainAxle.driveTorque *= driveTrain.percentShifting;
			}
		}
	}
}