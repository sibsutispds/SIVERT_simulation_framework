/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
	

namespace Veneris
{

	public class VehicleVisionPerceptionModel : TriggerEventPublisher
	{

		[System.Serializable]
		public class VehicleVisionCollisionWarning
		{
			public float timeAhead = 5f;
			public BoxCollider collisionPredictionTrigger = null;
			public TriggerEventPublisher tep = null;
			public Vector3 collisionPredictionSize;
			public Vector3 collisionPredictionCenter;
			public Vector3 vehicleTriggerSize;
			public List<Collider> intersectColliders = null;
			public Transform body = null;
			public float minCenter=3f;

			public VehicleVisionCollisionWarning (float ta, Vector3 csize, Vector3 ccenter, Vector3 vtriggersize, BoxCollider bc, TriggerEventPublisher tp, Transform body)
			{
				this.timeAhead = ta;
				collisionPredictionSize = csize;
				collisionPredictionCenter = ccenter;
				vehicleTriggerSize = vtriggersize;
				collisionPredictionTrigger = bc;
				tep = tp;
				intersectColliders = new List<Collider> (8);
				this.body = body;

			}

			public void UpdateSize (Vector3 velocity)
			{


			
				//collisionPredictionTrigger.transform.rotation = Quaternion.LookRotation (velocity - collisionPredictionTrigger.transform.forward);
				if (velocity.magnitude <= 1.2f) {
					collisionPredictionTrigger.transform.rotation =body.rotation;
					collisionPredictionCenter.z = minCenter;
				} else {
					collisionPredictionTrigger.transform.rotation = Quaternion.LookRotation (velocity);
					collisionPredictionCenter.z = velocity.magnitude * 0.5f * timeAhead;

				}


				collisionPredictionSize.z = 2f * collisionPredictionCenter.z + vehicleTriggerSize.z;

				collisionPredictionTrigger.size = collisionPredictionSize;
				collisionPredictionTrigger.center = collisionPredictionCenter;

			}

		}
		public float halfFrontFieldOfViewDistance = 25;
		public float halfUpFieldOfViewDistance = 2;
		public float viewDistance = 100;
		public float minViewDistance=50;
		public float maxViewDistance=200;

		public Vector3 viewSize;
		public Vector3 viewCenter;

		private AILogic ailogic = null;
		private BoxCollider bc = null;
		private int vehicleLayerMask;
		private int collisionPredictionLayerMask;
		private int vehicleSignalTriggerLayerMask;
		private int vehicleSafetyAreaLayerMask;
	
		public bool checkVehiclePositions = false;
		public List<Transform> checkVehiclePositionsList = null;
		public bool useCollisionPrediction=false;
		public VehicleVisionCollisionWarning collisionWarning = null;
		public float collisionPredictionTimeAhead=5f;
		public Vector3 frontBoxHalfSize;
	
		public Vector3 steerLookAtPoint;

		private Collider[] buffer;
		public int bufferSize=16;

		public VehicleSafetyAreaDetector frontArea = null;
		public VehicleSafetyAreaDetector backArea = null;
		public VehicleSafetyAreaDetector rightArea = null;
		public VehicleSafetyAreaDetector leftArea = null;

		void Start ()
		{
			
			bc = GetComponent<BoxCollider> ();
			if (bc != null) {
				//Debug.Assert (bc != null);

				bc.size = new Vector3 (halfFrontFieldOfViewDistance, halfUpFieldOfViewDistance, Mathf.Clamp (viewDistance, minViewDistance, maxViewDistance));
				bc.center = new Vector3 (0f, 0f, viewDistance / 2);
				viewSize = new Vector3 (halfFrontFieldOfViewDistance, halfUpFieldOfViewDistance, viewDistance);
				viewCenter = new Vector3 (0f, 0f, viewDistance * 0.5f);
				bc.enabled = false;
			}
			//frontVehicles = new Dictionary<int,VehicleInfo> ();
			ailogic = transform.parent.GetComponent<AILogic> ();
		
			if (ailogic == null) {
				Debug.Log ("VisionPerception no AILogic");
			}
			vehicleLayerMask = 1<<LayerMask.NameToLayer ("Vehicle");
			collisionPredictionLayerMask = 1 << LayerMask.NameToLayer ("CollisionPrediction");
			vehicleSignalTriggerLayerMask = 1 << LayerMask.NameToLayer ("VehicleSignalTrigger");
			vehicleSafetyAreaLayerMask = 1 << LayerMask.NameToLayer ("VehicleSafetyArea");

			if (useCollisionPrediction) {
				CreateCollisionPredictionObject ();
			}
			frontBoxHalfSize = new Vector3 (10f, 2f, minViewDistance);

			buffer = new Collider[bufferSize];

			frontArea = transform.Find ("SafetyArea/FrontSafetyArea").GetComponent<VehicleSafetyAreaDetector>();
			backArea = transform.Find ("SafetyArea/BackSafetyArea").GetComponent<VehicleSafetyAreaDetector>();
			leftArea = transform.Find ("SafetyArea/LeftSafetyArea").GetComponent<VehicleSafetyAreaDetector>();
			rightArea = transform.Find ("SafetyArea/RightSafetyArea").GetComponent<VehicleSafetyAreaDetector>();
			if (ailogic.vehicleManager != null) {
				ailogic.vehicleManager.AddRemoveListener (HandleDestroyTrigger);
			}

		}

