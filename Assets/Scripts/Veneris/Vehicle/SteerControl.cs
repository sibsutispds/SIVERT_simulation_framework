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
	public class SteerControl : MonoBehaviour
	{
		public Axle axle;

		public CarController carController;
		private BaseCarInputController input;
		private float wheelBase = 2.7f; // will be overwritten in Start()
		private float maxSteeringAngle;

		public Wheel wheelFR;
		public Wheel wheelFL;


		// Use this for initialization
		void Start ()
		{
			carController = GetComponent<CarController> ();
			input = carController.input;
			maxSteeringAngle = carController.maxSteeringAngle;
			wheelBase = carController.wheelBase;




		}
	
		// Update is called once per frame
		/*void Update ()
		{
			ClampSteeringWheelAngle ();
			calcSteeringAngles ();
		}*/
		void FixedUpdate ()
		{
			ClampSteeringWheelAngle ();
			calcSteeringAngles ();
		}

		public void ClampSteeringWheelAngle(){
			if (Mathf.Abs (input.steeringWheelRotation) > input.maxSteerWheelRotation) {
				input.steeringWheelRotation = Mathf.Sign (input.steeringWheelRotation) * input.maxSteerWheelRotation;
			}
		}

		public float ComputeTurningRadius(float steerWheelRotation) {
			if (Mathf.Abs (steerWheelRotation) > input.maxSteerWheelRotation) {
				steerWheelRotation = Mathf.Sign (steerWheelRotation) * input.maxSteerWheelRotation;
			}
			float innerSteerAngle = Mathf.Clamp(steerWheelRotation / input.maxSteerWheelRotation * maxSteeringAngle * Mathf.Deg2Rad,-Mathf.PI/2, Mathf.PI/2);
			return ((wheelBase / Mathf.Tan(innerSteerAngle))-(carController.trackWidth*0.5f));
		}

		public void calcSteeringAngles(){
			float innerSteer;
			float innerRadius;
			Vector3 center;
			Wheel outerWheel;


			if (input.maxSteerWheelRotation !=0) {
				
				innerSteer = input.steeringWheelRotation / input.maxSteerWheelRotation * maxSteeringAngle * Mathf.Deg2Rad; // steer of the wheel with higher steering angle
				//Debug.Log ("input.steeringWheelRotation ="+input.steeringWheelRotation+"input.maxSteerWheelRotation= "+input.maxSteerWheelRotation+"maxSteeringAngle="+maxSteeringAngle+"innerSteer1="+innerSteer);
			} else {
				innerSteer = 0.0f;
			}

			//innerSteer %= Mathf.PI/2;
			innerSteer = Mathf.Clamp(innerSteer, -Mathf.PI/2, Mathf.PI/2);
			//Debug.Log ("innerSteer="+innerSteer);
			if (wheelFR != null){  // the front axle has two wheels

				if (innerSteer>0.01f) { // turn to the right
					innerRadius = Mathf.Abs(wheelBase / transform.lossyScale.z / Mathf.Sin(innerSteer));
					wheelFR.steerAngle = innerSteer * Mathf.Rad2Deg;
					center = new Vector3 (wheelFR.transform.localPosition.x + Mathf.Cos (innerSteer) * innerRadius, 0.0f, wheelFR.transform.localPosition.z - wheelBase / transform.lossyScale.z); 
					Debug.DrawLine(wheelFR.transform.position, transform.TransformPoint(center), Color.magenta);
					outerWheel = wheelFL;
					//Debug.Log ("innerRadius =" + innerRadius);
//					Debug.Log ("Radius(goal)=" + (new Vector3((wheelFL.transform.localPosition.x + wheelFR.transform.localPosition.x)/2f ,wheelFL.transform.localPosition.y, wheelFL.transform.localPosition.z) - center).magnitude);
				} else if (innerSteer<-0.01f){
					innerRadius = Mathf.Abs(wheelBase / transform.lossyScale.z / Mathf.Sin(innerSteer));
					wheelFL.steerAngle = innerSteer * Mathf.Rad2Deg;
					center = new Vector3 (wheelFL.transform.localPosition.x - Mathf.Cos (innerSteer) * innerRadius, 0.0f, wheelFL.transform.localPosition.z - wheelBase / transform.lossyScale.z);
					Debug.DrawLine(wheelFL.transform.position, transform.TransformPoint(center), Color.magenta);
					outerWheel = wheelFR;
					//Debug.Log ("innerRadius =" + innerRadius);
//					Debug.Log ("Radius(goal)=" + (new Vector3(carController.body.centerOfMass.x, 0f, carController.body.centerOfMass.z) - center).magnitude);
					//Debug.Log ("Radius(goal)=" + (new Vector3(transform.localPosition.x + wheelFR.transform.localPosition.x)/2f ,wheelFL.transform.localPosition.y, wheelFL.transform.localPosition.z) - center).magnitude);
				} else {
					wheelFL.steerAngle = innerSteer * Mathf.Rad2Deg;
					wheelFR.steerAngle = innerSteer * Mathf.Rad2Deg;
					return;
				}
				outerWheel.steerAngle = Mathf.Atan2 (outerWheel.transform.localPosition.z - center.z, center.x - outerWheel.transform.localPosition.x) * Mathf.Rad2Deg;
				if (innerSteer < 0.0f)
					outerWheel.steerAngle = outerWheel.steerAngle -  180.0f;



			} else { // just one wheel vehicle
				wheelFL.steerAngle = innerSteer * Mathf.Rad2Deg; 
			}

		}

	}
}