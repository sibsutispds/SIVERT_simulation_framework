/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using Veneris.Osm;
using UnityEngine.Networking;
using System.IO;
using System;
using System.Xml.Serialization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Veneris
{
	public class SumoEnvironmentBuilder : MonoBehaviour
	{
		public bool GSCM = true;
		public string pathToPolys = "";
		public string pathToOSMJSON = "";
		public float defaultBuildingHeight = 10f;
		public float defaultLevelHeight = 3f;
		public bool buildTrafficLights = true;
		public bool useOPAL = false;
		public bool onlyBuildingsWithOpal = false;
		public GameObject root = null;
		public SumoBuilder builder = null;
		protected Dictionary<long,JSONObject> osmIdToJSON = null;
		protected additionalType polygons = null;
		protected JSONObject osmJSONData = null;
		protected int numberOfObjects = 0;
		protected int opalStaticMeshes = 0;

		protected string[] noBuildTags = {
			"population",      
			"landuse",  
			"man_made",
			"amenity",

		};

		public bool InNoBuildList(string t) {
			for (int i = 0; i < noBuildTags.Length; i++) {
				if (t.Contains (noBuildTags [i])) {
					return true;
				}
			}
			return false;
		}

		public struct TrafficLightIndexPair
		{
			public TrafficLight tl;
			public int index;
		}

		public virtual void BuildPolygons (SumoBuilder builder)
		{
			numberOfObjects = 0;
			this.builder = builder;
			root = new GameObject ("Environment");
			osmIdToJSON = new Dictionary<long, JSONObject> ();
			polygons = ReadPolygons ();
			locationType loc = GetLocationElement ();
			string bb = loc.origBoundary;
			if (string.IsNullOrEmpty (pathToOSMJSON)) {
				DownloadOSMBuildingData ();
			} else {
				osmJSONData = new JSONObject (File.ReadAllText (pathToOSMJSON));
				CreateOSMWayDictionary ();
				ProcessPolygons ();

			}
			if (buildTrafficLights) {
				BuildTrafficLightPosts ();
			}
			root.name = "Environment (" + numberOfObjects + " objects)";
		}

		public void BuildOnlyTrafficLights (SumoBuilder builder)
		{
			this.builder = builder;
			if (root == null) {
				root = new GameObject ("Environment");
			}
			BuildTrafficLightPosts ();
		}

		protected void CreateOSMWayDictionary ()
		{
			List<JSONObject> elements = osmJSONData ["elements"].list;
			for (int i = 0; i < elements.Count; i++) {
				if ("way".Equals (elements [i] ["type"].str)) {
					//					Debug.Log (i + " " + elements [i] ["type"].str);

					//					Debug.Log (Convert.ToInt64 (elements [i] ["id"].n));
					osmIdToJSON.Add (Convert.ToInt64 (elements [i] ["id"].n), elements [i] ["tags"]);
				}
			}
			//Null osmJSONData??
		}

		public virtual additionalType ReadPolygons ()
		{

			if (string.IsNullOrEmpty (pathToPolys)) {
				Debug.Log ("No polygons file");
				return null;
			}
			XmlSerializer serializer = new XmlSerializer (typeof(additionalType));
			XmlReader reader = XmlReader.Create (pathToPolys);
			try {
				additionalType poly = (additionalType)serializer.Deserialize (reader);
				reader.Close ();
				return poly;
			} catch (System.Exception e) {
				Debug.LogError (e);
				reader.Close ();
				return null;
			}

		}

		public locationType GetLocationElement ()
		{
			//There might be more than one location element
			return polygons.Items.Where (x => x.GetType () == typeof(locationType)).ToArray () [0] as locationType;
			
			

		}

		public float[] originalBoundaryToNSWE (string b)
		{
			float[] NSWE = new float[4];
			string[] val = b.Split (',');
			NSWE [0] = float.Parse (val [3]); //North
			NSWE [1] = float.Parse (val [1]); //South
			NSWE [2] = float.Parse (val [0]); //West
			NSWE [3] = float.Parse (val [2]); //East
			return NSWE;


		}

		protected virtual void ProcessPolygons ()
		{
			
			foreach (object o in polygons.Items) {
				if (o.GetType () == typeof(polygonType)) {
					polygonType pol = o as polygonType;
					//Debug.Log(pol.id);
					//if (pol.type.Contains ("landuse") || pol.type.Contains ("population") || pol.type.Contains("amenity")) {
					if (InNoBuildList(pol.type)) {
						//Do not create, they are only informational overlays
						continue;
					}
					GameObject polygon = CreatePolygon (pol.shape, pol.type, pol.id);
					

					if (polygon != null) {
//						polygon.tag = "GSCMmesh";
						SetPolygonMaterial (polygon, pol.type, pol.color);
						polygon.transform.SetParent (root.transform);
						numberOfObjects++;
					}
				}
			}
			root.name = "Environment (" + numberOfObjects + " objects. " + opalStaticMeshes + " Opal StaticMeshes)";
		}

		protected GameObject CreatePolygon (string shape, string type, string id)
		{
			Vector3[] corners = SumoUtils.SumoShapeToVector3Array (shape);
			if (corners.Length >= 3) {
				GameObject go = SumoUtils.CreateTriangulation (type, id, corners);
				if (!useOPAL) {
					go.isStatic = true;
				}
				if (type.Contains ("building")) {
					ElevateBuilding (go, id);
					//Use name 
					string bname=GetBuildingName(id);
					
					
					if (bname != null) {
						go.name = id + "-" + bname;
						
					}
				}
				if (useOPAL) {
					AssignOpalProperties (go, type);
				}

				return go;
			} else {
				Debug.Log (id + " has fewer than 3 corners");
				return null;
			}
		}

		protected GameObject AssignOpalProperties (GameObject go, string type)
		{
			if (onlyBuildingsWithOpal) {
				if (type.Contains ("building")) {
					Opal.StaticMesh opalsm = go.AddComponent<Opal.StaticMesh> ();
					Opal.OpalMeshProperties opalmp = go.AddComponent<Opal.OpalMeshProperties> ();
					//Brick
					opalmp.emProperties.a = 3.75f;
					opalmp.emProperties.c = 0.038f;
					opalStaticMeshes++;
				}
			} else {
				Opal.StaticMesh opalsm = go.AddComponent<Opal.StaticMesh> ();
				Opal.OpalMeshProperties opalmp = go.AddComponent<Opal.OpalMeshProperties> ();

				if (type.Contains ("building")) {
					//Brick
					opalmp.emProperties.a = 3.75f;
					//opalmp.emProperties.c = 0.038f;
					opalmp.emProperties.c = 0.0f;
				} else if (type.Contains ("leisure.park") || type.Contains ("leisure.garden")) {
					//Dry ground...here it is very dry
					opalmp.emProperties.a = 3f;
					opalmp.emProperties.b = 0f;
					opalmp.emProperties.c = 0.00015f;
					opalmp.emProperties.d = 2.52f;
				} else if (type.Contains ("parking")) {
					//Concrete
					opalmp.emProperties.a = 5.31f;
					opalmp.emProperties.b = 0f;
					opalmp.emProperties.c = 0.0326f;
					opalmp.emProperties.d = 0.8905f;
				} else {
					//Some default
					//Dry ground...here it is very dry
					opalmp.emProperties.a = 3f;
					opalmp.emProperties.b = 0f;
					opalmp.emProperties.c = 0.00015f;
					opalmp.emProperties.d = 2.52f;
				}
				opalStaticMeshes++;
			}
			return go;
		}

		protected virtual void SetPolygonMaterial (GameObject go, string type, string colorVal)
		{ 
//			Debug.Log ("type=" + type);
			Material mat = Resources.Load<Material> ("Materials/" + type) as Material;
			if (mat == null) {
				// Create a new 2x2 texture ARGB32 (32 bit with alpha) and no mipmaps
				Texture2D texture = new Texture2D (2, 2, TextureFormat.ARGB32, false);
				string[] colorvalues = colorVal.Split (',');
				Color32 color = new Color32 (byte.Parse (colorvalues [0]), byte.Parse (colorvalues [1]), byte.Parse (colorvalues [2]), 255);
				// set the pixel values
				texture.SetPixel (0, 0, color);
				texture.SetPixel (1, 0, color);
				texture.SetPixel (0, 1, color);
				texture.SetPixel (1, 1, color);

				// Apply all SetPixel calls
				texture.Apply ();


				go.GetComponent<MeshRenderer> ().material.mainTexture = texture;


			} else {
				go.GetComponent<MeshRenderer> ().material = mat;
			}
		}


		protected virtual void ElevateBuilding (GameObject go, string id)
		{
			List<Matrix4x4> sections = new List<Matrix4x4> ();
			float h = GetBuildingHeight (id);
			int levels = GetBuildingLevels (id);

			if (levels > 0) { //Have levels
				float levelHeight = defaultLevelHeight;//We do not have height. Just multiply levels by  defaultLevelHeight
				if (h > 0) {
					//Divide height by level and create sections
					levelHeight = h / levels;
				}




				//Debug.Log (id + " levelHeight=" + levelHeight + "h=" + h + " levels=" + levels);

				for (int l = levels; l > 0; l--) {
					sections.Add (Matrix4x4.TRS (new Vector3 (0f, levelHeight * l, 0f), Quaternion.identity, Vector3.one));

				}

				
			} else {
				if (h > 0) {
					sections.Add (Matrix4x4.TRS (new Vector3 (0f, h, 0f), Quaternion.identity, Vector3.one));
				} else {
					sections.Add (Matrix4x4.TRS (new Vector3 (0f, defaultBuildingHeight, 0f), Quaternion.identity, Vector3.one));
				}
			}
			/*if (h != null) {
				sections.Add (Matrix4x4.TRS (new Vector3 (0f, h, 0f), Quaternion.identity, Vector3.one));
			} else {
				sections.Add (Matrix4x4.TRS (new Vector3 (0f, 10f, 0f), Quaternion.identity, Vector3.one));
			}
			*/
			sections.Add (Matrix4x4.TRS (Vector3.zero, Quaternion.identity, Vector3.one));

			UnityEngine.Mesh m = new UnityEngine.Mesh ();
			MeshExtrusion.ExtrudeMesh (GetComponentMesh (go), m, sections.ToArray (), false);
			go.GetComponent<MeshFilter> ().mesh = m;

		}

		protected virtual UnityEngine.Mesh GetComponentMesh (GameObject go)
		{
			#if UNITY_EDITOR
			return go.GetComponent<MeshFilter> ().sharedMesh;
			#else
			return go.GetComponent<MeshFilter> ().mesh;
			#endif
		}

		protected float GetBuildingHeight (string i)
		{
			//Debug.Log ("GBH=" + i);
			if (i.Contains ("#")) {
				int position = i.LastIndexOf ('#');
				i = i.Substring (0, position);
			}
			long id = long.Parse (i);
			if (osmIdToJSON.ContainsKey (id)) { //The ids may change because the map has changed. If the network was generated and later OSM data is downloaded
				JSONObject o = osmIdToJSON [id];

				if (o != null && o.HasField ("height")) {
					//Sometimes units like m are used, try to match just numbers 
					Match match=Regex.Match(o ["height"].str,@"\d*\.*\d*");
					if (match.Success) {
						return float.Parse (match.Value);
					} else {
						return float.Parse (o ["height"].str);
					}
					return float.Parse (o ["height"].str);
				} else if (o != null && o.HasField ("building:height")) { //Now OSM use these tags
					//Sometimes units like m are used, try to match just numbers 
					Match match=Regex.Match(o ["building:height"].str,@"\d*\.*\d*");
					if (match.Success) {
						return float.Parse (match.Value);
					} else {
						return float.Parse (o ["building:height"].str);
					}
				} else {
					return -1f;
				}
			} else {
				return -1f;
			}
		}

		protected int GetBuildingLevels (string i)
		{
			//Debug.Log ("GBL=" + i);
			if (i.Contains ("#")) {
				int position = i.LastIndexOf ('#');
				i = i.Substring (0, position);
			}

			long id = long.Parse (i);
			if (osmIdToJSON.ContainsKey (id)) {
				JSONObject o = osmIdToJSON [id];
				if (o != null && o.HasField ("building:levels")) {
					return int.Parse (o ["building:levels"].str);
				} else {
					return -1;
				}
			} else {
				return -1;
			}

		}
		protected string GetBuildingName (string i)
		{
			//Debug.Log ("GBL=" + i);
			if (i.Contains ("#")) {
				int position = i.LastIndexOf ('#');
				i = i.Substring (0, position);
			}

			long id = long.Parse (i);
			if (osmIdToJSON.ContainsKey (id)) {
				JSONObject o = osmIdToJSON [id];
				if (o != null && o.HasField ("name")) {
					return o ["name"].str;
				} else {
					return null;
				}
			} else {
				return null;
			}

		}




		public void DownloadOSMData (string bb)
		{
			float[] nswe = originalBoundaryToNSWE (bb);
			string url = "http://overpass-api.de/api/interpreter";
			string query = "<osm-script output=\"json\" timeout=\"240\" element-limit=\"1073741824\"><union>";
			string bbox = "<bbox-query n=\"{0}\" s=\"{1}\" w=\"{2}\" e=\"{3}\"/><recurse type=\"node-relation\" into=\"rels\"/>";
			string queryend = "<recurse type=\"node-way\"/><recurse type=\"way-relation\"/></union><union><item/><recurse type=\"way-node\"/></union><print mode=\"body\"/></osm-script>";
			string overpassQuery = string.Format (query + bbox + queryend, nswe [0], nswe [1], nswe [2], nswe [3]);
			StartCoroutine (RunOverpassQuery (overpassQuery, DownloadOSMDataFinished));



		}

		public IEnumerator RunOverpassQuery (string overpassQuery, Action<string> f)
		{
			string url = "http://overpass-api.de/api/interpreter";
			Debug.Log (overpassQuery);
			Debug.Log ("Downloading OSM data......");
			WWWForm form = new WWWForm ();
			form.AddField ("data", overpassQuery);
			/*ObservableWWW.Post(url,form)
				.Subscribe(
					text => {f(text); }, //success
					exp => Debug.Log("Error fetching -> " + url + "query="+overpassQuery)); //failure
					*/
			using (UnityWebRequest www = UnityWebRequest.Post (url, form)) {
				yield return www.SendWebRequest ();
				//Debug.Log ("Error fetching -> " + url + "query=" + overpassQuery + "with error:" + www.error);
				if (www.isNetworkError || www.isHttpError) {
					Debug.Log ("Error fetching -> " + url + "query=" + overpassQuery + "with error:" + www.error);
				} else {
					Debug.Log ("OverpassQuery correctly finished ");
					f (www.downloadHandler.text);
				}
			}

		}

		protected virtual void DownloadOSMBuildingData ()
		{
			Debug.Log ("Downloading building data");
			string query = "[out:json];(";
			foreach (object o in polygons.Items) {
				if (o.GetType () == typeof(polygonType)) {
					polygonType pol = o as polygonType;
					if (pol.type.Contains ("building")) {
						if (pol.id.Contains ("#")) {
							int position = pol.id.LastIndexOf ('#');
							string id = pol.id.Substring (0, position);
							query += "way(" + id + ");";
						} else {
							query += "way(" + pol.id + ");";
						}
					}

				}
			}
			query += "); (._;>;); out body;";
			StartCoroutine (RunOverpassQuery (query, DownloadOSMDataFinished));

		}

		protected void DownloadOSMDataFinished (string text)
		{
			Debug.Log ("Finished downloading data");
			osmJSONData = GetOSMDataAsJSON (text);
			CreateOSMWayDictionary ();
			ProcessPolygons ();
		}

		public virtual JSONObject GetOSMDataAsJSON (string text)
		{

			return new JSONObject (text);
		}


		protected void BuildTrafficLightPosts ()
		{
			List<VenerisRoad> roads = builder.GetRoads ();

			for (int i = 0; i < roads.Count; i++) {
				
			
				if (TrafficLightsAtIntersection (roads [i].lanes) > 0) {
					GameObject tlp = BuildTrafficLightPostObject (roads [i].lanes);
					if (tlp != null) {
						tlp.transform.SetParent (root.transform);
					}
				}

					
			}
		}

	
		protected int TrafficLightsAtIntersection (VenerisLane[] lanes)
		{
			int tls = 0;
			for (int i = 0; i < lanes.Length; i++) {
				if (lanes [i].endIntersection != null) {
					tls += lanes [i].endIntersection.transform.GetComponent<StopLineInfo> ().numberOfTrafficLights;
				}
					
			}
			return tls;
		}

		protected int NumberOfTrafficLightsAtStopLine (VenerisLane lane)
		{
			

			if (lane.endIntersection != null) {
				return lane.endIntersection.transform.GetComponent<StopLineInfo> ().numberOfTrafficLights;
			} else {
				return 0;
			}

		}

		protected List<TrafficLightIndexPair> GetTrafficLightsAtStopLine (VenerisLane lane)
		{


			if (lane.endIntersection != null) {
				List<TrafficLightIndexPair> tlList = new List<TrafficLightIndexPair> ();
				PathConnector pc = lane.endIntersection.transform.GetComponentInChildren<PathConnector> ();
				List<long> pathids = pc.GetIncomingPathsToConnector ();
				for (int i = 0; i < pathids.Count; i++) {
					ConnectionInfo info = pc.GetPathsConnectedTo (pathids [i]);
					for (int j = 0; j < info.connectedPaths.Count; j++) {
						if (info.connectedPaths [j].trafficLight != null) {
							TrafficLightIndexPair pair;
							pair.tl = info.connectedPaths [j].trafficLight;
							pair.index = info.connectedPaths [j].trafficLightIndex;
							tlList.Add (pair);


						}
					}
				}
				return tlList;
			} else {
				return null;
			}

		}

		protected GameObject BuildTrafficLightPostObject (VenerisLane[] lanes)
		{
			if (lanes [lanes.Length - 1].endIntersection != null) {
				GameObject postPrefab = LoadPostPrefab (lanes.Length);
				TrafficLightObjectBuilder ob = new TrafficLightObjectBuilder ();
				GameObject tlpost = ob.CreatePostFromPrefab (postPrefab); 
				//TODO: We are assuming the leftmost lane has always the higher index...


				tlpost.transform.rotation = lanes [lanes.Length - 1].endIntersection.transform.rotation;
				tlpost.transform.position = lanes [lanes.Length - 1].endIntersection.transform.position;

				tlpost.transform.Translate (Vector3.left * lanes [lanes.Length - 1].laneWidth * 0.7f);

				//TODO: Scale a little bit, make it better
				//float fraction=3.2f/5.05f*lanes.Length; //(standard lane width/bar length)*number of lanes
				//tlpost.transform.localScale= new Vector3(fraction,1.0f,1.0f);
				//Now add traffic lights to post

				GameObject tlPrefab = LoadTrafficLightPrefab ();
				float baseOffset = 0f;
				for (int i = lanes.Length - 1; i >= 0; i--) {
					//int numberOfLights = TrafficLightsAtStopLine (lanes [i]);
					List<TrafficLightIndexPair> tlList = GetTrafficLightsAtStopLine (lanes [i]);
					if (tlList != null) {
						float laneOffset = (lanes [i].laneWidth) / (tlList.Count + 1);

						for (int j = 0; j < tlList.Count; j++) {
							GameObject tl = ob.AddTrafficLightPrefab (tlPrefab, laneOffset * (j + 1) + baseOffset);
							TrafficLightSet tlset = tl.AddComponent<TrafficLightSet> ();
							tlset.SetTrafficLight (tlList [j].tl);
							tlset.SetIndex (tlList [j].index);


						}

					}
					baseOffset += lanes [i].laneWidth;
				}

				return tlpost;
			} else {
				return null;
			}

		}

		protected virtual GameObject  LoadPostPrefab (int lanes)
		{
			if (lanes > 1) {
				return Resources.Load ("Prefabs/Signs/BasicTrafficLightPost" + lanes) as GameObject;
			} else {
				return Resources.Load ("Prefabs/Signs/BasicTrafficLightPost") as GameObject;
			}
		}

		protected virtual GameObject  LoadTrafficLightPrefab ()
		{
			return Resources.Load ("Prefabs/Signs/BasicTrafficLight") as GameObject;
		}

	}
}
