/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Veneris.Communications;
using FlatBuffers;

namespace Opal
{
	public class VenerisOpalManager : OpalManager
	{

	
		// Use this for initialization
		void Start ()
		{
			if (useOpal) {
				InitOpal ();
			} 
			isInitialized = true;
			//Register modules/vehicles, independently of using Opal
			if (cachedReceivers != null) {
				for (int i = 0; i < cachedReceivers.Count; i++) {
					RegisterReceiver (cachedReceivers [i]);
				}
			}
		
			if (cachedDynamicMeshes != null) {
				for (int i = 0; i < cachedDynamicMeshes.Count; i++) {
					if (useOpal) {
						RegisterDynamicMesh (cachedDynamicMeshes [i]);
					} else {
						//Disable component
						cachedDynamicMeshes [i].enabled=false;
						Debug.Log ("Disabling Dynamic Mesh");
					}
				}
			}
			
		}

		public override void InitOpal ()
		{
			//Enquee intialization 
			FlatBufferBuilder fbb = new FlatBufferBuilder (sizeof(float) + sizeof(int) * 3 + sizeof(bool));
			UseOpal.StartUseOpal (fbb);
			UseOpal.AddAzimuthDelta (fbb, (uint)azimuthDelta);
			UseOpal.AddElevationDelta (fbb, (uint)elevationDelta);
			UseOpal.AddFrequency (fbb, frequency);
			UseOpal.AddMaxReflections (fbb, maxReflections);
			UseOpal.AddUseDecimalDegrees (fbb, useSubStepSphere);
			var uo = UseOpal.EndUseOpal (fbb);
			UseOpal.FinishUseOpalBuffer (fbb, uo);

			MessageManager.enqueue (fbb.SizedByteArray (), VenerisMessageTypes.UseOpal);
			Debug.Log ("Enqueuing UseOpal");

			CollectAndSendStaticMeshes ();

			MessageManager.enqueue (null, VenerisMessageTypes.FinishOpalContext);
			Debug.Log ("Enqueuing FinishOpalContext");

		
	
		
		}

		public override void OnDestroy ()
		{
	
				Debug.Log ("Exiting VenerisOpal");
				MessageManager.enqueue (null, VenerisMessageTypes.End);
				isInitialized = false;

		}
		/*public override void Transmit (int id, float txPower, Vector3ToMarshal txPosition, Vector3ToMarshal polarization)
		{
			Debug.LogError ("VenerisOpalManager does not transmit. Transmission is delegated to Veneris (OMNET++)");
		}
		*/

