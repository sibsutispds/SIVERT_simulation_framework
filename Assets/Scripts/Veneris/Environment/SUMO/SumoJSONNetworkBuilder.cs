/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using UnityEngine.Assertions;
using System.Linq;

namespace Veneris
{
	public class SumoJSONNetworkBuilder : SumoNetworkBuilder
	{


		public JSONObject jsn;
		public string jsPath = "D:\\Users\\eegea\\MyDocs\\investigacion\\Unity\\builds\\wglbuilder\\osm.net.json";

		protected Dictionary<string, List<JSONObject>> laneIdToJSONConnectionDictionary = null;
		//TODO: have to add this to work on the editor with JSON in addition to XML
		//TODO: clean the architectural mess of editor and non editor objects for the builders
		#region EditorAdaptation
		public override void DestroyGameObject (GameObject o)
		{
			#if UNITY_EDITOR
			DestroyImmediate(o);
			#else 
			base.DestroyGameObject (o);
			#endif
		}
		public override Dictionary<string, VenerisRoad> GetEdgeIdToVenerisRoadDictionary ()
		{
			#if UNITY_EDITOR
			//Create a new dictionary from the network in the scene
			VenerisRoad[] roads=GameObject.FindObjectsOfType(typeof(VenerisRoad)) as VenerisRoad[];
			Dictionary<string,VenerisRoad> dict = new Dictionary<string, VenerisRoad> ();
			foreach (VenerisRoad r in roads) {
				dict.Add (VenerisRoadIdToSumoEdgeId(r),r);
			}
			return dict;
			#else
			return base.GetEdgeIdToVenerisRoadDictionary();
			#endif
		}

		public override Dictionary<string, TrafficLight> GetTLIdToTrafficLightDictionary ()
		{
			#if UNITY_EDITOR
			//Create a new dictionary from the network in the scene
			TrafficLight[] tls=GameObject.FindObjectsOfType(typeof(TrafficLight)) as TrafficLight[];
			Dictionary<string,TrafficLight> dict = new Dictionary<string, TrafficLight> ();
			foreach (TrafficLight t in tls) {

				dict.Add (t.sumoId, t);

			}
			return dict;
			#else
			return base.GetTLIdToTrafficLightDictionary();
			#endif
		}
		public override Dictionary<string, Path> GetLaneIdToPathDictionary ()
		{
			#if UNITY_EDITOR
			//Create a new dictionary from the network in the scene
			Path[] paths=GameObject.FindObjectsOfType(typeof(Path)) as Path[];
			Dictionary<string,Path> dict = new Dictionary<string, Path> ();
			foreach (Path p in paths) {
				if (!p.pathName.Equals ("Director Road Path")) {
					dict.Add (p.pathName, p);
				}
			}
			return dict;
			#else
			return base.GetLaneIdToPathDictionary();
			#endif
		}
		public override Dictionary<long, string> GetPathIdToLaneIdDictionary ()
		{
			#if UNITY_EDITOR
			Dictionary<string,Path> lp = GetLaneIdToPathDictionary ();
			Dictionary<long,string> dict = new Dictionary<long, string> ();
			foreach (Path p in lp.Values) {
				dict.Add (p.pathId, p.pathName);
			}
			return dict;
			#else
			return base.GetPathIdToLaneIdDictionary();
			#endif
		}
		public override Dictionary<string, PathConnector> GetLaneIdToPathConnectorDictionary ()
		{
			#if UNITY_EDITOR
			//Create a new dictionary from the network in the scene
			PathConnector[] conns=GameObject.FindObjectsOfType(typeof(PathConnector)) as PathConnector[];
			Dictionary<long,string> aux=GetPathIdToLaneIdDictionary();
			Dictionary<string,PathConnector> dict = new Dictionary<string, PathConnector> ();
			foreach (PathConnector c in conns) {
				List<long> incoming = c.GetIncomingPathsToConnector ();
				foreach (long pid in incoming) {
					dict.Add (aux [pid], c);
				}

			}
			return dict;
			#else
			return base.GetLaneIdToPathConnectorDictionary();
			#endif
		}
		public override Dictionary<string, VenerisLane> GetLaneIdToVenerisLaneDictionary ()
		{
			#if UNITY_EDITOR
			//Create a new dictionary from the network in the scene
			VenerisLane[] lanes=GameObject.FindObjectsOfType(typeof(VenerisLane)) as VenerisLane[];
			Dictionary<string,VenerisLane> dict = new Dictionary<string, VenerisLane> ();
			foreach (VenerisLane l in lanes) {
				dict.Add (l.sumoId, l);
			}
			return dict;
			#else
			return base.GetLaneIdToVenerisLaneDictionary();
			#endif

		}
		public override Dictionary<long, Path> GetPathIdToPathDictionary ()
		{
			#if UNITY_EDITOR
			//Create a new dictionary from the network in the scene
			Path[] paths=GameObject.FindObjectsOfType(typeof(Path)) as Path[];
			Dictionary<long,Path> dict = new Dictionary<long, Path> ();
			foreach (Path p in paths) {
				if (!p.pathName.Equals ("Director Road Path")) {
					dict.Add (p.pathId, p);
				}
			}
			return dict;
			#else
			return base.GetPathIdToPathDictionary();
			#endif


		}
		public override Dictionary<long, PathConnector> GetPathIdToPathConnectorDictionary ()
		{
			#if UNITY_EDITOR
			Dictionary<string, PathConnector> aux = GetLaneIdToPathConnectorDictionary ();
			Dictionary<string, Path> aux2 = GetLaneIdToPathDictionary ();
			Dictionary<long,PathConnector> dict = new Dictionary<long, PathConnector> ();
			foreach (string laneid in aux.Keys) {
				dict.Add (aux2 [laneid].pathId, aux [laneid]);
			}
			return dict;
			#else
			return base.GetPathIdToPathConnectorDictionary();
			#endif

		}

