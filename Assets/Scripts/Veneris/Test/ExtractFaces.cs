/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Opal;
public class ExtractFaces : MonoBehaviour {

	/*public class BoxEqualityComparer : IEqualityComparer<Vector3> {
		public bool Equals(
	}*/
	// Use this for initialization
	Dictionary<Vector3, int> faces;
	void Start () {
		MeshFilter mf = GetComponent<MeshFilter> ();
		if (mf != null) {
			faces = new Dictionary<Vector3, int> ();
			Vector3[] v = mf.mesh.vertices;
			Vector3ToMarshal[] vertices = new Vector3ToMarshal[v.Length];
			int[] indices = mf.mesh.triangles;
			Debug.Log ("verts=" + v.Length + "ind=" + indices.Length);
			if (v.Length % 3 != 0) {
				Debug.Log ("verts is not mult of 3");
			}
			/*for (int i = 0; i < v.Length; i++) {
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
			}*/
			int facesId = 0;
			for (int i = 0; i < indices.Length; i=i+3) {
				Vector3 p0 = v [indices[i]];
				Vector3 p1 = v [indices[i+1]];
				Vector3 p2 = v [indices[i+2]];
				Vector3 e0 = p1 - p0;
				Vector3 e1 = p0 - p2;
				Vector3 normal = Vector3.Cross (e1, e0);
				int value;
				if (faces.TryGetValue (normal, out value)) {
				} else {
					faces.Add (normal, facesId);
					facesId++;
				}

			}
			Debug.Log (faces.Keys.Count + " added");
			foreach (KeyValuePair<Vector3,int> kvp in faces) {
				Debug.Log ("faceId=" + kvp.Value + " v=" + kvp.Key);
				Debug.DrawRay (transform.position, kvp.Key, Color.black);
				
			}

		}
	}
	

}