		public void CreateCollisionPredictionObject() {
			GameObject cp = new GameObject ("Collision Prediction");
			cp.layer = LayerMask.NameToLayer ("CollisionPrediction");
			cp.transform.position = Vector3.zero;
			//collisionPredictionTrigger = cp.transform;
			cp.transform.SetParent (transform,false);
			Rigidbody rb = cp.AddComponent<Rigidbody> ();
			rb.isKinematic = true;
			rb.useGravity = false;
			BoxCollider box = cp.AddComponent<BoxCollider> ();

			box.isTrigger = true;
			Vector3 size = box.size;
			size.x = ailogic.vehicleInfo.carCollider.size.x * 0.85f;
			TriggerEventPublisher tep=cp.AddComponent<TriggerEventPublisher> ();
			collisionWarning = new VehicleVisionCollisionWarning (collisionPredictionTimeAhead, size, box.center, 2f * ailogic.vehicleTriggerColliderHalfSize, box, tep, ailogic.vehicleInfo.carBody);
			collisionWarning.tep.AddEnterListener (HandleEnterTriggerCollisionPrediction);
			collisionWarning.tep.AddExitListener (HandleExitTriggerCollisionPrediction);
			/*collisionPredictionCenter.z = (ailogic.vehicleInfo.speed) * 0.5f * collisionPredictionTimeAhead;
			collisionPredictionSize.z = (ailogic.vehicleInfo.speed) * collisionPredictionTimeAhead + vehicleTriggerSize.z;

			collisionPredictionTrigger.size=collisionPredictionSize;
			collisionPredictionTrigger.center = collisionPredictionCenter;
*/
		}

		public List<Collider> GetVehiclesInPotentialIntersection() {
			//First remove possible nulls (because vehicle has been destroyed, for instance)
			//collisionWarning.intersectColliders.RemoveAll (x => x == null);
			for (int i = collisionWarning.intersectColliders.Count-1; i>=0; i--) {
				if (collisionWarning.intersectColliders [i] == null) {
					collisionWarning.intersectColliders.RemoveAt (i);
				}
				
			}
			return collisionWarning.intersectColliders;
		}

		public List<Collider> GetVehiclesInFrontSafetyArea()  {
			return frontArea.detectedVehicles;
		}
		public List<Collider> GetVehiclesInBackSafetyArea()  {
			return backArea.detectedVehicles;
		}

		public List<Collider> GetVehiclesInLeftSafetyArea()  {
			return leftArea.detectedVehicles;
		}

		public List<Collider> GetVehiclesInRightSafetyArea()  {
			return rightArea.detectedVehicles;
		}


		public void AddEnterCollisionListener(Action<Collider> a) {
			collisionWarning.tep.AddEnterListener (a);
		}
		public void AddExitCollisionListener(Action<Collider> a) {
			collisionWarning.tep.AddExitListener (a);
		}
		public void RemoveEnterCollisionListener(Action<Collider> a) {
			collisionWarning.tep.RemoveEnterListener (a);
		}
		public void RemoveExitCollisionListener(Action<Collider> a) {
			collisionWarning.tep.RemoveExitListener (a);
		}

		public void SetCheckPositions(List<Transform> l) {
			checkVehiclePositions = true;
			checkVehiclePositionsList = l;
		}
		public void UnsetCheckPositions() {
			checkVehiclePositions = false;
			checkVehiclePositionsList = null;
		}

		public void SetSteerLookAtPoint(Vector3 position) 
		{
			steerLookAtPoint = position;
		}

