/******************************************************************************/
// 
// Copyright (c) 2019 Fernando Losilla 
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Veneris.Vehicle {
	public class VehicleInfo : MonoBehaviour {
		public int vehicleId;

		public Veneris.Vehicle.CarController carController { get; private set;}
		//public AILogic aiLogic { get; private set; }
		public float speed {
			get 
			{
				return Mathf.Sign(carController.vLong)*Mathf.Sqrt((carController.vLat * carController.vLat) + (carController.vLong * carController.vLong));
			}
		}
		public float sqrSpeed {
			get 
			{
				return ((carController.vLat * carController.vLat) + (carController.vLong * carController.vLong));
			}
		}
//		public long laneId {
//			get {
//				return aiLogic.currentLane.laneId;
//			}
//
//		}
//		public long roadId {
//			get {
//				return aiLogic.currentRoad.roadId;
//			}
//
//		}
//		public long roadEdgeId {
//			get {
//				return aiLogic.currentRoad.edgeId;
//			}
//
//		}
//		public Transform frontBumper { get; private set;	}
//		public Transform backBumper { get; private set;	}
//		public List<Collider> vehicleColliders=null;

		public Vector3 velocityV3 {
			get {
				return carController.velocityV3;
			}
		}

		public Vector3 accelV3 {
			get {
				return carController.accelV3;
			}
		}

		public Vector3 eulerAngles {
			get{
				return carController.transform.eulerAngles;
			}
		}

		// Use this for initialization
		void Awake () {

			carController = GetComponent<Veneris.Vehicle.CarController> ();
			if (carController==null) {
				Debug.Log ("No CarController");
			}

//			aiLogic = GetComponentInChildren<AILogic> ();
//			if (aiLogic==null) {
//				Debug.Log ("No AILogic");
//			}
//			frontBumper = transform.Find ("FrontBumper");
//			if (frontBumper == null) {
//				Debug.Log ("No front bumper");
//			}
//			backBumper= transform.Find ("BackBumper");
//			if (frontBumper == null) {
//				Debug.Log ("No back bumper");
//			}
		}
		void Start() {
//			vehicleColliders = new List<Collider> (transform.GetComponents<Collider> ());
//			vehicleColliders.AddRange (transform.GetComponentsInChildren<Collider> ());
			transform.root.name = "Vehicle " + vehicleId;
		}

	}
}
