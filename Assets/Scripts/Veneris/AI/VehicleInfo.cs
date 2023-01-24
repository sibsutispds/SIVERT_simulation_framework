/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Veneris.Vehicle;
namespace Veneris
{
	public class VehicleInfo : MonoBehaviour
	{
		public int vehicleId;

		public enum VehicleActionState
		{
			Undefined,
			Driving,
			WantToChangeLane,
			ChangingLane,
			PreparingToStop,
			CrossingIntersection,
			WaitingForClearance,
			WaitingAtRedLight,
			EvasiveManeuver,
		
		
		};

		public enum TurnSignalState
		{
			None, 
			Right, 
			Left, 
			Both,
		}



		public VenerisLane targetLaneChange = null;
		public TurnSignalState turnSignal;
		public VehicleActionState currentActionState;

		public CarController carController { get; private set; }

		public AILogic aiLogic { get; private set; }

		public float speed {
			get {
				return Mathf.Sign (carController.vLong) * Mathf.Sqrt ((carController.vLat * carController.vLat) + (carController.vLong * carController.vLong));
			}
		}

		public float sqrSpeed {
			get {
				return ((carController.vLat * carController.vLat) + (carController.vLong * carController.vLong));
			}
		}

		public long laneId {
			get {
				return aiLogic.currentLane.laneId;
			}
		
		}

		public long roadId {
			get {
				return aiLogic.currentRoad.roadId;
			}

		}

		public long roadEdgeId {
			get {
				return aiLogic.currentRoad.edgeId;
			}

		}

		public Vector3 velocity {
			get {
				return carController.body.velocity;
			}

		}

		public Transform carBody {
			get {
				return carController.transform;
			}
		}

		public float totalDistanceTraveled {
			get {
				return carController.distanceTraveled;
			}
		}


		public float gripForce {
			get {
				return carController.GripForce;
			}
		}

		public float maxDeceleration {
			get {
				return carController.maxDeceleration;
			}
		}

		public float vehicleLength = 5f;

		public VehicleInfo leadingVehicle = null;
		public LeaderReason leaderReason;

		public Transform frontBumper { get; private set; }

		public Transform backBumper { get; private set; }

		public List<VehicleInfo> waitingForVehicleLock = null;

		public BoxCollider carCollider;

		public List<Collider> vehicleColliders = null;
		// Use this for initialization
		void Awake ()
		{
	
			carController = GetComponent<CarController> ();
			if (carController == null) {
				Debug.Log ("No CarController");
			}
	
			aiLogic = GetComponentInChildren<AILogic> ();
			if (aiLogic == null) {
				Debug.Log ("No AILogic");
			}
			frontBumper = transform.Find ("FrontBumper");
			if (frontBumper == null) {
				Debug.Log ("No front bumper");
			}
			backBumper = transform.Find ("BackBumper");
			if (frontBumper == null) {
				Debug.Log ("No back bumper");
			}
			currentActionState = VehicleActionState.Undefined;
			vehicleLength = (frontBumper.position - backBumper.position).magnitude;
			vehicleColliders = new List<Collider> (transform.GetComponents<Collider> ());
			vehicleColliders.AddRange (transform.GetComponentsInChildren<Collider> ());
			transform.root.name = "Vehicle " + vehicleId;
			carCollider = FindColliderByTagName ("CarCollider") as BoxCollider;

		}

		void Start ()
		{
			waitingForVehicleLock = new List<VehicleInfo> ();
		

		}


		public Collider FindColliderByTagName(string tagName)  {
			for (int i = 0; i < vehicleColliders.Count; i++) {
				if (vehicleColliders [i].CompareTag (tagName)) {
					return vehicleColliders [i];
				}
			}
			return null;
		}

		public bool SetWaitingForClearance ()
		{
			currentActionState = VehicleActionState.WaitingForClearance;
			return true;
		}
		public bool SetWaitingAtRedLight ()
		{
			currentActionState = VehicleActionState.WaitingAtRedLight;
			return true;
		}

		public bool SetUndefined ()
		{
			currentActionState = VehicleActionState.Undefined;
			return true;
		}

		public bool SetDriving ()
		{
			currentActionState = VehicleActionState.Driving;
			return true;
		}
		public bool SetEvasiveManevuer ()
		{
			currentActionState = VehicleActionState.EvasiveManeuver;
			return true;
		}
		public bool SetWantToChangeLane (LaneChangeDirection direction, VenerisLane target) {
			currentActionState = VehicleActionState.WantToChangeLane;
			if (direction == LaneChangeDirection.Left) {
				turnSignal = TurnSignalState.Left;
			} else {
				turnSignal = TurnSignalState.Right;
			}
			targetLaneChange = target;
			return true;
		}
		public bool UnsetWantToChangeLane () {
			currentActionState = VehicleActionState.Driving;
			turnSignal = TurnSignalState.None;
			targetLaneChange = null;
			return true;
		}

		public bool SetChangingLane ()
		{
			currentActionState = VehicleActionState.ChangingLane;
			return true;
		
		}
		public bool UnsetChangingLane() {
			currentActionState = VehicleActionState.Driving;
			return true;
		
		}


		public bool SetPreparingToStop ()
		{
			currentActionState = VehicleActionState.PreparingToStop;
			return true;
		}

		public bool SetCrossingIntersection ()
		{
			currentActionState = VehicleActionState.CrossingIntersection;
			return true;
		}



	}
}
