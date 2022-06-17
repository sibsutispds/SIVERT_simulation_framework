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
	public class VenerisLane : MonoBehaviour
	{

		public long laneId = 0;
		public string sumoId = "";
		public List<Path> paths;
		private int laneSections = 10;
		public float laneWidth = 3f;
		public float speed = 32f;
		public bool isInternal = false;

		public List<LaneSection> sections = null;

		public IntersectionBehaviourProvider endIntersection = null;

		public List<VehicleInfo> registeredVehiclesList = null;

		protected int vehicleLayer;

		public float occupancy=0f; //This is how it is called in SUMO
		public WeightedAverage averageNumberOfVehicles = null;
		public WeightedAverage averageOccupancy= null;
		public void RegisterVehicleWithLane(VehicleInfo i) {
			//Sample before add, and sample before extract

			averageNumberOfVehicles.CollectWithLastTime(registeredVehiclesList.Count);
			averageOccupancy.CollectWithLastTime(occupancy);

			registeredVehiclesList.Add (i);
			//Update density
			if (i.vehicleLength > paths [0].totalPathLength) {
				occupancy += 1.0f;
			} else {
				occupancy += i.vehicleLength / paths [0].totalPathLength;
			}


		}
		public void UnRegisterVehicleWithLane(VehicleInfo info) {
			//Sample before add, and sample before extract

			averageNumberOfVehicles.CollectWithLastTime(registeredVehiclesList.Count);
			averageOccupancy.CollectWithLastTime(occupancy);

			for (int i = registeredVehiclesList.Count-1; i >=0 ; i--) {
				if (info == registeredVehiclesList [i]) {
					registeredVehiclesList.RemoveAt (i);
					if (info.vehicleLength > paths [0].totalPathLength) {
						occupancy -= 1.0f;
					} else {
						occupancy -= info.vehicleLength / paths [0].totalPathLength;
					}
					break;
				}
				
			}

		}
		public bool IsInternal() {
			return isInternal;
		}

		public void CollectStats() {
			//Force sample
			averageNumberOfVehicles.CollectWithLastTime(registeredVehiclesList.Count);
			averageOccupancy.CollectWithLastTime(occupancy);
		}
		void Awake ()
		{
			//CreateLaneSections ();

			vehicleLayer = LayerMask.NameToLayer ("VehicleTrigger");
		}

		void Start() {
			int capacity = Mathf.CeilToInt (paths [0].totalPathLength / 4f); //If a typical vehicle length is around 5 m,  the capacity is around the max number the lane can hold, it should be enough capacity 
			registeredVehiclesList = new List<VehicleInfo> (capacity);
			CreateLaneSections ();
			averageNumberOfVehicles = new WeightedAverage ();
			averageNumberOfVehicles.Init ();
			averageOccupancy = new WeightedAverage ();
			averageOccupancy.Init ();
		}

		public void AddPath (Path p)
		{
			if (paths == null) {
				paths = new List<Path> ();

			}
			paths.Add (p);
		}

		public void AddSection (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
		{
			if (sections == null) {
				sections = new List<LaneSection> ();
			}
			sections.Add (new  LaneSection (p0, p1, p2, p3));
		}

		public int FindPointInSections (Transform t)
		{
			for (int i = 0; i < sections.Count; i++) {
			
				if (sections [i].PointInSection (transform.InverseTransformPoint (t.position)) != 0) {
					return i;
				}
				
			}
			return -1;
			

		}
		public bool IsOnLane(Transform t) {
			if (FindPointInSections (t) == -1) {
				return false;
			} else {
				return true;
			}
		}


		public void CreateLaneSections ()
		{
			//If already created, sections are replaced
			Path p = GetComponent<Path>();
			if (p == null) {
				
				Debug.Log ("VenerisLane:No path  found to build the lane section" + transform.parent.GetComponent<VenerisRoad> ().name);
				//p = GetComponent<Path> ();
			} else {
				//Debug.Log ("Creating lane sections for" + transform.parent.GetComponent<VenerisRoad> ().name);
				sections = null;
				sections = new List<LaneSection> ();

				if (p.initialized) {
					//Debug.Log (transform.parent.GetComponent<VenerisRoad> ().name + " path is initialized");
					//Use path interpolation
					int totalPoints = p.interpolatedPath.Length; ////(Points include two control points, so segments=length-3) segments* interpolatedPointsPerSegment + nodes
					List<Matrix4x4> matrices = new List<Matrix4x4> ();
					Vector3 left = new Vector3 (-laneWidth / 2, 0.0f, 0.0f);
					Vector3 right = new Vector3 (laneWidth / 2, 0.0f, 0.0f);


					Vector3 pos;

					//Quaternion prevRot = _nodes[0].transform.rotation;

					Quaternion rotation = Quaternion.LookRotation (p.interpolatedPath[0].tangent );

					//_nodes[0].transform.forward=crspline.Tangent(0f);

					matrices.Add (Matrix4x4.TRS (Vector3.zero, rotation, Vector3.one));


					for (int i = 1; i < totalPoints; i++) {




						pos = p.interpolatedPath [i].position;
						rotation = Quaternion.LookRotation (p.interpolatedPath[i].tangent);

						//matrices.Add (Matrix4x4.TRS (_nodes [0].transform.InverseTransformPoint (pos), rotation, Vector3.one));
						matrices.Add (Matrix4x4.TRS (transform.InverseTransformPoint (pos), rotation, Vector3.one));






					}
					Vector3 p0;
					Vector3 p1;
					Vector3 p2;
					Vector3 p3;
					for (int i = 1; i < matrices.Count; i++) {
						//Debug.Log ("i=" + i);
						p0 = matrices [i - 1].MultiplyPoint (left);
						p1 = matrices [i - 1].MultiplyPoint (right);
						p2 = matrices [i].MultiplyPoint (left);
						p3 = matrices [i].MultiplyPoint (right);
						//Interchange p3 and p2
						AddSection (p0, p1, p3, p2);
						//Debug.Log (_nodes[0].transform.TransformPoint(p0));
						//Debug.Log (_nodes[0].transform.TransformPoint(p1));
						//Debug.Log (_nodes[0].transform.TransformPoint(p2));
						//Debug.Log (_nodes[0].transform.TransformPoint(p3));


					}
				
				} else {

					List<GameObject> _nodes = p.GetNodes ();
					if (_nodes.Count >= 2) {
						int totalPoints = (_nodes.Count - 1) * laneSections + _nodes.Count; ////(Points include two control points, so segments=length-3) segments* interpolatedPointsPerSegment + nodes
						List<Matrix4x4> matrices = new List<Matrix4x4> ();
						Spline crspline = new CentripetalCatmullRomSpline (p.PathControlPointGenerator ());
				
						float interval = 1f / (totalPoints - 1);

						Vector3 left = new Vector3 (-laneWidth / 2, 0.0f, 0.0f);
						Vector3 right = new Vector3 (laneWidth / 2, 0.0f, 0.0f);


						Vector3 pos;

						//Quaternion prevRot = _nodes[0].transform.rotation;

						Quaternion rotation = Quaternion.LookRotation (crspline.Tangent (0f));

						//_nodes[0].transform.forward=crspline.Tangent(0f);

						matrices.Add (Matrix4x4.TRS (Vector3.zero, rotation, Vector3.one));


						for (int i = 1; i < totalPoints; i++) {

							float pm = i * interval;
							pos = crspline.Interpolate (pm);



							rotation = Quaternion.LookRotation (crspline.Tangent (pm));

							//matrices.Add (Matrix4x4.TRS (_nodes [0].transform.InverseTransformPoint (pos), rotation, Vector3.one));
							matrices.Add (Matrix4x4.TRS (transform.InverseTransformPoint (pos), rotation, Vector3.one));


				



						}
						Vector3 p0;
						Vector3 p1;
						Vector3 p2;
						Vector3 p3;
						for (int i = 1; i < matrices.Count; i++) {
							//Debug.Log ("i=" + i);
							p0 = matrices [i - 1].MultiplyPoint (left);
							p1 = matrices [i - 1].MultiplyPoint (right);
							p2 = matrices [i].MultiplyPoint (left);
							p3 = matrices [i].MultiplyPoint (right);
							//Interchange p3 and p2
							AddSection (p0, p1, p3, p2);
							//Debug.Log (_nodes[0].transform.TransformPoint(p0));
							//Debug.Log (_nodes[0].transform.TransformPoint(p1));
							//Debug.Log (_nodes[0].transform.TransformPoint(p2));
							//Debug.Log (_nodes[0].transform.TransformPoint(p3));


						}
					}
				}
			}
		}

		void OnDrawGizmosSelected ()
		{
			//if (Application.isPlaying) {
			Path p = GetComponent<Path> ();
			Gizmos.color = Color.red;
			if (sections != null) {
				foreach (LaneSection l in sections) {
					//Debug.Log (l.vertices [0]);
					Gizmos.DrawLine (transform.TransformPoint (l.vertices [0]), transform.TransformPoint (l.vertices [1]));
					Gizmos.DrawLine (transform.TransformPoint (l.vertices [1]), transform.TransformPoint (l.vertices [2]));
					Gizmos.DrawLine (transform.TransformPoint (l.vertices [2]), transform.TransformPoint (l.vertices [3]));
					Gizmos.DrawLine (transform.TransformPoint (l.vertices [3]), transform.TransformPoint (l.vertices [4]));

					//Gizmos.DrawLine (p.nodes [0].transform.TransformPoint (l.vertices [0]), p.nodes [0].transform.TransformPoint (l.vertices [1]));
					//Gizmos.DrawLine (p.nodes [0].transform.TransformPoint (l.vertices [1]), p.nodes [0].transform.TransformPoint (l.vertices [2]));
					//Gizmos.DrawLine (p.nodes [0].transform.TransformPoint (l.vertices [2]), p.nodes [0].transform.TransformPoint (l.vertices [3]));
					//Gizmos.DrawLine (p.nodes [0].transform.TransformPoint (l.vertices [3]), p.nodes [0].transform.TransformPoint (l.vertices [4]));
					//Debug.Log ("tp=" + p.nodes [0].transform.TransformPoint (sections [0].vertices [0]));
				}
			} else {
				Debug.Log ("sections null");
			}
			//}
		}
		void OnTriggerEnter(Collider other) {
			if (other.gameObject.layer == vehicleLayer) {
				RegisterVehicleWithLane (other.gameObject.GetComponentInParent<VehicleInfo>());
				
			}
		}
		void OnTriggerExit(Collider other) {
			if (other.gameObject.layer == vehicleLayer) {
				UnRegisterVehicleWithLane (other.gameObject.GetComponentInParent<VehicleInfo>());
			}
		}
	}

}
