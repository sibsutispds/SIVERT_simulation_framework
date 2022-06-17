/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Veneris
{
	public class PathConnectorBuilder : MonoBehaviour
	{
		public Transform startSegment1;
		public Transform endSegment1;
		public Transform startSegment2;
		public Transform endSegment2;
		public Transform connectionPoint1;
		public Transform connectionPoint2;
		public Transform turningPoint;


		public GameObject CreateNewPath ()
		{
			GameObject go = new GameObject ("Path");
			BuildPath (go);
			return go;
		}

		//Calculate the intersection point of two lines. Returns true if lines intersect, otherwise false.
		//Note that in 3d, two lines do not intersect most of the time. So if the two lines are not in the
		//same plane, use ClosestPointsOnTwoLines() instead.
		public  bool LineLineIntersection (out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
		{

			Vector3 lineVec3 = linePoint2 - linePoint1;
			Vector3 crossVec1and2 = Vector3.Cross (lineVec1, lineVec2);
			Vector3 crossVec3and2 = Vector3.Cross (lineVec3, lineVec2);

			float planarFactor = Vector3.Dot (lineVec3, crossVec1and2);

			//is coplanar, and not parrallel
			if (Mathf.Abs (planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f) {
				float s = Vector3.Dot (crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
				intersection = linePoint1 + (lineVec1 * s);
				return true;
			} else {
				intersection = Vector3.zero;
				return false;
			}
		}

		public bool IsDataReady ()
		{
			return (startSegment1 != null && startSegment2 != null && endSegment1 != null && endSegment2 != null && connectionPoint1 != null && connectionPoint2 != null);
		}

		public Path BuildPath (GameObject go)
		{
			if (IsDataReady ()) {
				Path p = go.AddComponent<Path> ();
				//List<GameObject> nodes= new List<GameObject>();
				//nodes.Add (startPath.GetFirstNode());
				//GameObject h1 = new GameObject ();
				//h1.transform.position = Vector3.Lerp (startPath.GetFirstNode().transform.position, connectionPoint1.transform.position, 0.999f);
				//print(startPath.GetFirstNode().transform.position);
				//print (connectionPoint1.transform.position - startPath.GetFirstNode ().transform.position);
				//h1.transform.position = (connectionPoint1.transform.position-startPath.GetFirstNode().transform.position).normalized*10f;
				//h1.transform.position = Vector3.Lerp(connectionPoint1.transform.position,startPath.GetFirstNode().transform.position,0.001f);
				//h1.transform.position = Vector3.Lerp(connectionPoint1.transform.position,startSegment1.position,0.001f);
				p.AddNode (Vector3.Lerp (connectionPoint1.transform.position, startSegment1.position, 0.001f));
				p.AddNode (connectionPoint1.position);
				//GameObject intermediate = new GameObject ();
				//intermediate.transform.position = Vector3.Lerp (connectionPoint1.transform.position, connectionPoint2.transform.position, 0.5f);
				//intermediate.transform.position =GetTurningPoint();
				p.AddNode (GetTurningPointPosition ());
				p.AddNode (connectionPoint2.position);
				//GameObject h2 = new GameObject ();
				//h2.transform.position = Vector3.Lerp ( connectionPoint2.transform.position,endSegment2.position ,0.001f);
				//h2.transform.position = (connectionPoint2.transform.position-endPath.GetLastNode().transform.position).normalized*10f;
				p.AddNode (Vector3.Lerp (connectionPoint2.transform.position, endSegment2.position, 0.001f));
				//nodes.Add (endPath.GetLastNode ());
				//p.SetNodes (nodes);
				return p;

			}
			return null;
		}

		public Vector3 GetTurningPointPosition ()
		{
			if (turningPoint == null) {
				Vector3 intersection;
				Vector3 p1 = startSegment1.position;
				Vector3 dir1 = endSegment1.position - p1;
				Vector3 p2 = startSegment2.position;
				Vector3 dir2 = endSegment2.position - p2;
				bool succ = LineLineIntersection (out intersection, p1, dir1, p2, dir2);
				Vector3 middle = Vector3.Lerp (connectionPoint1.transform.position, connectionPoint2.transform.position, 0.5f);
				return Vector3.Lerp (intersection, middle, 0.4f);
			} else {
				return turningPoint.position;
			}
		}

		void OnDrawGizmos ()
		{
			if (IsDataReady ()) {
				Vector3 intersection;
				Vector3 p1 = startSegment1.position;
				Vector3 dir1 = endSegment1.position - p1;
				Vector3 p2 = startSegment2.position;
				Vector3 dir2 = endSegment2.position - p2;
				bool succ = LineLineIntersection (out intersection, p1, dir1, p2, dir2);
				Vector3 middle = Vector3.Lerp (connectionPoint1.transform.position, connectionPoint2.transform.position, 0.5f);
				Gizmos.DrawLine (intersection, middle);
			}
		}

	}
}
