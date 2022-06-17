/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/

// Parent to the path class
// can be used to create paths on the editor
//TODO: this class was written at the beginning, without a clear idea of what later has become the project. It mixes path creation with structures used by paths. Should be splitted into other classes

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//using UnityEditor;

namespace Veneris
{
	public class NodePathHelper: MonoBehaviour
	{

		public string pathName = "";
		public long pathId = 0;
		public Color pathColor = Color.black;
		public List<GameObject> nodes = null;
		//We need to use transform to use 3D path rotations
		public bool initialized = false;
		public bool pathVisible = true;
		public float totalPathLength = 0f;
		public bool bindToTransform = true;
		public bool bindToTerrain = false;
		public int interpolatedPointsPerSegment = 10;
		public float interpolatedPointsDensity = 0.5f; //Points per meter
		public bool useDensity = true;
		public  float tau = 0.5f;
		public bool drawNormals = false;


		void Reset ()
		{
			pathName = "";
			pathId = 0;
			pathColor = Color.black;
			List<GameObject> nodes = null;  //We need to use transform to use 3D path rotations
			initialized = false;
			pathVisible = true;
			totalPathLength = 0f;
			bindToTransform = true;
			bindToTerrain = false;

			tau = 0.5f;
			drawNormals = false;
		}

		public int nodeCount {
			get {
				if (nodes != null) {
					return nodes.Count;
				} else {
					return 0;
				}

			}
		}


		public  Vector3[] GetPathPointsArray ()
		{
			Vector3[] array = new Vector3[nodes.Count];

			for (int i = 0; i < nodes.Count; i++) {
			
				array [i] = nodes [i].transform.position;

			}
			return array;

		}

		public void SetNodes (List<GameObject> _nodes)
		{

			nodes = null;
			foreach (GameObject g in _nodes) {
				//AddNode(g.transform.position);
				AddCopyNode (g);
			}
		}

		public void SetNodes (Vector3[] _nodes)
		{
			nodes = null;
			foreach (Vector3 v in _nodes) {
				AddNode (v);
			}
		}


		public List<GameObject> GetNodes ()
		{
			return nodes;
		}

		public GameObject GetFirstNode ()
		{
			return nodes [0];

		}

		public GameObject GetLastNode ()
		{
			return nodes [nodes.Count - 1];

		}


		public GameObject CreateNewNode ()
		{
			GameObject go = new GameObject ();
			go.transform.parent = transform;
			//go.hideFlags = HideFlags.HideInHierarchy;
			return go;
		}

		public void AddNode ()
		{
			GameObject go = CreateNewNode ();


			//Debug.Log (go.transform.rotation.eulerAngles);
			if (nodes == null) {
				nodes = new List<GameObject> ();
				go.name = "Node 0";
				go.transform.position = transform.position + new Vector3 (20, 0, 20);


			} else {
				go.name = "Node " + (nodes.Count);
				go.transform.position = nodes [nodes.Count - 1].transform.position + new Vector3 (20, 0, 20);

			}
			nodes.Add (go);

			//go.transform.SetParent(transform);
			//go.hideFlags = HideFlags.HideInHierarchy;
		}

		public void AddCopyNode (GameObject g)
		{
		
			GameObject go = CreateNewNode ();
			go.transform.position = g.transform.position;
			go.transform.rotation = g.transform.rotation;
			go.transform.localScale = g.transform.localScale;
			if (nodes == null) {
				nodes = new List<GameObject> ();
				go.name = "Node 0";

			} else {
				go.name = "Node " + (nodes.Count);
			}
			nodes.Add (go);
		}

		public void AddNode (Vector3 node)
		{
			GameObject go = CreateNewNode ();

			if (nodes == null) {
				nodes = new List<GameObject> ();
				go.name = "Node 0";
				go.transform.position = node;
				nodes.Add (go);
			} else {
				go.name = "Node " + (nodes.Count);
				go.transform.position = node;


				nodes.Add (go);
			}

			//go.transform.SetParent(transform);
			//go.hideFlags = HideFlags.HideInHierarchy;
		}

		public void MakeLoop ()
		{
			if (nodes.Count > 0) {
				AddNode (nodes [nodes.Count - 1].transform.position);
			}
		}

		public void ReversePath ()
		{
			nodes.Reverse ();
		}


		void OnDrawGizmosSelected ()
		{
			if (pathVisible) {
				if (nodes != null) {
					Debug.Log ("Draw path " + pathId);
					DrawPath ();
				}	
			}
		}