		#endregion
		public override void LoadNetwork ()
		{
			var watch = System.Diagnostics.Stopwatch.StartNew ();

			#if UNITY_EDITOR
			Debug.Log ("Parsing JSON sumo network from " + jsPath);
			jsn = new JSONObject (System.IO.File.ReadAllText (jsPath));
			#else 
			jsn = new JSONObject (JavaScriptInterface.ReadJSONBuilderFile (1));
			#endif
			watch.Stop ();
			Debug.Log ("Time to read JSON network file=" + (watch.ElapsedMilliseconds / 1000f) + " s");
		

		}

		protected override void FindMaxAndMinRoadId ()
		{
			JSONObject net = jsn ["net"] ["edge"];
			foreach (JSONObject e in net.list) {
				string[] ids = e ["_attributes"] ["id"].str.Split ('#');

				//There may be no # (with AddedOffRamps AddedOnRamps, for example), try to extract a number
				string b = System.Text.RegularExpressions.Regex.Match (ids [0], @"-?\d+").Value;

				long id = long.Parse (b);
				if (id <= minRoadId) {
					minRoadId = id;
				} else if (id >= maxRoadId) {
					maxRoadId = id;
				}


			}
			Debug.Log ("max=" + maxRoadId + "min=" + minRoadId);
		}

		protected override void SetUpTerrain ()
		{
			//Find if there is a terrain or floor define or create a new one
			if (Terrain.activeTerrain == null) {
				//Assume there is a Floor plane
				floor = GameObject.Find ("Floor");
				if (floor == null) {
					Debug.Log ("No Terrain or Floor found. Creating a plane as terrain");
					floor = GameObject.CreatePrimitive (PrimitiveType.Plane);
					floor.name = "Floor";
					floor.tag = "SimTerrain";
					float[] bound = SumoUtils.SumoConvBoundaryToFloat (jsn ["net"] ["location"] ["_attributes"] ["convBoundary"].str);

					floor.transform.localScale = new  Vector3 (bound [2] * 0.15f, 1.0f, bound [3] * 0.15f); //give some margin
					floor.transform.position = new Vector3 (bound [2] * 0.5f, 0.0f, bound [3] * 0.5f);
					SumoScenarioInfo si = floor.AddComponent<SumoScenarioInfo> ();
					si.SetBounds (bound);

					if (useOPAL) {

						Opal.StaticMesh opalsm = floor.AddComponent<Opal.StaticMesh> ();
						Opal.OpalMeshProperties opalmp = floor.AddComponent<Opal.OpalMeshProperties> ();
						//Concrete
						opalmp.emProperties.a = 5.31f;
						opalmp.emProperties.b = 0f;
						opalmp.emProperties.c = 0.0326f;
						opalmp.emProperties.d = 0.8905f;
					}
					return;
				}
			} else {
				floor = Terrain.activeTerrain.gameObject;
			}
		}

