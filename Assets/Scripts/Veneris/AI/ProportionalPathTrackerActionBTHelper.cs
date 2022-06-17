/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;
using UnityEngine;
using FluentBehaviourTree;
using Veneris.Vehicle;
namespace Veneris
{
	public class ProportionalPathTrackerActionBTHelper : MonoBehaviour
	{

		public float lookAheadDistance = 18f;
		public float minLookAheadDistance = 1f;
		public float maxLookAheadDistance = 35f;
		public float lookAheadTime = 0.7f;
		//Speed dependent lookahead
		public bool UseSpeedDependentLookAhead = true;
		protected Vector3 localSteerLookAheadPoint;
		private float sqrLookAheadDistance = 0f;

		//public Path path=null;
		[SerializeField]
		protected Path _nextPath = null;
		public Path nextPath  {
			get { return _nextPath; }
			protected set { _nextPath = value; }
		}
		[SerializeField]
		protected Path _lookAtPath = null;
		public Path lookAtPath 	 {
			get { return _lookAtPath; }
			protected set { SetLookAtPath(value); }
		}
		public AILogic ailogic = null;
		public float maxDeltaAngle = 90f;
	

		public Vector3 modifiedPosition;




	

		public Transform carCenter = null;




		public Path.PathPointInfo _steerLookAheadPoint = null; 
		public Path.PathPointInfo steerLookAheadPoint {
			get {return _steerLookAheadPoint; }
			protected set { 
				_steerLookAheadPoint = value;
				ailogic.vision.SetSteerLookAtPoint (value.position);
					
			}
		}

	



		// Use this for initialization
		void Start ()
		{
			if (ailogic == null) {
				ailogic = GetComponent<AILogic> ();
			}
		

			if (carCenter == null) {
				//Vector3 fl = myCar.transform.Find ("Wheels/wheel_FL").position;
				//Vector3 fr = myCar.transform.Find ("Wheels/wheel_FR").position;
				//frontAxle = myCar.transform.Find ("Wheels/wheel_FL");
				//frontAxle = myCar.transform;
				carCenter = ailogic.vehicleInfo.carBody;

			}

		





		}

		public Vector3 GetLocalSteerLookAheadPoint() {
			return localSteerLookAheadPoint;
		}

		public void SetLookAtPath(Path path) {
			_lookAtPath = path;
			ailogic.routeManager.SetLookAtPath (path);
		}

		public FluentBehaviourTree.BehaviourTreeStatus GetPathPoint() {
			//1) Find the point (index in interpolated path) in the path closest to this vehicle

			int clindex =ailogic.routeManager.lookAtPath.FindClosestPointInInterpolatedPath (carCenter);
			if (UseSpeedDependentLookAhead) {
				lookAheadDistance = Mathf.Clamp (ailogic.vehicleInfo.speed * lookAheadTime, minLookAheadDistance, maxLookAheadDistance);
			}


			//2) Find the goal point along the path
			bool inPath;
			steerLookAheadPoint = ailogic.routeManager.lookAtPath.GetPathInfoAtDistanceFromInterpolatedPath (clindex, lookAheadDistance, carCenter.position, out inPath);

			if (inPath) {
				SetLookAtPath (ailogic.routeManager.lookAtPath);
				return FluentBehaviourTree.BehaviourTreeStatus.Success;
			} else {
				
				return FluentBehaviourTree.BehaviourTreeStatus.Failure;
			}
		}
		public FluentBehaviourTree.BehaviourTreeStatus GetPathPoint(float lookAheadDistance) {
			//1) Find the point (index in interpolated path) in the path closest to this vehicle

			int clindex =ailogic.routeManager.lookAtPath.FindClosestPointInInterpolatedPath (carCenter);
		


			//2) Find the goal point along the path
			bool inPath;
			steerLookAheadPoint = ailogic.routeManager.lookAtPath.GetPathInfoAtDistanceFromInterpolatedPath (clindex, lookAheadDistance, carCenter.position, out inPath);

			if (inPath) {
				SetLookAtPath (ailogic.routeManager.lookAtPath);
				return FluentBehaviourTree.BehaviourTreeStatus.Success;
			} else {

				return FluentBehaviourTree.BehaviourTreeStatus.Failure;
			}
		}
		public FluentBehaviourTree.BehaviourTreeStatus GetNextPathPoint() {
			bool inPath;
			//nextPath = ailogic.routeManager.FollowingPath (lookAtPath.pathId);


			nextPath = ailogic.routeManager.FollowingPath (ailogic.routeManager.lookAtPath.pathId);

			if (nextPath != null) {
				
				int clindex = nextPath.FindClosestPointInInterpolatedPath (carCenter);
				steerLookAheadPoint = nextPath.GetPathInfoAtDistanceFromInterpolatedPath (clindex, lookAheadDistance, carCenter.position, out inPath);
				SetLookAtPath (nextPath);
			
				if (inPath) {
					return FluentBehaviourTree.BehaviourTreeStatus.Success;
				} else {
					//ailogic.Log (1, "nex path not inpath"+steerLookAheadPoint.position);
					return FluentBehaviourTree.BehaviourTreeStatus.Failure;
				}
			} else {
				//ailogic.Log (1, "nex path null");
				return FluentBehaviourTree.BehaviourTreeStatus.Failure;
			}
		}
		//To simulate needed steering to some position
		public float GetSteeringWheelRotationToPosition(Vector3 position) {
			Vector3 localPoint=carCenter.InverseTransformPoint (position);
			//ailogic.Log ("old position" + frontAxle.InverseTransformPoint(steerLookAheadPoint.position) + "new position " + lgoal);
			// work out the local angle towards the target
			float angle = Mathf.Atan2 (localPoint.x, localPoint.z) * Mathf.Rad2Deg;
			return angle;
		}
		public float GetSteeringWheelRotationToLookAheadPoint() {

			localSteerLookAheadPoint = carCenter.InverseTransformPoint (steerLookAheadPoint.position);
			return  Mathf.Atan2 (localSteerLookAheadPoint.x, localSteerLookAheadPoint.z) * Mathf.Rad2Deg;
		}

