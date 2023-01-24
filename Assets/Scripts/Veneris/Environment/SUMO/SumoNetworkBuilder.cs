/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Xml;
using System.Xml.Serialization;
using Veneris.Routes;
using System.Linq;

namespace Veneris
{
	public class SumoNetworkBuilder : MonoBehaviour
	{
		public string pathToNet = "";
		protected float sumoLaneWidth = 3.2f;
		protected bool showReport = false;
		public bool onlyRoadVehicleTypes = true;
		public bool useOPAL = false;
		public bool generateIdFromSumoId = false;
		//if true, generate road and lane id from sumo ids
		public long initialRoadId = 0;


		protected netType net = null;


		public long minRoadId = long.MaxValue;
		public long maxRoadId = long.MinValue;
		//Unfortunately, when off or on-ramps are added, the road id is not unique, we have to keep track in the network

		public GameObject networkRoot = null;
		public GameObject intersectionRoot = null;
		public GameObject intersectionDeadEnds = null;
		public GameObject roadRoot = null;
		public GameObject tlRoot = null;
		public SumoBuilder builder = null;

		public GameObject floor = null;


		protected Dictionary<string, Path> laneIdToPathDictionary = null;
		protected Dictionary<long, string> pathIdToLaneIdDictionary = null;
		protected Dictionary<long, Path> pathIdToPathDictionary = null;

		protected Dictionary<string, VenerisLane> lanedIdToVenerisLanesDictionary = null;
		protected Dictionary<string, List<string>> laneIdToConnectedLaneIdDictionary = null;
		protected Dictionary<string, VenerisRoad> edgeIdToVenerisRoadDictionary = null;
		Dictionary<string, List<connectionType>> laneIdToConnectionTypeDictionary = null;

		protected Dictionary<string, PathConnector> laneIdToPathConnectorDictionary = null;
		protected Dictionary<long, PathConnector> pathIdToPathConnectorDictionary = null;

		protected Dictionary<string, GameObject> junctionIdToInternalStop = null;
		protected Dictionary<string, IntersectionInfo> junctionIdToIntersectionInfo = null;
		//Dictionary of built intersections
		//Build an auxiliary dictionary to finish the internal junctions later
		protected Dictionary<string,IntersectionPriorityInfo> laneIdToIntersectionPriorityInfoDictionary = null;

		//Traffic Lights
		protected Dictionary<string,TrafficLight> tlIdToTrafficLightDictionary = null;

		public List<SumoConnection> sumoConnectionList = null;

		protected HashSet<string> placedConnectors = null;
		protected HashSet<string> notCreatedPaths = null;
		protected HashSet<string> unconnectedLanes = null;


		protected string[] railRoadTypes = {
			"railway.rail",      
			"railway.tram",  
			"railway.light_rail",
			"railway.subway",
			"railway.preserved",
		};

		protected string[] motorRoadTypes = {

			"highway.motorway",    
			"highway.motorway_link",
			"highway.trunk",   
			"highway.trunk_link",
			"highway.primary",   
			"highway.primary_link",
			"highway.secondary",   
			"highway.secondary_link",
			"highway.tertiary",  
			"highway.tertiary_link",
			"highway.unclassified",
			"highway.residential", 
			"highway.living_street",
			//"highway.service",      
			"highway.track",        
			"highway.services",   
			"highway.unsurfaced",
			"highway.road",
		};

		protected string[] pedestrianRoadTypes = {
			"highway.footway",
			"highway.pedestrian",
			"highway.path",
			"highway.bridleway",
			"highway.cycleway",
			"highway.step",
			"highway.steps",
			"highway.stairs"
		};
		protected  string[] pedestrianVehicleTypes = {
			"bicycle",
			"pedestrian",
			"delivery"
		};
		protected string[] roadVehicleTypes = {
			"passenger",
			"bus",
			"taxi",
			"trailer",
			"emergency",
			"motorcycle",
			"evehicle"
			//"delivery" //Consider this a road vehicle also
		};
		protected HashSet<string> pedestrianRoadTypesSet = null;
		protected HashSet<string> motorRoadTypesSet = null;
		protected HashSet<string> pedestrianVehicleTypesSet = null;
		protected HashSet<string> roadVehicleTypesSet = null;

		protected void InitNetworkDictionaries ()
		{
			edgeIdToVenerisRoadDictionary = new Dictionary<string, VenerisRoad> ();
			laneIdToPathDictionary = new Dictionary<string, Path> ();
			pathIdToLaneIdDictionary = new Dictionary<long, string> ();
			pathIdToPathDictionary = new Dictionary<long, Path> ();
			lanedIdToVenerisLanesDictionary = new Dictionary<string, VenerisLane> ();
			pedestrianRoadTypesSet = new HashSet<string> (pedestrianRoadTypes);
			pedestrianVehicleTypesSet = new HashSet<string> (pedestrianVehicleTypes);
			motorRoadTypesSet = new HashSet<string> (motorRoadTypes);
			roadVehicleTypesSet = new HashSet<string> (roadVehicleTypes);
			notCreatedPaths = new HashSet<string> ();
			unconnectedLanes = new HashSet<string> ();
			placedConnectors = new HashSet<string> ();
			tlIdToTrafficLightDictionary = new Dictionary<string, TrafficLight> ();
		}

		public virtual Dictionary<string, TrafficLight> GetTLIdToTrafficLightDictionary ()
		{
			return tlIdToTrafficLightDictionary;
		}

		public virtual Dictionary<string, Path> GetLaneIdToPathDictionary ()
		{
			return laneIdToPathDictionary;
		}

		public virtual Dictionary<long, string> GetPathIdToLaneIdDictionary ()
		{
			return pathIdToLaneIdDictionary;
		}

		public virtual Dictionary<string,PathConnector> GetLaneIdToPathConnectorDictionary ()
		{
			return laneIdToPathConnectorDictionary;
		}

		public virtual Dictionary<string,VenerisRoad> GetEdgeIdToVenerisRoadDictionary ()
		{
			
			return edgeIdToVenerisRoadDictionary;
		}

		public virtual Dictionary<string,VenerisLane> GetLaneIdToVenerisLaneDictionary ()
		{
			return lanedIdToVenerisLanesDictionary;
		}

		public virtual Dictionary<long,Path> GetPathIdToPathDictionary ()
		{
			return pathIdToPathDictionary;
		}

		public virtual Dictionary<long,PathConnector> GetPathIdToPathConnectorDictionary ()
		{
			return pathIdToPathConnectorDictionary;
		}

		public virtual connectionType[] GetSUMOConnections ()
		{
			return net.connection;
		}

