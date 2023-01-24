/******************************************************************************/
// 
// Copyright (c) 2020 Nikita Lyamin nikitavlyamin@gmail.com
// 
/*******************************************************************************/

using UnityEngine;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using Random=UnityEngine.Random;

[RequireComponent (typeof(LineRenderer))]

	public class GSCMreflectors : MonoBehaviour
	{
//		public GameObject ReflectorPrefab = (GameObject)Resources.Load("Prefabs/reflector", typeof(GameObject));
		

//		public GameObject ReflectorPrefab = GameObject.Find("reflctor");
		
		//this game object's Transform
		private Transform goTransform;

		//the attached line renderer
		private LineRenderer lineRenderer;

		private CapsuleCollider GSCMCollider;



		//a ray
		private Ray ray;

		private int nReflections = 1;

		private float MaxDistance = 300;

		public LayerMask layermask;

		public RaycastHit GSCMHit;

//	public Vector3 direction;

		private Vector3 origin;

		public RaycastHit hit, direct;
		// Use this for initialization

		//the number of points at the line renderer
		private int numPoints;
		//private int pointCount;

		private const int cast_directions = 360 * 10;


		private Vector3[] all_direct;


		void Awake()
		{
			//get the attached Transform component  
			goTransform = this.GetComponent<Transform>();
			//get the attached LineRenderer component  
			lineRenderer = this.GetComponent<LineRenderer>();
			GSCMCollider = this.GetComponent<CapsuleCollider>();
			lineRenderer.startWidth = 0.0001f;
			lineRenderer.endWidth = 0.0001f;
			lineRenderer.startColor = Color.yellow;
			lineRenderer.endColor = Color.green;

		}

		void Start()
		{
//		direction = transform.forward;
//		origin = transform.position;
		}

		// Update is called once per frame
		void Update()
		{
			foreach (Transform t in goTransform) {
				Destroy(t.gameObject);
			}
//			Stack meshesHit = new Stack();
			Stack<string> meshesHit = new Stack<string>();
//			Dictionary<string, int> meshesHit = new Dictionary<string, int>();
			all_direct = GetSphereDirections(cast_directions);
			foreach (var direction in GetSphereDirections(cast_directions))
			{

//				Debug.DrawRay(transform.position, direction * 3, Color.black);


//				for (int i = 0; i < nReflections; i++)
//				{

//					int iter_outer = i;
				if (Physics.Raycast(goTransform.position, direction, out hit, MaxDistance))
				{


					MeshCollider meshCollider = hit.collider as MeshCollider;


//					if (meshCollider != null && meshCollider.sharedMesh != null &&
//					    !meshesHit.Contains(meshCollider.name))

					if ((meshCollider != null) && (meshCollider.sharedMesh != null))
					{
						string entry = meshCollider.name + hit.triangleIndex;


						if (!(meshesHit.Contains(entry)))
							//					if (meshCollider != null && meshCollider.sharedMesh != null)
						{


							Mesh mesh = meshCollider.sharedMesh;
							Vector3[] vertices = mesh.vertices;
							int[] triangles = mesh.triangles;
							Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
							Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
							Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];
							Transform hitTransform = hit.collider.transform;
							p0 = hitTransform.TransformPoint(p0);
							p1 = hitTransform.TransformPoint(p1);
							p2 = hitTransform.TransformPoint(p2);
							//						Debug.DrawLine(p0, p1, Color.blue);
							//						Debug.DrawLine(p1, p2, Color.blue);
							//						Debug.DrawLine(p2, p0, Color.blue);

							// point on triangle
							float r = Random.value;
							float s = Random.value;

							if (r + s >= 1)
							{
								r = 1 - r;
								s = 1 - s;
							}

							//and then turn them back to a Vector3
							Vector3 pointOnMesh = p0 + r * (p1 - p0) + s * (p2 - p0);
//							Vector3 offset = new Vector3(0, 1, 0);
//							Debug.DrawLine(pointOnMesh, pointOnMesh, Color.yellow);
//							Debug.DrawLine(goTransform.position, hit.point, Color.cyan);
							
							GameObject reflector = (GameObject)Instantiate(Resources.Load("Prefabs/reflector"), pointOnMesh, Quaternion.identity);
							reflector.transform.parent = goTransform;
							Debug.DrawLine(goTransform.position, pointOnMesh, Color.cyan);
							
							meshesHit.Push(entry);
						}
						
					}
				}
			}
		}






		// update remaining length and set up ray for next loop
//						remainingLength -= Vector3.Distance(ray.origin, hit.point);
//
//						nodes[i + 1] = hit.point;
//						ray = new Ray(hit.point, Vector3.Reflect(ray.direction, hit.normal));
//						if (hit.collider.tag == "GSCMAntenna" &&
//						    Vector3.SqrMagnitude(hit.point - goTransform.position) > GSCMCollider.radius)
//						{
//							for (int j = 0; j < nReflections + 1; j++)
//							{
//								if (nodes[j + 1].Equals(Vector3.zero))
//								{
//									if (j == 1)
//									{
//										Debug.DrawLine(nodes[j - 1], nodes[j], Color.red);
//									}
//
//									break;
//								}
//								else
//								{
//									Debug.DrawLine(nodes[j], nodes[j + 1], Color.green);
//								}
//							}
//
//							break;
//						}

//						if (remainingLength <= 0)
//						{
//							break;
//							// break loop if we don't hit a antenna
//						}


//					}
//					else
//					{
//						// We didn't hit anything. exit loop
//
//						break;
//					}

	
		


		private Vector3[] GetSphereDirections(int numDirections)
		{
			var pts = new Vector3[numDirections];
			var inc = Math.PI * (3 - Math.Sqrt(5));
			var off = 2f / numDirections;

			foreach (var k in Enumerable.Range(0, numDirections))
			{
//			var y = k * off - 1 + (off / 2); // shoot in all directions
				var y = 0; // shoot plain around antenna
				var r = Math.Sqrt(1 - y * y);
//			var phi = k * inc;
				var phi = (2 * Math.PI / numDirections) * k; // evenly divide the circle into regions
				var x = (float) (Math.Cos(phi) * r);
				var z = (float) (Math.Sin(phi) * r);
				pts[k] = new Vector3(x, y, z);
			}

			return pts;
		}

	}