		public FluentBehaviourTree.BehaviourTreeStatus SteerRelativeToCurrentPoint (Vector3 vector)
		{
			// calculate the local-relative position of the target, to steer towards
			modifiedPosition = vector;
			localSteerLookAheadPoint = carCenter.InverseTransformPoint (steerLookAheadPoint.position + vector);
			//ailogic.Log ("old position" + frontAxle.InverseTransformPoint(steerLookAheadPoint.position) + "new position " + lgoal);
			// work out the local angle towards the target
			float angle = Mathf.Atan2 (localSteerLookAheadPoint.x, localSteerLookAheadPoint.z) * Mathf.Rad2Deg;
	

			ailogic.steeringWheelRotation = angle;
		
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}
		public FluentBehaviourTree.BehaviourTreeStatus SteerTo (Vector3 position)
		{
			// calculate the local-relative position of the target, to steer towards

			//localSteerLookAheadPoint = frontAxle.InverseTransformPoint (position);
			Vector3 localPoint=carCenter.InverseTransformPoint (position);
			//ailogic.Log ("old position" + frontAxle.InverseTransformPoint(steerLookAheadPoint.position) + "new position " + lgoal);
			// work out the local angle towards the target
			float angle = Mathf.Atan2 (localPoint.x, localPoint.z) * Mathf.Rad2Deg;


			ailogic.steeringWheelRotation = angle;

			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}
		public FluentBehaviourTree.BehaviourTreeStatus SteerToLocalPoint (Vector3 localPosition)
		{
			// calculate the local-relative position of the target, to steer towards

			//localSteerLookAheadPoint = localPosition;
			//ailogic.Log ("old position" + frontAxle.InverseTransformPoint(steerLookAheadPoint.position) + "new position " + lgoal);
			// work out the local angle towards the target
			float angle = Mathf.Atan2 (localPosition.x, localPosition.z) * Mathf.Rad2Deg;


			ailogic.steeringWheelRotation = angle;

			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}
		public void SetSteerLookAheadPosition(Vector3 position) {
			steerLookAheadPoint.position = position;
		}
		public FluentBehaviourTree.BehaviourTreeStatus ProportionalSteerController ()
		{

			//Implement the steering as a proportional controller (independent of scale):
			//We need the error angle (difference between our heading and point position)
			//The Kp_steer gain is 1


			// calculate the local-relative position of the target, to steer towards
			localSteerLookAheadPoint = carCenter.InverseTransformPoint (steerLookAheadPoint.position);
			// work out the local angle towards the target
			float angle = Mathf.Atan2 (localSteerLookAheadPoint.x, localSteerLookAheadPoint.z) * Mathf.Rad2Deg;
		

			ailogic.steeringWheelRotation = angle;

			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		


		}

	

		void OnDrawGizmos ()
		{
			if (Application.isPlaying && steerLookAheadPoint != null) {

				Gizmos.color = Color.blue;
				Gizmos.DrawLine (carCenter.position, steerLookAheadPoint.position);
				//Debug.Log ("steergoal=" + steerGoal.position);
				//Gizmos.color = Color.green;
				//Gizmos.DrawLine (frontAxle.position, steerLookAheadPoint.position + modifiedPosition);
		





			}

		}



	}
}