		virtual public  void DrawPath (bool changeColor = false, Color c = default(Color))
		{

			if (nodes.Count > 1) {
				//totalPathLength = 0f;
				Spline crspline = new CentripetalCatmullRomSpline (PathControlPointGenerator ());

				//Line Draw:
				Vector3 prevPt = crspline.Interpolate (0);
				if (changeColor) {
					Gizmos.color = c;

				} else {
					Gizmos.color = pathColor;
				}
				int SmoothAmount = nodes.Count * 20;
				for (int i = 1; i <= SmoothAmount; i++) {
					float pm = (float)i / SmoothAmount;
					Vector3 currPt = crspline.Interpolate (pm);
					//totalPathLength += Vector3.Distance (currPt, prevPt);
					Gizmos.DrawLine (currPt, prevPt);
					if (drawNormals) {
						Gizmos.color = Color.red;
						Gizmos.DrawRay (currPt, crspline.Tangent (pm));
						Gizmos.color = Color.blue;

						Gizmos.DrawRay (currPt, crspline.Normal (pm));
						Gizmos.color = Color.yellow;
						Gizmos.DrawRay (currPt, crspline.Binormal (pm));
						Gizmos.color = pathColor;
					}
					prevPt = currPt;
				}
			}
		}

		public void BindToTerrain ()
		{
			if (nodes != null) {
				for (int i = 0; i < nodes.Count; i++) {
					Vector3 aux = nodes [i].transform.position;
					if (Terrain.activeTerrain == null) {
						//Assume there is a Floor plane
						GameObject floor = GameObject.Find ("Floor");
						if (floor == null) {
							Debug.Log ("RoadBuilder::No Terrain or Floor found");
							return;
						}
						aux.y = floor.transform.position.y;
						nodes [i].transform.position = aux;
						nodes [i].transform.up = floor.transform.up;
						
					} else {
						aux.y = Terrain.activeTerrain.SampleHeight (nodes [i].transform.position);
					
						nodes [i].transform.position = aux;
						Vector3 terrainLocalPos = nodes [i].transform.position - Terrain.activeTerrain.transform.position;
						float x = Mathf.InverseLerp (0.0f, Terrain.activeTerrain.terrainData.size.x, terrainLocalPos.x);
						float y = Mathf.InverseLerp (0.0f, Terrain.activeTerrain.terrainData.size.z, terrainLocalPos.z);
						//Vector2 normalizedPos = new Vector2(Mathf.InverseLerp(0.0, Terrain.activeTerrain.terrainData.size.x, terrainLocalPos.x),Mathf.InverseLerp(0.0, Terrain.activeTerrain.terrainData.size.z, terrainLocalPos.z));
						Vector3 terrainNormal = Terrain.activeTerrain.terrainData.GetInterpolatedNormal (x, y);

						nodes [i].transform.up = terrainNormal;
					}
				}
			}


		}

		public  Vector3[] PathControlPointGenerator ()
		{

			Vector3[] vector3s;



			//populate calculate path;
			int offset = 2;

			vector3s = new Vector3[nodes.Count + offset];

			System.Array.Copy (GetPathPointsArray (), 0, vector3s, 1, nodes.Count);

			//populate start and end control points:
			//vector3s[0] = vector3s[1] - vector3s[2];
			vector3s [0] = vector3s [1] + (vector3s [1] - vector3s [2]);
			vector3s [vector3s.Length - 1] = vector3s [vector3s.Length - 2] + (vector3s [vector3s.Length - 2] - vector3s [vector3s.Length - 3]);

			//is this a closed, continuous loop? yes? well then so let's make a continuous Catmull-Rom spline!
			if (vector3s [1] == vector3s [vector3s.Length - 2]) {
				Vector3[] tmpLoopSpline = new Vector3[vector3s.Length];
				System.Array.Copy (vector3s, tmpLoopSpline, vector3s.Length);
				tmpLoopSpline [0] = tmpLoopSpline [tmpLoopSpline.Length - 3];
				tmpLoopSpline [tmpLoopSpline.Length - 1] = tmpLoopSpline [2];
				vector3s = new Vector3[tmpLoopSpline.Length];
				System.Array.Copy (tmpLoopSpline, vector3s, tmpLoopSpline.Length);
			}	

			return(vector3s);
		}





		public  Vector3[] GetPathPointsArrayReversed ()
		{

			List<GameObject> revNodes = nodes.GetRange (0, nodes.Count);
			revNodes.Reverse ();
			Vector3[] array = new Vector3[nodes.Count];

			for (int i = 0; i < revNodes.Count; i++) {
				array [i] = revNodes [i].transform.position;
			}
			return array;

		}


	}
}