		protected override void SendStaticMeshToOpal (Transform t, StaticMesh sm)
		{
			
			Vector3[] v = sm.meshFilter.mesh.vertices;
			int[] indices = sm.meshFilter.mesh.triangles;
			Debug.Log (t.name + ".Triangle length=" + indices.Length);

			FlatBufferBuilder fbb = new FlatBufferBuilder (sizeof(float) * 3 * v.Length + sizeof(int) * indices.Length + sizeof(float) * 20);


			//Keep the order of triangles an vertices. Loop in forward or reverse order for both of them
			Veneris.Communications.StaticMesh.StartVerticesVector (fbb, v.Length);
			for (int i = v.Length - 1; i >= 0; i--) {
				float x = v [i].x;
				float y = v [i].y;
				float z = v [i].z;
				/*Debug.Log (v [i].x.ToString ("E8"));
				Debug.Log (v [i].y.ToString ("E8"));
				Debug.Log (v [i].z.ToString ("E8"));
				*/
				if (Mathf.Abs (v [i].x) < 1E-15f) {
					x = 0.0f;
				}
				if (Mathf.Abs (v [i].y) < 1E-15f) {
					y = 0.0f;
				}
				if (Mathf.Abs (v [i].z) < 1E-15f) {
					z = 0.0f;
				}
				Vec3.CreateVec3 (fbb, x, y, z);
			}
			var vof = fbb.EndVector ();

		
			Veneris.Communications.StaticMesh.StartIndexesVector (fbb, indices.Length);
			for (int i = indices.Length - 1; i >= 0; i--) {
				fbb.AddInt (indices [i]);
			}
			var iof = fbb.EndVector ();
	
		
			UnityEngine.Matrix4x4 u = t.transform.localToWorldMatrix;
			//var tof=Veneris.Communications.Matrix4x4.CreateMatrix4x4(fbb, u [0, 0], u [0, 1], u [0, 2], u [0, 3], u [1, 0],u [1, 1],u [1, 2], u [1, 3], u [2, 0], u [2, 1], u[2, 2], u [2, 3],u [3, 0], u [3, 1], u [3, 2],u [3, 3]);
		
			//	var emof=Veneris.Communications.MaterialEMP.CreateMaterialEMP (fbb, sm.GetOpalMeshProperties ().emProperties.a, sm.GetOpalMeshProperties ().emProperties.b, sm.GetOpalMeshProperties ().emProperties.c, sm.GetOpalMeshProperties ().emProperties.d);

			//Now serialize all the fields
			Veneris.Communications.StaticMesh.StartStaticMesh (fbb);
			Veneris.Communications.StaticMesh.AddVertices (fbb, vof);
			Veneris.Communications.StaticMesh.AddIndexes (fbb, iof);
			Veneris.Communications.StaticMesh.AddTransform (fbb, Veneris.Communications.Matrix4x4.CreateMatrix4x4 (fbb, u [0, 0], u [0, 1], u [0, 2], u [0, 3], u [1, 0], u [1, 1], u [1, 2], u [1, 3], u [2, 0], u [2, 1], u [2, 2], u [2, 3], u [3, 0], u [3, 1], u [3, 2], u [3, 3]));
			Veneris.Communications.StaticMesh.AddMaterial (fbb, Veneris.Communications.MaterialEMP.CreateMaterialEMP (fbb, sm.GetOpalMeshProperties ().emProperties.a, sm.GetOpalMeshProperties ().emProperties.b, sm.GetOpalMeshProperties ().emProperties.c, sm.GetOpalMeshProperties ().emProperties.d));
			var stof = Veneris.Communications.StaticMesh.EndStaticMesh (fbb);

			Veneris.Communications.StaticMesh.FinishStaticMeshBuffer (fbb, stof);
			MessageManager.enqueue (fbb.SizedByteArray (), VenerisMessageTypes.StaticMesh);
			//Debug.Log ("Enqueuing StaticMesh");
		}

		public override void SendReceiverToOpal (Receiver rec)
		{
			
			//Send a new vehicle 
			FlatBufferBuilder fbb = new FlatBufferBuilder (sizeof(float) * 3 + sizeof(uint));
			CreateVehicle.StartCreateVehicle (fbb);
			CreateVehicle.AddId (fbb, (uint)rec.id);
			CreateVehicle.AddPosition (fbb, Vec3.CreateVec3 (fbb, rec.transform.position.x, rec.transform.position.y, rec.transform.position.z));
			CreateVehicle.AddRadius (fbb, rec.radius);
			var cvo = CreateVehicle.EndCreateVehicle (fbb);
			CreateVehicle.FinishCreateVehicleBuffer (fbb, cvo);
			MessageManager.enqueue (fbb.SizedByteArray (), VenerisMessageTypes.Create);
			//Debug.Log ("Enqueuing receiver (Create) " + rec.id + ". radius=" + rec.radius);

		}