		public virtual List<VenerisRoad> GetRoads ()
		{
			List<VenerisRoad> roads = new List<VenerisRoad> ();
			for (int i = 0; i < roadRoot.transform.childCount; i++) {
				roads.Add (roadRoot.transform.GetChild (i).GetComponent<VenerisRoad> ());
			}
			return roads;
		}



		public virtual void BuildConnectionList ()
		{
			Debug.Log ("build connection list");
			if (sumoConnectionList != null) {
				sumoConnectionList.Clear ();
				Debug.Log ("build connection list clear");
			} else {
				sumoConnectionList = new List<SumoConnection> ();
			}
			connectionType[] conns = GetSUMOConnections ();
			edgeIdToVenerisRoadDictionary = GetEdgeIdToVenerisRoadDictionary ();
			lanedIdToVenerisLanesDictionary = GetLaneIdToVenerisLaneDictionary ();
			laneIdToPathDictionary = GetLaneIdToPathDictionary ();
			for (int i = 0; i < conns.Length; i++) {
				SumoConnection c = null;
//				Debug.Log (conns [i].from);

				//First check if this is an internal connection
				if (SumoUtils.IsInternalEdge (conns [i].from)) {
					string laneId = conns [i].from + "_" + conns [i].fromLane;
					if (edgeIdToVenerisRoadDictionary.ContainsKey (conns [i].to) && laneIdToPathDictionary.ContainsKey (laneId)) {
						Path ip = laneIdToPathDictionary [laneId];

						VenerisRoad toRoad = edgeIdToVenerisRoadDictionary [conns [i].to];
						VenerisLane toLane = lanedIdToVenerisLanesDictionary [conns [i].to + "_" + conns [i].toLane];
						//Use laneid as from in this connection for the next step
						c = new SumoConnection (laneId, null, conns [i].to, toRoad, null, toLane, conns [i].via, true, ip);
					}

				} else {
					if (edgeIdToVenerisRoadDictionary.ContainsKey (conns [i].from) && edgeIdToVenerisRoadDictionary.ContainsKey (conns [i].to)) {
						VenerisRoad fromRoad = edgeIdToVenerisRoadDictionary [conns [i].from];
						VenerisRoad toRoad = edgeIdToVenerisRoadDictionary [conns [i].to];
						VenerisLane fromLane = lanedIdToVenerisLanesDictionary [conns [i].from + "_" + conns [i].fromLane];
						VenerisLane toLane = lanedIdToVenerisLanesDictionary [conns [i].to + "_" + conns [i].toLane];


						c = new SumoConnection (conns [i].from, fromRoad, conns [i].to, toRoad, fromLane, toLane, conns [i].via, false, null);
					}
				}
				if (c != null) {
					sumoConnectionList.Add (c);
				}

				
			}

			//Second pass, link via
			/*	for (int j = 0; j < conns.Length; j++) {
				
				if (conns [j].via != null) {
					Debug.Log ("Looking for via " + conns [j].via +" for "+conns[j].from);
					SumoConnection via = FindViaInConnectionList (conns [j].via);
					if (via != null) {
						SumoConnection fr = FindFromInConnectionList (conns [j].from);
						if (fr != null) {
							Debug.Log ("assigning via to " + fr.fromSumoId + "via=" + via.fromSumoId);
							fr.viaConnection = via;
						}

					}

				}
				
			}*/
		}

		public SumoConnection FindViaInConnectionList (string via)
		{
			for (int k = 0; k < sumoConnectionList.Count; k++) {
				if (sumoConnectionList [k].internalLane) {
					if (sumoConnectionList [k].fromSumoId.Equals (via)) {
						Debug.Log ("found via " + sumoConnectionList [k].fromSumoId);
						return  sumoConnectionList [k];

					}
				}
			}
			return null;
		}

		public SumoConnection FindFromInConnectionList (string from)
		{
			for (int h = 0; h < sumoConnectionList.Count; h++) {
				if (sumoConnectionList [h].fromSumoId.Equals (from)) {
					Debug.Log ("found from " + sumoConnectionList [h].fromSumoId);
					return sumoConnectionList [h];

					break;
				}
			}
			return null;
		}

		public virtual List<SumoConnection> GetSumoConnectionList ()
		{
			return sumoConnectionList;
		}


		protected virtual void FindMaxAndMinRoadId ()
		{
			foreach (edgeType e in net.edge) {
				string[] ids = e.id.Split ('#');

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



		//Recursively get all the roads connected to a path, through path connectors
		public void GetConnectedRoadsFromPath (Path p, PathConnector pc, List<VenerisRoad> roadlist, bool init = true)
		{
			ConnectionInfo cinfo = pc.GetPathsConnectedTo (p.pathId);

			if (cinfo == null) {
				Debug.Log ("null cinfo:" + p.pathId);
				return;
			}
			if (init) {
				pathIdToPathConnectorDictionary = GetPathIdToPathConnectorDictionary ();

			}
			foreach (ConnectionInfo.PathDirectionInfo pair in cinfo.connectedPaths) {
				//All paths should be internal, check it
				if (pair.p.IsInternal ()) {
					//Debug.Log ("pair.p.pathId=" + pair.p.pathId);
					PathConnector endconnector = pathIdToPathConnectorDictionary [pair.p.pathId];
					ConnectionInfo endcinfo = endconnector.GetPathsConnectedTo (pair.p.pathId);

					foreach (ConnectionInfo.PathDirectionInfo endpair in endcinfo.connectedPaths) {
						if (endpair.p.IsInternal ()) {
							GetConnectedRoadsFromPath (pair.p, endconnector, roadlist, false);

						} else {
							roadlist.Add (endpair.p.gameObject.GetComponentInParent<VenerisRoad> ());
						}
					}
				}

			}
		}



		public void CreateGlobalRouteManager ()
		{
			if (networkRoot != null) {
				Debug.Log ("Creating GlobalRouteManager");
				//Make sure that we have the connection list before running the GlobalRouteManager
				List<SumoConnection> scl = GetSumoConnectionList ();
				if (scl == null) {
					BuildConnectionList ();
				} else {
					Debug.Log ("Connection list already created");
				}
				GameObject go = new GameObject ("GlobalRouteManager");
				GlobalRouteManager grm = go.AddComponent<GlobalRouteManager> ();

				grm.SetSumoConnectionList (GetSumoConnectionList ());

				grm.transform.SetParent (networkRoot.transform);
			} else {
				Debug.Log ("No network. A network should be built before creating the connection list and the associated Global Route Manager");
			}

		}

		public virtual void LoadNetwork ()
		{
			var watch = System.Diagnostics.Stopwatch.StartNew ();
			Debug.Log ("Parsing XML sumo network file");
			XmlSerializer serializer = new XmlSerializer (typeof(netType));
			XmlReader reader = XmlReader.Create (pathToNet);
			net = (netType)serializer.Deserialize (reader);
			reader.Close ();
			watch.Stop ();
			Debug.Log ("Time to parse network file=" + (watch.ElapsedMilliseconds / 1000f) + " s");

		}

		public void BuildNetwork (SumoBuilder builder)
		{
			
			this.builder = builder;
			InitNetworkDictionaries ();

			if (networkRoot != null) {
				Debug.Log ("A network already exists. A new one will be created and the root replaced for this component");
				GameObject globalRouteManager = GameObject.Find ("GlobalRouteManager");
				if (globalRouteManager != null) {
					Debug.Log ("A GlobalRouteManager already exists!. It will be destroyed");
					DestroyGameObject (globalRouteManager);
				}
			}
			networkRoot = new GameObject ("SUMO Network");

			LoadNetwork ();

		

			FindMaxAndMinRoadId ();
			SetUpTerrain ();
			Debug.Log ("Building roads ....");
			BuildRoads ();
			AssignPathIds ();
			Debug.Log ("Building traffic lights ....");
			BuildTrafficLights ();
			Debug.Log ("Building connections ....");
			BuildConnections ();
			Debug.Log ("Building intersections ....");
			BuildIntersections ();
			Debug.Log ("Placing connector triggers ....");
			PlaceConnectorTriggers ();

			int trafficLights = 0;
			if (tlRoot != null) {
				trafficLights = tlRoot.transform.childCount;
			}


			networkRoot.AddComponent<GenerationInfo> ().SetGenerationInfo ("SUMO Network generated from " + pathToNet + ".Roads=" + roadRoot.transform.childCount + ".Traffic lights=" + trafficLights + ".Intersections=" + intersectionRoot.transform.childCount);
			FixTerrain ();
			if (showReport) {
				foreach (string s in notCreatedPaths) {
					Debug.Log ("Not created: " + s);
				}
				foreach (string s in unconnectedLanes) {
					Debug.Log ("Unconnected: " + s);
				}
			}


		}

		protected virtual void FixTerrain ()
		{
			//For better visualization
			//Find if there is a terrain or floor define or create a new one
			if (Terrain.activeTerrain == null) {
				//Assume there is a Floor plane
				floor = GameObject.Find ("Floor");
				if (floor != null) {
					floor.transform.position = floor.transform.position - new Vector3 (0.0f, 0.01f, 0.0f);
				}
			}
		}

		protected virtual void SetUpTerrain ()
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
					float[] bound = SumoUtils.SumoConvBoundaryToFloat (net.location.convBoundary);

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

				}
			} else {
				floor = Terrain.activeTerrain.gameObject;
			}

		}