		protected override void BuildRoads ()
		{
			roadRoot = new GameObject ("Roads");
			roadRoot.transform.parent = networkRoot.transform;
			JSONObject net = jsn ["net"] ["edge"];
			List<JSONObject> edges;
			if (net.IsArray) {
				edges = net.list;
			} else if (net.IsObject) {
				edges = new List<JSONObject> ();
				edges.Add (net);
			} else {
				Debug.Log ("JSON builder: no road found");
				throw new UnityException ("JSON builder: no road found");
				return;
			}

			foreach (JSONObject e in edges) {
				try {
					//Debug.Log ("eid=" + e ["_attributes"] ["id"].str);
					//				Debug.Log (e.function);
					if (e ["_attributes"].HasField ("function")) {
						if (e ["_attributes"] ["function"].str.Equals ("internal")) {

//						Debug.Log (e ["_attributes"] ["id"].str + "=" + e ["lane"].type);
							CreateInternalPaths (e);
						}
					} else {
					
						if (onlyRoadVehicleTypes) {
							if (e ["_attributes"].HasField ("type")) { //TODO: what if the type is null? At the moment consider it a motor road
								if (!motorRoadTypesSet.Contains (e ["_attributes"] ["type"].str)) {
									//Debug.Log (e ["_attributes"] ["id"].str + " road type is not for vehicles " + e ["_attributes"] ["type"].str);
									if (e ["lane"].type == JSONObject.Type.ARRAY) {
										foreach (JSONObject l in e["lane"].list) {
											notCreatedPaths.Add (l ["_attributes"] ["id"].str);
										}
									} else {
										notCreatedPaths.Add (e ["lane"] ["_attributes"] ["id"].str);
									}
									continue;
								}
							}
						} 
						GameObject rs = CreateRoadSegment (e);
						rs.transform.parent = roadRoot.transform;
						edgeIdToVenerisRoadDictionary.Add (e ["_attributes"] ["id"].str, rs.GetComponent<VenerisRoad> ());

					}

				} catch (System.Exception ex) {
					Debug.Log ("Exception thrown for key :" + e ["_attributes"] ["id"].str);
					Debug.Log ("Exception thrown in BuildRoads(): " + ex.StackTrace);
				}


			}
		}

		protected  void CreateInternalPaths (JSONObject e)
		{
			foreach (JSONObject o in e["lane"].list) {
				JSONObject l;

				if (e ["lane"].type == JSONObject.Type.ARRAY) {
					
					l = o ["_attributes"];
				} else {

					l = o;
				}


			

				if (onlyRoadVehicleTypes) {
					string allow = null;
					string disallow = null;
					if (l.HasField ("allow")) {
						allow = l ["allow"].str;
					}
					if (l.HasField ("disallow")) {
						disallow = l ["disallow"].str;
					}

					if (!IsRoadVehicleAllowedOnLane (allow, disallow)) {
						notCreatedPaths.Add (l ["id"].str);
						Debug.Log ("Vehicle not allowed on internal path " + l ["id"].str);
						continue;
					}
				} 
				//laneIdToPathDictionary.Add (l.id, CreateInternalPath (l).GetComponent<Path> ());
				Path p = CreateInternalPath (l ["id"].str, l ["shape"].str);
				//CreateInternalLane (l["_attributes"]["spreadType"].str, l["_attributes"]["id"].str,float.Parse(l["_attributes"]["speed"].str), float.Parse(l["_attributes"]["length"].str), p);
				string spreadType = null;
				if (e ["_attributes"].HasField ("spreadType")) {
					spreadType = e ["_attributes"] ["spreadType"].str;
				} else {
					spreadType = "center";
				}
				//Debug.Log (l ["id"].str);
				CreateInternalLane (spreadType, l ["index"].str, float.Parse (l ["speed"].str), float.Parse (l ["length"].str), p);

			}
		}