		public override void UpdateReceiver (Receiver rec)
		{
			FlatBufferBuilder fbb = new FlatBufferBuilder (sizeof(float) * 3 * 2 + sizeof(uint));
			VehicleState.StartVehicleState (fbb);
			VehicleState.AddId (fbb, (uint)rec.id);
			VehicleState.AddPosition (fbb, Vec3.CreateVec3 (fbb, rec.transform.position.x, rec.transform.position.y, rec.transform.position.z));
			Rigidbody rb = rec.GetComponent<Rigidbody> ();
			if (rb != null) {
				VehicleState.AddVelocity (fbb, Vec3.CreateVec3 (fbb, rb.velocity.x, rb.velocity.y, rb.velocity.z));

			}
			var vso = VehicleState.EndVehicleState (fbb);
			VehicleState.FinishVehicleStateBuffer (fbb, vso);
			MessageManager.enqueue (fbb.SizedByteArray (), VenerisMessageTypes.VehicleState);
			//Debug.Log ("Enqueuing UpdateReceiver " + rec.id);
			
		}

		public override void UnregisterReceiver (Receiver rec)
		{
			FlatBufferBuilder fbb = new FlatBufferBuilder (sizeof(int));
			DestroyVehicle.StartDestroyVehicle (fbb);
			DestroyVehicle.AddId (fbb, rec.id);
			var dvo = DestroyVehicle.EndDestroyVehicle (fbb);
			DestroyVehicle.FinishDestroyVehicleBuffer (fbb, dvo);
			MessageManager.enqueue (fbb.SizedByteArray (), VenerisMessageTypes.Destroy);
			//Debug.Log ("Enqueuing Destroy Vehicle " + rec.id);

		}

		public override void RemoveDynamicMeshGroup (int id)
		{
			FlatBufferBuilder fbb = new FlatBufferBuilder (sizeof(int));
			Veneris.Communications.RemoveDynamicMeshGroup.StartRemoveDynamicMeshGroup (fbb);
			Veneris.Communications.RemoveDynamicMeshGroup.AddId (fbb, id);
			var rdo = Veneris.Communications.RemoveDynamicMeshGroup.EndRemoveDynamicMeshGroup (fbb);
			Veneris.Communications.RemoveDynamicMeshGroup.FinishRemoveDynamicMeshGroupBuffer (fbb, rdo);
			MessageManager.enqueue (fbb.SizedByteArray (), VenerisMessageTypes.RemoveDynamicMeshGroup);
			//Debug.Log ("Enqueuing RemoveDynamicMeshGroup" + id);


		}

		public override void AddDynamicMeshGroup (int id)
		{

			FlatBufferBuilder fbb = new FlatBufferBuilder (sizeof(int));
			Veneris.Communications.AddDynamicMeshGroup.StartAddDynamicMeshGroup (fbb);
			Veneris.Communications.AddDynamicMeshGroup.AddId (fbb, id);
			var adof = Veneris.Communications.AddDynamicMeshGroup.EndAddDynamicMeshGroup (fbb);
			Veneris.Communications.AddDynamicMeshGroup.FinishAddDynamicMeshGroupBuffer (fbb, adof);
			MessageManager.enqueue (fbb.SizedByteArray (), VenerisMessageTypes.AddDynamicMeshGroup);
			//Debug.Log ("Enqueueing AddDynamicMeshGroup:" + id);	
		
		}

		public override void FinishDynamicMeshGroup (int id)
		{
		
			FlatBufferBuilder fbb = new FlatBufferBuilder (sizeof(int));

			Veneris.Communications.FinishDynamicMeshGroup.StartFinishDynamicMeshGroup (fbb);
			Veneris.Communications.FinishDynamicMeshGroup.AddId (fbb, id);
			var adof = Veneris.Communications.FinishDynamicMeshGroup.EndFinishDynamicMeshGroup (fbb);
			Veneris.Communications.FinishDynamicMeshGroup.FinishFinishDynamicMeshGroupBuffer (fbb, adof);
			MessageManager.enqueue (fbb.SizedByteArray (), VenerisMessageTypes.FinishDynamicMeshGroup);
			//Debug.Log ("Enqueueing FinishDynamicMeshGroup:" + id);	
			
		}

