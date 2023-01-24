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
	//Based on iTweenPath
	[AddComponentMenu ("Veneris/Path")]

	public class Path : NodePathHelper
	{


		public Vector3[] points = null;
		public PathPointInfo[] interpolatedPath = null;

		public float[] arcDistances = null;
		public float maxCurvature = -1f;
		public float averageCurvature = -1f;

		protected float lookAheadForTargetOffset = 5;


		protected int p0n;
		protected int p1n;
		protected int p2n;
		protected int p3n;


		protected float i;
		protected Vector3 P0;
		protected Vector3 P1;
		protected Vector3 P2;
		protected Vector3 P3;


		protected int currentPointIndex;

		public bool internalPath = false; //Used with SUMO

		[System.Serializable]
		public class PathPointInfo
		{
			public Vector3 position;
			public Vector3 normal;
			public Vector3 tangent;
			public float curvature;


			public PathPointInfo (Vector3 position, Vector3 normal, Vector3 tangent, float curvature)
			{
				this.position = position;
				this.normal = normal;
				this.tangent = tangent;
				this.curvature = curvature;
			}
		}

		void Awake ()
		{
			if (!initialized) {

				InitPathStructures ();
			}
		}

		void Start ()
		{
			
			if (!initialized) {

				InitPathStructures ();
			}

		}


		void OnDrawGizmosSelected ()
		{
			if (pathVisible) {
				if (nodes != null) {
					if (initialized) {
						DrawInterpolatedPath ();
					}
				}	
			}
		}

		public bool IsInternal() {
			return internalPath;
		}
		public void SetInternal(bool v) {
			internalPath = v;
		}
	

		public virtual void InitPathStructures ()
		{
			if (nodes != null) {
				//Debug.Log ("Init path structures for path " + pathId +"totalPathLength="+totalPathLength);
				//Cache points into a vector when not in editor mode
				//Remember that we have now two additional control points
				//Debug.Log("initializing path");
				points = PathControlPointGenerator ();
				//Debug.Log ("numpoints="+points.Length);
				totalPathLength = 0f;



				InterpolatePath ();

				for (int i = 0; i < arcDistances.Length; i++) {
					totalPathLength += arcDistances [i];
						
				}


				maxCurvature = FindMaxCurvature ();
				currentPointIndex = 0; //Start with the first node in the interpolated  path, which corresponds to node 1 recall that there are two extra control points
				initialized = true;
			} else {
				initialized = false;
			}
		}

		public float FindMaxCurvature ()
		{
			float max = float.MinValue;
			averageCurvature = 0f;
			int n = 0;
			if (interpolatedPath != null) {
				foreach (PathPointInfo i in interpolatedPath) {
					if (i.curvature > max) {
						max = i.curvature;
					}
					averageCurvature += i.curvature;
					++n;
				}
			}
			averageCurvature = averageCurvature / n;
			return max;
		}

		public float GetPathDistanceFromIndexToEnd(int index) {
			float sum = 0f;
			for (int i = index; i < arcDistances.Length; i++) {
				sum += arcDistances [i];
				
			}
			return sum;
		}


		public List<Vector3> GetInterpolatedPathPositions() {
			
			if (initialized) {
				List<Vector3> l = new List<Vector3> ();
				for (int i = 0; i < interpolatedPath.Length; i++) {
					l.Add (interpolatedPath [i].position);
				}
				return l;
			} else {
				return null;
			}
		}

		public PathPointInfo FindClosestPointInfoInPath (Transform target, int index = 0)
		{
			
			if (initialized) {
				return interpolatedPath [FindClosestPointInInterpolatedPath (target, index)];
			} else {
				if (nodes == null) {
					Debug.Log ("nodes is null");
				}
				PathPointInfo[] ip = GenerateInterpolatedPath ();
				int j = index;
				float min = float.MaxValue;
				float dist = 0f;
				for (int i = index; i < ip.Length; i++) {
					dist = (target.position - ip [i].position).sqrMagnitude;

					if (dist <= min) {
						min = dist;
						j = i;
					}
				}

				return ip [j];
			}
		}


		public int FindClosestPointInInterpolatedPath (Transform target, int index = 0)
		{

			int j = index;
			float min = float.MaxValue;
			float dist = 0f;
			for (int i = index; i < interpolatedPath.Length; i++) {
				dist = (target.position - interpolatedPath [i].position).sqrMagnitude;

				if (dist <= min) {
					min = dist;
					j = i;
				}
			}
			//Debug.Log (min);
			return j;
		}


		public int FindClosestPointInInterpolatedPath (Vector3 target, int index = 0)
		{

			int j = index;
			float min = float.MaxValue;
			float dist = 0f;
			for (int i = index; i < interpolatedPath.Length; i++) {
				dist = (target - interpolatedPath [i].position).sqrMagnitude;

				if (dist <= min) {
					min = dist;
					j = i;
				}
			}
			//Debug.Log (min);
			return j;
		}


		public Vector3 GetPointAtDistanceFromInterpolatedPath (int index, float dist, Vector3 pos)
		{
			//Starting from an index, return the largest index of a segment whose initial point is at distance >= dist from pos

			//We use squared distances
			float sdist = dist * dist;
			float rdist = (interpolatedPath [index].position - pos).sqrMagnitude;
			while ((rdist < sdist) && (index < interpolatedPath.Length - 1)) {
				++index;
				//Debug.Log ("index=" + index);
				rdist = (interpolatedPath [index].position - pos).sqrMagnitude;
			}
			return interpolatedPath [index].position;
		}


		public PathPointInfo GetPathInfoAtDistanceFromInterpolatedPath (int index, float dist, Vector3 pos)
		{
			//Starting from an index, return the largest index of a segment whose initial point is at distance >= dist from pos

			//We use squared distances
			float sdist = dist * dist;
			float rdist = (interpolatedPath [index].position - pos).sqrMagnitude;
			while ((rdist < sdist) && (index < interpolatedPath.Length - 1)) {
				++index;
				//Debug.Log ("index=" + index);
				rdist = (interpolatedPath [index].position - pos).sqrMagnitude;
			}
			return interpolatedPath [index];
		}

		public PathPointInfo GetPathInfoAtDistanceFromInterpolatedPath (int index, float dist, Vector3 pos, out bool inPath)
		{
			//Starting from an index, return the largest index of a segment whose initial point is at distance >= dist from pos and whether it is still in the path.
			//Useful to know if we have to switch to the next path
			inPath = true;
			//We use squared distances
			float sdist = dist * dist;
			float rdist = (interpolatedPath [index].position - pos).sqrMagnitude;

			while ((rdist < sdist) && inPath) {
				++index;
				if (index == interpolatedPath.Length) {
					inPath = false;
					return interpolatedPath [index - 1];
				}
				//Debug.Log ("index=" + index);
				rdist = (interpolatedPath [index].position - pos).sqrMagnitude;
			}
			return interpolatedPath [index];
		}



		public PathPointInfo GetLastPathPoint ()
		{
			return interpolatedPath [interpolatedPath.Length - 1];
		}



		public int GetIndexAtDistanceAlongInterpolatedPath (int index, float dist)
		{ 
			//distance measured along the path
//			Debug.Log("current index="+index);
			float sum = arcDistances [index];
			while (sum < dist) {
				++index;
				if (index >= arcDistances.Length) {
					break;
				}
				sum += arcDistances [index];
				//Debug.Log(sum);
			}
//			Debug.Log("next path index="+index);
			return index;
		}

	

		//void OnDrawGizmos ()
		//{
			/*if (Application.isPlaying) {


				Gizmos.color = Color.red;

				int index = GetIndexAtDistanceAlongInterpolatedPath (currentPointIndex, lookAheadForTargetOffset);

				Gizmos.DrawWireSphere (interpolatedPath [index].position, 2);
				Gizmos.color = Color.yellow;
				DrawInterpolatedPath ();
			} */

		//}


		public virtual PathPointInfo[] GenerateInterpolatedPath ()
		{
			PathPointInfo[] intpath = null;
			//Points include two control points
			Vector3[] aux = PathControlPointGenerator ();
			if (aux.Length > 3) {

				Spline crspline = new CentripetalCatmullRomSpline (aux);

				int totalPoints = (aux.Length - 3) * interpolatedPointsPerSegment + nodes.Count;  ////(Points include two control points, so segments=length-3) segments* interpolatedPointsPerSegment + nodes

				intpath = new PathPointInfo[totalPoints];
	
				for (int i = 0; i < totalPoints; i++) {
					float pm = (float)i / (totalPoints - 1); //Last one is t=1
					intpath [i] = new PathPointInfo (crspline.Interpolate (pm), crspline.Normal (pm), crspline.Tangent (pm), crspline.Curvature (pm));
				}
			}
			return intpath;
		}

		//Rough approximation of an arc-length parameterization. We assume that the arc-length is close to the euclidean distance between nodes.
		//This  works approximately for straight paths
		protected void InterpolateWithDensity() {

			//Points include two control points
			if (points.Length > 3) {

				CentripetalCatmullRomSpline crspline = new CentripetalCatmullRomSpline (points);
				List<PathPointInfo> pointList = new List<PathPointInfo> ();
				List<float> arcDistList = new List<float> ();
				for (int i = 0; i < nodes.Count-1; i++) {
					float dist = (nodes [i].transform.position - nodes [i + 1].transform.position).magnitude;
					int totalPoints = Mathf.CeilToInt(interpolatedPointsDensity * dist);
					if (dist <= 1e-2) {
						//In some cases, ie when SUMO adds an OnRampNode,both nodes are equal
						//Force one point
						totalPoints = 1;
					} 

						for (int j = 0; j < totalPoints; j++) {
							float pm = (float)j / (totalPoints ); //last element go the following section
							Vector3 pos = crspline.InterpolateAtSegment (i, pm);
							Vector3 norm = crspline.NormalAtSegment (i, pm);
							Vector3 tang = crspline.TangentAtSegment (i, pm);
							float curv = crspline.CurvatureAtSegment (i, pm);
							pointList.Add (new PathPointInfo (pos, norm, tang, curv));

						}

				}
				//Add one last point to cover the last node
				int k=nodes.Count-2;
				Vector3 posl = crspline.InterpolateAtSegment (k, 1f);
				Vector3 norml = crspline.NormalAtSegment (k, 1f);
				Vector3 tangl = crspline.TangentAtSegment (k, 1f);
				float curvl = crspline.CurvatureAtSegment (k, 1f);
				pointList.Add (new PathPointInfo (posl, norml, tangl, curvl));

				arcDistances = new float[pointList.Count - 1];
				for (int i = 0; i < pointList.Count-1; i++) {
					arcDistances [i] = (pointList [i].position - pointList [i + 1].position).magnitude;
				}

				interpolatedPath = pointList.ToArray ();




			} else {
				Debug.Log ("Only one node in path");
				return;
			}
		}

		public virtual void InterpolatePath ()
		{
			if (useDensity) {
				InterpolateWithDensity ();
			} else {

				//Points include two control points
				if (points.Length > 3) {

					Spline crspline = new CentripetalCatmullRomSpline (points);

				                 
					int totalPoints = (points.Length - 3) * interpolatedPointsPerSegment + nodes.Count;  ////(Points include two control points, so segments=length-3) segments* interpolatedPointsPerSegment + nodes
					//interpolatedPath=new Vector3[SmoothAmount+1];
					//interpolatedPath[0] = crspline.Interp (0);

					interpolatedPath = new PathPointInfo[totalPoints];
					Vector3 pos = crspline.Interpolate (0);
					Vector3 norm = crspline.Normal (0);
					Vector3 tang = crspline.Tangent (0);
					float curv = crspline.Curvature (0);


					interpolatedPath [0] = new PathPointInfo (pos, norm, tang, curv);
					arcDistances = new float[totalPoints - 1];


					for (int i = 1; i < totalPoints; i++) {
						Debug.Log ("i=" + i);
						float pm = (float)i / (totalPoints - 1); //Last one is t=1
				


						pos = crspline.Interpolate (pm);
						Debug.Log ("pos=" + pos);
						norm = crspline.Normal (pm);
						curv = crspline.Curvature (pm);
						tang = crspline.Tangent (pm);
						interpolatedPath [i] = new PathPointInfo (pos, norm, tang, curv);
						arcDistances [i - 1] = (interpolatedPath [i].position - interpolatedPath [i - 1].position).magnitude;
						//Debug.Log ("curvature[" + i + "]=" + crspline.Curvature (pm));
						//Debug.Log ("normal[" + i + "]=" + crspline.Normal (pm));


					}

				} else {
					Debug.Log ("Only one node in path");
					return;
				}
			}
		}


		public  void DrawInterpolatedPath ()
		{
			if (interpolatedPath.Length > 0) {

				Gizmos.color = Color.black;
				//Gizmos.DrawWireSphere (interpolatedPath [0].position, 1);

				for (int i = 1; i < interpolatedPath.Length; i++) {
					
					Gizmos.DrawLine ( interpolatedPath [i - 1].position, interpolatedPath [i].position);
					//Gizmos.color=Color.green;
					//	Gizmos.DrawWireSphere (interpolatedPath [i].position,1);
						
					//	Gizmos.DrawRay (interpolatedPath [i].position, interpolatedPath [i].normal/interpolatedPath [i].curvature);

				}
			}
		}



	}
}