		protected  GameObject CreateRoadSegment (JSONObject e)
		{
			GameObject rbgo = new GameObject ("Sumo RoadBuilder");
			RoadBuilder rb = rbgo.AddComponent<RoadBuilder> ();



			//Road builder already adds NodePathHelper
			NodePathHelper np = rbgo.GetComponent<NodePathHelper> ();
			np.interpolatedPointsPerSegment = 60;
			rb.ways = VenerisRoad.Ways.OneWay;

			rb.laneWidth = sumoLaneWidth; //Def in SUMO: StdDefs.h
			if (e ["_attributes"].HasField ("spreadType")) {
				if (e ["_attributes"] ["spreadType"].str.Equals ("center")) {
					rb.spread = RoadBuilder.SpreadType.Center;
				} else {
					rb.spread = RoadBuilder.SpreadType.Right;
				}
			} else {
				rb.spread = RoadBuilder.SpreadType.Right;
			}

			if (e ["lane"].type == JSONObject.Type.ARRAY) {
				foreach (JSONObject l in e["lane"].list) {
					rb.AddCustomLane (SumoUtils.SumoShapeToVector3Array (l ["_attributes"] ["shape"].str), long.Parse (l ["_attributes"] ["index"].str), rb.laneWidth, float.Parse (l ["_attributes"] ["speed"].str));
				}
			} else {

				rb.AddCustomLane (SumoUtils.SumoShapeToVector3Array (e ["lane"] ["_attributes"] ["shape"].str), long.Parse (e ["lane"] ["_attributes"] ["index"].str), rb.laneWidth, float.Parse (e ["lane"] ["_attributes"] ["speed"].str));
			}

				
			//GameObject road=rb.Build ();
			if (e ["_attributes"].HasField ("shape")) {
				np.SetNodes (SumoUtils.SumoShapeToVector3Array (e ["_attributes"] ["shape"].str));
			}
			GameObject road = rb.BuildWithCustomLanes (e ["_attributes"] ["id"].str);

			/*if (e.shape != null) {
				

				GameObject eo = CreateTriangulation ("EdgeMesh", e.id, e.shape);
			}*/
			//Debug.Log (e ["_attributes"] ["id"].str);
			string name = "";
			if (e ["_attributes"].HasField ("name")) {
				name = e ["_attributes"] ["name"].str;
			}
			SumoEdgeIdToVeneris (road.GetComponent<VenerisRoad> (), e ["_attributes"] ["id"].str, name);

			DestroyGameObject (rbgo);

			return road;


		}

		protected override void BuildTrafficLights ()
		{
			tlRoot = new GameObject ("Traffic Lights");
			tlRoot.transform.parent = networkRoot.transform;
			if (jsn ["net"].HasField ("tlLogic")) {
				if (jsn ["net"] ["tlLogic"].IsObject) {
					//We have to distinguish if we only have one traffi light
					CreateTrafficLight (jsn ["net"] ["tlLogic"]);
				} else if (jsn ["net"] ["tlLogic"].IsArray) {
					foreach (JSONObject t in jsn["net"]["tlLogic"].list) {

						CreateTrafficLight (t);
					}

				}
				
			}
		}

		protected void CreateTrafficLight (JSONObject t)
		{
			GameObject go = new GameObject ("Traffic Light " + t ["_attributes"] ["id"].str);
			go.transform.parent = tlRoot.transform;
			TrafficLight tl = go.AddComponent<TrafficLight> ();
			tl.sumoId = t ["_attributes"] ["id"].str;
			tl.sumoProgramId = t ["_attributes"] ["programID"].str;
			if (t ["_attributes"] ["type"].str.Equals ("actuated")) {
				tl.sumoType = TrafficLight.TrafficLightType.Actuated;
			} else if (t ["_attributes"] ["type"].str.Equals ("static")) {

				tl.sumoType = TrafficLight.TrafficLightType.Static;
			}
			tl.offset = float.Parse (t ["_attributes"] ["offset"].str);
			foreach (JSONObject o in t["phase"].list) {

				TrafficLight.TrafficLightPhase phase = new TrafficLight.TrafficLightPhase (float.Parse (o ["_attributes"] ["duration"].str), SumoPhaseToVenerisSequence (o ["_attributes"] ["state"].str));
				tl.AddTrafficLightPhase (phase);

			}
			tlIdToTrafficLightDictionary.Add (t ["_attributes"] ["id"].str, tl);
		}

