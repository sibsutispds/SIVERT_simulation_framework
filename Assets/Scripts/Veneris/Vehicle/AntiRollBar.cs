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
	public class AntiRollBar : MonoBehaviour
	{
		public WheelCollider wheelL;
		public WheelCollider wheelR;
		public Rigidbody body;
		private CarController carController;
	
		public float antilRollCoef = 5000.0f;
	
		public float antiRollForce;
	
		// Use this for initialization
		void Start ()
		{
			body = GetComponent<Rigidbody> ();
			carController = GetComponent<CarController> ();
	
		}
		
		// Update is called once per frame
		void FixedUpdate ()
		{
			WheelHit hit;
			float travelL = 1.0f;
			float travelR = 1.0f;
	
			bool isGroundedL = wheelL.GetGroundHit (out hit);
			if (isGroundedL) {
				//travelL = (-wheelL.transform.InverseTransformPoint (hit.point).y - wheelL.radius) / wheelL.suspensionDistance;
				travelL = (-wheelL.transform.InverseTransformPoint (hit.point).y - wheelL.radius);
			}
	
			bool isGroundedR = wheelR.GetGroundHit (out hit);
			if (isGroundedR) {
				//travelR = (-wheelR.transform.InverseTransformPoint (hit.point).y - wheelR.radius) / wheelR.suspensionDistance;
				travelR = (-wheelR.transform.InverseTransformPoint (hit.point).y - wheelR.radius);
			}
	
			antiRollForce = (travelL - travelR) * antilRollCoef; 
			antiRollForce = Mathf.Atan2(travelL - travelR, carController.trackWidth) * Mathf.Rad2Deg * antilRollCoef * carController.trackWidth/4f ; // /4 = /2 (half axle) + /2 (2 wheels)
			//antiRollForce = (travelL - travelR/ carController.trackWidth) * Mathf.Rad2Deg * antilRollCoef * carController.trackWidth/4f ; // /4 = /2 (half axle) + /2 (2 wheels)
			//antiRollForce = body.transform.localEulerAngles.z * antilRollCoef / wheelL.transform.localPosition.x /2f;
			//Debug.Log("Angulo estimado = " + Mathf.Atan2(travelL - travelR, 1.68f) * Mathf.Rad2Deg);
			//Debug.Log ("transform:" + transform.eulerAngles.z);
			//Debug.Log ("Grounded = " + isGroundedL +" "+ isGroundedR);


			if (isGroundedL && isGroundedR) {
				body.AddForceAtPosition (wheelL.transform.up * -antiRollForce, wheelL.transform.position);
				body.AddForceAtPosition (wheelR.transform.up * antiRollForce, wheelR.transform.position);
			}
//			if (isGroundedL) {
//				body.AddForceAtPosition (wheelL.transform.up * -antiRollForce, wheelL.transform.position);
//			}
//	
//			if (isGroundedR) {
//				body.AddForceAtPosition (wheelR.transform.up * antiRollForce, wheelR.transform.position);
//			}
		}
	}
}