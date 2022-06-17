/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Veneris
{
	public class VehicleCollisionManager : MonoBehaviour
	{
		public AILogic ailogic;
		public float maxTimeBeforeSeparation=2f;
		public float maxTimeBeforeRemoving=6f;
		public List<CollisionData> colliding=null;

		[System.Serializable]
		public class CollisionData {
			public VehicleInfo colliderVehicle = null;
			public float startTimer=0f;
			public bool separated=false;
			public CollisionData(VehicleInfo c, float t) {
				colliderVehicle = c;
				startTimer = t;
				separated=false;
			}
		}


		void Start ()
		{



			ailogic = GetComponentInChildren<AILogic> ();
			if (ailogic == null) {
				Debug.Log ("No AILogic");
			}
			colliding = new List<CollisionData> (8);

		}
	
		void OnCollisionEnter(Collision c)
		{

			try
			{
				c.gameObject.GetComponentInChildren<MOBILIDMPathTracker>().IsEmergencyEnabled = true;
			}
			catch (Exception e)
			{
				
				Debug.LogWarning(e);
				// Console.WriteLine(e);
				// throw;
			}
			
				VehicleInfo i = c.transform.root.GetComponent<VehicleInfo> ();
				if (i == null) {
					Debug.Log (Time.time + ": Vehicle  " + ailogic.vehicleInfo.vehicleId + " has collided with " + c.transform.root.name + " with collider " + c.collider.name + " with collider=" + c.contacts [0].thisCollider.name);
					//Destroy this vehicle to allow the simulation to go on 
					SimulationManager.Instance.RecordVariableWithTimestamp ("Vehicle  " + ailogic.vehicleInfo.vehicleId + " has collided with " + c.transform.root.name + " with collider " + c.collider.name + " with collider=" + c.contacts [0].thisCollider.name,"Removed");
					// NOTE: uncomment if want to remove vehicles from simulation upon collision
					// ailogic.RemoveVehicleFromSimulation ();
					//Debug.Break ();
				} else
				{
					
					ailogic.Log ("Collided with " + i.vehicleId);
					CollisionData d = new CollisionData (i, Time.time);
					colliding.Add (d);
					if (ailogic.vehicleInfo.leadingVehicle == c.collider.GetComponent < VehicleInfo> ()) {
						ailogic.Log ("have collided with my leading vehicle" + i.vehicleId + "name=" + c.collider.name);
						

					}
					
					foreach (ContactPoint p in c.contacts) {
						Debug.DrawLine (ailogic.vehicleInfo.carBody.position, p.point, Color.red);
						Debug.DrawRay (p.point, p.normal, Color.white);
						//ailogic.Log ("normal=" + p.normal.magnitude+"relvel="+c.relativeVelocity.magnitude);

					}

				}
			

		}
		public bool SetSeparated(VehicleInfo info) {
			for (int i = 0; i < colliding.Count; i++) {
				if (colliding [i].colliderVehicle == info) {
					colliding [i].separated = true;
					return true;
					
				}
			}
			return false;
		}
		void OnCollisionStay(Collision c) {
			for (int i = 0; i < colliding.Count; i++) {
				VehicleInfo info =c.transform.root.GetComponent<VehicleInfo> ();
				if (colliding [i].colliderVehicle == info) {
			
					if ((Time.time - colliding[i].startTimer) >= maxTimeBeforeRemoving) {
						ailogic.Log ("Colliding longer than " + maxTimeBeforeRemoving + ". Removing from simulation");
						ailogic.RemoveAndReinsert ("Colliding longer than " + maxTimeBeforeRemoving + ".colliderVehicle="+info.vehicleId);
						return;
					}

					if (colliding[i].separated) {
						return;
					}
					if ((Time.time - colliding[i].startTimer) >= maxTimeBeforeSeparation) {
						//Try to separate along normaks
						//First, check that we are not going to collider with anybody
						if (ailogic.vision.CheckPositionOccupiedByOtherVehicle (ailogic.vehicleInfo.carBody.position + c.contacts [0].normal * 0.5f)) {
							return;
						}
						if (info.aiLogic.vision.CheckPositionOccupiedByOtherVehicle (info.carBody.position - c.contacts [0].normal * 0.5f)) {
							return;
						}
						ailogic.Log ("Separating vehicles along normals " + info.vehicleId);
						ailogic.vehicleInfo.carBody.Translate (c.contacts [0].normal * 0.5f);
						colliding[i].separated = true;
						info.carBody.Translate (-c.contacts [0].normal * 0.5f);
						info.GetComponent<VehicleCollisionManager> ().SetSeparated (ailogic.vehicleInfo);
						//Debug.Break ();
					}
					break;
				}

			}

		}
		void OnCollisionExit(Collision c) {
			for (int i = colliding.Count - 1; i >= 0; i--) {
				VehicleInfo info = c.transform.root.GetComponent<VehicleInfo> ();
				if (info == colliding [i].colliderVehicle) {
					colliding.RemoveAt (i);
					ailogic.Log ("Collision exit with " + info.vehicleId + ". Collider=" + c.collider.name);
					break;
				}
			}
		

		}
	}
}
