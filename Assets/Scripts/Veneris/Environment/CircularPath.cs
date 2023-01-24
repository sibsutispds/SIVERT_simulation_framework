/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Veneris
{
	public class CircularPath : Path
	{
		public float radius = 1f;

		void Awake ()
		{
			if (!initialized) {

				InitPathStructures ();
			}
		}

		void OnDrawGizmos ()
		{
			if (Application.isPlaying) {


				Gizmos.color = Color.red;

				//int index = GetIndexAtDistanceAlongInterpolatedPath (currentPointIndex, lookAheadForTargetOffset);

				//Gizmos.DrawWireSphere (interpolatedPath [index].position, 2);
				Gizmos.color = Color.blue;
				//DrawInterpolatedPath ();
				DrawPath();
			} 

		}

		override public void InterpolatePath ()
		{
			if (nodes.Count < 1) {
				return;
			}

			List<PathPointInfo> pointList = new List<PathPointInfo> ();
			List<float> arcDistList = new List<float> ();
			int totalPoints = 720;
			Vector3 center = nodes [0].transform.position - radius * nodes [0].transform.right;

			for (int i = 0; i < totalPoints; i++) {
				Vector3 pos = new Vector3 ();
				pos.y = center.y + 0f;
				pos.x = center.x + radius * Mathf.Sin (2 * Mathf.PI * i / totalPoints);
				pos.z = center.z + radius * Mathf.Cos (2 * Mathf.PI * i / totalPoints);
				//pos = pos + nodes [0].transform.position;
				Vector3 norm = new Vector3 (-Mathf.Sin (2 * Mathf.PI * i / totalPoints), 0f, -Mathf.Cos (2 * Mathf.PI * i / totalPoints));
				Vector3 tang = Vector3.Cross (new Vector3 (0f,1f,0f), norm.normalized);
				float curv;
				if (radius != 0f) {
					curv = 1 / radius;
				} else {
					curv = 999999f;
				}
				pointList.Add (new PathPointInfo (pos, norm, tang, curv));
			}		

			arcDistances = new float[pointList.Count - 1];
			for (int i = 0; i < pointList.Count - 1; i++) {
				arcDistances [i] = (pointList [i].position - pointList [i + 1].position).magnitude;
			}

			

			interpolatedPath = pointList.ToArray ();

//			//Points include two control points
//			if (points.Length > 3) {
//
//
//				CentripetalCatmullRomSpline crspline = new CentripetalCatmullRomSpline (points);
//				List<PathPointInfo> pointList = new List<PathPointInfo> ();
//				List<float> arcDistList = new List<float> ();
//				for (int i = 0; i < nodes.Count-1; i++) {
//					float dist = (nodes [i].transform.position - nodes [i + 1].transform.position).magnitude;
//					int totalPoints = Mathf.CeilToInt(interpolatedPointsDensity * dist);
//					for (int j = 0; j < totalPoints; j++) {
//						float pm = (float)j / (totalPoints ); //last element go the following section
//						Vector3 pos = crspline.InterpolateAtSegment (i, pm);
//						Vector3 norm = crspline.NormalAtSegment (i,pm);
//						Vector3 tang = crspline.TangentAtSegment(i,pm);
//						float curv = crspline.CurvatureAtSegment (i,pm);
//						pointList.Add (new PathPointInfo (pos, norm, tang, curv));
//
//					}
//				}
//				arcDistances = new float[pointList.Count - 1];
//
//				for (int i = 0; i < pointList.Count-1; i++) {
//					arcDistances [i] = (pointList [i].position - pointList [i + 1].position).magnitude;
//				}
//
//				interpolatedPath = pointList.ToArray ();
//
//
//
//
//			} else {
//				Debug.Log ("Only one node in path");
//				return;
//			}
		}

		override public PathPointInfo[] GenerateInterpolatedPath ()
		{
			PathPointInfo[] intpath = null;
			int totalPoints;
			if (nodes.Count >= 1)
				totalPoints = 720;
			else
				totalPoints = 0;
			intpath = new PathPointInfo[totalPoints];

			Vector3 center = nodes [0].transform.position - radius * nodes [0].transform.right;

			for (int i = 0; i < totalPoints; i++) {
				Vector3 pos = new Vector3 ();
				pos.y = center.y + 0f;
				pos.x = center.x + radius * Mathf.Sin (2 * Mathf.PI * i / totalPoints);
				pos.z = center.z + radius * Mathf.Cos (2 * Mathf.PI * i / totalPoints);
				//pos = pos + nodes [0].transform.position;
				Vector3 norm = new Vector3 (-Mathf.Sin (2 * Mathf.PI * i / totalPoints), 0f, -Mathf.Cos (2 * Mathf.PI * i / totalPoints));
				Vector3 tang = Vector3.Cross (new Vector3 (0f,1f,0f), norm.normalized);
				float curv;
				if (radius != 0f) {
					curv = 1 / radius;
				} else {
					curv = 999999f;
				}
				intpath [i] = new PathPointInfo (pos, norm, tang, curv);
			}

			return intpath;

//			PathPointInfo[] intpath = null;
//			//Points include two control points
//			Vector3[] aux = PathControlPointGenerator ();
//			if (aux.Length > 3) {
//
//				Spline crspline = new CentripetalCatmullRomSpline (aux);
//
//				int totalPoints = (aux.Length - 3) * interpolatedPointsPerSegment + nodes.Count;  ////(Points include two control points, so segments=length-3) segments* interpolatedPointsPerSegment + nodes
//
//				intpath = new PathPointInfo[totalPoints];
//
//				for (int i = 0; i < totalPoints; i++) {
//					float pm = (float)i / (totalPoints - 1); //Last one is t=1
//					intpath [i] = new PathPointInfo (crspline.Interpolate (pm), crspline.Normal (pm), crspline.Tangent (pm), crspline.Curvature (pm));
//				}
//			}
//			return intpath;
		}
		override public void InitPathStructures ()
		{
			if (nodes != null) {
				//Cache points into a vector when not in editor mode
				//Remember that we have now two additional control points
				//Debug.Log("initializing path");
				points = new Vector3[1];
				System.Array.Copy (GetPathPointsArray (), 0, points, 0, nodes.Count);

//				points = PathControlPointGenerator ();
//				//Debug.Log ("numpoints="+points.Length);
				totalPathLength = 0f;
				//distances = new float[1];
				//distances[0] = 0f;
				totalPathLength = 2 * Mathf.PI * radius;
//				distances = new float[points.Length - 1];
//				for (int i = 0; i < distances.Length; ++i) {
//					distances [i] = totalPathLength;
//					totalPathLength += (points [i + 1] - points [i]).magnitude;
//					//Debug.Log ("distances[" + i + "]=" + distances [i]);
//				}

				InterpolatePath ();
				maxCurvature = FindMaxCurvature ();
				currentPointIndex = 0; //Start with the first node in the interpolated  path, which corresponds to node 1 recall that there are two extra control points
				initialized = true;
			} else {
				initialized = false;
			}
		}

		override public  void DrawPath (bool changeColor = false, Color c = default(Color))
		{

			if (nodes.Count >= 1) {
				totalPathLength = 0f;

				int totalPoints = 720;
				Vector3 oldPos = new Vector3 ();
				Vector3 center = nodes [0].transform.position - radius * nodes [0].transform.right;

				oldPos.y = center.y + 0f;
				oldPos.x = center.x + radius * Mathf.Sin (0f);
				oldPos.z = center.z + radius * Mathf.Cos (0f);

				for (int i = 1; i < totalPoints; i++) {
					Vector3 pos = new Vector3 ();
					pos.y = center.y + 0f;
					pos.x = center.x + radius * Mathf.Sin (2 * Mathf.PI * i / totalPoints);
					pos.z = center.z + radius * Mathf.Cos (2 * Mathf.PI * i / totalPoints);
					//pos = pos + nodes [0].transform.position;
					if (oldPos != null) {
						Debug.DrawLine (oldPos, pos, Color.red);
					}

					oldPos = pos;
				}


//				Spline crspline = new CentripetalCatmullRomSpline (PathControlPointGenerator ());
//
//				//Line Draw:
//				Vector3 prevPt = crspline.Interpolate (0);
//				if (changeColor) {
//					Gizmos.color = c;
//
//				} else {
//					Gizmos.color = pathColor;
//				}
//				int SmoothAmount = nodes.Count * 20;
//				for (int i = 1; i <= SmoothAmount; i++) {
//					float pm = (float)i / SmoothAmount;
//					Vector3 currPt = crspline.Interpolate (pm);
//					totalPathLength += Vector3.Distance (currPt, prevPt);
//					Gizmos.DrawLine (currPt, prevPt);
//					if (drawNormals) {
//						Gizmos.color = Color.red;
//						Gizmos.DrawRay (currPt, crspline.Tangent (pm));
//						Gizmos.color = Color.blue;
//
//						Gizmos.DrawRay (currPt, crspline.Normal (pm));
//						Gizmos.color = Color.yellow;
//						Gizmos.DrawRay (currPt, crspline.Binormal (pm));
//						Gizmos.color = pathColor;
//					}
//					prevPt = currPt;
//				}
			}
		}
	}

}