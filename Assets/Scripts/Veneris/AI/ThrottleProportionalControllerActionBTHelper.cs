/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Veneris.Vehicle;
namespace Veneris
{
	[System.Serializable]
	public  class  ThrottleGoalForPoint {
		public Vector3 point;
		public float desiredSpeed;
		public float initialDistance;
		public float lastDistanceTraveled;
		public float initialSpeed;
		public bool stoppedAtPoint;
		public float distanceMargin;
		public bool areaReached;
		public bool useAreaTrigger;
		public Collider areaTrigger;
		public bool forceStop = false;

		/*public ThrottleGoalForPoint(Vector3 point, float desiredSpeed, float initialSpeed, float initialDistance, float initialDistaceTraveled,  bool forceStop=false, bool useAreaTrigger=true, float distanceMargin=0f) {
			
			this.point = point;
			this.desiredSpeed = desiredSpeed;
			this.initialSpeed = initialSpeed;
			if (initialDistance > 0) {
				this.initialDistance = initialDistance;
			} else if (initialDistance==0f) {
				//make it a little larger to avoid division by zero
				this.initialDistance = 0.1f;
			} else {
				Debug.Log ("initialDistance at ThrottleGoalForPoint cannot be < 0" + initialDistance);
				throw new UnityException ();
			}

			this.stoppedAtPoint = false;
			this.areaReached = false;
			this.useAreaTrigger = false;
			this.areaTrigger = null;
			this.distanceMargin = distanceMargin;
			this.lastDistanceTraveled = initialDistaceTraveled;
			this.forceStop = forceStop;
		}*/
		public ThrottleGoalForPoint(Vector3 point, float desiredSpeed, float initialSpeed, float initialDistance, float initialDistaceTraveled, Collider areaTrigger, bool forceStop=false ) {
			this.point = point;
			this.desiredSpeed = desiredSpeed;
			this.initialSpeed = initialSpeed;
			if (initialDistance > 0) {
				this.initialDistance = initialDistance;
			} else if (initialDistance==0f) {
				//make it a little larger to avoid division by zero
				this.initialDistance = 0.1f;
			} else {
				Debug.Log ("initialDistance at ThrottleGoalForPoint cannot be < 0" + initialDistance);
				throw new UnityException ();
			}

			this.stoppedAtPoint = false;
			this.areaReached = false;
			this.useAreaTrigger = true;
			this.areaTrigger = areaTrigger;
			this.distanceMargin = 0f;
			this.lastDistanceTraveled = initialDistaceTraveled;
			this.forceStop = forceStop;
		}
		public void SetGoal(Vector3 point, float desiredSpeed, float initialSpeed, float initialDistance, float initialDistaceTraveled, Collider areaTrigger, bool forceStop=false ) {
			this.point = point;
			this.desiredSpeed = desiredSpeed;
			this.initialSpeed = initialSpeed;
			if (initialDistance > 0) {
				this.initialDistance = initialDistance;
			} else if (initialDistance==0f) {
				//make it a little larger to avoid division by zero
				this.initialDistance = 0.1f;
			} else {
				Debug.Log ("initialDistance at ThrottleGoalForPoint cannot be < 0" + initialDistance);
				throw new UnityException ();
			}

			this.stoppedAtPoint = false;
			this.areaReached = false;
			this.useAreaTrigger = true;
			this.areaTrigger = areaTrigger;
			this.distanceMargin = 0f;
			this.lastDistanceTraveled = initialDistaceTraveled;
			this.forceStop = forceStop;
		}

/*		public ThrottleGoalForPoint(Vector3 point, float desiredSpeed, float initialSpeed, float initialDistance, float initialDistaceTraveled, bool forceStop=false,  bool useAreaTrigger=true, float distanceMargin=0f) {
			
			this.point = point;//new Path.PathPointInfo (point, Vector3.zero, Vector3.zero, 0f);
			this.desiredSpeed = desiredSpeed;
			this.initialSpeed = initialSpeed;
			if (initialDistance > 0) {
				//Otherwise we will have a division by zero
				this.initialDistance = initialDistance;
			} else if (initialDistance==0f) {
				//make it a little larger to avoid division by zero
				this.initialDistance = 0.1f;
			} else {
				Debug.Log ("initialDistance at ThrottleGoalForPoint cannot be < 0" + initialDistance);
				throw new UnityException ();
			}

			this.stoppedAtPoint = false;
			this.areaReached = false;
			this.useAreaTrigger = useAreaTrigger;
			this.distanceMargin = distanceMargin;
			this.lastDistanceTraveled = initialDistaceTraveled;
			this.forceStop = forceStop;
		}
		*/
	}
	public enum ThrottleMode 
	{
		ApplyBehaviour,
		//Applies specific behaviour by derived component
		Accelerate,
		// the car simply accelerates at full throttle all the time.
		Brake,
		//the car simply brakes at full brake all the time.
		AdaptToCurvature,
		// the car will brake according to the upcoming change in direction, slowing for corners.
		StopAtPoint,
		// the car will brake as it approaches its target
		SpeedAtPoint,
		//The car will accelerate/brake to reach a point with some given speed
	}
	public class ThrottleProportionalControllerActionBTHelper : MonoBehaviour
	{
		//public Path path=null;
		public AILogic ailogic = null;