		public void SetViewDistance(float d) {
			
			//viewDistance=Mathf.Clamp(d,minViewDistance,maxViewDistance) ;
		/*	float nd=Mathf.Clamp(d,minViewDistance,maxViewDistance) ;
			if (nd != viewDistance) {
				//ailogic.Log ("viewDistance" + viewDistance + "halfUpFieldOfViewDistance=" + halfUpFieldOfViewDistance + "halfFrontFieldOfViewDistance=" + halfFrontFieldOfViewDistance);
				//Vector3 s=new Vector3 (halfFrontFieldOfViewDistance, halfUpFieldOfViewDistance, viewDistance);
				//bc.size=s;
				viewDistance=nd;
				viewSize.z = viewDistance;
				viewCenter.z = viewDistance * 0.5f;
				//bc.size = new Vector3 (halfFrontFieldOfViewDistance, halfUpFieldOfViewDistance, viewDistance); 
				bc.size = viewSize;
				bc.center = viewCenter;
				//bc.center = new Vector3 (0f, 0f, viewDistance / 2);

			}
			*/



			if (useCollisionPrediction) {
				collisionWarning.UpdateSize ( ailogic.vehicleInfo.velocity);

			}
		}

		public RaycastHit[] CheckForEntitiesWithTag (Vector3 pointToLook, string filter = null, float vdistance=-1f)
		{
			//Debug.Log(Physics.AllLayers);
			if (vdistance < 0) {
				vdistance = viewDistance;
			}
			RaycastHit[] hits =	Physics.BoxCastAll (transform.position, ailogic.vehicleTriggerColliderHalfSize, pointToLook - transform.position, transform.rotation, vdistance, Physics.AllLayers);


			ExtDebug.DrawBoxCastBox (transform.position, ailogic.vehicleTriggerColliderHalfSize, transform.rotation, pointToLook - transform.position, vdistance, Color.black);
		


			Debug.DrawLine (transform.position, pointToLook);
	


			if (filter == null) {
				return hits;
			} else {
				List<RaycastHit> entities = new List<RaycastHit> ();
				//foreach (RaycastHit h in hits) {
				for (int i = 0; i < hits.Length; i++) {
					
				
					//TODO: We should do this check some other way. Maybe add a component at the front and start boxcasting from them, to avoid my own internal car colliders
					if (ailogic.vehicleInfo.vehicleColliders.Contains (hits[i].collider)) {
						continue;
					}
					if (hits[i].collider.CompareTag (filter)) {
						entities.Add (hits[i]);
					}
				}
				if (entities.Count == 0) {
					return null;
				} else {
					return entities.ToArray ();
				}
			}
		}
		public bool CheckFrontForFirstEntityWithTag (out RaycastHit hit, string filter = null) {
			ExtDebug.DrawBoxCastBox (transform.position, ailogic.vehicleTriggerColliderHalfSize, transform.rotation,transform.forward, viewDistance, Color.red);

			if (Physics.BoxCast (transform.position, ailogic.vehicleTriggerColliderHalfSize, transform.forward, out hit, transform.rotation, viewDistance, Physics.AllLayers)) {
				
				if (filter == null) {
					return true;
				} else if (hit.collider.CompareTag (filter)) {
					return true;
				}
			} 
			return false;

		}
		public bool CheckPositionForVehicle (out RaycastHit hit, Vector3 pointToLook) {
			//ExtDebug.DrawBoxCastBox (transform.position, new Vector3 (1, 1, 1), Quaternion.identity,pointToLook - transform.position, viewDistance, Color.green);


			return Physics.BoxCast (transform.position, ailogic.vehicleTriggerColliderHalfSize, pointToLook - transform.position, out hit, transform.rotation, viewDistance, vehicleLayerMask);
		}

		public Collider[] CollisionPredictionInSphere(Vector3 center, float radius) {
			//Return all vehicles whose collision prediction trigger is inside the sphere
			return Physics.OverlapSphere(center,radius,collisionPredictionLayerMask);
		}

		public Collider[] VehiclesInSphere(Vector3 center, float radius) {
			//Return all vehicles  inside the sphere
			return Physics.OverlapSphere(center,radius,vehicleLayerMask);
		}
		public bool CheckVehiclesInSphere(Vector3 center, float radius) {
			//Return all vehicles whose collision prediction trigger is inside the sphere
			return Physics.CheckSphere(center,radius,vehicleLayerMask);
		}

