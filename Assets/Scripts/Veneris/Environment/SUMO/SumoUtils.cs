/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using TriangleNet;
using TriangleNet.Geometry;
using System.Linq;
namespace Veneris
{
	public static class SumoUtils
	{
		

		public static  Vector3[] SumoShapeToVector3Array (string shape)
		{
			//Debug.Log (shape);
			string[] nodesString = shape.Split ();
			List<Vector3> nodes = new List<Vector3> ();
			foreach (string n  in nodesString) {
				//Debug.Log (n);
				string[] pos = n.Split (',');
				foreach (string s in pos) {
					//	Debug.Log (s);

				}
				nodes.Add (new Vector3 (StringToFloat (pos [0]), 0f, StringToFloat (pos [1])));
			}
			return nodes.ToArray ();
		}
		public static float StringToFloat (string s)
		{
			return float.Parse (s, System.Globalization.CultureInfo.InvariantCulture);
		}

		public static string SumoJunctionInternalIdToJunctionId(string internalId) {
			string[] tokens = internalId.Split ('_');
			//Remove 2 trailing numbers and first :
			string iname = tokens [0].Substring (1);
			for (int i = 1; i < tokens.Length - 2; i++) {
				iname = iname + "_" + tokens [i];
			}
			return iname;

		}

		public static float[] SumoConvBoundaryToFloat(string b) {
			float[] boundary = new float[4];
			string[] tokens = b.Split (',');
			if (tokens.Length == 4) {
				for (int i = 0; i < tokens.Length; i++) {
					boundary [i] = float.Parse (tokens [i]);
				}
				return boundary;
			} else {
				Debug.Log ("Invalid convBoundary");
				return null;
			}
		}

		public static GameObject CreateTriangulation (string  name, string id, Vector3[] shape)
		{

			GameObject eo = new GameObject (name + " " + id);
			UnityEngine.Mesh mesh = new 	UnityEngine.Mesh ();

			//UnityEngine.Mesh mesh  = eo.AddComponent<MeshFilter> ().mesh;

			eo.AddComponent<MeshRenderer> ();
			MeshData tb = new MeshData ();

			CreateEdgeTriangulation (shape, tb);

			Assert.IsNotNull (tb.Vertices);
			Assert.IsNotNull (mesh);





			//Debug.Log ("name=" + eo.name);
			mesh.vertices = tb.Vertices.ToArray ();
			mesh.triangles = tb.Indices.ToArray ();
			mesh.SetUVs (0, tb.UV);
			mesh.RecalculateNormals ();
			eo.AddComponent<MeshFilter> ().mesh = mesh;

			return eo;

		}

		public static int CreateEdgeTriangulation (Vector3[] corners, MeshData data)
		{

			TriangleNet.Mesh _mesh = new TriangleNet.Mesh ();

			var inp = new InputGeometry (corners.Length);

			for (int i = 0; i < corners.Length; i++) {
				var v = corners [i];
				inp.AddPoint (v.x, v.z);
				inp.AddSegment (i, (i + 1) % corners.Length);
			}
			_mesh.Behavior.Algorithm = TriangulationAlgorithm.Incremental;
			_mesh.Behavior.Quality = true;
			_mesh.Triangulate (inp);

			var vertsStartCount = data.Vertices.Count;

			//data.Vertices.AddRange (corners.Select (x => new Vector3 (x.x, 0, x.z)).ToList ());

			//Sometimes the number of output vertices is greater than the number of input ones in Triangle.Net during triangulation, see 
			//http://www.cs.cmu.edu/~quake/triangle.trouble.html
			//For example, some SUMO junction shapes do not form a closed polygon, try as an example junction id=4578017558 from OSM
			//So we use the Triangle.Net. Mesh vertices and have to change the UVs accordingly
			List<Vector3> verts =_mesh.Vertices.Select (x => new Vector3 (System.Convert.ToSingle(x.X), 0, System.Convert.ToSingle(x.Y))).ToList ();
			data.Vertices.AddRange (verts);

			foreach (var tri in _mesh.Triangles) {
				data.Indices.Add (vertsStartCount + tri.P1);
				data.Indices.Add (vertsStartCount + tri.P0);
				data.Indices.Add (vertsStartCount + tri.P2);


			}
			//Create UVs
			Vector2 min = FindMinCoordinates(verts.ToArray());
			Vector2 max = FindMaxCoordinates(verts.ToArray());
			float rangex = max.x - min.x;
			float rangey = max.y - min.y;


			foreach (Vector3 c in verts)
			{

				data.UV.Add(new Vector2((c.x - min.x)/rangex, (c.z - min.y)/rangey));
			}

			return vertsStartCount;
		}
		public static Vector2 FindMinCoordinates(Vector3[] shape) {
			Vector2 min = new Vector2 (float.MaxValue, float.MaxValue);
			foreach (Vector3 v in shape) {
				if (v.x <= min.x) {
					min.x = v.x;
				}
				if (v.z <= min.y) {
					min.y = v.z;
				}
			}
			return min;
		}
		public static Vector2 FindMaxCoordinates(Vector3[] shape) {
			Vector2 max = new Vector2 (float.MinValue, float.MinValue);
			foreach (Vector3 v in shape) {
				if (v.x >= max.x) {
					max.x = v.x;
				}
				if (v.z >= max.y) {
					max.y = v.z;
				}
			}
			return max;
		}
		public static List<string> SumoAttributeStringToStringList (string s)
		{
			List<string> list = new List<string> ();
			list.AddRange (s.Split (new char[0], System.StringSplitOptions.RemoveEmptyEntries));
			return list;
		}
		public static List<int> SumoResponseToIntArray (string response)
		{

			return response.ToCharArray ().Select (x => int.Parse (x.ToString ())).ToList ();
		}

		public static bool IsInternalEdge(string eId) {
			return eId [0].Equals (':');

		}
	}
}