		public float lookAheadForSpeed = 9f;
		public float lookAheadForSpeedFactor=0.5f;
		public float maxLookAheadForSpeed=8f;
		public float minLookAheadForSpeed=2f;


		public float CautionFactor = 2f;
		public float brakingStyle=2f;
		public float defaultBrakingStyle;
		public float brakingStyleReduction=0.1f;
		public float maxGripForce = 0f;
		public float stopFullThrottleMargin = 2f;

		[SerializeField]
		protected float _freeSpeed = 32f;
		public float freeSpeed {
			 get {return _freeSpeed;}
			protected set {
				if (float.IsNaN (value)) {
					throw new UnityException ();
				} else {
					
					_freeSpeed = value; 
				}
			}
		}


		public float defaultFreeSpeed=32f;
		//Speed we want in absence of stimuli. Should be adapted to road speed limits and driver style

		public float proportionalControllerSpeedTarget = 15f;
		//Current speed target we are aiming to with the PID




		[SerializeField]
		protected ThrottleMode _throttleMode = ThrottleMode.ApplyBehaviour;
		public ThrottleMode throttleMode {
			get { return _throttleMode; }
			protected set { _throttleMode = value; }
		}

	

		protected Path.PathPointInfo speedLookAheadPoint = null;



		[SerializeField]
		protected ThrottleGoalForPoint goalForPoint = null;

		public ThrottleMode previousThrottleMode;
		public ThrottleGoalForPoint previousGoalForPoint = null;
		public bool savedThrottleState=false;

		protected virtual void Init ()
		{
			
			if (ailogic == null) {
				ailogic = GetComponent<AILogic> ();
			}
		




			//Start at the beginning of the path

			maxGripForce = ailogic.GetMaxGripForce ();

			proportionalControllerSpeedTarget = freeSpeed;
		

		
			defaultBrakingStyle = brakingStyle;



		}
	
		public void SaveThrottleState() {
			if (savedThrottleState) {
				ailogic.Log ("Saving again state" + previousThrottleMode);
				Debug.Break ();
			}
			savedThrottleState = true;
			previousThrottleMode = throttleMode;
			previousGoalForPoint = goalForPoint;

		}
		public void recoverThrottleState() {
			savedThrottleState = false;
			throttleMode= previousThrottleMode;
			goalForPoint=previousGoalForPoint;
			previousGoalForPoint = null;
		}

		public bool SetCurrentLaneOrDefaultFreeSpeed () {
			if (ailogic.currentLane != null) {
				SetSpeedLimit(ailogic.currentLane.speed);
				return true;
			} else {
				SetSpeedLimit(defaultFreeSpeed);
				return false;
			}
		}
		//Use to change the free and defaultFreeSpeed when speed limits are enforced
		public void SetSpeedLimit (float speedLimit) {
			


			freeSpeed = speedLimit;
			defaultFreeSpeed = speedLimit;
		}





