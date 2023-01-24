using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Veneris
{



	public class GSCMController : MonoBehaviour
	{

		// Use this for initialization

		public GameObject[] GSCMenvironment;
		private MeshCollider[] GSCMColliders;
		

		void Start()
		{
			GSCMenvironment = GameObject.FindGameObjectsWithTag("GSCMmesh");
		}

		// Update is called once per frame
		void Update()
		{
//			GSCMenvironment = GameObject.Find("Environment");
			
			int i = 0;
			foreach (GameObject go in GSCMenvironment)
			{
				MeshCollider GSCMCollider = go.GetComponent<MeshCollider>();
				print(GSCMCollider.name);
				float[] sizes = GetTriSizes(GSCMCollider.sharedMesh.triangles, GSCMCollider.sharedMesh.vertices);
				++i;
			}
		}
		
		float[] GetTriSizes(int[] tris, Vector3[] verts)
		{
			int triCount = tris.Length / 3;

			float[] sizes = new float[triCount];
			for (int i = 0; i<triCount;
				i++)
			{
				sizes[i] = .5f * Vector3
					           .Cross(verts[tris[i * 3 + 1]] - verts[tris[i * 3]], verts[tris[i * 3 + 2]] - verts[tris[i * 3]])
					           .magnitude;
			}
			return sizes;
		}
		
		
	}
}