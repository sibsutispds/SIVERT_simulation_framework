/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Opal
{
	public class SaveStaticMeshesToFolder : MonoBehaviour
	{

		public static string folder;

		[MenuItem ("Opal/Save Static Meshes to Current Folder")]
		public static void SaveStaticMeshesToCurrentFolder ()
		{ 
			folder = Directory.GetCurrentDirectory ();
			Directory.CreateDirectory ("meshes");
			StaticMesh[] staticMeshes = FindObjectsOfType<StaticMesh> (); 
			int m = 0;
			List<string> fileList = new List<string> ();
			for (int i = 0; i < staticMeshes.Length; i++) {
				SaveStaticMesh (staticMeshes [i].transform, staticMeshes [i]);
				fileList.Add (staticMeshes [i].transform.name);
			}
			//Save List to file
			FileStream lFile = new FileStream(folder+"/meshes/names.txt",FileMode.Create, FileAccess.ReadWrite);
			StreamWriter l_sw= new StreamWriter (lFile, System.Text.Encoding.ASCII);
			for (int i = 0; i < fileList.Count; i++) {
				l_sw.WriteLine (fileList [i]);
			}
			l_sw.Flush ();
			lFile.Close ();

		}
		protected static void SaveStaticMesh (Transform t, StaticMesh sm) {
			MeshFilter meshFilter = sm.GetComponent<MeshFilter> ();
			Debug.Log (t.name);
			Vector3[] v = meshFilter.sharedMesh.vertices;
			Vector3ToMarshal[] vertices = new Vector3ToMarshal[v.Length];
			int[] indices = meshFilter.sharedMesh.triangles;
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

			SaveMeshToFile (folder+"/meshes/"+t.transform.name, vertices, indices, matrix, sm.GetComponent<OpalMeshProperties> ().emProperties);
			
		}
		public static void  SaveMeshToFile (string name, Vector3ToMarshal[] vertices, int[] indices, Matrix4x4ToMarshal tm, MaterialEMProperties em) {
			
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
	}
}