		public RaycastHit[] CheckPositionForVehicles (Vector3 pointToLook) {
			//ExtDebug.DrawBoxCastBox (transform.position, new Vector3 (1, 1, 1), Quaternion.identity,pointToLook - transform.position, viewDistance, Color.black);

			return Physics.BoxCastAll (transform.position, ailogic.vehicleTriggerColliderHalfSize, pointToLook - transform.position,  transform.rotation, viewDistance, vehicleLayerMask);
		}
		public RaycastHit[] CheckPositionForVehicles (Vector3 pointToLook, float distance) {
			ExtDebug.DrawBoxCastBox (transform.position,ailogic.vehicleTriggerColliderHalfSize, transform.rotation,pointToLook - transform.position, viewDistance, Color.black);

			return Physics.BoxCastAll (transform.position, ailogic.vehicleTriggerColliderHalfSize, pointToLook - transform.position, transform.rotation, distance, vehicleLayerMask);
		}
		public bool CheckLineForVehicle (out RaycastHit hit, Vector3 startPoint, Vector3 lineDirection, Quaternion orientation) {
			ExtDebug.DrawBoxCastBox (startPoint, ailogic.vehicleTriggerColliderHalfSize,  orientation, lineDirection, viewDistance, Color.blue);

			return Physics.BoxCast (startPoint, ailogic.vehicleTriggerColliderHalfSize, lineDirection, out hit, orientation, viewDistance, vehicleLayerMask);
		}
		public bool CheckLineForVehicleSignal (out RaycastHit hit, Vector3 startPoint, Vector3 lineDirection, Quaternion orientation) {
			ExtDebug.DrawBoxCastBox (startPoint, ailogic.vehicleTriggerColliderHalfSize,  orientation, lineDirection, viewDistance, Color.black);

			return Physics.BoxCast (startPoint, ailogic.vehicleTriggerColliderHalfSize, lineDirection, out hit, orientation, viewDistance, vehicleSignalTriggerLayerMask);
		}
	

		public bool CheckPositionForVehicle (out RaycastHit hit, Vector3 pointToLook, float distance) {
			ExtDebug.DrawBoxCastBox (transform.position, ailogic.vehicleTriggerColliderHalfSize, transform.rotation,pointToLook - transform.position, distance, Color.blue);
			return Physics.BoxCast (transform.position, ailogic.vehicleTriggerColliderHalfSize, pointToLook - transform.position, out hit, transform.rotation, distance, vehicleLayerMask);
		}
		public bool CheckFrontForVehicle (out RaycastHit hit) {
			ExtDebug.DrawBoxCastBox (transform.position, ailogic.vehicleTriggerColliderHalfSize, transform.rotation,transform.forward, viewDistance, Color.red);

			/*if (Physics.BoxCast (transform.position, new Vector3 (1, 1, 1), transform.forward, out hit, Quaternion.identity, viewDistance, vehicleLayer)) {
				Debug.Log ("enter");
				Debug.Log (ailogic.vehicleInfo.vehicleId + " vision ent=" + hit.collider.name);
				Debug.Log (ailogic.vehicleInfo.vehicleId + " vision ent=" + hit.transform.name);
				Debug.Log ("end");
					return true;

			} 
			return false;
			*/
			return Physics.BoxCast (transform.position, ailogic.vehicleTriggerColliderHalfSize, transform.forward, out hit,transform.rotation, viewDistance, vehicleLayerMask);

		}
	
		public bool CheckFrontForVehicleSignal (out RaycastHit hit) {
			ExtDebug.DrawBoxCastBox (transform.position, ailogic.vehicleTriggerColliderHalfSize, transform.rotation,transform.forward, viewDistance, Color.black);

			/*if (Physics.BoxCast (transform.position, new Vector3 (1, 1, 1), transform.forward, out hit, Quaternion.identity, viewDistance, vehicleLayer)) {
				Debug.Log ("enter");
				Debug.Log (ailogic.vehicleInfo.vehicleId + " vision ent=" + hit.collider.name);
				Debug.Log (ailogic.vehicleInfo.vehicleId + " vision ent=" + hit.transform.name);
				Debug.Log ("end");
					return true;

			} 
			return false;
			*/
			return Physics.BoxCast (transform.position, ailogic.vehicleTriggerColliderHalfSize, transform.forward, out hit, transform.rotation, viewDistance, vehicleSignalTriggerLayerMask);

		}
	