		public void SetApplyBehaviour() {
			


			if (savedThrottleState) {
				//Apply when recovered
				previousThrottleMode = ThrottleMode.ApplyBehaviour;
			} else {
				throttleMode= ThrottleMode.ApplyBehaviour;
			}

		}
		public void SetAccelerate() {
			if (savedThrottleState) {
				previousThrottleMode = ThrottleMode.Accelerate;

			} else {
				throttleMode = ThrottleMode.Accelerate;
			}
		}
		public void SetAdaptToCurvature() {
			if (savedThrottleState) {
				previousThrottleMode = ThrottleMode.AdaptToCurvature;
			} else {
				throttleMode = ThrottleMode.AdaptToCurvature;
			}
		}
		public void SetSpeedAtPoint(ThrottleGoalForPoint goal) {
			if (savedThrottleState) {
				previousThrottleMode = ThrottleMode.SpeedAtPoint;
				previousGoalForPoint = goal;
				brakingStyle = defaultBrakingStyle;
			} else {
				throttleMode = ThrottleMode.SpeedAtPoint;
				goalForPoint = goal;
				brakingStyle = defaultBrakingStyle;
			}
		}
		public void SetStopAtPoint(ThrottleGoalForPoint goal) {
			if (savedThrottleState) {
				previousThrottleMode= ThrottleMode.StopAtPoint;
				previousGoalForPoint = goal;
				brakingStyle = defaultBrakingStyle;
			} else {
				throttleMode = ThrottleMode.StopAtPoint;
				goalForPoint = goal;
				brakingStyle = defaultBrakingStyle;
			}

		}

		public void SetBrake() {
			if (savedThrottleState) {
				previousThrottleMode = ThrottleMode.Brake;
			} else {
				throttleMode = ThrottleMode.Brake;
			}
		}
		/*public void SetSpeedAtPoint(Vector3 point, float initalSpeed, float desiredSpeed) {
			throttleMode= ThrottleMode.SpeedAtPoint;
			SetDesiredSpeedAtStopPoint(point,initalSpeed,desiredSpeed);
		}
		public void SetStopAtPoint(Vector3 point,float initialSpeed, float distMargin) {
			throttleMode= ThrottleMode.StopAtPoint;
			SetDesiredSpeedAtStopPoint(point,initialSpeed,0f,distMargin);
		}
		public void SetStopAtPointWithTrigger(Vector3 point,float initialSpeed) {
			throttleMode= ThrottleMode.StopAtPoint;
			driveUntilAreaReachedTrigger = true;
			SetDesiredSpeedAtStopPoint(point,initialSpeed,0f);
		}*/

		public bool HasStoppedAtGoalPoint ()
		{
			if (goalForPoint.areaReached == true) {
				if (ailogic.vehicleInfo.sqrSpeed < 1e-6f) {
					return true;
				} 
			}

			return false;
			//return stopAtPointInfo.stoppedAtStopPoint;
		}

	/*	public float GetMaxGripForce ()
		{

			return GripForce;
			//return 800f;

		}


		public float GetSqrBrakingDistanceAtMaxDeceleration ()
		{
			//Basic estimate: assume maxDeceleration  then to stop we need d=v*v/2a, so d*d=v*v*v*v/4*a*a
			//(2*3.4)^2=46.24

			return ( ailogic.vehicleInfo.sqrSpeed*ailogic.vehicleInfo.sqrSpeed / 4*maxDeceleration*maxDeceleration);


		}
		public float GetBrakingDistanceAtMaxDeceleration ()
		{
			//Basic estimate: assume maxDeceleration, then to stop we need d=v*v/2a, 
			//(2*3.4)^2=46.24


			return (ailogic.vehicleInfo.sqrSpeed / 2*maxDeceleration);


		}
		public float GetEstimatedSqrBrakingDistance (float dec)
		{
			//Basic estimate: use the "real" deceleration

			//Deceleration depends on the time interval, it should be computed by the caller
			//Assuming this is actually a deceleration

			return (ailogic.vehicleInfo.sqrSpeed * ailogic.vehicleInfo.sqrSpeed) / (dec * dec * 4f);


		}

		public float GetEstimatedBrakingDistance (float dec, float desiredSpeed)
		{
			//Assuming constant acc/dece, s=(v^2-vo^2)/2a
			return ((ailogic.vehicleInfo.sqrSpeed) - (desiredSpeed * desiredSpeed) / (2f * dec));
		}

		public float GetEstimatedSqrBrakingDistance (float dec, float desiredSpeed)
		{
			float s = GetEstimatedBrakingDistance (dec, desiredSpeed);
			return (s * s);
		}

		public float GetEstimatedBrakingDistance (float dec)
		{
			//Basic estimate: use the "real" deceleration

			//Deceleration depends on the time interval, it should be computed by the caller
			return (ailogic.vehicleInfo.sqrSpeed / (Mathf.Abs (dec) * 2f));


		}

*/