		public override void UpdateTransformInGroup (int id, Transform t)
		{
			FlatBufferBuilder fbb = new FlatBufferBuilder (sizeof(float) * 16 + sizeof(int));
			Veneris.Communications.UpdateTransformInGroup.StartUpdateTransformInGroup (fbb);
			Veneris.Communications.UpdateTransformInGroup.AddId (fbb, id);
			UnityEngine.Matrix4x4 u = t.localToWorldMatrix;
			Veneris.Communications.UpdateTransformInGroup.AddTransform (fbb, Veneris.Communications.Matrix4x4.CreateMatrix4x4 (fbb, u [0, 0], u [0, 1], u [0, 2], u [0, 3], u [1, 0], u [1, 1], u [1, 2], u [1, 3], u [2, 0], u [2, 1], u [2, 2], u [2, 3], u [3, 0], u [3, 1], u [3, 2], u [3, 3]));
			var stof = Veneris.Communications.UpdateTransformInGroup.EndUpdateTransformInGroup (fbb);
			Veneris.Communications.UpdateTransformInGroup.FinishUpdateTransformInGroupBuffer (fbb, stof);
			MessageManager.enqueue (fbb.SizedByteArray (), VenerisMessageTypes.UpdateTransformInGroup);
			//Debug.Log ("Enqueueing UpdateTransformInGroup:" + id);	
			
		}

		public override void SendMeshToGroup (int id, Transform rootT, Transform meshT, Mesh mesh, MaterialEMProperties em)
		{
			Vector3[] v = mesh.vertices;
			int[] indices = mesh.triangles;
			//Debug.Log ("Mesh Group:" + id + ". " + meshT.name + ".Triangle length=" + indices.Length);
			FlatBufferBuilder fbb = new FlatBufferBuilder (sizeof(float) * 3 * v.Length + sizeof(int) * indices.Length + sizeof(float) * 4 + sizeof(int));


			//Keep the order of triangles an vertices. Loop in forward or reverse order for both of them
			Veneris.Communications.DynamicMesh.StartVerticesVector (fbb, v.Length);
			for (int i = v.Length - 1; i >= 0; i--) {
				Vector3 aux = rootT.InverseTransformPoint (meshT.TransformPoint (v [i]));
				float x = aux.x;
				float y = aux.y;
				float z = aux.z;
				/*Debug.Log (x.ToString ("E8"));
				Debug.Log (y.ToString ("E8"));
				Debug.Log (z.ToString ("E8"));
				*/

				if (Mathf.Abs (v [i].x) < 1E-15f) {
					x = 0.0f;
				}
				if (Mathf.Abs (v [i].y) < 1E-15f) {
					y = 0.0f;
				}
				if (Mathf.Abs (v [i].z) < 1E-15f) {
					z = 0.0f;
				}
				Vec3.CreateVec3 (fbb, x, y, z);
			}
			var vof = fbb.EndVector ();


			Veneris.Communications.DynamicMesh.StartIndexesVector (fbb, indices.Length);
			for (int i = indices.Length - 1; i >= 0; i--) {
				fbb.AddInt (indices [i]);
			}
			var iof = fbb.EndVector ();

			//Now serialize all the fields

			Veneris.Communications.DynamicMesh.StartDynamicMesh (fbb);
			Veneris.Communications.DynamicMesh.AddId (fbb, id);
			Veneris.Communications.DynamicMesh.AddVertices (fbb, vof);
			Veneris.Communications.DynamicMesh.AddIndexes (fbb, iof);
			Veneris.Communications.DynamicMesh.AddMaterial (fbb, Veneris.Communications.MaterialEMP.CreateMaterialEMP (fbb, em.a, em.b, em.c, em.d));

			var stof = Veneris.Communications.DynamicMesh.EndDynamicMesh (fbb);

			Veneris.Communications.DynamicMesh.FinishDynamicMeshBuffer (fbb, stof);
			MessageManager.enqueue (fbb.SizedByteArray (), VenerisMessageTypes.DynamicMesh);
			//Debug.Log ("Enqueuing DynamicMesh: " + id);
		}
	}
}