		public int CheckPositionOccupiedByVehicleSignal(Vector3 position, Collider[] buffer) {
			ExtDebug.DrawBox (position, ailogic.vehicleTriggerColliderHalfSize, ailogic.vehicleInfo.carBody.rotation, Color.red);

			return Physics.OverlapBoxNonAlloc (position, ailogic.vehicleTriggerColliderHalfSize, buffer, ailogic.vehicleInfo.carBody.rotation, vehicleSignalTriggerLayerMask);

		}
		public bool CheckFrontForVehicle (out RaycastHit hit, float distance) {
			ExtDebug.DrawBoxCastBox (transform.position, ailogic.vehicleTriggerColliderHalfSize, transform.rotation,transform.forward, distance, Color.black);

			/*if (Physics.BoxCast (transform.position, new Vector3 (1, 1, 1), transform.forward, out hit, Quaternion.identity, viewDistance, vehicleLayer)) {
				Debug.Log ("enter");
				Debug.Log (ailogic.vehicleInfo.vehicleId + " vision ent=" + hit.collider.name);
				Debug.Log (ailogic.vehicleInfo.vehicleId + " vision ent=" + hit.transform.name);
				Debug.Log ("end");
					return true;

			} 
			return false;
			*/
			return Physics.BoxCast (transform.position, ailogic.vehicleTriggerColliderHalfSize, transform.forward, out hit,transform.rotation, distance, vehicleLayerMask);

		}
		public bool CheckPositionOccupiedByOtherVehicle(Vector3 position) {
			
			int hits = CheckPositionOccupiedByVehicle (position, buffer);
			if (hits > buffer.Length) {
				hits = buffer.Length;
				ailogic.Log ("Vision buffer short");
			}
			for (int i = 0; i < hits; i++) {
				if (buffer [i].gameObject.GetComponentInParent<VehicleInfo> () != ailogic.vehicleInfo) {

					return true;
				}
			}
			return false;

		}
		public bool CheckRotatedPositionOccupiedByOtherVehicle(Vector3 position, float angle) {

			int hits = CheckRotatedPositionOccupiedByVehicle (position, angle, buffer);
			if (hits > buffer.Length) {
				hits = buffer.Length;
				ailogic.Log ("Vision buffer short");
			}
			for (int i = 0; i < hits; i++) {
				if (buffer [i].gameObject.GetComponentInParent<VehicleInfo> () != ailogic.vehicleInfo) {

					return true;
				}
			}
			return false;

		}
		public int CheckPositionOccupiedByVehicle(Vector3 position, Collider[] buffer) {
			ExtDebug.DrawBox (position, ailogic.vehicleTriggerColliderHalfSize, ailogic.vehicleInfo.carBody.rotation, Color.red);

			return Physics.OverlapBoxNonAlloc (position, ailogic.vehicleTriggerColliderHalfSize, buffer, ailogic.vehicleInfo.carBody.rotation, vehicleLayerMask);
			
		}
		public int CheckRotatedPositionOccupiedByVehicle(Vector3 position, float angle, Collider[] buffer) {
			Quaternion relativeRotation = Quaternion.AngleAxis (angle, Vector3.up);
			ExtDebug.DrawBox (position, ailogic.vehicleTriggerColliderHalfSize, ailogic.vehicleInfo.carBody.rotation*relativeRotation, Color.yellow);

			return Physics.OverlapBoxNonAlloc (position, ailogic.vehicleTriggerColliderHalfSize, buffer, ailogic.vehicleInfo.carBody.rotation*relativeRotation, vehicleLayerMask);

		}
		public int CheckSteerPositionOccupiedByVehicle(Vector3 position, Collider[] buffer) {
			ExtDebug.DrawBox (position, ailogic.vehicleTriggerColliderHalfSize, Quaternion.LookRotation(steerLookAtPoint-ailogic.vehicleInfo.carBody.position), Color.white);

			return Physics.OverlapBoxNonAlloc (position, ailogic.vehicleTriggerColliderHalfSize, buffer, Quaternion.LookRotation(steerLookAtPoint-ailogic.vehicleInfo.carBody.position), vehicleLayerMask);

		}

		public Collider[] CheckSphereForVehicles () {
			return Physics.OverlapSphere (transform.position, 150f, vehicleLayerMask);
		}