		protected void AddConnections (JSONObject cc)
		{
			JSONObject	c = cc ["_attributes"];
			string laneid = c ["from"].str + "_" + c ["fromLane"].str;
			string destid = null;
			if (c.HasField ("via")) {
				destid = c ["via"].str;
			} else {
				destid = c ["to"].str + "_" + c ["toLane"].str;
			}
			//Check that all paths have been created previously
			if (laneIdToPathDictionary.ContainsKey (laneid) && laneIdToPathDictionary.ContainsKey (destid)) {

				if (!laneIdToConnectedLaneIdDictionary.ContainsKey (laneid)) {
					laneIdToConnectedLaneIdDictionary [laneid] = new List<string> ();
				}

				laneIdToConnectedLaneIdDictionary [laneid].Add (destid);

				if (!laneIdToJSONConnectionDictionary.ContainsKey (laneid)) {
					laneIdToJSONConnectionDictionary [laneid] = new List<JSONObject> ();
				}

				laneIdToJSONConnectionDictionary [laneid].Add (c);

			}


		}

		protected override void BuildConnections ()
		{
			laneIdToConnectedLaneIdDictionary = new Dictionary<string, List<string>> ();

			laneIdToJSONConnectionDictionary = new Dictionary<string, List<JSONObject>> ();

			laneIdToPathConnectorDictionary = new Dictionary<string, PathConnector> ();

			pathIdToPathConnectorDictionary = new Dictionary<long, PathConnector> ();


			//Group all connections by  lane
			if (jsn ["net"] ["connection"].IsArray) {
				foreach (JSONObject cc in jsn["net"]["connection"].list) {
					AddConnections (cc);

				}
			} else if (jsn ["net"] ["connection"].IsObject) {
				AddConnections (jsn ["net"] ["connection"]);
			}

			//foreach (string k in laneIdToConnectedLaneIdDictionary.Keys) {
			foreach (string k in laneIdToJSONConnectionDictionary.Keys) {
				
			 	
				GameObject go = new GameObject ("Connection Connector " + k);
				PathConnector pc = go.AddComponent<PathConnector> ();
				//				Debug.Log ("Connector from k=" + k);
				foreach (JSONObject con in laneIdToJSONConnectionDictionary[k]) {

					string id = null;
					if (con.HasField ("via")) {
						id = con ["via"].str;
					} else {
						id = con ["to"].str + "_" + con ["toLane"].str;
					}
					//Check that all paths have been created previously
					Assert.IsTrue (laneIdToPathDictionary.ContainsKey (k));
					Assert.IsTrue (laneIdToPathDictionary.ContainsKey (id));

					ConnectionInfo.ConnectionDirection dir = connectionTypeDirToVenerisDirection (con ["dir"].str);

					//Check traffic lights
					if (con.HasField ("tl")) {
						if (tlIdToTrafficLightDictionary.ContainsKey (con ["tl"].str)) {

							TrafficLight trafficLight = tlIdToTrafficLightDictionary [con ["tl"].str];
							//Debug.Log ("Adding traffic light " + trafficLight.sumoId + " to " + laneIdToPathDictionary [k].pathId);
							pc.AddPathConnection (laneIdToPathDictionary [k].pathId, laneIdToPathDictionary [id], dir, trafficLight, int.Parse (con ["linkIndex"].str));

						} else {
							
							Debug.Log ("There is no tlLogic for "+con ["tl"].str + "even though it appears in the connection for "+ k +". Check your net file");
							//Add this connector without traffic light. TODO: check
							pc.AddPathConnection (laneIdToPathDictionary [k].pathId, laneIdToPathDictionary [id], dir, null, -1);
						}
					} else {
						pc.AddPathConnection (laneIdToPathDictionary [k].pathId, laneIdToPathDictionary [id], dir, null, -1);
					}


				}

				laneIdToPathConnectorDictionary.Add (k, pc);
				//Fill the pathIdDictionary
				pathIdToPathConnectorDictionary.Add (laneIdToPathDictionary [k].pathId, pc);
			}
		}

		protected ConnectionInfo.ConnectionDirection connectionTypeDirToVenerisDirection (string dir)
		{
			switch (dir) {
			case "s":
				return ConnectionInfo.ConnectionDirection.Straight;
			case "l":
				return ConnectionInfo.ConnectionDirection.Left;
			case "r":
				return ConnectionInfo.ConnectionDirection.Right;
			case "L":
				return ConnectionInfo.ConnectionDirection.PartiallyLeft;
			case "R":
				return ConnectionInfo.ConnectionDirection.PartiallyRight;
			case "t":
				return ConnectionInfo.ConnectionDirection.Turn;

			default:
				return ConnectionInfo.ConnectionDirection.Invalid;
			}
		}

