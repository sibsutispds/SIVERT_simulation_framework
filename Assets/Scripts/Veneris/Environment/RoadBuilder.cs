/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Veneris
{

	[AddComponentMenu ("Veneris/RoadBuilder")]
	[RequireComponent (typeof(NodePathHelper))]
	public class RoadBuilder : MonoBehaviour
	{


		public int roadId = 1;

		//Dimensions
		public float laneWidth = 2.5f;
		public float roadHeight = 0f;
		public int meshSections = 10;
		public float shoulderWidth = 2f;
		public float totalWidth = 0f;
		public bool addShoulders = false;
		public float laneSpeed = 32f;
		public VenerisRoad.Ways ways = VenerisRoad.Ways.TwoWay;
		public int lanesPerWay = 1;
		public List<long> forwardLanes = null;
		public List<long> backwardLanes = null;
		public bool flattenTerrain = false;
		public int laneLayer;
		public enum Direction
		{
Forward,
			Backward}

		;

		public enum SpreadType
		{
Center,
			Right}

		;

		public enum RelativePosition
		{
			Right,
			Left

		}

		public class CustomLane
		{
			public Vector3[] nodes;
			public long id;
			public float width;
			public float speed;

			public CustomLane (Vector3[] nodes, long id, float width, float speed)
			{
				this.nodes = nodes;
				this.id = id;
				this.width = width;
				this.speed = speed;
			}
		};

		public List<CustomLane> customLanes = null;


		public SpreadType spread = SpreadType.Right;
		private int totalLanes = 0;

		void Reset ()
		{
			roadId = 1;
			laneWidth = 2.5f;
			roadHeight = 0f;
			meshSections = 10;
			shoulderWidth = 2f;
			addShoulders = false;
			ways = VenerisRoad.Ways.TwoWay;
			lanesPerWay = 1;

			flattenTerrain = false;
		}


		public GameObject Build ()
		{
			List<GameObject> nodes = GetComponent<NodePathHelper> ().GetNodes ();

			if (nodes.Count < 1) {
				Debug.Log ("We need at least two nodes to build a road");
				return null;
			}

			GameObject roadObject = CreateMainObject ();
			roadObject.transform.position = nodes [0].transform.position;

	

			Path p = roadObject.AddComponent<Path> () as Path;
			//Path p = roadObject.GetComponent<Path> ();
			p.SetNodes (nodes);
			p.pathName = "Director Road Path";
			p.interpolatedPointsPerSegment = GetComponent<NodePathHelper> ().interpolatedPointsPerSegment;
			VenerisRoad vr = roadObject.AddComponent<VenerisRoad> () as VenerisRoad;
			vr.roadId = roadId;
			vr.directorRoadPath = p;
			vr.ways = ways;
			BuildLanes (vr);


			createMesh (roadObject);
			roadObject.AddComponent<MeshCollider> ();
		
			if (Application.isPlaying) {
				//We have to initialize the path if this road has been created during execution because Path::Awake has been called at AddComponent
				p.InitPathStructures ();

			}
			return roadObject;
	

		}

		public GameObject BuildWithCustomLanes (string rname = "")
		{
			GameObject roadObject = CreateMainObject (rname);
			VenerisRoad vr = roadObject.AddComponent<VenerisRoad> ();
			vr.roadId = roadId;
			laneLayer = LayerMask.NameToLayer ("Lane");
			/*List<GameObject> nodes = GetComponent<NodePathHelper> ().GetNodes ();
			if (nodes != null) {
				Path pr = roadObject.AddComponent<Path> ();
				//Path p = roadObject.GetComponent<Path> ();
				pr.SetNodes (nodes);
				pr.pathName = "Director Road Path";
				vr.roadId = roadId;
				vr.directorRoadPath = pr;
			}*/

			vr.ways = ways;
			List<VenerisLane> lanes = new List<VenerisLane> ();
			foreach (CustomLane cl in customLanes) {
				GameObject go = new GameObject ("Lane " + cl.id);
				go.tag = "Lane";
				go.layer = laneLayer;
				go.transform.position = cl.nodes [0];
				go.transform.parent = roadObject.transform;
				go.isStatic = true;
				Path p = go.AddComponent<Path> ();
				p.SetNodes (cl.nodes);

				p.BindToTerrain ();

				//Create lane and sections
				VenerisLane lane = go.AddComponent<VenerisLane> ();
				lane.laneId = cl.id;
				lane.laneWidth = laneWidth;
				lane.CreateLaneSections ();
				lane.speed = cl.speed;
				if (Application.isPlaying) {
					p.InitPathStructures ();
				}
				lane.AddPath (p);
				AddCustomLaneMesh (go);
				lanes.Add (lane);
			}
			vr.lanes = lanes.ToArray ();
			return roadObject;
		}

		public GameObject CreateMainObject (string rname = null)
		{
			GameObject roadObject = new GameObject ("Road " + rname);
			roadObject.tag = "Road";
			roadObject.isStatic = true;
			return roadObject;
		}

		public void AddCustomLane (Vector3[] n, long i, float w, float s)
		{
			if (customLanes == null) {
				customLanes = new List<CustomLane> ();
			}

			customLanes.Add (new CustomLane (n, i, w, s));
		}

		public void AddForwardLane (long id)
		{
			if (forwardLanes == null) {
				forwardLanes = new List<long> ();
			}
			forwardLanes.Add (id);
			totalLanes = totalLanes + 1;
		}

		public void AddBackwardLane (long id)
		{
			if (backwardLanes == null) {
				backwardLanes = new List<long> ();
			}
			backwardLanes.Add (id);
			totalLanes = totalLanes + 1;
		}

		public void BuildLanes (VenerisRoad road)
		{
			
		
			if (totalLanes == 0) {
				totalLanes = ((int)ways + 1) * lanesPerWay;
				road.lanes = BuildDefaultLanes (road);
			} else {
				List<VenerisLane> lanes = new List<VenerisLane> ();
				float baseDistance = 0.5f * laneWidth;
				switch (road.ways) {
				case (VenerisRoad.Ways.OneWay):
				//Only Create forward or backward ways
					if (forwardLanes != null) {
						switch (spread) {
						case (SpreadType.Right):
							//All lanes go on the right of the director path
							for (int i = 0; i < forwardLanes.Count; i++) {
								lanes.Add (CreateBasicLane (road, baseDistance + (i * laneWidth), Direction.Forward, RelativePosition.Right, forwardLanes [i]));
							}
							break;
						case (SpreadType.Center):
							//Lanes alternatively on the right and left
							//
							if (totalLanes == 1) {
								//If one line, the path coincides with the director path
								baseDistance = 0f;
							}
							int j = 0;
							for (int i = 0; i < forwardLanes.Count; i++) {
								if (i % 2 == 0) {
									lanes.Add (CreateBasicLane (road, baseDistance + (j * laneWidth), Direction.Forward, RelativePosition.Right, forwardLanes [i]));

								} else {
									lanes.Add (CreateBasicLane (road, baseDistance + (j * laneWidth), Direction.Forward, RelativePosition.Left, forwardLanes [i]));
									j = j + 1;
								}

							}
							break;
						}

					} else {
						if (backwardLanes != null) {
							
							switch (spread) {
							case (SpreadType.Right):
								//All lanes go on the right of the director path
								for (int i = 0; i < backwardLanes.Count; i++) {
									lanes.Add (CreateBasicLane (road, baseDistance + (i * laneWidth), Direction.Backward, RelativePosition.Right, backwardLanes [i]));
								}
								break;
							case (SpreadType.Center):
								//Lanes alternatively on the right and left
								if (totalLanes == 1) {
									//If one line, the path coincides with the director path
									baseDistance = 0f;
								}
								int j = 0;
								for (int i = 0; i < backwardLanes.Count; i++) {
									if (i % 2 == 0) {
										lanes.Add (CreateBasicLane (road, baseDistance + (j * laneWidth), Direction.Backward, RelativePosition.Right, backwardLanes [i]));

									} else {
										lanes.Add (CreateBasicLane (road, baseDistance + (j * laneWidth), Direction.Backward, RelativePosition.Left, backwardLanes [i]));
										j = j + 1;
									}

								}
								break;
							}
						}
					}
					break;
				case (VenerisRoad.Ways.TwoWay):
					switch (spread) {
					case (SpreadType.Right):
						//All lanes go on the right of the director path
						//Start with left lanes

						if (backwardLanes != null) {
							for (int i = 0; i < backwardLanes.Count; i++) {
								lanes.Add (CreateBasicLane (road, baseDistance + (i * laneWidth), Direction.Backward, RelativePosition.Right, backwardLanes [i]));
								 
							}
						}
						float startdistance = backwardLanes.Count * laneWidth;
						if (forwardLanes != null) {
							for (int i = 0; i < forwardLanes.Count; i++) {
								lanes.Add (CreateBasicLane (road, startdistance + baseDistance + (i * laneWidth), Direction.Forward, RelativePosition.Right, forwardLanes [i]));
							}
						}
						break;
					case (SpreadType.Center):
						//Backward lanes on the left and forward on the right
						if (forwardLanes != null) {
							lanes.AddRange (BuildRightForwardLanes (road));
						}
						if (backwardLanes != null) {
							lanes.AddRange (BuildLeftBackwardLanes (road));
						}
						break;
					}
				
					break;
				}

				road.lanes = lanes.ToArray ();



			}

		}


		public VenerisLane[] BuildRightForwardLanes (VenerisRoad road)
		{
			float baseDistance = 0.5f * laneWidth;
			if (totalLanes == 1) {
				baseDistance = 0f;
			}
			VenerisLane[] lanes = new VenerisLane[forwardLanes.Count];

			for (int i = 0; i < lanes.Length; i++) {
				lanes [i] = CreateBasicLane (road, baseDistance + (i * laneWidth), Direction.Forward, RelativePosition.Right, forwardLanes [i]);
			}
			return lanes;
		}

		public VenerisLane[] BuildLeftBackwardLanes (VenerisRoad road)
		{
			float baseDistance = 0.5f * laneWidth;
			if (totalLanes == 1) {
				baseDistance = 0f;
			}
			VenerisLane[] lanes = new VenerisLane[backwardLanes.Count];

			for (int i = 0; i < lanes.Length; i++) {
				lanes [i] = CreateBasicLane (road, baseDistance + (i * laneWidth), Direction.Backward, RelativePosition.Left, backwardLanes [i]);

			}
			return lanes;
		}

		public VenerisLane CreateBasicLane (VenerisRoad road, float offset, Direction d, RelativePosition rel, long id)
		{
			
			GameObject go = new GameObject ("Lane " + id);
			go.tag = "Lane";
			go.transform.parent = road.transform;
			go.isStatic = true;
			VenerisLane lane = go.AddComponent<VenerisLane> ();
			lane.laneId = id;
			lane.laneWidth = laneWidth;
			lane.speed = laneSpeed;
			Path p = go.AddComponent<Path> ();
			p.interpolatedPointsPerSegment = road.directorRoadPath.interpolatedPointsPerSegment;
			Spline spline = new CentripetalCatmullRomSpline (road.directorRoadPath.PathControlPointGenerator ());
			//p.transform.parent=road.transform;
			float[] f = spline.GetParameterFractionAtPoints ();
			List<GameObject> pathNodes = road.directorRoadPath.GetNodes ();

			//To get the correct location, we align the transform of the nodes with the tangent, then we set the lanes to the right or left 	
			for (int j = 0; j < pathNodes.Count; j++) {

				Quaternion prevRot = pathNodes [j].transform.rotation;
				Vector3 n;
				pathNodes [j].transform.forward = spline.Tangent (f [j]);
				if (rel == RelativePosition.Right) {
					n = pathNodes [j].transform.position + pathNodes [j].transform.right * offset;
				} else {
					
					n = pathNodes [j].transform.position + pathNodes [j].transform.right * (-1.0f * offset);
				}
				if (j == 0) {
					go.transform.position = n;
				}
				p.AddNode (n);
				//Set back the previous rotation of the node, to keep the terrain orientation
				pathNodes [j].transform.rotation = prevRot;
			}
			lane.CreateLaneSections ();
			if (d == Direction.Backward) {
				p.ReversePath ();
			}
			if (Application.isPlaying) {
				p.InitPathStructures ();
			}
			lane.AddPath (p);
			return lane;
		}

		public VenerisLane[] BuildDefaultLanes (VenerisRoad road)
		{
		
			float baseDistance = 0.5f * laneWidth;
			if (totalLanes == 1) {
				baseDistance = 0f;
			}

			VenerisLane[] lanes = new VenerisLane[totalLanes];

			//CatmullRomSpline spline = new CatmullRomSpline (road.directorRoadPath.PathControlPointGenerator ());
			Debug.Log ("tl=" + totalLanes);
			for (int i = 0; i < totalLanes; i++) {
				VenerisLane lane = null;
				if (i < lanesPerWay) {
					lane = CreateBasicLane (road, baseDistance + i * laneWidth, Direction.Forward, RelativePosition.Right, i);
				} else {
					lane = CreateBasicLane (road, baseDistance + ((i - lanesPerWay) * laneWidth), Direction.Backward, RelativePosition.Left, i);

				}


				/*GameObject go = new GameObject ("Lane " + i);
				go.transform.parent = road.transform;
				VenerisLane lane = go.AddComponent<VenerisLane> ();
				lane.laneId = i;
				lane.laneWidth = laneWidth;
				Path p = go.AddComponent<Path> ();
				p.interpolatedPointsPerSegment = road.directorRoadPath.interpolatedPointsPerSegment;

				//p.transform.parent=road.transform;
				float[] f = spline.GetParameterFractionAtPoints ();
				List<GameObject> pathNodes = road.directorRoadPath.GetNodes ();


				//To get the correct location, we align the transform of the nodes with the tangent, then we set the lanes to the right or left 	
			   for (int j = 0; j < pathNodes.Count; j++) {

				Quaternion prevRot = pathNodes [j].transform.rotation;
				Vector3 n;

				if (i < lanesPerWay) {
						//Right lanes
					pathNodes[j].transform.forward=spline.Tangent(f[j]);
					 n = pathNodes [j].transform.position+ pathNodes [j].transform.right * (baseDistance + (i * laneWidth));



				} else {
					//left lanes
					pathNodes[j].transform.forward=spline.Tangent(f[j]);
					n = pathNodes [j].transform.position+ pathNodes [j].transform.right *(-1.0f*(baseDistance + ((i-lanesPerWay) * laneWidth)));



				}
				if (j==0) {
					go.transform.position = n;
				}
				p.AddNode(n);
				//Set back the previous rotation of the node, to keep the terrain orientation
				pathNodes [j].transform.rotation = prevRot;
			}



			lane.CreateLaneSections ();
				if (ways==VenerisRoad.Ways.TwoWay && i >= lanesPerWay) {
					p.ReversePath ();
				}
				if (Application.isPlaying) {
					p.InitPathStructures ();
				}
				lane.AddPath (p);
				*/
				lanes [i] = lane;


			}
			return lanes;
					
		}

		private Mesh CreateBasicMesh (float width, float height)
		{

			Mesh m = new Mesh ();
			m.name = "RoadMesh";

			m.vertices = new Vector3[] {
				//new Vector3(-width,0f, -height),
				//new Vector3(width, 0f,-height),
				//new Vector3(width,0f,height),
				//new Vector3(-width,0f, height)

				new Vector3 (-width, -height, 0f),
				new Vector3 (width, -height, 0f),
				new Vector3 (width, height, 0f),
				new Vector3 (-width, height, 0f)
			};
			m.uv = new Vector2[] {
				//new Vector2 (0, 0),
				//new Vector2 (0, 1),
				//new Vector2 (1, 1),
				//new Vector2 (1, 0)

				new Vector2 (0, 0),
				new Vector2 (1, 0),
				new Vector2 (1, 1),
				new Vector2 (0, 1)
			};
			m.triangles = new int[] { 0, 2, 1, 0, 3, 2 };


			m.RecalculateNormals ();




			return m;
		}

	
		public  void CreateCustomLaneMesh(GameObject lane, bool addRenderer) {
			List<GameObject> _nodes = lane.GetComponent<NodePathHelper> ().GetNodes ();
			if (_nodes.Count >= 2) {
				int totalPoints = (_nodes.Count - 1) * meshSections + _nodes.Count; ////(Points include two control points, so segments=length-3) segments* interpolatedPointsPerSegment + nodes
				List<Matrix4x4> sections = new List<Matrix4x4> ();
				Spline crspline = new CentripetalCatmullRomSpline (lane.GetComponent<NodePathHelper> ().PathControlPointGenerator ());

				float interval = 1f / (totalPoints - 1);
				//float totalWidth = ((int) ways +1)*lanesPerWay*laneWidth;
				totalWidth = lane.GetComponent<VenerisLane> ().laneWidth;

				//	Debug.Log ("Custom Lane Mesh: totalWidth=" + totalWidth);
				Mesh basicSegment = CreateBasicMesh (totalWidth * 0.5f, roadHeight);

				Vector3 pos;

				Quaternion rotation = Quaternion.LookRotation (crspline.Tangent (0f));



				sections.Add (Matrix4x4.TRS (Vector3.zero, rotation, Vector3.one));


				for (int i = 1; i < totalPoints; i++) {

					float pm = i * interval;
					pos = crspline.Interpolate (pm);

					//This translation matrix moves our transform to the next point in the path
					//Recall that a matrix m= Matrix4x4.TRS( new Vector(10,20,5),Quaternion.lookRotation(tangent),Vector3.one) makes that any point p (Vector) multiplied by it (p*m), gets translated x+10 y+20 and z+5
					//So, we have to convert first the next point to local space


					rotation = Quaternion.LookRotation (crspline.Tangent (pm));

					sections.Add (Matrix4x4.TRS (lane.transform.InverseTransformPoint (pos), rotation, Vector3.one));


					//Alternatively, we can first convert our transform to localMatrix and then multiply by the TRS with the next point directly
					//sections[i]=wtl*Matrix4x4.TRS(pos,Quaternion.identity,Vector3.one);



				}
				Mesh m = new Mesh ();


				MeshExtrusion.ExtrudeMesh (basicSegment, m, sections.ToArray (), false);

				lane.AddComponent<MeshFilter> ().mesh = m;
				if (addRenderer) {
					lane.AddComponent<MeshRenderer> ();
					Material mat = null;
					#if UNITY_EDITOR
					mat = new Material (AssetDatabase.LoadAssetAtPath<Material> ("Assets/Resources/Materials/Asphalt.mat"));
					#endif
					if (Application.isPlaying) {
						mat = Resources.Load ("Materials/Asphalt") as Material;
					}

					lane.GetComponent<MeshRenderer> ().material = mat;
				}

				if (flattenTerrain) {
					MeshExtrusion.Edge[] edges = new MeshExtrusion.Edge[1];
					edges [0] = new MeshExtrusion.Edge ();
					edges [0].vertexIndex [0] = 0;
					edges [0].vertexIndex [0] = 1;
					FlattenTerrain (basicSegment, sections.ToArray (), lane.transform, edges);
				}
			} else {
				Debug.Log ("Not enough nodes to build the road");
			}
			lane.AddComponent<MeshCollider> ();
		}



		private void AddCustomLaneMesh (GameObject lane)
		{
			List<GameObject> _nodes = lane.GetComponent<NodePathHelper> ().GetNodes ();
			if (_nodes.Count >= 2) {
				int totalPoints = (_nodes.Count - 1) * meshSections + _nodes.Count; ////(Points include two control points, so segments=length-3) segments* interpolatedPointsPerSegment + nodes
				List<Matrix4x4> sections = new List<Matrix4x4> ();
				Spline crspline = new CentripetalCatmullRomSpline (lane.GetComponent<NodePathHelper> ().PathControlPointGenerator ());

				float interval = 1f / (totalPoints - 1);
				//float totalWidth = ((int) ways +1)*lanesPerWay*laneWidth;
				totalWidth = lane.GetComponent<VenerisLane> ().laneWidth;

				//	Debug.Log ("Custom Lane Mesh: totalWidth=" + totalWidth);
				Mesh basicSegment = CreateBasicMesh (totalWidth * 0.5f, roadHeight);

				Vector3 pos;

				Quaternion rotation = Quaternion.LookRotation (crspline.Tangent (0f));



				sections.Add (Matrix4x4.TRS (Vector3.zero, rotation, Vector3.one));


				for (int i = 1; i < totalPoints; i++) {

					float pm = i * interval;
					pos = crspline.Interpolate (pm);

					//This translation matrix moves our transform to the next point in the path
					//Recall that a matrix m= Matrix4x4.TRS( new Vector(10,20,5),Quaternion.lookRotation(tangent),Vector3.one) makes that any point p (Vector) multiplied by it (p*m), gets translated x+10 y+20 and z+5
					//So, we have to convert first the next point to local space


					rotation = Quaternion.LookRotation (crspline.Tangent (pm));

					sections.Add (Matrix4x4.TRS (lane.transform.InverseTransformPoint (pos), rotation, Vector3.one));


					//Alternatively, we can first convert our transform to localMatrix and then multiply by the TRS with the next point directly
					//sections[i]=wtl*Matrix4x4.TRS(pos,Quaternion.identity,Vector3.one);



				}
				Mesh m = new Mesh ();


				MeshExtrusion.ExtrudeMesh (basicSegment, m, sections.ToArray (), false);

				lane.AddComponent<MeshFilter> ().mesh = m;
				lane.AddComponent<MeshRenderer> ();
				Material mat = null;
				#if UNITY_EDITOR
				mat = new Material (AssetDatabase.LoadAssetAtPath<Material> ("Assets/Resources/Materials/Asphalt.mat"));
				#endif
				if (Application.isPlaying) {
					mat = Resources.Load ("Materials/Asphalt") as Material;
				}

				lane.GetComponent<MeshRenderer> ().material = mat;


				if (flattenTerrain) {
					MeshExtrusion.Edge[] edges = new MeshExtrusion.Edge[1];
					edges [0] = new MeshExtrusion.Edge ();
					edges [0].vertexIndex [0] = 0;
					edges [0].vertexIndex [0] = 1;
					FlattenTerrain (basicSegment, sections.ToArray (), lane.transform, edges);
				}
			} else {
				Debug.Log ("Not enough nodes to build the road");
			}
			lane.AddComponent<MeshCollider> ();
		}

		private void createMesh (GameObject road)
		{
		
			List<GameObject> _nodes = GetComponent<NodePathHelper> ().GetNodes ();
			if (_nodes.Count >= 2) {
				int totalPoints = (_nodes.Count - 1) * meshSections + _nodes.Count; ////(Points include two control points, so segments=length-3) segments* interpolatedPointsPerSegment + nodes
				List<Matrix4x4> sections = new List<Matrix4x4> ();
				Spline crspline = new CentripetalCatmullRomSpline (GetComponent<NodePathHelper> ().PathControlPointGenerator ());
			
				float interval = 1f / (totalPoints - 1);
				//float totalWidth = ((int) ways +1)*lanesPerWay*laneWidth;
				totalWidth = totalLanes * laneWidth;
				if (addShoulders) {
					totalWidth += shoulderWidth * 2f;
				}
			
				Debug.Log ("totalWidth=" + totalWidth);
				Mesh basicSegment = CreateBasicMesh (totalWidth * 0.5f, roadHeight);
		
				Vector3 pos;

				Quaternion rotation = Quaternion.LookRotation (crspline.Tangent (0f));



				sections.Add (Matrix4x4.TRS (Vector3.zero, rotation, Vector3.one));
		
		
				for (int i = 1; i < totalPoints; i++) {
				
					float pm = i * interval;
					pos = crspline.Interpolate (pm);

					//This translation matrix moves our transform to the next point in the path
					//Recall that a matrix m= Matrix4x4.TRS( new Vector(10,20,5),Quaternion.lookRotation(tangent),Vector3.one) makes that any point p (Vector) multiplied by it (p*m), gets translated x+10 y+20 and z+5
					//So, we have to convert first the next point to local space


					rotation = Quaternion.LookRotation (crspline.Tangent (pm));
			
					sections.Add (Matrix4x4.TRS (road.transform.InverseTransformPoint (pos), rotation, Vector3.one));


					//Alternatively, we can first convert our transform to localMatrix and then multiply by the TRS with the next point directly
					//sections[i]=wtl*Matrix4x4.TRS(pos,Quaternion.identity,Vector3.one);
					


				}
				Mesh m = new Mesh ();


				MeshExtrusion.ExtrudeMesh (basicSegment, m, sections.ToArray (), false);

				road.AddComponent<MeshFilter> ().mesh = m;
				road.AddComponent<MeshRenderer> ();
				Material mat = null;
				#if UNITY_EDITOR
				mat = new Material (AssetDatabase.LoadAssetAtPath<Material> ("Assets/Resources/Materials/Asphalt.mat"));
				#endif
				if (Application.isPlaying) {
					mat = Resources.Load ("Materials/Asphalt") as Material;
				}

				road.GetComponent<MeshRenderer> ().material = mat;


				if (flattenTerrain) {
					MeshExtrusion.Edge[] edges = new MeshExtrusion.Edge[1];
					edges [0] = new MeshExtrusion.Edge ();
					edges [0].vertexIndex [0] = 0;
					edges [0].vertexIndex [0] = 1;
					FlattenTerrain (basicSegment, sections.ToArray (), road.transform, edges);
				}
			} else {
				Debug.Log ("Not enough nodes to build the road");
			}
		}
		//TODO
		private void FlattenTerrain (Mesh srcMesh, Matrix4x4[] extrusion, Transform road, MeshExtrusion.Edge[] baseEdges)
		{
			//baseEdges are the edges corresponding to the base of the basicMesh, that is, the one that will be in contact with the terrain.
			//Adapted from MeshExtrusion.cs
			Vector3[] inputVertices = srcMesh.vertices;
			Vector3 p1;
			Vector3 p2;
			Vector3 p3;
			Vector3 p4;
			int hmW = Terrain.activeTerrain.terrainData.heightmapResolution;
			int hmH = Terrain.activeTerrain.terrainData.heightmapResolution;
	
			float offset = 0.5f;
			float[,] heights = null;
			//float[,] heights = Terrain.activeTerrain.terrainData.GetHeights (0, 0, hmW, hmH);
			for (int i = 1; i < extrusion.Length; i++) {
				Matrix4x4 matrix = extrusion [i];
				Matrix4x4 matrixPrev = extrusion [i - 1];
				foreach (MeshExtrusion.Edge e in baseEdges) {
					//We need to get the four points (per edge) that define this section. 
					//Then, we compute the rectangle that comprises this section: find corners x and z (y) coordinates 
					//Get the terrain heights float[,] TerrainData.GetHeights().
					//Iterate all the i,j of that matrix and if the point (i,j) is inside our mesh section, change the height. Otherwise, do not change.
					//Set new heights. 
					//Should be slow, do in another thread.

					//Find the corner points

					Vector3 rightC = new Vector3 (float.MinValue, 0.0f, float.MinValue);
					Vector3 leftC = new Vector3 (float.MaxValue, 0.0f, float.MaxValue);
					float maxTerrainHeight = float.MaxValue;
					p1 = matrixPrev.MultiplyPoint (inputVertices [e.vertexIndex [0]]);
					leftC = leftCorner (p1, leftC);
					rightC = rightCorner (p1, rightC);
					if (road.TransformPoint (p1).y < maxTerrainHeight) {
						maxTerrainHeight = road.TransformPoint (p1).y;
					}
					p2 = matrixPrev.MultiplyPoint (inputVertices [e.vertexIndex [1]]);
					leftC = leftCorner (p2, leftC);
					rightC = rightCorner (p2, rightC);
					if (road.TransformPoint (p2).y < maxTerrainHeight) {
						maxTerrainHeight = road.TransformPoint (p2).y;
					}
					//New vertices corresponding to this section. 
					p3 = matrix.MultiplyPoint (inputVertices [e.vertexIndex [0]]);
					leftC = leftCorner (p3, leftC);
					rightC = rightCorner (p3, rightC);
					if (road.TransformPoint (p3).y < maxTerrainHeight) {
						maxTerrainHeight = road.TransformPoint (p3).y;
					}
					p4 = matrix.MultiplyPoint (inputVertices [e.vertexIndex [1]]);
					leftC = leftCorner (p4, leftC);
					rightC = rightCorner (p4, rightC);
					if (road.TransformPoint (p4).y < maxTerrainHeight) {
						maxTerrainHeight = road.TransformPoint (p4).y;
					}

	



					//Convert to terrain coordinates
					Vector3 terrainLocalPos = road.TransformPoint (leftC) - Terrain.activeTerrain.transform.position;

					float x = Mathf.InverseLerp (0.0f, Terrain.activeTerrain.terrainData.size.x, terrainLocalPos.x);
					float y = Mathf.InverseLerp (0.0f, Terrain.activeTerrain.terrainData.size.z, terrainLocalPos.z);


					int xBase = (int)(x * hmW);
					int yBase = (int)(y * hmH);
			
					//Convert to terrain coordinates
					terrainLocalPos = road.TransformPoint (rightC) - Terrain.activeTerrain.transform.position;
					x = Mathf.InverseLerp (0.0f, Terrain.activeTerrain.terrainData.size.x, terrainLocalPos.x);
					y = Mathf.InverseLerp (0.0f, Terrain.activeTerrain.terrainData.size.z, terrainLocalPos.z);
					int xTop = (int)(x * hmW);
					int yTop = (int)(y * hmH);


					int height = (yTop - yBase) > 0 ? (yTop - yBase) : 1;
					int width = (xTop - xBase) > 0 ? (xTop - xBase) : 1;


					heights = Terrain.activeTerrain.terrainData.GetHeights (xBase, yBase, width, height);

					//Debug.Log(heights.GetLength(0));
					//Debug.Log(heights.GetLength(1));
					for (int k = 0; k < heights.GetLength (0); k++) {
						for (int j = 0; j < heights.GetLength (1); j++) {
						

							if (heights [k, j] * Terrain.activeTerrain.terrainData.size.y > maxTerrainHeight) {
								heights [k, j] = (maxTerrainHeight - offset) / Terrain.activeTerrain.terrainData.size.y;

							}
						}
					}
					Terrain.activeTerrain.terrainData.SetHeightsDelayLOD (xBase, yBase, heights);



				}
			}
			Terrain.activeTerrain.ApplyDelayedHeightmapModification ();

	
		}

		private Vector3 leftCorner (Vector3 p, Vector3 left)
		{
			if (p.x <= left.x) {
				left.x = p.x;
			}
			if (p.z <= left.z) {
				left.z = p.z;
			}
			return left;
		}

		private Vector3 rightCorner (Vector3 p, Vector3 right)
		{
			if (p.x >= right.x) {
				right.x = p.x;
			}
			if (p.z >= right.z) {
				right.z = p.z;
			}
			return right;
		}


		// Copyright 2000 softSurfer, 2012 Dan Sunday
		// This code may be freely used and modified for any purpose
		// providing that this copyright notice is included with it.
		// SoftSurfer makes no warranty for this code, and cannot be held
		// liable for any real or imagined damage resulting from its use.
		// Users of this code must verify correctness for their application.
		//http://geomalgorithms.com/a03-_inclusion.html


		// a Point is defined by its coordinates {int x, y;}
		//===================================================================


		// isLeft(): tests if a point is Left|On|Right of an infinite line.
		//    Input:  three points P0, P1, and P2
		//    Return: >0 for P2 left of the line through P0 and P1
		//            =0 for P2  on the line
		//            <0 for P2  right of the line
		//    See: Algorithm 1 "Area of Triangles and Polygons"
		private float	isLeft (Vector3 P0, Vector3 P1, Vector3 P2)
		{
			return ((P1.x - P0.x) * (P2.z - P0.z)	- (P2.x - P0.x) * (P1.z - P0.z));
		}
		//===================================================================


		// cn_PnPoly(): crossing number test for a point in a polygon
		//      Input:   P = a point,
		//               V[] = vertex points of a polygon V[n+1] with V[n]=V[0]
		//      Return:  0 = outside, 1 = inside
		// This code is patterned after [Franklin, 2000]
		private int	cn_PnPoly (Vector3 P, Vector3[] V, int n)
		{
			int cn = 0;    // the  crossing number counter

			// loop through all edges of the polygon
			for (int i = 0; i < n; i++) {    // edge from V[i]  to V[i+1]
				if (((V [i].z <= P.z) && (V [i + 1].z > P.z))// an upward crossing
				   || ((V [i].z > P.z) && (V [i + 1].z <= P.z))) { // a downward crossing
					// compute  the actual edge-ray intersect x-coordinate
					float vt = (float)(P.z - V [i].z) / (V [i + 1].z - V [i].z);
					if (P.x < V [i].x + vt * (V [i + 1].x - V [i].x)) // P.x < intersect
					++cn;   // a valid crossing of y=P.y right of P.x
				}
			}
			return (cn & 1);    // 0 if even (out), and 1 if  odd (in)

		}
		//===================================================================


		// wn_PnPoly(): winding number test for a point in a polygon
		//      Input:   P = a point,
		//               V[] = vertex points of a polygon V[n+1] with V[n]=V[0]
		//      Return:  wn = the winding number (=0 only when P is outside)
		private int	wn_PnPoly (Vector3 P, Vector3[] V, int n)
		{
			int wn = 0;    // the  winding number counter

			// loop through all edges of the polygon
			for (int i = 0; i < n; i++) {   // edge from V[i] to  V[i+1]
				if (V [i].z <= P.z) {          // start y <= P.y
					if (V [i + 1].z > P.z)      // an upward crossing
				if (isLeft (V [i], V [i + 1], P) > 0)  // P left of  edge
					++wn;            // have  a valid up intersect
				} else {                        // start y > P.y (no test needed)
					if (V [i + 1].z <= P.z)     // a downward crossing
				if (isLeft (V [i], V [i + 1], P) < 0)  // P right of  edge
					--wn;            // have  a valid down intersect
				}
			}
			return wn;
		}




	}
}
