/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Opal
{
	public class OpalManager :Singleton<OpalManager>
	{
		public bool useOpal=false;

		public static bool isInitialized = false;


		public bool multiTransmitter = false;
		public float frequency = 5.9e9f;
		public bool useExactSpeedOfLight = true;

		public int azimuthDelta = 1;
		public int elevationDelta = 1;
		public delegate void OpalInitialized();
		protected OpalInitialized initializedListeners=null;

		public uint maxReflections = 10;
		public bool useSubStepSphere = false;
		protected List<Receiver> cachedReceivers = null;
		protected List<Transmitter> cachedTransmitters = null;
		protected List<DynamicMesh> cachedDynamicMeshes = null;

		// Use this for initialization
		void Start ()
		{
			if (useOpal)
			{
				InitOpal ();
				if (initializedListeners != null) {
					initializedListeners ();
				}
			}
			

		}
		public void RegisterOpalInitializedListener(OpalInitialized l) {
			initializedListeners += l;
		}

		public virtual void InitOpal ()
		{
			Debug.Log ("Initializing opal: multiTransmitter=" + multiTransmitter +  ";useExactSpeedOfLight="+useExactSpeedOfLight);
			int r = OpalInterface.Init (frequency,  useExactSpeedOfLight, multiTransmitter); 

			if (r != 0) {
				throw new System.InvalidOperationException ("Could not initialize Opal:" + r);
			}
			isInitialized = true;
			CollectAndSendStaticMeshes ();



			CreateRaySphere (elevationDelta, azimuthDelta);
			Debug.Log ("Max reflections=" + maxReflections);
			OpalInterface.SetMaxReflections (maxReflections);
			//Debug.Log ("Enabled printing");
			//WARNING: enabling printing with substep ray sphere and more than a few transmits will probably hang your applicatio due to the large number of lines logged
			//OpalInterface.SetPrintEnabled (1024 * 1024 * 1024);

			if (cachedReceivers != null) {
				for (int i = 0; i < cachedReceivers.Count; i++) {
					RegisterReceiver (cachedReceivers [i]);
				}
			}
			if (multiTransmitter) {
				if (cachedTransmitters != null) {
					for (int i = 0; i < cachedTransmitters.Count; i++) {
						RegisterTransmitter (cachedTransmitters [i]);
					}
				}

			}

			if (cachedDynamicMeshes != null) {
				for (int i = 0; i < cachedDynamicMeshes.Count; i++) {
					RegisterDynamicMesh (cachedDynamicMeshes [i]);
				}
			}
			r = OpalInterface.FinishSceneContext ();
			if (r != 0) {
				throw new System.InvalidOperationException ("Error in FinishSceneContext:" + r);
			}



			Debug.Log ("Opal initialized");
		}


		// Update is called once per frame
		//	void Update ()
		//	{
		//
		//	}
		public override void OnDestroy ()
		{
			if (useOpal)
			{
				base.OnDestroy ();
				Debug.Log ("Exiting opal");

				OpalInterface.Exit ();
				isInitialized = false;
			}
			
		}

		public virtual void Transmit (int id, float txPower, Vector3 txPosition, Vector3ToMarshal polarization)
		{
			if (isInitialized) {
				Debug.Log (Time.time+"\t. Transmit:"+txPosition );
				int r = OpalInterface.Transmit (id, txPower, OpalInterface.ToMarshal(txPosition), polarization);
				if (r != 0) {
					throw new System.InvalidOperationException ("Error in Transmit: id=" + id + ". Error:" + r);
				}
			}
		}

		/*public virtual void Transmit (int id, float txPower, Vector3 pos, Vector3 pol)
		{
			if (isInitialized) {

				int r = OpalInterface.Transmit (id, txPower, pos.x,pos.y,pos.z, pol.x,pol.y,pol.z);
				if (r != 0) {
					throw new System.InvalidOperationException ("Error in Transmit: id=" + id + ". Error:" + r);
				}
			}
		}
		*/
		public void CreateCustomRaySphere ()
		{


			Vector3ToMarshal[] rays = new Vector3ToMarshal[2];
			rays [0] = OpalInterface.ToMarshal (Vector3.forward);
			rays [1] = OpalInterface.ToMarshal ((Quaternion.Euler (45f, 0.0f, 0.0f) * Vector3.forward).normalized);

			int r = OpalInterface.FillRaySphere2D (2, 1, rays);
			if (r != 0) {
				throw new System.InvalidOperationException ("Error in CreateCustomRaySphere: " + r);
			}
		}

		public void RegisterReceiver (Receiver rec)
		{
			if (isInitialized) {
				SendReceiverToOpal (rec);
			} else {
				if (cachedReceivers == null) {
					cachedReceivers = new List<Receiver> ();
				}
				if (cachedReceivers.Contains (rec)) {
					Debug.Log ("Receiver is already cached " + rec.id);
					return;
				}
				Debug.Log ("Caching receiver " + rec.id);
				cachedReceivers.Add (rec);
			}

		}

		public void RegisterDynamicMesh (DynamicMesh dm)
		{
			if (useOpal) {
				if (isInitialized) {
					dm.CreateGroup ();
				} else {
					if (cachedDynamicMeshes == null) {
						cachedDynamicMeshes = new List<DynamicMesh> ();
					}
					if (cachedDynamicMeshes.Contains (dm)) {
						Debug.Log ("DynamicMesh is already cached " + dm.id);
						return;
					}
					Debug.Log ("Caching DynamicMesh " + dm.id);
					cachedDynamicMeshes.Add (dm);
				
				}
			} else {
				dm.enabled=false;
				//Debug.Log ("Disabling Dynamic Mesh "+dm.id);
			}

		}

		public virtual void SendReceiverToOpal (Receiver rec)
		{
			Debug.Log ("Adding receiver " + rec.id + ". radius=" + rec.radius);
			int r = OpalInterface.AddReceiverFromUnity (rec.id, OpalInterface.ToMarshal (rec.transform), rec.radius, rec.GetCallback ());
			if (r != 0) {
				throw new System.InvalidOperationException ("Error in AddReceiverFromUnity: id=" + rec.id + ". Error:" + r);
			}
		}

		public virtual void UpdateReceiver (Receiver rec)
		{
			if (OpalManager.isInitialized) {
				Debug.Log ("Updating receiver " + rec.id);
				OpalInterface.UpdateReceiver (rec.id, OpalInterface.ToMarshal (rec.transform.position));
			}
		}

		public virtual void UnregisterReceiver (Receiver rec)
		{
			if (isInitialized) {
				int r = OpalInterface.RemoveReceiverFromUnity (rec.id);
				if (r != 0) {
					throw new System.InvalidOperationException ("Error in RemoveReceiverFromUnity: id=" + rec.id + ". Error:" + r);
				}
			}

		}

		public void RegisterTransmitter (Transmitter tx)
		{
			if (multiTransmitter) {
				if (isInitialized) {
					//TODO: reimplement when adapted
					//SendTransmitterToOpal (tx);
				} else {
					if (cachedTransmitters == null) {
						cachedTransmitters = new List<Transmitter> ();
					}
					if (cachedTransmitters.Contains (tx)) {
						Debug.Log ("Transmitter is already cached " + tx.id);
						return;
					}
					Debug.Log ("Caching Transmitter " + tx.id);
					cachedTransmitters.Add (tx);
				}
			} else {
				Debug.LogError ("Cannot register transmitter in single transmitter mode");
			}

		}

		//TODO: reimplement when multitransmitter is adapted to new filtering

		/*public void SendTransmitterToOpal (Transmitter tx)
		{
			Debug.Log ("Adding transmitter " + tx.id);
			int r = OpalInterface.AddTransmitter (tx.id, OpalInterface.ToMarshal (tx.transform), tx.GetPolarizationMarshaled (), tx.txPower);
			if (r != 0) {
				throw new System.InvalidOperationException ("Error in AddReceiverFromUnity: id=" + tx.id + ". Error:" + r);
			}
		}

		public virtual void UnregisterTransmitter (Transmitter tx)
		{
			if (multiTransmitter) {
				if (isInitialized) {
					int r = OpalInterface.RemoveReceiverFromUnity (tx.id);
					if (r != 0) {
						throw new System.InvalidOperationException ("Error in RemoveTransmitter: id=" + tx.id + ". Error:" + r);
					}
				}
			}

		}

		public void AddTransmitterToGroup (Transmitter tx)
		{
			if (multiTransmitter) {
				OpalInterface.AddTransmitterToGroup (tx.id, tx.txPower, OpalInterface.ToMarshal (tx.transform), tx.GetPolarizationMarshaled ()); 
			}
		}

		public void ClearGroup ()
		{
			if (multiTransmitter) {
				OpalInterface.ClearGroup ();
			}
		}

		public void GroupTransmit ()
		{
			if (multiTransmitter) {
				OpalInterface.GroupTransmit ();
			}
		}
*/
		public void CollectAndSendReceivers ()
		{
			Receiver[] receivers = FindObjectsOfType<Receiver> (); 
			for (int i = 0; i < receivers.Length; i++) {
				Debug.Log ("Adding receiver " + receivers [i].id + ". radius=" + receivers [i].radius);
				int r = OpalInterface.AddReceiverFromUnity (receivers [i].id, OpalInterface.ToMarshal (receivers [i].transform), receivers [i].radius, receivers [i].GetCallback ());
				if (r != 0) {
					throw new System.InvalidOperationException ("Error in AddReceiverFromUnity: id=" + receivers [i].id + ". Error:" + r);
				}
			}
		}

		public void CollectAndSendStaticMeshes ()
		{
			Debug.Log ("Sending static meshes");
			//Get all static meshes in the opal layer
			StaticMesh[] staticMeshes = FindObjectsOfType<StaticMesh> (); 
			int m = 0;
			for (int i = 0; i < staticMeshes.Length; i++) {

				SendStaticMeshToOpal (staticMeshes [i].transform, staticMeshes [i]);
				m++;

			}
			Debug.Log (m + " static meshes sent");
		}

		public void CreateRaySphere (int elevationDelta, int azimuthDelta)
		{
			int r;
			if (useSubStepSphere) {
				r = OpalInterface.CreateRaySphere2DSubstep (elevationDelta, azimuthDelta);
			} else {
				r = OpalInterface.CreateRaySphere2D (elevationDelta, azimuthDelta);
			}
			if (r != 0) {
				throw new System.InvalidOperationException ("Error in CreateRaySphere2D:" + r);
			}
		}

		public void SaveMeshToFile (string name, Vector3ToMarshal[] vertices, int[] indices, Matrix4x4ToMarshal tm, MaterialEMProperties em)
		{
			FileStream m_FileStream = new FileStream (name + "-v.txt", FileMode.Create, FileAccess.ReadWrite);
			StreamWriter m_mesh = new StreamWriter (m_FileStream, System.Text.Encoding.ASCII);
			FileStream m_FileStream2 = new FileStream (name + "-i.txt", FileMode.Create, FileAccess.ReadWrite);
			StreamWriter m_tri = new StreamWriter (m_FileStream2, System.Text.Encoding.ASCII);
			FileStream m_FileStream3 = new FileStream (name + "-t.txt", FileMode.Create, FileAccess.ReadWrite);
			StreamWriter m_tm = new StreamWriter (m_FileStream3, System.Text.Encoding.ASCII);
			FileStream m_FileStream4 = new FileStream (name + "-em.txt", FileMode.Create, FileAccess.ReadWrite);
			StreamWriter m_em = new StreamWriter (m_FileStream4, System.Text.Encoding.ASCII);
			for (int i = 0; i < indices.Length; i++) {
				//Debug.Log ("index=" + indices [i]);
				m_tri.WriteLine (indices [i]);

			}
			for (int i = 0; i < vertices.Length; i++) {
				m_mesh.WriteLine (vertices [i].x.ToString ("E8") + "\t" + vertices [i].y.ToString ("E8") + "\t" + vertices [i].z.ToString ("E8"));
			}
			//translation matrix
			m_tm.WriteLine (tm.m00.ToString ("E8") + "\t" + tm.m01.ToString ("E8") + "\t" + tm.m02.ToString ("E8") + "\t" + tm.m03.ToString ("E8"));
			m_tm.WriteLine (tm.m10.ToString ("E8") + "\t" + tm.m11.ToString ("E8") + "\t" + tm.m12.ToString ("E8") + "\t" + tm.m13.ToString ("E8"));
			m_tm.WriteLine (tm.m20.ToString ("E8") + "\t" + tm.m21.ToString ("E8") + "\t" + tm.m22.ToString ("E8") + "\t" + tm.m23.ToString ("E8"));
			m_tm.WriteLine (tm.m30.ToString ("E8") + "\t" + tm.m31.ToString ("E8") + "\t" + tm.m32.ToString ("E8") + "\t" + tm.m33.ToString ("E8"));

			m_em.WriteLine (em.a);
			m_em.WriteLine (em.b);
			m_em.WriteLine (em.c);
			m_em.WriteLine (em.d);

			m_mesh.Flush ();
			m_mesh.Close ();
			m_tri.Flush ();
			m_tri.Close ();
			m_tm.Flush ();
			m_tm.Close ();
			m_em.Flush ();
			m_em.Close ();
		}

		public virtual void SendMeshToGroup (int id, Transform rootT, Transform meshT, Mesh mesh, MaterialEMProperties em)
		{
			Vector3[] v = mesh.vertices;
			Vector3ToMarshal[] vertices = new Vector3ToMarshal[v.Length];
			int[] indices = mesh.triangles;
			//Debug.Log ("Sending mesh " + transform.name + " to group " + id + ". vertices=" + v.Length);
			for (int j = 0; j < v.Length; j++) {
				//Get the vertex position relative to the root
				//Debug.Log("v="+v [j].x.ToString ("E2") + "\t" + v [j].y.ToString ("E2") + " \t" + v [j].z.ToString ("E2"));
				Vector3 aux = rootT.InverseTransformPoint (meshT.TransformPoint (v [j]));
				//Debug.Log ("aux="+aux.x.ToString ("E2") + "\t" + aux.y.ToString ("E2") + " \t" + aux.z.ToString ("E2"));
				if (Mathf.Abs (aux.x) < 1E-15f) {
					aux.x = 0.0f;
				}
				if (Mathf.Abs (aux.y) < 1E-15f) {
					aux.y = 0.0f;
				}
				if (Mathf.Abs (aux.z) < 1E-15f) {
					aux.z = 0.0f;
				}
				vertices [j] = OpalInterface.ToMarshal (aux);
				//Debug.Log (vertices [i]);
			}
			//OpalManager.Instance.SaveMeshToFile (mf [i].name, vertices, indices);
			OpalManager.Instance.SendMeshToGroup (id, vertices, indices, em);
		}

		public virtual void AddDynamicMeshGroup (int id)
		{
			int dmg = OpalInterface.AddDynamicMeshGroup (id);
			if (dmg != 0) {
				throw new System.InvalidOperationException ("Error in DynamicMesh::CreateGroup():" + dmg);
			}

		}

		public virtual void FinishDynamicMeshGroup (int id)
		{
			OpalInterface.FinishDynamicMeshGroup (id);
		}

		public virtual void UpdateTransformInGroup (int id, Transform t)
		{
			Matrix4x4 tm = t.localToWorldMatrix;
			Matrix4x4ToMarshal matrix = new Matrix4x4ToMarshal ();

			//Debug.Log ("DynamicMesh Matrix of " + t.name + " is " + tm);
			OpalInterface.MarshalMatrix4x4 (ref tm, ref matrix);
			OpalInterface.UpdateTransformInGroup (id, matrix);
			
		}

		public virtual void RemoveDynamicMeshGroup (int id)
		{
			OpalInterface.RemoveDynamicMeshGroup (id);
		}
		//We get the vertices already transformed
		public void SendMeshToGroup (int id, Vector3ToMarshal[] vertices, int[] indices, MaterialEMProperties em)
		{

			//Debug.Log ("Sending " + vertices.Length + " vertices. indices=" + indices.Length);
			int r = OpalInterface.AddMeshToGroupFromUnity (id, vertices.Length, vertices, indices.Length, indices, em);
			if (r != 0) {
				throw new System.InvalidOperationException ("Could not add dynamic mesh to opal: " + id + ". Error:" + r);
			}


		}

		protected virtual void SendStaticMeshToOpal (Transform t, StaticMesh sm)
		{
			Vector3[] v = sm.meshFilter.mesh.vertices;
			Vector3ToMarshal[] vertices = new Vector3ToMarshal[v.Length];
			int[] indices = sm.meshFilter.mesh.triangles;
			//if (t.gameObject.name.Equals ("Cube")) {
			//	FileStream m_FileStream = new FileStream ("vert.txt", FileMode.Create, FileAccess.ReadWrite);
			//	StreamWriter m_mesh = new StreamWriter (m_FileStream, System.Text.Encoding.ASCII);
			//	FileStream m_FileStream2 = new FileStream ("tri.txt", FileMode.Create, FileAccess.ReadWrite);
			//	StreamWriter m_tri = new StreamWriter (m_FileStream2, System.Text.Encoding.ASCII);
			//	for (int i = 0; i < indices.Length; i++) {
			//		Debug.Log ("index=" + indices [i]);
			//		m_tri.WriteLine (indices [i]);
			//	}
			Matrix4x4 tm = t.transform.localToWorldMatrix;
			for (int i = 0; i < v.Length; i++) {
				if (Mathf.Abs (v [i].x) < 1E-15f) {
					v [i].x = 0.0f;
				}
				if (Mathf.Abs (v [i].y) < 1E-15f) {
					v [i].y = 0.0f;
				}
				if (Mathf.Abs (v [i].z) < 1E-15f) {
					v [i].z = 0.0f;
				}
				vertices [i] = OpalInterface.ToMarshal (v [i]);
				//vertices [i].x = v [i].x;
				//vertices [i].y = v [i].y;
				//vertices [i].z = v [i].z;
				//Debug.Log (v [i].x.ToString ("E2"));
				//Debug.Log (v [i].y.ToString ("E2"));
				//Debug.Log (v [i].z.ToString ("E2"));
				//Debug.Log (vertices [i].z.ToString ("E2"));
				//		m_mesh.WriteLine (v [i].x.ToString ("E2") + "\t" + v [i].y.ToString ("E2") + "\t" + v [i].z.ToString ("E2"));
				//Debug.Log(v [i].x.ToString ("E2") + "," + v [i].y.ToString ("E2") + "," + v [i].z.ToString ("E2"));
				//Vector4 nv = new Vector4 (v [i].x, v [i].y, v [i].z, 1.0f);
				//Debug.Log (tm *nv);
				//Debug.Log (tm.MultiplyPoint3x4 (v [i]));

			}
			//	m_mesh.Flush ();
			//	m_mesh.Close ();
			//	m_tri.Flush ();
			//	m_tri.Close ();

			//}

			Matrix4x4ToMarshal matrix = new Matrix4x4ToMarshal ();

			//Debug.Log("Matrix of"+t.transform.name+"is "+tm);
			OpalInterface.MarshalMatrix4x4 (ref tm, ref matrix);


			//SaveMeshToFile (t.transform.name, vertices, indices, matrix, sm.GetOpalMeshProperties ().emProperties);
			Debug.Log("Sending "+t.name);

			int r = OpalInterface.AddStaticMeshFromUnity (v.Length, vertices, indices.Length, indices, matrix, sm.GetOpalMeshProperties ().emProperties);
			if (r != 0) {
				throw new System.InvalidOperationException ("Could not add static mesh to opal: " + t.name + ". Error:" + r);
			}

		}
	}
}