		protected void AssignPathIds ()
		{
			long i = 1;
			foreach (Path p in laneIdToPathDictionary.Values) {
				p.pathId = i;
				pathIdToLaneIdDictionary.Add (i, p.pathName);
				pathIdToPathDictionary.Add (i, p);
				if (p.pathName.ElementAt (0).Equals (':')) {
					p.SetInternal (true);
				}
				i = i + 1;

			}
		}

		protected void PlaceConnectorTriggers ()
		{
			//Some of the connectors have been placed on the intersection stop lines, now we place the remaining
			HashSet<string> unplaced = new HashSet<string> (laneIdToPathConnectorDictionary.Keys);
			unplaced.SymmetricExceptWith (placedConnectors);
			foreach (string id in unplaced) {
				PathConnector pc = laneIdToPathConnectorDictionary [id];
				GameObject sl = new GameObject ("ConnectorTrigger " + id);
				sl.tag = "ConnectorTrigger";
				//Try to assign them to insersections if that is the case
				string jid = SumoUtils.SumoJunctionInternalIdToJunctionId (id);
				//string[] key=junctionIdToIntersectionInfo.Keys.Where(x=>id.Contains(x)).ToArray();

				string[] key = junctionIdToIntersectionInfo.Keys.Where (x => jid.Equals (x)).ToArray ();
				if (key.Length > 0) {
					foreach (string k in key) {
						//	Debug.Log ("ConnectorTrigger " + id + "key[0]=" + k);
						if (junctionIdToIntersectionInfo.ContainsKey (k)) {
							if (junctionIdToIntersectionInfo [k] != null) {
								sl.transform.SetParent (junctionIdToIntersectionInfo [k].transform);
							}
						}
					}

				} 
				sl.transform.position = laneIdToPathDictionary [id].GetLastNode ().transform.position;
				Vector3 f = laneIdToPathDictionary [id].FindClosestPointInfoInPath (sl.transform).tangent;
				//sl.transform.forward = laneIdToPathDictionary [id].FindClosestPointInfoInPath (sl.transform).tangent;
				if (f.Equals (Vector3.zero)) {
					Debug.Log (sl.name);
				}
				sl.transform.forward = f;
				BoxCollider bc = sl.AddComponent<BoxCollider> ();

				if (lanedIdToVenerisLanesDictionary.ContainsKey (id)) {

					bc.size = new Vector3 (lanedIdToVenerisLanesDictionary [id].laneWidth, 2f, 2f);
				} else {
					//standard size
					bc.size = new Vector3 (sumoLaneWidth, 2f, 2f);
				}
				bc.isTrigger = true;
				pc.name = "Connector " + id;
				pc.transform.SetParent (sl.transform, false);

			}

		}

		#region Intersections

