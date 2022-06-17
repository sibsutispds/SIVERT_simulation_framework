/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Veneris
{
	public class PlayerAILogic : AILogic
	{
		public LeadingVehicleSelector leadingVehicleSelector = null;
		Vector3 predictedPos;
		Quaternion predictedRot;
		FileResultLogger log=null;
		FileResultLogger distLog=null;
		// Use this for initialization
		void Start ()
		{
			
			controller.Init ();
			leadingVehicleSelector = new LeadingVehicleSelector (this, 2);
			log = new FileResultLogger ("D:\\Users\\eegea\\MyDocs\\MATLAB\\unity\\vehicle", "speedftt", true,false);
			distLog = new FileResultLogger("D:\\Users\\eegea\\MyDocs\\MATLAB\\unity\\vehicle", "distft", true,false);
			log.CreateStream ();
			distLog.CreateStream ();
		}
	
		protected override void FixedUpdate ()
		{
			
			//vision.SetViewDistance (vehicleInfo.sqrSpeed * 0.25f);
		
			throttle = 1f;
			steeringWheelRotation = 0.0f;
			log.RecordWithTimestamp (vehicleInfo.speed);
			distLog.RecordWithTimestamp(vehicleInfo.totalDistanceTraveled);

				

			
		}
		protected override void Update ()
		{
			/*if (controller.steeringWheelRotation != 0) {
				Log ("CurrentPos=" + vehicleInfo.carBody.position + "pred=" + predictedPos);
				Log ("CurrentRot=" + vehicleInfo.carBody.rotation.eulerAngles + "pred=" + predictedRot.eulerAngles);
			
				float tr = ComputeTurningRadius (controller.steeringWheelRotation);
				float angle =  vehicleInfo.carController.vLat* 20* Time.deltaTime/Mathf.Abs(tr);
				Log ("controller.steeringWheelRotation=" + controller.steeringWheelRotation + "vehicleInfo.carController.vLat=" + vehicleInfo.carController.vLat + "Tr=" + tr + "angle=" + angle + "angled=" + (Mathf.Rad2Deg * angle));
				//Vector3 position = new Vector3 (tr*(1.0f-Mathf.Cos (angle)), 0.0f, Mathf.Sin (angle));
				Vector3 position = vehicleInfo.carBody.position + (vehicleInfo.velocity)*20*Time.deltaTime;
				Quaternion relRotation = Quaternion.AngleAxis (Mathf.Rad2Deg * angle, Vector3.up);
				ExtDebug.DrawBox (position, vehicleTriggerColliderHalfSize, vehicleInfo.carBody.rotation * relRotation, Color.red);
				predictedPos = position;
				predictedRot = vehicleInfo.carBody.rotation * relRotation;
				Log (vehicleInfo.carBody.rotation.eulerAngles + " rel rot=" + relRotation.eulerAngles + "totalRot=" + predictedRot.eulerAngles);
			}
			*/
			
		}
		void OnDestroy() {
			log.Close ();
			distLog.Close ();
		}
	}
}
