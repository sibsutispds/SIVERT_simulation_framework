/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

namespace Veneris
{
	public class SumoNetworkBuilderOnEditor : SumoNetworkBuilder
	{
		public override Dictionary<string, VenerisRoad> GetEdgeIdToVenerisRoadDictionary ()
		{
			//Create a new dictionary from the network in the scene
			VenerisRoad[] roads=GameObject.FindObjectsOfType(typeof(VenerisRoad)) as VenerisRoad[];
			Dictionary<string,VenerisRoad> dict = new Dictionary<string, VenerisRoad> ();
			foreach (VenerisRoad r in roads) {
				dict.Add (VenerisRoadIdToSumoEdgeId(r),r);
			}
			return dict;
		}

		public override Dictionary<string, TrafficLight> GetTLIdToTrafficLightDictionary ()
		{
			//Create a new dictionary from the network in the scene
			TrafficLight[] tls=GameObject.FindObjectsOfType(typeof(TrafficLight)) as TrafficLight[];
			Dictionary<string,TrafficLight> dict = new Dictionary<string, TrafficLight> ();
			foreach (TrafficLight t in tls) {

				dict.Add (t.sumoId, t);

			}
			return dict;
		}
		public override Dictionary<string, Path> GetLaneIdToPathDictionary ()
		{
			//Create a new dictionary from the network in the scene
			Path[] paths=GameObject.FindObjectsOfType(typeof(Path)) as Path[];
			Dictionary<string,Path> dict = new Dictionary<string, Path> ();
			foreach (Path p in paths) {
				if (!p.pathName.Equals ("Director Road Path")) {
					dict.Add (p.pathName, p);
				}
			}
			return dict;

		}
		public override Dictionary<long, string> GetPathIdToLaneIdDictionary ()
		{
			Dictionary<string,Path> lp = GetLaneIdToPathDictionary ();
			Dictionary<long,string> dict = new Dictionary<long, string> ();
			foreach (Path p in lp.Values) {
				dict.Add (p.pathId, p.pathName);
			}
			return dict;
		}
		public override Dictionary<string, PathConnector> GetLaneIdToPathConnectorDictionary ()
		{
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
		}
		public override Dictionary<string, VenerisLane> GetLaneIdToVenerisLaneDictionary ()
		{
			//Create a new dictionary from the network in the scene
			VenerisLane[] lanes=GameObject.FindObjectsOfType(typeof(VenerisLane)) as VenerisLane[];
			Dictionary<string,VenerisLane> dict = new Dictionary<string, VenerisLane> ();
			foreach (VenerisLane l in lanes) {
				dict.Add (l.sumoId, l);
			}
			return dict;

		}
		public override Dictionary<long, Path> GetPathIdToPathDictionary ()
		{
			//Create a new dictionary from the network in the scene
			Path[] paths=GameObject.FindObjectsOfType(typeof(Path)) as Path[];
			Dictionary<long,Path> dict = new Dictionary<long, Path> ();
			foreach (Path p in paths) {
				if (!p.pathName.Equals ("Director Road Path")) {
					dict.Add (p.pathId, p);
				}
			}
			return dict;


		}
		public override Dictionary<long, PathConnector> GetPathIdToPathConnectorDictionary ()
		{
			Dictionary<string, PathConnector> aux = GetLaneIdToPathConnectorDictionary ();
			Dictionary<string, Path> aux2 = GetLaneIdToPathDictionary ();
			Dictionary<long,PathConnector> dict = new Dictionary<long, PathConnector> ();
			foreach (string laneid in aux.Keys) {
				dict.Add (aux2 [laneid].pathId, aux [laneid]);
			}
			return dict;

		}
		public override connectionType[] GetSUMOConnections ()
		{
			//Have to load the net file
			XmlSerializer serializer = new XmlSerializer (typeof(netType));
			XmlReader reader = XmlReader.Create (pathToNet);
			net = (netType)serializer.Deserialize (reader);
			connectionType[] conn = net.connection;
			reader.Close ();
			return conn;
		}

		public override void DestroyGameObject (GameObject o)
		{
			DestroyImmediate (o);
		}


		public override List<VenerisRoad> GetRoads ()
		{
			
			VenerisRoad[] roads=GameObject.FindObjectsOfType(typeof(VenerisRoad)) as VenerisRoad[];
			return new List<VenerisRoad> (roads);
		}
	
	}
}