		protected void AddIntersectionInfo (JSONObject j)
		{
			switch (j ["_attributes"] ["type"].str) {
			case "internal": 
			//Debug.Log ("Creating internal stop " + j.id);
				junctionIdToInternalStop.Add (j ["_attributes"] ["id"].str, CreateInternalStop (j ["_attributes"] ["id"].str, float.Parse (j ["_attributes"] ["x"].str), float.Parse (j ["_attributes"] ["y"].str)));
				break;
			default:
			//	Debug.Log ("id=" + j ["_attributes"] ["id"].str);
				IntersectionInfo aux = CreateDefaultIntersection (j, intersectionRoot, intersectionDeadEnds);
				if (aux == null) {
					Debug.Log ("null junction = " + j ["_attributes"] ["id"].str);
				} 
				junctionIdToIntersectionInfo.Add (j ["_attributes"] ["id"].str, aux);



				break;
			}
		}

		protected void AddFinishIntersection (JSONObject j)
		{
			switch (j ["_attributes"] ["type"].str) {
			case "internal": 
				AssignInternalJuncion (j ["_attributes"] ["id"].str, j ["_attributes"] ["incLanes"].str);
				break;
			default:
				IntersectionInfo info = junctionIdToIntersectionInfo [j ["_attributes"] ["id"].str];
				if (info != null) {
					FinishIntersection (j ["_attributes"] ["incLanes"].str, j ["_attributes"] ["intLanes"].str, info);
					//Debug.Log (j ["_attributes"] ["id"].str);
					BuildJunctionRules (j);
				}
				break;
			}
		}

		protected void AddBuildInternalRules (JSONObject j)
		{
			switch (j ["_attributes"] ["type"].str) {
			case "internal": 
				BuildInternalJunctionRules (j ["_attributes"] ["incLanes"].str, j ["_attributes"] ["id"].str);
				break;

			}
		}

		protected void AddFillIntersection (JSONObject j)
		{
			string iname;
			switch (j ["_attributes"] ["type"].str) {
			case "internal": 

			//Find the insersection this internal junction belongs to and assign it
				string[] tokens = j ["_attributes"] ["id"].str.Split ('_');
			//Remove 2 trailing numbers and first :
				iname = tokens [0].Substring (1);
				for (int i = 1; i < tokens.Length - 2; i++) {
					iname = iname + "_" + tokens [i];
				}
				break;
			default:
				iname = j ["_attributes"] ["id"].str;
				break;
			}
			BuildIntersectionInfo (iname);
		}

		protected override void BuildIntersections ()
		{
			//First pass: populate dictionaries
			junctionIdToInternalStop = new Dictionary<string, GameObject> ();
			junctionIdToIntersectionInfo = new Dictionary<string, IntersectionInfo> ();
			laneIdToIntersectionPriorityInfoDictionary = new Dictionary<string, IntersectionPriorityInfo> ();
			intersectionRoot = new GameObject ("Intersections");
			intersectionRoot.transform.parent = networkRoot.transform;
			intersectionDeadEnds = new GameObject ("DeadEnds");
			intersectionDeadEnds.transform.parent = intersectionRoot.transform;
			//Group all connections by  lane
			if (jsn ["net"] ["junction"].IsArray) {
				foreach (JSONObject j in jsn["net"]["junction"].list) {
					AddIntersectionInfo (j);
				}
			} else if (jsn ["net"] ["junction"].IsObject) {
				AddIntersectionInfo (jsn ["net"] ["junction"]);
			} else {
				Debug.Log ("JSON Builder: No interesections found");
				return;
			}
			//Make internal stops children of intersection
			foreach (string s in junctionIdToIntersectionInfo.Keys) {
				//Debug.Log ("s=" + s);

				//string[] index = junctionIdToInternalStop.Keys.Where (x => x.Contains (s)).ToArray ();
				string[] index = junctionIdToInternalStop.Keys.Where (x => SumoUtils.SumoJunctionInternalIdToJunctionId (x).Equals (s)).ToArray ();
				foreach (string id in index) {
					//Debug.Log ("internal stop id=" + id);
					GameObject stop = junctionIdToInternalStop [id];
					//if (stop == null) {
					//	Debug.Log ("internal stop null="+junctionIdToInternalStop.Keys.Contains (id));
					//}
					//Debug.Log ("junction id=" + s);

					IntersectionInfo aux2 = junctionIdToIntersectionInfo [s];
					if (aux2 != null) { 
						//	Debug.Log ("intersectionInfo not null for " + s + " intersectioninfo=" + aux2.intersectionId);
						stop.transform.SetParent (junctionIdToIntersectionInfo [s].transform);
					} else {
						Debug.Log ("junctionIdToIntersectionInfo is null for " + s);
					}
				}
				//Debug.Log ("Ending s=" + s);
			}


			//Now finish intersections
			if (jsn ["net"] ["junction"].IsArray) {
				foreach (JSONObject j in jsn["net"]["junction"].list) {
					AddFinishIntersection (j);

				}
			} else if (jsn ["net"] ["junction"].IsObject) {
				AddFinishIntersection (jsn ["net"] ["junction"]);

			}
			//Another pass to build internal junctions priorities: has to be done after constructing intersections priority info in FinishIntersection->BuildJunctionRules()
			if (jsn ["net"] ["junction"].IsArray) {
				foreach (JSONObject j in jsn["net"]["junction"].list) {
					AddBuildInternalRules (j);

				}
			} else if (jsn ["net"] ["junction"].IsObject) {
				AddBuildInternalRules (jsn ["net"] ["junction"]);
			}

			//Another pass to fill intersection info with complete information
			if (jsn ["net"] ["junction"].IsArray) {
				foreach (JSONObject j in jsn["net"]["junction"].list) {
					AddFillIntersection (j);
				}
			} else if (jsn ["net"] ["junction"].IsObject) {
				AddFillIntersection (jsn ["net"] ["junction"]);
			}
			
		}