		// Use this for initialization
		void Start ()
		{
			Init ();

		}

	

		
		public float ComputeMaxCorneringSpeed (float curvature)
		{
			return Mathf.Sqrt (maxGripForce / (curvature *ailogic.vehicleInfo.carController.body.mass));
		}

		public float ProportionalThrottleControl ()
		{
			//Debug.Log ("steeringWheelRotation=" + steeringWheelRotation);

			float tc = 0f;
			switch (throttleMode) {
			case ThrottleMode.AdaptToCurvature:
				{
					tc = AdaptToCurvature ();
					break;

				}
			case ThrottleMode.Accelerate:
				{
					tc = 1;
					break;
				}
			case ThrottleMode.StopAtPoint:
				{
					tc = StopAtPoint ();
					break;
				}
			case ThrottleMode.SpeedAtPoint:
				{
					tc = SpeedAtPoint ();
					break;
				}
			case ThrottleMode.Brake:
				{
					tc = -1f;
					break;
				}

			}
			


			return tc;

		}

		
		protected Path.PathPointInfo GetLookaheadPoint(float lookAhead) {
			int clindex = ailogic.routeManager.lookAtPath.FindClosestPointInInterpolatedPath (ailogic.vehicleInfo.backBumper);

			//2) Find the goal point along the path
			Path.PathPointInfo ppi=null;
			bool inPath = false;
			ppi=ailogic.routeManager.lookAtPath.GetPathInfoAtDistanceFromInterpolatedPath (clindex, lookAhead, ailogic.vehicleInfo.backBumper.position, out inPath);
			if (inPath) {
				return ppi;
			} else {
				//Debug.Log ("Trying next path for lookaheadspeed");
				long nextid = ailogic.routeManager.NextTrackedPathId ();
				Path nextpath = ailogic.routeManager.NextTrackedPath ();
				while (!inPath && nextpath!=null) {
					
					ppi=nextpath.GetPathInfoAtDistanceFromInterpolatedPath (0, lookAhead, ailogic.vehicleInfo.backBumper.position, out inPath);
					nextid = ailogic.routeManager.FollowingPathId (nextid);
					nextpath = ailogic.routeManager.GetPathInRoute (nextid);
				}
				//Debug.Log("Path id in lookahead="+nextid);
				return ppi;
			}
		}
		public float GetMaxCorneringSpeedAtPoint(ref Path.PathPointInfo point) {
			
			float lookAhead = Mathf.Clamp (lookAheadForSpeedFactor*ailogic.vehicleInfo.speed, minLookAheadForSpeed, maxLookAheadForSpeed);
			point = GetLookaheadPoint(lookAhead);
			return ComputeMaxCorneringSpeed(speedLookAheadPoint.curvature);
		}

		
		protected virtual float AdaptToCurvature ()
		{
			float t = 0f;
			
			float maxCorneringSpeed = GetMaxCorneringSpeedAtPoint (ref speedLookAheadPoint);


			if (maxCorneringSpeed <= proportionalControllerSpeedTarget) {
				proportionalControllerSpeedTarget = maxCorneringSpeed;
			} else {
				proportionalControllerSpeedTarget = freeSpeed;
			}

			//Proportional controller with Kp_speed=1
			t = Mathf.Clamp ((proportionalControllerSpeedTarget - ailogic.vehicleInfo.speed), -1, 1);
			//Debug.Log ("lookAhead=" + lookAhead);
			//Debug.Log ("goal.curvature=" + speedLookAheadPoint.curvature + "maxCorneringSpeed=" + maxCorneringSpeed + " speedTarget=" + speedTarget + " throttle=" + t + " deltav=" + (speedTarget - myCar.vLong));
			return t;

		}
		//Linearly decrease the speed from the initial one up to the desired one, according to the distance to point
		public float GetLinearSpeedForSpeedAtPoint() {
			//float prjDistanceToGoalPoint = Mathf.Abs (Vector3.Dot (goalForPoint.point.position-ailogic.vehicleInfo.frontBumper.position, ailogic.vehicleInfo.frontBumper.forward));
			//float f=(goalForPoint.initialDistance-prjDistanceToGoalPoint)/goalForPoint.initialDistance;
			float deltad=ailogic.vehicleInfo.totalDistanceTraveled-goalForPoint.lastDistanceTraveled;
			float f =  Mathf.Clamp01(deltad/ goalForPoint.initialDistance);

			return Mathf.Lerp (goalForPoint.initialSpeed, goalForPoint.desiredSpeed, f);

		}
		//Linearly decrease the speed from the initial one up to the desired one, according to the distance to point. Returns also the projected distance to goal
		public float GetLinearSpeedForSpeedAtPoint(ref float  prjDistanceToGoalPoint) {
			//prjDistanceToGoalPoint = Mathf.Abs (Vector3.Dot (goalForPoint.point.position-ailogic.vehicleInfo.frontBumper.position, ailogic.vehicleInfo.frontBumper.forward));
			//float f=(goalForPoint.initialDistance-prjDistanceToGoalPoint)/goalForPoint.initialDistance;
			float deltad=ailogic.vehicleInfo.totalDistanceTraveled-goalForPoint.lastDistanceTraveled;
			float f =  Mathf.Clamp01(deltad/ goalForPoint.initialDistance);
			prjDistanceToGoalPoint = goalForPoint.initialDistance - deltad;
			return Mathf.Lerp (goalForPoint.initialSpeed, goalForPoint.desiredSpeed, f);

		}
		public float GetSpeedForSpeedAtPoint(ref float  prjDistanceToGoalPoint) {
			if (defaultBrakingStyle == 1) {
				return GetLinearSpeedForSpeedAtPoint (ref prjDistanceToGoalPoint);
			} else {
				//prjDistanceToGoalPoint = Mathf.Abs (Vector3.Dot (goalForPoint.point.position - ailogic.vehicleInfo.frontBumper.position, ailogic.vehicleInfo.frontBumper.forward));
				//float f = (goalForPoint.initialDistance - prjDistanceToGoalPoint) / goalForPoint.initialDistance;
				float deltad=ailogic.vehicleInfo.totalDistanceTraveled-goalForPoint.lastDistanceTraveled;
				float f =  Mathf.Clamp01(deltad/ goalForPoint.initialDistance);
				prjDistanceToGoalPoint = goalForPoint.initialDistance - deltad;
				//ailogic.Log ("getspeed f=" + f + "deltad=" + deltad +"indis"+goalForPoint.initialDistance +" inis"+goalForPoint.initialSpeed+"des="+goalForPoint.desiredSpeed);

				//(vo-vf)(1-f^brakingStyle)+vf
				if (goalForPoint.forceStop) {
					
					//float nextSpeed=(goalForPoint.initialSpeed - goalForPoint.desiredSpeed) * (1f - Mathf.Pow (f, defaultBrakingStyle)) + goalForPoint.desiredSpeed;
					float ed=ailogic.GetBrakingDistanceAtMaxDeceleration();
					if ((goalForPoint.initialDistance-deltad)  <ed) {
						brakingStyle = Mathf.Clamp(brakingStyle - brakingStyleReduction,1f,defaultBrakingStyle);
						return ((goalForPoint.initialSpeed - goalForPoint.desiredSpeed) * (1f - Mathf.Pow (f, brakingStyle)) + goalForPoint.desiredSpeed);
					}

				} 
					return ((goalForPoint.initialSpeed - goalForPoint.desiredSpeed) * (1f - Mathf.Pow (f, defaultBrakingStyle)) + goalForPoint.desiredSpeed);

			}


		}
		public float GetSpeedForSpeedAtPoint() {
			if (defaultBrakingStyle == 1) {
				return GetLinearSpeedForSpeedAtPoint ();
			} else {
				//float prjDistanceToGoalPoint = Mathf.Abs (Vector3.Dot (goalForPoint.point.position - ailogic.vehicleInfo.frontBumper.position, ailogic.vehicleInfo.frontBumper.forward));
				//float f = (goalForPoint.initialDistance - prjDistanceToGoalPoint) / goalForPoint.initialDistance;
				float deltad=ailogic.vehicleInfo.totalDistanceTraveled-goalForPoint.lastDistanceTraveled;
				float f =  Mathf.Clamp01(deltad/ goalForPoint.initialDistance);
				if (goalForPoint.forceStop) {
					
					//float nextSpeed = (goalForPoint.initialSpeed - goalForPoint.desiredSpeed) * (1f - Mathf.Pow (f, brakingStyle)) + goalForPoint.desiredSpeed;
					float ed=ailogic.GetBrakingDistanceAtMaxDeceleration();
					if ((goalForPoint.initialDistance-deltad)  <ed) {
						brakingStyle = Mathf.Clamp (brakingStyle - brakingStyleReduction, 1f, defaultBrakingStyle);
						return ((goalForPoint.initialSpeed - goalForPoint.desiredSpeed) * (1f - Mathf.Pow (f, brakingStyle)) + goalForPoint.desiredSpeed);
					}

				} 
				//(vo-vf)(1-f^brakingStyle)+vf
				return ((goalForPoint.initialSpeed -goalForPoint.desiredSpeed)*(1f- Mathf.Pow(f,defaultBrakingStyle))+goalForPoint.desiredSpeed);
			}

		}