		//TODO:Should we move this to another component?
		protected virtual  void BuildIntersections ()
		{
			//First pass: populate dictionaries
			junctionIdToInternalStop = new Dictionary<string, GameObject> ();
			junctionIdToIntersectionInfo = new Dictionary<string, IntersectionInfo> ();
			laneIdToIntersectionPriorityInfoDictionary = new Dictionary<string, IntersectionPriorityInfo> ();
			intersectionRoot = new GameObject ("Intersections");
			intersectionRoot.transform.parent = networkRoot.transform;
			intersectionDeadEnds = new GameObject ("DeadEnds");
			intersectionDeadEnds.transform.parent = intersectionRoot.transform;

			foreach (junctionType j in net.junction) {
				switch (j.type) {
				case junctionTypeType.@internal: 
					//Debug.Log ("Creating internal stop " + j.id);
					junctionIdToInternalStop.Add (j.id, CreateInternalStop (j.id, j.x, j.y));
					break;
				default:
					IntersectionInfo aux = CreateDefaultIntersection (j, intersectionRoot, intersectionDeadEnds);
					if (aux == null) {
						Debug.Log ("null junction = " + j.id);
					} 
					junctionIdToIntersectionInfo.Add (j.id, aux);



					break;
				}

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
			foreach (junctionType j in net.junction) {
				switch (j.type) {
				case junctionTypeType.@internal: 
					AssignInternalJuncion (j.id, j.incLanes);
					break;
				default:
					IntersectionInfo info = junctionIdToIntersectionInfo [j.id];
					if (info != null) {
						FinishIntersection (j.incLanes, j.intLanes, info);
						//Construct right of way rules
						BuildJunctionRules (j);
					}
					break;
				}

			}
			//Another pass to build internal junctions priorities: has to be done after constructing intersections priority info in FinishIntersection->BuildJunctionRules()
			foreach (junctionType j in net.junction) {
				switch (j.type) {
				case junctionTypeType.@internal: 
					BuildInternalJunctionRules (j.incLanes, j.id);
					break;

				}

			}

			//Another pass to fill intersection info with complete information
			foreach (junctionType j in net.junction) {
				string iname;
				switch (j.type) {
				case junctionTypeType.@internal: 
					//Find the insersection this internal junction belongs to and assign it
					string[] tokens = j.id.Split ('_');
					//Remove 2 trailing numbers and first :
					iname = tokens [0].Substring (1);
					for (int i = 1; i < tokens.Length - 2; i++) {
						iname = iname + "_" + tokens [i];
					}
					break;
				default:
					iname = j.id;
					break;
				}
				BuildIntersectionInfo (iname);
			}
		}

		protected GameObject CreateInternalStop (string id, float x, float y)
		{
			GameObject stop = new GameObject ("Internal Stop  " + id);
			//TODO: elevation and terrain
			stop.transform.position = new Vector3 (x, 0.0f, y);
			return stop;
		}

		protected IntersectionInfo CreateDefaultIntersection (junctionType j, GameObject root, GameObject deadend)
		{
			//Use internal lanes start as stop position
			GameObject jun = null;
			//Add junction meshes
			if (!string.IsNullOrEmpty (j.shape)) {
				Vector3[] corners = SumoUtils.SumoShapeToVector3Array (j.shape);
				if (corners.Length > 2) {
					List<string> inc = SumoUtils.SumoAttributeStringToStringList (j.incLanes);
					bool connected = false;
					foreach (string s in inc) {
						if (laneIdToPathDictionary.ContainsKey (s)) {
							connected = true;
							break;
						}
					}
					if (connected) {
						jun = SumoUtils.CreateTriangulation ("Junction", j.id, corners);
						jun.AddComponent<MeshCollider> ();
						jun.isStatic = true;
						jun.tag = "Junction";
					} else {
						jun = SumoUtils.CreateTriangulation ("Unconnected Junction", j.id, corners);
						jun.AddComponent<MeshCollider> ();
						jun.isStatic = true;
						jun.tag = "Junction";
					}
				}

			}
			if (!string.IsNullOrEmpty (j.intLanes)) {
				//Debug.Log (j.intLanes);

				GameObject intersection = new GameObject ("Intersection " + j.id);

				IntersectionInfo i = intersection.AddComponent<IntersectionInfo> ();
				if (j.id.Contains ("cluster")) {
					string[] tokens = j.id.Split ('_');
					i.intersectionId = long.Parse (tokens [1]);
				} else {
					string b = System.Text.RegularExpressions.Regex.Match (j.id, @"-?\d+").Value;
					i.intersectionId = long.Parse (b);
				}
				i.sumoJunctionId = j.id;
				intersection.transform.position = new Vector3 (j.x, 0f, j.y);
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

		protected void AssignInternalJuncion (string id, string incLanes)
		{
			//Find the insersection this internal junction belongs to and assign it
			//string[] tokens = j.id.Split ('_');
			//Remove 2 trailing numbers and first :
			//string iname = tokens [0].Substring (1);
			//for (int i = 1; i < tokens.Length - 2; i++) {
			//	iname = iname + "_" + tokens [i];
			//}
			string iname = SumoUtils.SumoJunctionInternalIdToJunctionId (id);
			if (junctionIdToIntersectionInfo.ContainsKey (iname)) {
				IntersectionInfo info = junctionIdToIntersectionInfo [iname];
				//We would need to look up in the internal paths, the one that belongs to this intersection, according to  incLanes
				List<string> inc = SumoUtils.SumoAttributeStringToStringList (incLanes);
				foreach (string s in inc) {
					if (s.Contains (':')) {
						//This is an internal lane, assign to intersection
						if (laneIdToPathDictionary.Keys.Contains (s)) {
							laneIdToPathDictionary [s].transform.SetParent (info.transform);
						}

					}
				}
			}
		}

		protected void FinishIntersection (string incLanes, string intLanes, IntersectionInfo intersection)
		{
			HashSet<string> incoming = new HashSet<string> (SumoUtils.SumoAttributeStringToStringList (incLanes));
			HashSet<string> inter = new HashSet<string> (SumoUtils.SumoAttributeStringToStringList (intLanes));

			//Make sure that we only connect to paths that have been created
			incoming.IntersectWith (laneIdToPathDictionary.Keys);
			inter.IntersectWith (laneIdToPathDictionary.Keys);
			List<string> unconnected = CheckConnectedInternalLanesAtIntersection (incoming.ToList (), inter.ToList ());
			//if (unconnected.Count > 0) {
			//	Debug.Log ("Intersection has unconnected internal lanes " + intersection.sumoJunctionId);
			//				foreach (string s in unconnected) {
			//						Debug.Log ("** "+s);
			//						Debug.Log ("s is in "+notCreatedPaths.Contains (s));
			//				}

			//

			//Build StopLines: one stop line at each incoming lane
			int slineindex = 0;
			foreach (string c in incoming) {
				//Debug.Log ("incoming lane=" + c);
				//Some lanes are not connected
				if (laneIdToConnectedLaneIdDictionary.ContainsKey (c)) {
					Path incPath = laneIdToPathDictionary [c];
					string interPathId = laneIdToConnectedLaneIdDictionary [c] [0];
					GameObject sl = new GameObject ("StopLine " + slineindex);
					sl.tag = "Intersection";
					sl.layer = LayerMask.NameToLayer ("Vision");
					sl.transform.position = laneIdToPathDictionary [interPathId].GetFirstNode ().transform.position;
					//sl.transform.LookAt(incPath.GetLastNode().transform.position);
					sl.transform.forward = incPath.FindClosestPointInfoInPath (sl.transform).tangent;
					sl.transform.SetParent (intersection.transform);

					//StopLine Info 

					StopLineInfo slinfo = sl.AddComponent<StopLineInfo> ();

					//Box Collider
					BoxCollider bc = sl.AddComponent<BoxCollider> ();
					bc.size = new Vector3 (lanedIdToVenerisLanesDictionary [c].laneWidth, 2f, 2f);
					//Move a little back  from the intersection 
					bc.center = new Vector3 (0f, 0f, -1f);
					//Make it trigger
					bc.isTrigger = true;

					//Add connectors at stop line
					PathConnector pc = laneIdToPathConnectorDictionary [c];
					pc.transform.SetParent (sl.transform, false);
					pc.name = "Connector " + c;

					ConnectionInfo cinfo = pc.GetPathsConnectedTo (laneIdToPathDictionary [c].pathId);
					int numberOfTrafficLights = 0;
					for (int k = 0; k < cinfo.connectedPaths.Count; k++) {
						if (cinfo.connectedPaths [k].trafficLight != null) {
							numberOfTrafficLights++;
						}
					}
					slinfo.numberOfTrafficLights = numberOfTrafficLights;

					//And add  them to placed connectors
					placedConnectors.Add (c);


					//Fill intersection info
					//FillIntersectionInfo(incPath,intersection,pc,sl.transform);

					//Assign AI behaviour
					BuildAIRules (sl, incPath);
					slineindex++;
				} else {

					if (!laneIdToPathDictionary.ContainsKey (c)) {
						notCreatedPaths.Add (c);
					}
					if (!laneIdToConnectedLaneIdDictionary.ContainsKey (c)) {
						unconnectedLanes.Add (c);
					}


				}
			}

		
			//Make internal path a children of the intersection
			//Have to be done separately because there are more internal lanes than incoming lanes

			foreach (string id in inter) {

				laneIdToPathDictionary [id].transform.SetParent (intersection.transform);
			}



		}


		protected List<string> CheckConnectedInternalLanesAtIntersection (List<string> incLanes, List<string> intLanes)
		{
			//Check that all incoming lanes are connected to the corresponding internal lanes as in connection dictionary
			foreach (string c in incLanes) {
				//if (!notCreatedPaths.Contains (c)) {
				if (laneIdToConnectedLaneIdDictionary.Keys.Contains (c)) {

					intLanes.RemoveAll (item => laneIdToConnectedLaneIdDictionary [c].Contains (item));
				}

			}
			return intLanes;
		}

		protected void BuildAIRules (GameObject o, Path incomingPath)
		{
			IntersectionBehaviourProvider bp = o.AddComponent<IntersectionBehaviourProvider> ();
			bp.AddAffectedLane (incomingPath.pathId);

		}

		protected void BuildJunctionRules (junctionType j)
		{
			/*switch (j.type) {
			case junctionTypeType.unregulated:
				return;
			case junctionTypeType.priority: 
				BuildPriorityChecks (j);
				return;

			}
			*/

			//Some SUMO networks use requests for unregulated junctions and others, so let us priority info for all that haver <request>
			BuildPriorityChecks (j);
		}

		protected void BuildPriorityChecks (junctionType j)
		{
			//<request> elements are associated to internal lanes
			List<string> internalLanes = SumoUtils.SumoAttributeStringToStringList (j.intLanes);
			//Debug.Log ("building request for intersection " + j.id);

			foreach (requestType r in j.request) {

				string laneid = internalLanes [int.Parse (r.index)];
				//Do it only for created paths
				BuildResponse (laneid, r.response, internalLanes);


			}
		}

		protected void BuildResponse (string laneid, string response, List<string> internalLanes)
		{
			if (laneIdToPathDictionary.ContainsKey (laneid)) {
				GameObject internalPath = laneIdToPathDictionary [laneid].gameObject;
				IntersectionPriorityInfo info = internalPath.AddComponent<IntersectionPriorityInfo> ();
				laneIdToIntersectionPriorityInfoDictionary.Add (laneid, info);
				//Now add the positions of the first node of the priority lanes
				List<int> bitset = SumoUtils.SumoResponseToIntArray (response);
				//Response bitsets must be read from right to left so we have to reverse
				bitset.Reverse ();
				//Debug.Log ("junction id=" + j.id);
				//Debug.Log ("internal="+laneid+"index="+r.index);


				for (int i = 0; i < bitset.Count; i++) {
					//Debug.Log ("bitset[" + i + "]=" + bitset [i]);
					if (!internalLanes [i].Equals (laneid)) {
						if (bitset [i] == 1) {
							//Debug.Log ("intelane priority over this=" + internalLanes [i]);
							if (laneIdToPathDictionary.Keys.Contains (internalLanes [i])) { //Some lanes might no be created if only vehicles and not trams or trains are allowed
								info.AddCheckPosition (laneIdToPathDictionary [internalLanes [i]].GetFirstNode ().transform);
							}

						}
					}

				}
			}
		}

		protected void BuildInternalJunctionRules (string incLanes, string id)
		{
			//From SUMO source code  MSInternalJunction.cpp line 70 
			//  the first lane in the list of incoming lanes is special. It defines the
			// link that needs to do all the checking for this internal junction
			//So, we need the response information from the first incoming lane
			List<string> inc = SumoUtils.SumoAttributeStringToStringList (incLanes);
			if (inc.Count > 0) {
				string laneid = inc [0];
				//Get the path
				if (laneIdToPathDictionary.Keys.Contains (laneid)) {
					Path internalPath = laneIdToPathDictionary [laneid];
					//Copy the intersection priority info to the previous path

					if (laneIdToIntersectionPriorityInfoDictionary.Keys.Contains (id)) {
						IntersectionPriorityInfo info = laneIdToIntersectionPriorityInfoDictionary [id];
						IntersectionPriorityInfo internalJunctionInfo =	internalPath.gameObject.AddComponent<IntersectionPriorityInfo> ();

						foreach (Transform t in info.GetCheckPositions()) {
							internalJunctionInfo.AddCheckPosition (t);
						}


						GameObject internalStop = junctionIdToInternalStop [id];
						//internalJunctionInfo.SetInternalStop (internalPath.GetLastNode ().transform);
						internalJunctionInfo.SetInternalStop (internalStop.transform);

						//Make orientation equal to path tangent
						/*internalStop.transform.forward = internalPath.FindClosestPointInfoInPath (internalStop.transform).tangent;

				BoxCollider bc =internalStop.AddComponent<BoxCollider> ();

				bc.size = new Vector3 (sumoLaneWidth, 2f, 2f);
				bc.isTrigger = true;
				internalStop.tag = "InternalStopTrigger";
				*/
						//Place path connector at the internal stops
						PathConnector pc = laneIdToPathConnectorDictionary [laneid];
						//GameObject sl = new GameObject ("ConnectorTrigger  " + laneid);
						internalStop.tag = "InternalStopTrigger";
						internalStop.transform.forward = internalPath.FindClosestPointInfoInPath (internalStop.transform).tangent;
						BoxCollider bc = internalStop.AddComponent<BoxCollider> ();
						//Standard size because internal paths do not have VenerisLanes
						bc.size = new Vector3 (sumoLaneWidth, 2f, 1f);
						bc.isTrigger = true;
						pc.transform.SetParent (internalStop.transform, false);
						placedConnectors.Add (laneid);
						pc.name = "Connector " + laneid;

						//Fill again intersection info

					}

				}

			}

		}

		protected void BuildIntersectionInfo (string iname)
		{
			//string iname;

			IntersectionInfo info = junctionIdToIntersectionInfo [iname];
			//Now find the stop line
			if (info != null) {
				for (int i = 0; i < info.transform.childCount; i++) {
					Transform t = info.transform.GetChild (i);
					if (t.name.Contains ("StopLine")) {
						//One path per stop line, so
						PathConnector pc = t.GetComponentInChildren<PathConnector> ();
						List<long> ipaths = pc.GetIncomingPathsToConnector ();
						Assert.IsTrue (ipaths.Count == 1);
						//Add end information to lane
						Path path = pathIdToPathDictionary [ipaths [0]];
						VenerisLane lane = path.GetComponent<VenerisLane> ();
						lane.endIntersection = t.GetComponent<IntersectionBehaviourProvider> ();
						//
						//Check traffic lights
						//connectionType ct= laneIdToConnectionTypeDictionary[lane.sumoId];
						//if (ct.tl != null) {
						//	TrafficLight tl = tlIdToTrafficLightDictionary [ct.tl];
						//}

						FillIntersectionInfo (pathIdToPathDictionary [ipaths [0]], info, pc, t);
					}
				}
			}
		}

		protected void FillIntersectionInfo (Path incPath, IntersectionInfo intersection, PathConnector pc, Transform stop)
		{
			VenerisRoad road = incPath.gameObject.GetComponentInParent<VenerisRoad> ();
			intersection.SetRoad (road);
			List<VenerisRoad> roadlist = new List<VenerisRoad> ();
			GetConnectedRoadsFromPath (incPath, pc, roadlist);
			foreach (VenerisRoad toRoad in roadlist) {
				intersection.AddAjacencyInfo (road, toRoad, stop, pc);

			}
			/*ConnectionInfo cinfo = pc.GetPathsConnectedTo (incPath.pathId);
			foreach (ConnectionInfo.PathDirectionPair pair in cinfo.connectedPaths) {
				//All paths should be internal, check it
				if (pair.p.IsInternal()) {
					PathConnector endconnector = pathIdToPathConnectorDictionary [pair.p.pathId];
					ConnectionInfo endcinfo = endconnector.GetPathsConnectedTo (pair.p.pathId);
					foreach (ConnectionInfo.PathDirectionPair endpair in endcinfo.connectedPaths) {
						VenerisRoad toRoad = endpair.p.gameObject.GetComponentInParent<VenerisRoad> ();
						if (toRoad == null) {
							Debug.Log (endpair.p.transform.GetComponentInParent<VenerisRoad> ());
							Debug.Log ("gameobject=" + endpair.p.gameObject.name);
							Debug.Log ("pathid="+endpair.p.pathId);
							Debug.Log ("pathname="+endpair.p.transform.name);
						} else {
							intersection.AddAjacencyInfo (road, toRoad, stop, pc);
						}
					}
				} else {
					Debug.Log ("Not internal pathid="+pair.p.pathId);
					Debug.Log ("Not internal pathname="+pair.p.transform.name);
				}

			}*/

		}

		#endregion

		#region Connections

		protected virtual void BuildConnections ()
		{

			laneIdToConnectedLaneIdDictionary = new Dictionary<string, List<string>> ();

			laneIdToConnectionTypeDictionary = new Dictionary<string, List<connectionType>> ();

			laneIdToPathConnectorDictionary = new Dictionary<string, PathConnector> ();

			pathIdToPathConnectorDictionary = new Dictionary<long, PathConnector> ();


			//Group all connections by  lane
			foreach (connectionType c in net.connection) {
				string laneid = c.from + "_" + c.fromLane;
				string destid = null;
				if (c.via != null) {
					destid = c.via;
				} else {
					destid = c.to + "_" + c.toLane;
				}
				//Check that all paths have been created previously
				if (laneIdToPathDictionary.ContainsKey (laneid) && laneIdToPathDictionary.ContainsKey (destid)) {

					if (!laneIdToConnectedLaneIdDictionary.ContainsKey (laneid)) {
						laneIdToConnectedLaneIdDictionary [laneid] = new List<string> ();
					}

					laneIdToConnectedLaneIdDictionary [laneid].Add (destid);

					if (!laneIdToConnectionTypeDictionary.ContainsKey (laneid)) {
						laneIdToConnectionTypeDictionary [laneid] = new List<connectionType> ();
					}

					laneIdToConnectionTypeDictionary [laneid].Add (c);

				}






			}

			//foreach (string k in laneIdToConnectedLaneIdDictionary.Keys) {
			foreach (string k in laneIdToConnectionTypeDictionary.Keys) {
				Debug.Log ("Connector for k=" + k);	
				if (laneIdToConnectionTypeDictionary [k].Count == 0) {
					Debug.Log ("No connections for PathConnector " + k);
					continue;
				} 
				GameObject go = new GameObject ("Connection Connector " + k);
				PathConnector pc = go.AddComponent<PathConnector> ();
				//				Debug.Log ("Connector from k=" + k);
				foreach (connectionType con in laneIdToConnectionTypeDictionary[k]) {

					string id = null;
					if (con.via != null) {
						id = con.via;
					} else {
						id = con.to + "_" + con.toLane;

					}
					//Check that all paths have been created previously
					Assert.IsTrue (laneIdToPathDictionary.ContainsKey (k));
					Assert.IsTrue (laneIdToPathDictionary.ContainsKey (id));

					ConnectionInfo.ConnectionDirection dir = connectionTypeDirToVenerisDirection (con.dir);

					//Check traffic lights
					if (con.tl != null) {
						if (tlIdToTrafficLightDictionary.ContainsKey (con.tl)) {

							TrafficLight trafficLight = tlIdToTrafficLightDictionary [con.tl];
							//Debug.Log ("Adding traffic light " + trafficLight.sumoId + " to " + laneIdToPathDictionary [k].pathId);
							pc.AddPathConnection (laneIdToPathDictionary [k].pathId, laneIdToPathDictionary [id], dir, trafficLight, int.Parse (con.linkIndex));
						} else {
							Debug.Log ("There is no tlLogic for "+con.tl + "even though it appears in the connection for "+ k +". Check your net file");
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

		protected ConnectionInfo.ConnectionDirection connectionTypeDirToVenerisDirection (connectionTypeDir dir)
		{
			switch (dir) {
			case connectionTypeDir.s:
				return ConnectionInfo.ConnectionDirection.Straight;
			case connectionTypeDir.l:
				return ConnectionInfo.ConnectionDirection.Left;
			case connectionTypeDir.r:
				return ConnectionInfo.ConnectionDirection.Right;
			case connectionTypeDir.L:
				return ConnectionInfo.ConnectionDirection.PartiallyLeft;
			case connectionTypeDir.R:
				return ConnectionInfo.ConnectionDirection.PartiallyRight;
			case connectionTypeDir.t:
				return ConnectionInfo.ConnectionDirection.Turn;

			default:
				return ConnectionInfo.ConnectionDirection.Invalid;
			}
		}

		#endregion

		#region TrafficLights

		protected virtual void BuildTrafficLights ()
		{
			if (net.tlLogic == null) {
				Debug.Log ("No traffic lights in scenario");
				return;
			}
			tlRoot = new GameObject ("Traffic Lights");
			tlRoot.transform.parent = networkRoot.transform;
			foreach (tlLogicType t in net.tlLogic) {
				GameObject go = new GameObject ("Traffic Light " + t.id);
				go.transform.parent = tlRoot.transform;
				TrafficLight tl = go.AddComponent<TrafficLight> ();
				tl.sumoId = t.id;
				tl.sumoProgramId = t.programID;
				if (t.type == tlTypeType.actuated) {
					tl.sumoType = TrafficLight.TrafficLightType.Actuated;
				}
				if (t.type == tlTypeType.@static) {
					tl.sumoType = TrafficLight.TrafficLightType.Static;
				}
				tl.offset = t.offset;
				foreach (object o in t.Items) {
					if (o.GetType () == typeof(phaseType)) {
						phaseType pt = (phaseType)o;
						TrafficLight.TrafficLightPhase phase = new TrafficLight.TrafficLightPhase (pt.duration, SumoPhaseToVenerisSequence (pt.state));
						tl.AddTrafficLightPhase (phase);
					}
				}
				tlIdToTrafficLightDictionary.Add (t.id, tl);
			}
		}

		protected List<TrafficLight.TrafficLightState> SumoPhaseToVenerisSequence (string states)
		{
			List<TrafficLight.TrafficLightState> seq = new List<TrafficLight.TrafficLightState> ();
			char[] st = states.ToCharArray ();
			foreach (char c in st) {
				seq.Add (SumoTLStateToVenerisTLState (c));
			}
			return seq;
		}

		protected TrafficLight.TrafficLightState SumoTLStateToVenerisTLState (char s)
		{
			switch (s) {
			case 'r':
				return TrafficLight.TrafficLightState.Red;
			case 'y':
				return TrafficLight.TrafficLightState.Amber;
			case 'g':
				return TrafficLight.TrafficLightState.GreenNoPriority;
			case 'G':
				return TrafficLight.TrafficLightState.Green;
			case 'u':
				return TrafficLight.TrafficLightState.RedAmber;
			case 'o':
				return TrafficLight.TrafficLightState.OffBlinking;
			case 'O':
				return TrafficLight.TrafficLightState.OffNoSignal;
			default:

				return TrafficLight.TrafficLightState.Undefined;

			}
		}


		#endregion

		#region Roads

		protected virtual void BuildRoads ()
		{
			roadRoot = new GameObject ("Roads");
			roadRoot.transform.parent = networkRoot.transform;
			foreach (edgeType e in net.edge) {

				//				Debug.Log (e.id);
				//				Debug.Log (e.function);

				if (e.function != edgeTypeFunction.@internal) {
					if (onlyRoadVehicleTypes) {
						if (e.type != null) { //TODO: what if the type is null? At the moment consider it a motor road
							if (!motorRoadTypesSet.Contains (e.type)) {
								Debug.Log ("not type " + e.id);
								foreach (laneType l in e.lane) {
									notCreatedPaths.Add (l.id);
								}
								continue;
							}
						}
					} 
					GameObject rs = CreateRoadSegment (e);
					rs.transform.parent = roadRoot.transform;
					edgeIdToVenerisRoadDictionary.Add (e.id, rs.GetComponent<VenerisRoad> ());


				} else {
					CreateInternalPaths (e);
					/*foreach (laneType l in e.lane) {
						if (onlyRoadVehicleTypes) {
							if (!IsRoadVehicleAllowedOnLane (l)) {
								notCreatedPaths.Add (l.id);
								Debug.Log ("Not allowed road vehicle " + l.id);
								continue;
							}
						} 
							//laneIdToPathDictionary.Add (l.id, CreateInternalPath (l).GetComponent<Path> ());
						CreateInternalPath (l);

					}
					*/
				}

			

			}

		}

		protected  void CreateInternalPaths (edgeType e)
		{
			foreach (laneType l in e.lane) {
				if (onlyRoadVehicleTypes) {
					if (!IsRoadVehicleAllowedOnLane (l.allow, l.disallow)) {
						notCreatedPaths.Add (l.id);
						Debug.Log ("Not allowed road vehicle " + l.id);
						continue;
					}
				} 
				//laneIdToPathDictionary.Add (l.id, CreateInternalPath (l).GetComponent<Path> ());
				Path p = CreateInternalPath (l.id, l.shape);
				string spreadType = null;
				if (e.spreadType == edgeTypeSpreadType.center) {
					spreadType = "center";
				} else {
					spreadType = "right";
				}
				CreateInternalLane (spreadType, l.index, l.speed, l.length, p);

			}
		}

		protected bool IsRoadVehicleAllowedOnLane (string allow, string disallow)
		{


			if (allow != null) {
				string[] allowed = allow.Split (null);
				if (roadVehicleTypesSet.Overlaps (allowed)) {
					//At least on type of road vehicle is allowed
					return true;
				} else {
					return false;
				}


			}
			if (disallow != null) {
				HashSet<string> disallowed = new HashSet<string> (disallow.Split (null));
				if (roadVehicleTypesSet.IsSubsetOf (disallowed)) {
					//All road vehicle types are prohibited
					return false;
				} else {
					return true;
				}

			}
			return true;


		}

		protected Path CreateInternalPath (string id, string shape)
		{
			GameObject go = new GameObject ("InternalPath " + id);
			Path p = go.AddComponent<Path> ();
			p.pathName = id;
			//First get the parents to create the path directly on the first node.
			//Otherwise if we move the parent, all the children move
			Vector3[] nodes = SumoUtils.SumoShapeToVector3Array (shape);
			go.transform.position = nodes [0];
			p.SetNodes (nodes);
			laneIdToPathDictionary.Add (id, p);

			return p;

		}

		protected void CreateInternalLane (string spreadType, string lindex, float lspeed, float llength, Path p)
		{
			GameObject rbgo = new GameObject ("Sumo RoadBuilder");
			RoadBuilder rb = rbgo.AddComponent<RoadBuilder> ();



			//Road builder already adds NodePathHelper
			NodePathHelper np = rbgo.GetComponent<NodePathHelper> ();
			np.interpolatedPointsPerSegment = 60;
			rb.ways = VenerisRoad.Ways.OneWay;
			/*
			if (e.shape != null) {
				
				np.SetNodes(SumoShapeToVector3Array(e.shape));
			

			} else {
				if (e.lane [0].shape != null) {
					np.SetNodes(SumoShapeToVector3Array(e.lane [0].shape));
				}
			}

			np.BindToTerrain ();
			//rb.addLanePaths = true;
			*/
			rb.laneWidth = sumoLaneWidth; //Def in SUMO: StdDefs.h
			if (spreadType.Equals ("center")) {
				rb.spread = RoadBuilder.SpreadType.Center;
			} else {
				rb.spread = RoadBuilder.SpreadType.Right;
			}
			p.gameObject.isStatic = true;
			p.BindToTerrain ();

			//Create lane and sections
			VenerisLane lane = p.gameObject.AddComponent<VenerisLane> ();
			//TODO: if we do not add a "Lane" tag, it is going to be ignored by the AILogic
			lane.sumoId = p.pathName;
			lane.laneId = long.Parse (lindex);
			lane.laneWidth = rb.laneWidth;
			lane.isInternal = true;
			lane.speed = lspeed;
			lane.AddPath (p);

			if (llength < 1f) {
				if (CheckIfGeometryNull (p)) {
					//Do not create mesh
					Debug.Log (lane.sumoId + " is null geometry");
					DestroyGameObject (rbgo);
					return;
				}
			} 
				
			rb.CreateCustomLaneMesh (p.gameObject, false);

			DestroyGameObject (rbgo);


		}

		public bool CheckIfGeometryNull (Path p)
		{
			List<GameObject> _nodes = p.GetNodes ();
			if (_nodes.Count < 2) {
				return true;
			} else {
				if (_nodes.Count == 2) {
					if (_nodes [0].transform.position.Equals (_nodes [1].transform.position)) {
						return true;
					}
				}
			}
			return false;
			
		}

		protected  GameObject CreateRoadSegment (edgeType e)
		{
			GameObject rbgo = new GameObject ("Sumo RoadBuilder");
			RoadBuilder rb = rbgo.AddComponent<RoadBuilder> ();



			//Road builder already adds NodePathHelper
			NodePathHelper np = rbgo.GetComponent<NodePathHelper> ();
			np.interpolatedPointsPerSegment = 60;
			rb.ways = VenerisRoad.Ways.OneWay;
			/*
			if (e.shape != null) {
				
				np.SetNodes(SumoShapeToVector3Array(e.shape));
			

			} else {
				if (e.lane [0].shape != null) {
					np.SetNodes(SumoShapeToVector3Array(e.lane [0].shape));
				}
			}

			np.BindToTerrain ();
			//rb.addLanePaths = true;
			*/
			rb.laneWidth = sumoLaneWidth; //Def in SUMO: StdDefs.h
			if (e.spreadType == edgeTypeSpreadType.center) {
				rb.spread = RoadBuilder.SpreadType.Center;
			} else {
				rb.spread = RoadBuilder.SpreadType.Right;
			}

			foreach (laneType l in e.lane) {
				//rb.AddForwardLane (long.Parse(l.index));

				rb.AddCustomLane (SumoUtils.SumoShapeToVector3Array (l.shape), long.Parse (l.index), rb.laneWidth, l.speed);
			}
			//GameObject road=rb.Build ();
			if (e.shape != null) {
				np.SetNodes (SumoUtils.SumoShapeToVector3Array (e.shape));
			}
			GameObject road = rb.BuildWithCustomLanes (e.id);

			/*if (e.shape != null) {
				

				GameObject eo = CreateTriangulation ("EdgeMesh", e.id, e.shape);
			}*/


			SumoEdgeIdToVeneris (road.GetComponent<VenerisRoad> (), e.id, e.name);

			DestroyGameObject (rbgo);

			return road;


		}

		protected  void SumoEdgeIdToVeneris (VenerisRoad road, string id, string name)
		{
			road.sumoId = id;


			if (generateIdFromSumoId) {

				//There may be no # (with AddedOffRamp AddedOnRamp, for example), try to extract a number
				//string b = System.Text.RegularExpressions.Regex.Match (ids [0], @"-?\d+").Value;
				//Debug.Log (b);
				if (id.Contains ("Added")) {
					//For on off-ramps assign a unique id in the network
					//TODO: it is going to be a problem if we want to merge two way roads in the mesh. We should add a new specfic type of road. In fact, visually they suck



					string aux = id.Substring (0, id.LastIndexOf ('-'));
					string[] ids = aux.Split ('#');
					Debug.Log ("Found a ramp: " + id + " stripped=" + aux);
					if (ids.Length > 1) {

						road.edgeId = long.Parse (ids [1]);
					} else {
						road.edgeId = -1;
					}
					if (long.Parse (ids [0]) < 0) {
						minRoadId--;
						road.roadId = minRoadId;

					} else {
						maxRoadId++;
						road.roadId = maxRoadId;

					}
				} else {
					string[] ids = id.Split ('#');
					//Some scenarios contains double -- as ids, just make them positive
					if (ids [0].Contains ("--")) {
						ids [0] = ids [0].Substring (2);
					}
					road.roadId = long.Parse (ids [0]);
					if (ids.Length > 1) {

						road.edgeId = long.Parse (ids [1]);
					} else {
						road.edgeId = -1;
					}
				}


			} else {
				road.roadId = initialRoadId;
				initialRoadId++;
				road.edgeId = initialRoadId;

			}
			road.roadName = name;
			foreach (VenerisLane l in road.lanes) {
				laneIdToPathDictionary.Add (id + "_" + l.laneId, l.paths [0]);
				l.paths [0].pathName = id + "_" + l.laneId;
				lanedIdToVenerisLanesDictionary.Add (id + "_" + l.laneId, l);
				l.sumoId = id + "_" + l.laneId;
			}
		}

		protected string VenerisRoadIdToSumoEdgeId (VenerisRoad r)
		{
			/*if (r.edgeId != -1) {
				Assert.IsTrue (r.name.Equals (r.roadId.ToString () + "#" + r.edgeId.ToString ()));
				Debug.Log ("name=" + r.name + "rcre=" + (r.roadId.ToString () + "#" + r.edgeId.ToString ()));
				return  (r.roadId.ToString () + "#" + r.edgeId.ToString ());
			} else {
				Debug.Log ("name=" + r.name + "rcre=" + r.roadId.ToString ());
				Assert.IsTrue (r.name.Equals (r.roadId.ToString ()));
				return (r.roadId.ToString ());
			}*/
			return r.sumoId;

		}

		#endregion

		public virtual void DestroyGameObject (GameObject o)
		{
			//In order to have an editor version of this component
			Destroy (o);
		}

	}
}