		protected IntersectionInfo CreateDefaultIntersection (JSONObject j, GameObject root, GameObject deadend)
		{
			//Use internal lanes start as stop position
			GameObject jun = null;
			//Add junction meshes
			if (j ["_attributes"].HasField ("shape")) {
				Vector3[] corners = SumoUtils.SumoShapeToVector3Array (j ["_attributes"] ["shape"].str);
				if (corners.Length > 2) {
					List<string> inc = SumoUtils.SumoAttributeStringToStringList (j ["_attributes"] ["incLanes"].str);
					bool connected = false;
					foreach (string s in inc) {
						if (laneIdToPathDictionary.ContainsKey (s)) {
							connected = true;
							break;
						}
					}
					if (connected) {
						jun = SumoUtils.CreateTriangulation ("Junction", j ["_attributes"] ["id"].str, corners);
						jun.AddComponent<MeshCollider> ();
						jun.isStatic = true;
						jun.tag = "Junction";
					} else {
						jun = SumoUtils.CreateTriangulation ("Unconnected Junction", j ["_attributes"] ["id"].str, corners);
						jun.AddComponent<MeshCollider> ();
						jun.isStatic = true;
						jun.tag = "Junction";
					}
				}

			}
			if (j ["_attributes"].HasField ("intLanes")) {
				//Debug.Log (j.intLanes);

				GameObject intersection = new GameObject ("Intersection " + j ["_attributes"] ["id"].str);
				
				MeshRenderer IntersectionRenderer = intersection.AddComponent<MeshRenderer>();
				Material AsphaltIntersection = Resources.Load("Asphalt", typeof(Material)) as Material;
				IntersectionRenderer.material = AsphaltIntersection;
				
				IntersectionInfo i = intersection.AddComponent<IntersectionInfo> ();
				if (j ["_attributes"] ["id"].str.Contains ("cluster")) {
					string[] tokens = j ["_attributes"] ["id"].str.Split ('_');
					i.intersectionId = long.Parse (tokens [1]);
				} else {
					string b = System.Text.RegularExpressions.Regex.Match (j ["_attributes"] ["id"].str, @"-?\d+").Value;
					i.intersectionId = long.Parse (b);
				}
				i.sumoJunctionId = j ["_attributes"] ["id"].str;
				intersection.transform.position = new Vector3 (float.Parse (j ["_attributes"] ["x"].str), 0f, float.Parse (j ["_attributes"] ["y"].str));
				if (jun != null) {
					jun.transform.SetParent (intersection.transform);
				}

				intersection.transform.parent = root.transform;
				return i;
			} else {
				//No internal lanes, usually mean that this is a dead-end junction 
				if (jun != null) {
					jun.transform.SetParent (deadend.transform);
				}
				return null;
			}

		}