		protected virtual float SpeedAtPoint ()
		{
			
			//Linearly decrease the speed from the initial one up to the desired one, according to the distance to point
			proportionalControllerSpeedTarget =  GetLinearSpeedForSpeedAtPoint();
			//Proportional controller with Kp_speed=1
			return  Mathf.Clamp ((proportionalControllerSpeedTarget - ailogic.vehicleInfo.speed), -1, 1);
			
			
		}

	protected virtual float StopAtPoint ()
		{
			float t = 0f;
			/*
			if (goalForPoint.stoppedAtPoint) {
				return -1f;;
			}
			float prjDistanceToStop = Mathf.Abs (Vector3.Dot (ailogic.vehicleInfo.frontBumper.position - goalForPoint.point.position, ailogic.vehicleInfo.frontBumper.forward));
			float speed = ailogic.vehicleInfo.speed;
			float dec = (at.lastSpeed - speed) / (Time.time - at.lastTime);
			at.lastSpeed = speed;
			at.lastTime = Time.time;
			if (goalForPoint.useAreaTrigger) {
				if (goalForPoint.areaReached && speed > 0) {
					//Dont want to go reverse
					Debug.Log ("stopped");
					goalForPoint.stoppedAtPoint = true;
					t = -1f;
				} else {


					Debug.Log ("dec=" + dec + "; prjToStop=" + prjDistanceToStop + "GetEstimatedSqrBrakingDistance=" + GetEstimatedBrakingDistance (dec));
					t = Mathf.Clamp (prjDistanceToStop - GetEstimatedBrakingDistance (dec), -1, 1);

				}
			} else {
				if ((prjDistanceToStop <= goalForPoint.distanceMargin) && speed > 0) {
					Debug.Log ("prjDistanceToStop=" + prjDistanceToStop);
					Debug.Log ("pr=" + Vector3.Dot (goalForPoint.point.position - ailogic.vehicleInfo.frontBumper.position, ailogic.vehicleInfo.frontBumper.forward));
					Debug.Log ("ailogic.vehicleInfo.speed=" + ailogic.vehicleInfo.speed);
					Debug.Log (dec);
					Debug.DrawLine (ailogic.vehicleInfo.frontBumper.position, goalForPoint.point.position, Color.black);
					Debug.DrawLine (ailogic.vehicleInfo.frontBumper.position, ailogic.vehicleInfo.frontBumper.position + prjDistanceToStop * ailogic.vehicleInfo.frontBumper.forward, Color.cyan);



					//Dont want to go reverse
					Debug.Log ("stopped");
					goalForPoint.stoppedAtPoint = true;
					t = -1f;


				} else {


					Debug.Log ("dec=" + dec + "; prjToStop=" + prjDistanceToStop + "GetEstimatedSqrBrakingDistance=" + GetEstimatedBrakingDistance (dec));
					t = Mathf.Clamp (prjDistanceToStop - GetEstimatedBrakingDistance (dec), -1, 1);

				}
			}
			Debug.Log (Time.time + " throttle=" + t);
			*/
			return t;
		
		}
	

	}



}