		public Collider[] SweepFrontForVehicles () {
			if (ailogic != null) {
				ExtDebug.DrawBox (ailogic.vehicleInfo.frontBumper.position + (transform.forward * viewDistance * 0.5f), frontBoxHalfSize, transform.rotation, Color.green);


				/*if (Physics.BoxCast (transform.position, new Vector3 (1, 1, 1), transform.forward, out hit, Quaternion.identity, viewDistance, vehicleLayer)) {
				Debug.Log ("enter");
				Debug.Log (ailogic.vehicleInfo.vehicleId + " vision ent=" + hit.collider.name);
				Debug.Log (ailogic.vehicleInfo.vehicleId + " vision ent=" + hit.transform.name);
				Debug.Log ("end");
					return true;

			} 
			return false;
			*/
				frontBoxHalfSize.z = viewDistance * 0.5f;

				return Physics.OverlapBox (ailogic.vehicleInfo.frontBumper.position + (transform.forward * viewDistance * 0.5f), frontBoxHalfSize, transform.rotation, vehicleLayerMask);
			} 
			return new Collider[0];

		}
		public RaycastHit[] CheckFrontForEntitiesWithTag ( string filter = null)
		{//Debug.Log(Physics.AllLayers);
			RaycastHit[] hits =	Physics.BoxCastAll (transform.position, ailogic.vehicleTriggerColliderHalfSize, transform.forward, transform.rotation, viewDistance, Physics.AllLayers);


			ExtDebug.DrawBoxCastBox (transform.position, ailogic.vehicleTriggerColliderHalfSize, transform.rotation,transform.forward, viewDistance, Color.blue);



			Debug.DrawLine (transform.position, transform.forward*viewDistance);



			if (filter == null) {
				return hits;
			} else {
				List<RaycastHit> entities = new List<RaycastHit> ();
				foreach (RaycastHit h in hits) {
					//TODO: We should do this check some other way. Maybe add a component at the front and start boxcasting from them, to avoid my own internal car colliders
					if (ailogic.vehicleInfo.vehicleColliders.Contains (h.collider)) {
						continue;
					}
					if (h.collider.CompareTag (filter)) {
						entities.Add (h);
					}
				}
				if (entities.Count == 0) {
					return null;
				} else {
					return entities.ToArray ();
				}
			}
			
		}
	

		void OnTriggerEnter (Collider other)
		{
			//ailogic.Log ("trigger called on VehicleVision  " + other.name +" root="+other.transform.root.name);
			TriggerEnterListeners (other);
		}

		void OnTriggerExit (Collider other)
		{
			//Debug.Log ("trigger called on VehicleVision");
			TriggerExitListeners (other);
		}

		void HandleEnterTriggerCollisionPrediction(Collider other) {
			//ailogic.Log ("Collision prediction  with " + other.transform.root.name);
			/*if (ailogic.vehicleInfo.vehicleId == 5 || ailogic.vehicleInfo.vehicleId == 1) {
				Debug.Log (ailogic.vehicleInfo.vehicleId + "Collision prediction collided with " + other.transform.root.name);
				RaycastHit hit;
				Ray ray = new Ray (transform.position, transform.forward);
				Debug.DrawRay (transform.position, 1000f*transform.forward, Color.black);
				if (other.Raycast (ray, out hit, 1000f)) {
					float d = Vector3.Distance (transform.position, hit.point);
					Debug.Log (hit.point + " dist to point" + d + " time to col" + (d / ailogic.vehicleInfo.speed) + "time=" + Time.time + "coll will occur at t=" + (Time.time + (d / ailogic.vehicleInfo.speed)));
				}
				Debug.Break ();
			}
			*/
			collisionWarning.intersectColliders.Add (other);
		}
		void HandleExitTriggerCollisionPrediction(Collider other) {
			//Debug.Log (Time.time+"--"+ailogic.vehicleInfo.vehicleId + "Collision prediction  exit " + other.transform.root.name);

			//collisionWarning.intersectColliders.Remove (other);
			for (int i = collisionWarning.intersectColliders.Count-1; i >=0; i--) {
				if (collisionWarning.intersectColliders [i] == other) {
					collisionWarning.intersectColliders.RemoveAt (i);
				}
				
			}

		}
		protected void HandleDestroyTrigger (VehicleInfo info)
		{
			frontArea.RemoveVehicle (info);
			backArea.RemoveVehicle (info);
			rightArea.RemoveVehicle (info);
			leftArea.RemoveVehicle (info);
		}
		void OnDestroy ()
		{
			if (ailogic != null) {
				if (ailogic.vehicleManager != null) {
					ailogic.vehicleManager.RemoveRemoveListener (HandleDestroyTrigger);
				}
			}
		}
	
	}
}
