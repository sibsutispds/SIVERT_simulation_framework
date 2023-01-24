/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Opal
{
	//A (compound) mesh that may be moved. Use it for compound meshes that KEEP CONSTANT their position relative to the parent, since
	//only the transform of the parent is sent to Opal and updated. If the child meshes vary their position, they have to use their own dynamic mesh
	public class DynamicMesh : MonoBehaviour
	{

		public int id;
		[SerializeField]
		protected bool registered = false;
		// Use this for initialization
		void Start ()
		{
			//Opal has already called InitOpal() in most of the cases

			//Try to find a receiver/transceiver and get the id from it
			Receiver r=GetComponentInChildren<Receiver>();
			if (r != null) {
				this.id = r.id;
			} 


			//Announce that we want to register and let the manager call CreateGroup when it is initialized
			OpalManager.Instance.RegisterDynamicMesh (this);

		}

		public void CreateGroup ()
		{


			//Get the root Properties
			OpalMeshProperties opRoot = GetComponent<OpalMeshProperties> ();
			Transform rootTransform = transform;

			if (opRoot == null) { 
				OpalMeshProperties[] ops = GetComponentsInChildren<OpalMeshProperties> ();
				if (ops.Length == 0) {
					Debug.Log ("No material electromagnetic  properties  found. Dynamic mesh not created for " + name);
					return;
				}
				bool send = false;
				for (int i = 0; i < ops.Length; i++) {
					if (ops [i].sendToOpal == true) {
						send = true;
						break;
					}
				}
				if (send == false) {
					return;
				}
			} 
			//At least one OpalMeshProperties
			//All meshes in  children share the same EM properties unless they have their own OpalMeshProperties
			int sent=0;
			MeshFilter[] mf = GetComponentsInChildren<MeshFilter> (false);
			if (mf.Length > 0) {
				//Create group with our id
				OpalManager.Instance.AddDynamicMeshGroup (id);

				// We provide all the vertices relative to the root transform of the group,

				for (int i = 0; i < mf.Length; i++) {
					OpalMeshProperties op = mf [i].transform.GetComponent<OpalMeshProperties> ();
					if (op == null) {
						op = opRoot;
					}
					if (op.sendToOpal == false) {
						continue;
					}
					/*Vector3[] v = mf [i].mesh.vertices;
					Vector3ToMarshal[] vertices = new Vector3ToMarshal[v.Length];
					int[] indices = mf [i].mesh.triangles;
					Debug.Log ("Sending mesh " + mf [i].name + " to group " + id +". vertices="+v.Length) ;
					for (int j = 0; j < v.Length; j++) {
						//Get the vertex position relative to the root
						//Debug.Log("v="+v [j].x.ToString ("E2") + "\t" + v [j].y.ToString ("E2") + " \t" + v [j].z.ToString ("E2"));
						Vector3 aux = transform.InverseTransformPoint (mf [i].transform.TransformPoint (v [j]));
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
					OpalManager.Instance.SendMeshToGroup (id, vertices, indices, op.emProperties);
					*/
					OpalManager.Instance.SendMeshToGroup (id, rootTransform, mf [i].transform, mf [i].mesh, op.emProperties);
					sent++;
				}
			}

			//Finally add the transform  of the parent
			Matrix4x4 tm = transform.localToWorldMatrix;
			//Matrix4x4ToMarshal matrix = new Matrix4x4ToMarshal ();

			//Debug.Log ("Matrix of " + name + " is " + tm);
			//OpalInterface.MarshalMatrix4x4 (ref tm, ref matrix);
			//Debug.Log ("Marshal matrix is " + matrix);
			OpalManager.Instance.UpdateTransformInGroup (id, transform);
			OpalManager.Instance.FinishDynamicMeshGroup (id);
			transform.hasChanged = false;
			registered = true;
			//Debug.Log("Created dynamic mesh group with "+sent+" submeshes");
			 




		}

		public void UpdateTransform ()
		{
			//Debug.Log ("Update transform");
			OpalManager.Instance.UpdateTransformInGroup(id,transform);
			transform.hasChanged = false;
		}

		public void RemoveGroup ()
		{
			if (OpalManager.isInitialized) {
				if (registered) {
					OpalManager.Instance.RemoveDynamicMeshGroup (id);
					registered = false;
				}
			}
		}

		void FixedUpdate ()
		{
			if (transform.hasChanged) {
			//	Debug.Log ("Dynamic mesh " + id + " transform has changed");
				UpdateTransform ();
			
			}
		}

		void OnDestroy ()
		{
			if (registered) {
				RemoveGroup ();
			}
		}
		void OnDisable() {
			if (registered) {
				RemoveGroup ();
			}
		}

	}
}
