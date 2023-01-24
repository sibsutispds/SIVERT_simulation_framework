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
	public class CollisionPrediction
	{


		public struct TTCInfo
		{
			public float ttcSphere;
			public float ttcCenter;
			public bool inside;
			public float distance;

		}

		public static float vehicleLengthIncreaseFactor = 1.2f;	//Less accurate but safer
	
		#region FittedModelApproximations
		public static float SpeedFullThrottleFittedModel(float time) {
			//A third degree polynomial regression fitted to the v(t) standard vehicle data of our tests. Speed as a function of time for maximum throttle (t=1), up to 40 m/s
			//v(t)-0.680795+4.13758 x-0.168803 x^2+0.00280834 x^3
			return (-0.680795f+4.13758f*time-0.168803f*time*time+0.00280834f*time*time*time);
		
		}
		public static  float DistanceFullThrottleFittedModel(float time) {
			//Integration of the third degree polynomial regression fitted to the v(t) standard vehicle data of our tests. Distance as a function of time for maximum throttle (t=1), up to 40 m/s
			//d(x)=-0.680795 x+2.06879 x^2-0.0562677 x^3+0.000702085 x^4
			return (-0.680795f*time+2.06879f*time*time-0.0562677f*time*time*time+0.00070208f*time*time*time*time);

		}
		public static float InverseSpeedFullThrottleFittedModel(float speed) {
			//A third degree polynomial fitted to the inverse of the v(t) standard vehicle data of our tests. t=iv(v) That is,  return the time needed, starting from speed=0, to reach speed v, under maximum throttle
			//t=iv(x)=-0.0870299+0.383389 x-0.0113066 x^2+0.000418353 x^3
			return( -0870299f +0.383389f*speed- 0.011306f*speed*speed+0.00041835f*speed*speed*speed);
		
		}
		public static float InverseDistanceFullThrottleFittedModel(float distance) { 
			//A fourth degree polynomial fitted to the inverse of the d(t) standard vehicle data of our tests. t=id(d) That is,  return the time needed, starting from speed=0, to reach distance d, under maximum throttle
			//t=id(x)=1.27249 +0.0930424 x-0.000314014 x^2+6.67884*10^-7 x^3-5.12018*10^-10 x^4
			return 1.27249f +0.0930424f*distance-0.000314014f*distance*distance+6.67884E-7f*distance*distance*distance-5.12018E-10f*distance*distance*distance*distance;
		}

		public static float AverageSpeedFullThrottleFittedModel(float timeSpan) { 
			//A second degree polynomial regression fitted to the average speed achieved over a time spam, in steps of 0.02s (usual fixedDeltaTime), at full throttle. It reproduces a cumulative average over the v(t) data from our standard vehicle tests.
			//In matlab it would be cumsum(speed)'./[1:length(speed)] and we would ask for the timeSpan index
			//avs(x)=-0.176122+1.81983 x-0.0306923 x^2
			return (-0.176122f+1.81983f*timeSpan-0.0306923f*timeSpan*timeSpan);
		}
		#endregion


		//Compute velocity obstacle with the current velocity
		public static void ComputeVelocityObstacle (VehicleInfo me, VehicleInfo other, float timeHorizon)
		{

			float invTime = 1.0f / timeHorizon;

			//Plot relative positions
			Debug.DrawLine (me.carBody.position, other.carBody.position, Color.yellow);
			//Plot relative velocity
			Debug.DrawRay (me.carBody.position, me.velocity - other.velocity, Color.red);
			Debug.DrawRay (me.carBody.position, me.velocity, Color.blue);
			BoxCollider bc = other.carCollider.GetComponent<BoxCollider> ();
			float radiusSqr = (bc.size.z) * (bc.size.z) + (bc.size.x) * (bc.size.x);//sum radius 
			Vector3 lp = other.carBody.position - me.carBody.position;
			float dr = lp.sqrMagnitude;

			float alpha = Mathf.Asin (Mathf.Sqrt (radiusSqr / dr));
			float angle = Vector3.SignedAngle (me.carBody.forward, lp, Vector3.up);
			float lsqr = lp.sqrMagnitude - radiusSqr;
			float l = Mathf.Sqrt (lsqr);
			float a1 = (angle * Mathf.Deg2Rad) + alpha;
			float a2 = (angle * Mathf.Deg2Rad) - alpha;

			float x = l * Mathf.Sin (a1);
			float z = l * Mathf.Cos (a1);
			float x2 = l * Mathf.Sin (a2);
			float z2 = l * Mathf.Cos (a2);
			Vector3 p1 = me.carBody.TransformPoint (new Vector3 (x, 0.0f, z));
			Vector3 p2 = me.carBody.TransformPoint (new Vector3 (x2, 0.0f, z2));

			/*	GameObject go = new GameObject ("sphere");
			SphereCollider sc = go.AddComponent<SphereCollider> ();
			go.transform.position = bc.transform.position;
			sc.radius = Mathf.Sqrt (radiusSqr);
			sc.isTrigger = true;
			go.transform.SetParent (me.carBody);
*/
			Debug.DrawLine (me.carBody.position, p1, Color.black);
			Debug.DrawLine (me.carBody.position, p2, Color.black);
			/*Mesh m = new Mesh();
			m.name = "VelocityObstacle";
			GameObject vo = new GameObject ("Velocity Obstacle");
			m.vertices = new Vector3[] {
				Vector3.zero,
				new Vector3 (x, 0.0f, z),
				new Vector3 (x2, 0.0f, z2)

			};
			m.triangles = new int[] { 0, 2,1};
			MeshFilter mf=vo.AddComponent<MeshFilter> ();
			mf.sharedMesh = m;
			MeshRenderer mr=vo.AddComponent<MeshRenderer> ();
		*/
			//Translate to velocity
			//Vector3 localVelocity = me.carBody.InverseTransformVector(other.velocity);
			//vo.transform.position = me.carBody.TransformPoint (localVelocity);
			Debug.DrawRay (me.carBody.position, other.velocity, Color.magenta);

			//Try with relative velocity
			Vector3 tp = me.carBody.position + (me.velocity - other.velocity);
			Vector3 ov = me.carBody.position;

			//Inside means "left" of p1 and "right" of p2
			//( (P1.x - P0.x) * (P2.z - P0.z)	- (P2.x -  P0.x) * (P1.z - P0.z) );
			float t1 = ((p1.x - ov.x) * (tp.z - ov.z)	- (tp.x - ov.x) * (p1.z - ov.z));
			float t2 = ((p2.x - ov.x) * (tp.z - ov.z)	- (tp.x - ov.x) * (p2.z - ov.z));
			if (t1 >= 0 && t2 <= 0) {
				//Now check with time interval
				Vector3 lpScaled = lp * timeHorizon;
				float radiusSqrScal = radiusSqr * timeHorizon * timeHorizon;
				if ((tp - lpScaled).sqrMagnitude <= radiusSqrScal) {
					me.aiLogic.Log ("Relative My velocity is in OV=" + me.velocity + "of " + other.vehicleId);
				} else {
					me.aiLogic.Log ("Relative My velocity NOT in OV scaled=" + me.velocity + "of " + other.vehicleId);
				}
			} else {
				me.aiLogic.Log ("Relative Not in OV " + me.velocity + " of " + other.vehicleId);
			}

			//Check velocity against VO
			/*tp = me.carBody.position + me.velocity;
				ov = me.carBody.position + other.velocity;
				p1 = p1 + other.velocity;
				p2 = p2 + other.velocity;
				Debug.DrawLine (ov, p1, Color.green);
				Debug.DrawLine (ov, p2, Color.green);

				Debug.DrawRay (me.carBody.position, me.velocity * timeHorizon, Color.magenta);
				Debug.DrawRay (other.carBody.position, other.velocity * timeHorizon, Color.magenta);
				//Inside means "left" of p1 and "right" of p2
				//( (P1.x - P0.x) * (P2.z - P0.z)	- (P2.x -  P0.x) * (P1.z - P0.z) );
				t1 = ((p1.x - ov.x) * (tp.z - ov.z)	- (tp.x - ov.x) * (p1.z - ov.z));
				t2 = ((p2.x - ov.x) * (tp.z - ov.z)	- (tp.x - ov.x) * (p2.z - ov.z));
				if (t1 >= 0 && t2 <= 0) {
					//Debug.Log ("My velocity is in OV="+me.velocity);
				
				} else {
					//Debug.Log ("Not in OV");
				}
				*/

		
		}

		//Compute velocity obstacle with a simulated velocity
		public static void ComputeVelocityObstacle (VehicleInfo me, VehicleInfo other, Vector3 simVelocity,float timeHorizon)
		{
			
			float invTime = 1.0f / timeHorizon;

			//Plot relative positions
			Debug.DrawLine (me.carBody.position, other.carBody.position, Color.yellow);
			//Plot relative velocity
			Debug.DrawRay (me.carBody.position, simVelocity - other.velocity, Color.red);
			Debug.DrawRay (me.carBody.position, simVelocity, Color.blue);
			BoxCollider bc = other.carCollider.GetComponent<BoxCollider> ();
			float radiusSqr = (bc.size.z) * (bc.size.z) + (bc.size.x) * (bc.size.x);//sum radius 
			Vector3 lp = other.carBody.position - me.carBody.position;
			float dr = lp.sqrMagnitude;

			float alpha = Mathf.Asin (Mathf.Sqrt (radiusSqr / dr));
			float angle = Vector3.SignedAngle (me.carBody.forward, lp, Vector3.up);
			float lsqr = lp.sqrMagnitude - radiusSqr;
			float l = Mathf.Sqrt (lsqr);
			float a1 = (angle * Mathf.Deg2Rad) + alpha;
			float a2 = (angle * Mathf.Deg2Rad) - alpha;

			float x = l * Mathf.Sin (a1);
			float z = l * Mathf.Cos (a1);
			float x2 = l * Mathf.Sin (a2);
			float z2 = l * Mathf.Cos (a2);
			Vector3 p1 = me.carBody.TransformPoint (new Vector3 (x, 0.0f, z));
			Vector3 p2 = me.carBody.TransformPoint (new Vector3 (x2, 0.0f, z2));

			/*	GameObject go = new GameObject ("sphere");
			SphereCollider sc = go.AddComponent<SphereCollider> ();
			go.transform.position = bc.transform.position;
			sc.radius = Mathf.Sqrt (radiusSqr);
			sc.isTrigger = true;
			go.transform.SetParent (me.carBody);
*/
			Debug.DrawLine (me.carBody.position, p1, Color.black);
			Debug.DrawLine (me.carBody.position, p2, Color.black);
			/*Mesh m = new Mesh();
			m.name = "VelocityObstacle";
			GameObject vo = new GameObject ("Velocity Obstacle");
			m.vertices = new Vector3[] {
				Vector3.zero,
				new Vector3 (x, 0.0f, z),
				new Vector3 (x2, 0.0f, z2)

			};
			m.triangles = new int[] { 0, 2,1};
			MeshFilter mf=vo.AddComponent<MeshFilter> ();
			mf.sharedMesh = m;
			MeshRenderer mr=vo.AddComponent<MeshRenderer> ();
		*/
			//Translate to velocity
			//Vector3 localVelocity = me.carBody.InverseTransformVector(other.velocity);
			//vo.transform.position = me.carBody.TransformPoint (localVelocity);
			Debug.DrawRay (me.carBody.position, other.velocity, Color.magenta);

			//Try with relative velocity
			Vector3 tp = me.carBody.position + (simVelocity - other.velocity);
			Vector3 ov = me.carBody.position;

			//Inside means "left" of p1 and "right" of p2
			//( (P1.x - P0.x) * (P2.z - P0.z)	- (P2.x -  P0.x) * (P1.z - P0.z) );
			float t1 = ((p1.x - ov.x) * (tp.z - ov.z)	- (tp.x - ov.x) * (p1.z - ov.z));
			float t2 = ((p2.x - ov.x) * (tp.z - ov.z)	- (tp.x - ov.x) * (p2.z - ov.z));
			if (t1 >= 0 && t2 <= 0) {
				//Now check with time interval
				Vector3 lpScaled = lp * timeHorizon;
				float radiusSqrScal = radiusSqr * timeHorizon * timeHorizon;
				if ((tp - lpScaled).sqrMagnitude <= radiusSqrScal) {
					me.aiLogic.Log ("Relative My velocity is in OV=" + me.velocity + "of " + other.vehicleId);
				} else {
					me.aiLogic.Log ("Relative My velocity NOT in OV scaled=" + me.velocity + "of " + other.vehicleId);
				}
			} else {
				me.aiLogic.Log ("Relative Not in OV " + me.velocity + " of " + other.vehicleId);
			}

			//Check velocity against VO
			/*tp = me.carBody.position + me.velocity;
				ov = me.carBody.position + other.velocity;
				p1 = p1 + other.velocity;
				p2 = p2 + other.velocity;
				Debug.DrawLine (ov, p1, Color.green);
				Debug.DrawLine (ov, p2, Color.green);

				Debug.DrawRay (me.carBody.position, me.velocity * timeHorizon, Color.magenta);
				Debug.DrawRay (other.carBody.position, other.velocity * timeHorizon, Color.magenta);
				//Inside means "left" of p1 and "right" of p2
				//( (P1.x - P0.x) * (P2.z - P0.z)	- (P2.x -  P0.x) * (P1.z - P0.z) );
				t1 = ((p1.x - ov.x) * (tp.z - ov.z)	- (tp.x - ov.x) * (p1.z - ov.z));
				t2 = ((p2.x - ov.x) * (tp.z - ov.z)	- (tp.x - ov.x) * (p2.z - ov.z));
				if (t1 >= 0 && t2 <= 0) {
					//Debug.Log ("My velocity is in OV="+me.velocity);
				
				} else {
					//Debug.Log ("Not in OV");
				}
				*/


		}


		public static float ComputeTimeToCollision (Vector3  myVelocity, Vector3  otherVelocity, Vector3 myPosition, Vector3 otherPosition, float mySphereRadius, float otherSphereRadius, out bool inside)
		{
			return  ComputeTimeToCollision (myVelocity, otherVelocity, myPosition, otherPosition, (mySphereRadius + otherSphereRadius), out  inside);
		}

		public static float ComputeTimeToCollision (Vector3  myVelocity, Vector3  otherVelocity, Vector3 myPosition, Vector3 otherPosition, float sphereRadius, out bool inside)
		{
			//Estimate collision time. 
			//See https://physics.stackexchange.com/questions/36186/what-is-the-general-approach-to-calculating-time-of-impact-in-3d
			float sphereSqrRadius = sphereRadius * sphereRadius;
			Vector3 deltaV = myVelocity - otherVelocity;
			Vector3 deltax = otherPosition - myPosition;
			float ttc = Vector3.Dot (deltaV, deltax) / deltaV.sqrMagnitude;

			//With sphere radius

			float alpha = (deltax.sqrMagnitude - sphereSqrRadius) / deltaV.sqrMagnitude;
			float ttcSphere = -1f;
			float argDif = (ttc * ttc) - alpha;
			if (argDif >= 0f) {
				//Quadratic equation has a solution. Take the minus solution as the shortest ttc (first contact of the spheres, the other one should be the exit point)
				ttcSphere = ttc - Mathf.Sqrt (argDif);
			} 

			inside = false;
			if (ttcSphere >= 0) {
				inside = false;
			} else if (deltax.sqrMagnitude < sphereSqrRadius) {
				//Already inside

				inside = true;

			}

			return ttcSphere;

		}

		public static TTCInfo ComputeTimeToCollision (VehicleInfo me, VehicleInfo other, float sphereRadius)
		{

			TTCInfo info;
			float sphereSqrRadius = sphereRadius * sphereRadius;
			Vector3 deltaV = me.velocity - other.velocity; 
			Vector3 deltax = other.carBody.position - me.carBody.position;
			float deltaxSqr = deltax.sqrMagnitude;
			float deltavSqr = deltaV.sqrMagnitude;
			info.distance = Mathf.Sqrt (deltaxSqr);
			info.ttcCenter = Vector3.Dot (deltaV, deltax) / deltavSqr;
			info.inside = false;
			info.ttcSphere = -1f;
			//With sphere radius

			float alpha = (deltaxSqr - sphereSqrRadius) / deltavSqr;

			float argDif = (info.ttcCenter * info.ttcCenter) - alpha;
			if (argDif >= 0f) {
				//Quadratic equation has a solution. Take the minus solution as the shortest ttc
				info.ttcSphere = info.ttcCenter - Mathf.Sqrt (argDif);
			} 
			//me.ailogic.Log ("ttcSphere=" + ttcSphere+"ttc="+ttc+"argDif="+argDif+"alpha="+alpha+"deltax.sqrMagnitude="+deltax.sqrMagnitude+"frontTTC=" + frontTTC);
		
			if (info.ttcSphere >= 0) {
				info.inside = false;
			} else if (deltaxSqr < sphereSqrRadius) {
				//Already inside

				info.inside = true;

			}
			return info;

		}

		public static float ComputeTimeToCollision (VehicleInfo me, VehicleInfo other, float sphereRadius, out bool inside)
		{
			
			float sphereSqrRadius = sphereRadius * sphereRadius;
			Vector3 deltaV = me.velocity - other.velocity;
			Vector3 deltax = other.carBody.position - me.carBody.position;
			float deltaxSqr = deltax.sqrMagnitude;
			float deltavSqr = deltaV.sqrMagnitude;
			//distance = Mathf.Sqrt (deltaxSqr);
			float ttcCenter = Vector3.Dot (deltaV, deltax) / deltavSqr;

			//With sphere radius

			float alpha = (deltaxSqr - sphereSqrRadius) / deltavSqr;
			float ttcSphere = -1f;
			float argDif = (ttcCenter * ttcCenter) - alpha;
			if (argDif >= 0f) {
				//Quadratic equation has a solution. Take the minus solution as the shortest ttc
				ttcSphere = ttcCenter - Mathf.Sqrt (argDif);
			} 
			//me.ailogic.Log ("ttcSphere=" + ttcSphere+"ttc="+ttc+"argDif="+argDif+"alpha="+alpha+"deltax.sqrMagnitude="+deltax.sqrMagnitude+"frontTTC=" + frontTTC);
			inside = false;
			if (ttcSphere >= 0) {
				inside = false;
			} else if (deltaxSqr < sphereSqrRadius) {
				//Already inside

				inside = true;

			}
			/*if (me.vehicleId == 75 ) {
				//Draw spheres
				if (ttcSphere >= 0) {
					GameObject sphere = GameObject.CreatePrimitive (PrimitiveType.Sphere);
					sphere.name = "75";
					//sphere.transform.position = me.carBody.position + me.velocity * (ttcSphere + (0.5f * sphereRadius / me.velocity.magnitude));
					sphere.transform.position = me.carBody.position + me.velocity * ttcSphere;
					SphereCollider sc = sphere.GetComponent<SphereCollider> ();
					sc.radius = sphereRadius * 0.5f;
					sc.isTrigger = true;
					GameObject sphere2 = GameObject.CreatePrimitive (PrimitiveType.Sphere);
					sphere2.name = other.vehicleId.ToString ();
					//sphere2.transform.position = other.carBody.position + other.velocity * (ttcSphere + (0.5f * sphereRadius / other.velocity.magnitude));
					sphere2.transform.position = other.carBody.position + other.velocity * ttcSphere;
					SphereCollider sc2 = sphere2.GetComponent<SphereCollider> ();
					sc2.radius = sphereRadius * 0.5f;
					sc2.isTrigger = true;
					GameObject capsule = GameObject.CreatePrimitive (PrimitiveType.Capsule);
					capsule.transform.position = GetCollisionPoint (me, other, ttcSphere);
					CapsuleCollider cc = capsule.GetComponent<CapsuleCollider> ();
					cc.isTrigger = true;

				}
				if (inside) {
					GameObject sphere = GameObject.CreatePrimitive (PrimitiveType.Sphere);
					sphere.name = "75";
					//sphere.transform.position = me.carBody.position + me.velocity * (ttcSphere + (0.5f * sphereRadius / me.velocity.magnitude));
					sphere.transform.position = me.carBody.position ;
					SphereCollider sc = sphere.GetComponent<SphereCollider> ();
					sc.radius = sphereRadius * 0.5f;
					sc.isTrigger = true;
					GameObject sphere2 = GameObject.CreatePrimitive (PrimitiveType.Sphere);
					sphere2.name = other.vehicleId.ToString ();
					//sphere2.transform.position = other.carBody.position + other.velocity * (ttcSphere + (0.5f * sphereRadius / other.velocity.magnitude));
					sphere2.transform.position = other.carBody.position ;
					SphereCollider sc2 = sphere2.GetComponent<SphereCollider> ();
					sc2.radius = sphereRadius * 0.5f;
					sc2.isTrigger = true;
				}
			}*/
			return ttcSphere;


		}

		public static Vector3 GetCollisionPoint (VehicleInfo me, VehicleInfo other, float ttc)
		{
			//Given a ttc, return point of collision
			Vector3 x = me.carBody.position + me.velocity * ttc;
			Vector3 y = other.carBody.position + other.velocity * ttc;
			return Vector3.Lerp (x, y, 0.5f);

		}

		public static float ComputeTimeToCollision (VehicleInfo me, VehicleInfo other, out bool inside)
		{
			float sphereRadius = (me.vehicleLength + other.vehicleLength) * 0.5f * vehicleLengthIncreaseFactor;
			return ComputeTimeToCollision (me, other, sphereRadius, out inside);


		}

		public static TTCInfo ComputeTimeToCollision (VehicleInfo me, VehicleInfo other)
		{
			float sphereRadius = (me.vehicleLength + other.vehicleLength) * 0.5f * vehicleLengthIncreaseFactor;
			return ComputeTimeToCollision (me, other, sphereRadius);


		}
		//Two non-parallel lines which may or may not touch each other have a point on each line which are closest
		//to each other. This function finds those two points. If the lines are not parallel, the function
		//outputs true, otherwise false.
		public static bool ClosestPointsOnTwoLines (out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
		{

			closestPointLine1 = Vector3.zero;
			closestPointLine2 = Vector3.zero;

			float a = Vector3.Dot (lineVec1, lineVec1);
			float b = Vector3.Dot (lineVec1, lineVec2);
			float e = Vector3.Dot (lineVec2, lineVec2);

			float d = a * e - b * b;

			//lines are not parallel
			if (d != 0.0f) {

				Vector3 r = linePoint1 - linePoint2;
				float c = Vector3.Dot (lineVec1, r);
				float f = Vector3.Dot (lineVec2, r);

				float s = (b * f - c * e) / d;
				float t = (a * f - c * b) / d;

				closestPointLine1 = linePoint1 + lineVec1 * s;
				closestPointLine2 = linePoint2 + lineVec2 * t;

				return true;
			} else {
				return false;
			}
		}
	}
}