		protected void BuildJunctionRules (JSONObject j)
		{
		

			//Some SUMO networks use requests for unregulated junctions and others, so let us priority info for all that haver <request>
			BuildPriorityChecks (j);
		}

		protected void BuildPriorityChecks (JSONObject  j)
		{
			//<request> elements are associated to internal lanes
			List<string> internalLanes = SumoUtils.SumoAttributeStringToStringList (j ["_attributes"] ["intLanes"].str);
			//Debug.Log ("building request for intersection " + j.id);
			if (j.HasField ("request")) {
				if (j ["request"].IsArray) {
					foreach (JSONObject r in j["request"].list) {
						
						string laneid = internalLanes [int.Parse (r ["_attributes"] ["index"].str)];
						//Do it only for created paths
						BuildResponse (laneid, r ["_attributes"] ["response"].str, internalLanes);
					}
				} else {
					string laneid = internalLanes [int.Parse (j ["request"] ["_attributes"] ["index"].str)];
					//Do it only for created paths
					BuildResponse (laneid, j ["request"] ["_attributes"] ["response"].str, internalLanes);
				}
			}
			
		}

		public override void BuildConnectionList ()
		{
			Debug.Log ("build connection list");
			if (sumoConnectionList != null) {
				sumoConnectionList.Clear ();
				Debug.Log ("build connection list clear");
			} else {
				sumoConnectionList = new List<SumoConnection> ();
			}
			List<JSONObject> conns;
			if (jsn ["net"] ["connection"].IsArray) {
				conns = jsn ["net"] ["connection"].list;
			} else if (jsn ["net"] ["connection"].IsObject) {
				conns = new List<JSONObject> ();
				conns.Add (jsn ["net"] ["connection"]);
			} else {
				Debug.Log ("JSON builder: no connections found");
				return;
			}
			edgeIdToVenerisRoadDictionary = GetEdgeIdToVenerisRoadDictionary ();
			lanedIdToVenerisLanesDictionary = GetLaneIdToVenerisLaneDictionary ();
			laneIdToPathDictionary = GetLaneIdToPathDictionary ();
			for (int i = 0; i < conns.Count; i++) {
				SumoConnection c = null;
				//				Debug.Log (conns [i].from);

				//First check if this is an internal connection
				string fromS = conns [i] ["_attributes"] ["from"].str;
				string toS = conns [i] ["_attributes"] ["to"].str;
				string fromLaneS = conns [i] ["_attributes"] ["fromLane"].str;
				string viaS = null;
				if (conns [i] ["_attributes"].HasField ("via")) {
					viaS = conns [i] ["_attributes"] ["via"].str;
				}
				string toLaneS = conns [i] ["_attributes"] ["toLane"].str;
				if (SumoUtils.IsInternalEdge (fromS)) {
					string laneId = fromS + "_" + fromLaneS;
					if (edgeIdToVenerisRoadDictionary.ContainsKey (toS) && laneIdToPathDictionary.ContainsKey (laneId)) {
						Path ip = laneIdToPathDictionary [laneId];

						VenerisRoad toRoad = edgeIdToVenerisRoadDictionary [toS];
						VenerisLane toLane = lanedIdToVenerisLanesDictionary [toS + "_" + toLaneS];
						//Use laneid as from in this connection for the next step
						c = new SumoConnection (laneId, null, toS, toRoad, null, toLane, viaS, true, ip);
					}

				} else {
					if (edgeIdToVenerisRoadDictionary.ContainsKey (fromS) && edgeIdToVenerisRoadDictionary.ContainsKey (toS)) {
						VenerisRoad fromRoad = edgeIdToVenerisRoadDictionary [fromS];
						VenerisRoad toRoad = edgeIdToVenerisRoadDictionary [toS];
						VenerisLane fromLane = lanedIdToVenerisLanesDictionary [fromS + "_" + fromLaneS];
						VenerisLane toLane = lanedIdToVenerisLanesDictionary [toS + "_" + toLaneS];


						c = new SumoConnection (fromS, fromRoad, toS, toRoad, fromLane, toLane, viaS, false, null);
					}
				}
				if (c != null) {
					sumoConnectionList.Add (c);
				}


			}

		
		}
	}
}
