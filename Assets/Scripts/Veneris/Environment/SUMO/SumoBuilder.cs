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
	public class SumoBuilder : MonoBehaviour
	{
		public bool useJSONFiles = false;
		public string pathToNet = "";
		public string pathToRoutes = "";
		public string pathToPolys = "";
		public string pathToOSMJSON = "";
		public bool buildOnlyTrafficLights = false;


		public SumoNetworkBuilder networkBuilder = null;
		public SumoEnvironmentBuilder envBuilder = null;
		public SumoRouteBuilder routeBuilder = null;


		private bool dn = true;
		// Use this for initialization
		void Start ()
		{
			//BuildNetwork ();
			//BuildRoutes();
			//BuildPolygons();

		}

		#region NetworkDictionaries

		public virtual Dictionary<string, TrafficLight> GetTLIdToTrafficLightDictionary ()
		{
			return networkBuilder.GetTLIdToTrafficLightDictionary ();
		}

		public virtual Dictionary<string, Path> GetLaneIdToPathDictionary ()
		{
			return networkBuilder.GetLaneIdToPathDictionary ();
		}

		public virtual Dictionary<long, string> GetPathIdToLaneIdDictionary ()
		{
			return networkBuilder.GetPathIdToLaneIdDictionary ();
		}

		public virtual Dictionary<string,PathConnector> GetLaneIdToPathConnectorDictionary ()
		{
			return networkBuilder.GetLaneIdToPathConnectorDictionary ();
		}

		public virtual Dictionary<string,VenerisRoad> GetEdgeIdToVenerisRoadDictionary ()
		{
			return networkBuilder.GetEdgeIdToVenerisRoadDictionary ();
		}

		public virtual Dictionary<string,VenerisLane> GetLaneIdToVenerisLaneDictionary ()
		{
			return networkBuilder.GetLaneIdToVenerisLaneDictionary ();
		}

		public virtual Dictionary<long,Path> GetPathIdToPathDictionary ()
		{
			return networkBuilder.GetPathIdToPathDictionary ();
		}

		public virtual Dictionary<long,PathConnector> GetPathIdToPathConnectorDictionary ()
		{
			return networkBuilder.GetPathIdToPathConnectorDictionary ();
		}

		#endregion


		public virtual connectionType[] GetSUMOConnections ()
		{
			return networkBuilder.GetSUMOConnections ();
		}

		public virtual List<SumoConnection> GetSUMOConnectionList ()
		{
			if (networkBuilder.GetSumoConnectionList () == null) {
				networkBuilder.BuildConnectionList ();
			}
			return networkBuilder.GetSumoConnectionList ();
		}

		public virtual void BuildScenario ()
		{
			//if (CheckScenarioFiles ()) {

				SumoNetworkBuilder nb = BuildNetwork ();
				nb.CreateGlobalRouteManager ();
				SumoRouteBuilder rb = BuildRoutes ();
				SumoEnvironmentBuilder eb = BuildPolygons ();

			//}
		}

		public bool CheckScenarioFiles ()
		{
			if (string.IsNullOrEmpty (pathToNet)) {
				Debug.LogError ("Missing network file");
				return false;

			}
			if (string.IsNullOrEmpty (pathToRoutes)) {
				Debug.LogError ("Missing route file");
				return false;

			}
			if (string.IsNullOrEmpty (pathToPolys)) {
				if (buildOnlyTrafficLights == false) {
					Debug.LogError ("Missing polygons file");
					return false;
				} else {
					return true;
				}

			}
			return true;

			
		}



		public virtual SumoNetworkBuilder BuildNetwork ()
		{
			if (networkBuilder == null) {
				GameObject go = new GameObject ("SumoNetworkBuilder");
				if (useJSONFiles) {
					networkBuilder = go.AddComponent<SumoJSONNetworkBuilder> ();
					((SumoJSONNetworkBuilder)networkBuilder).jsPath = pathToNet;
				} else {
					networkBuilder = go.AddComponent<SumoNetworkBuilder> ();
					networkBuilder.pathToNet = pathToNet;
				}
				go.transform.parent = transform;
			} 
			var watch = System.Diagnostics.Stopwatch.StartNew ();
			Debug.Log ("BUILDING NETWORK. It may take some time...");
			networkBuilder.BuildNetwork (this);
			watch.Stop ();
			Debug.Log ("Time to build network=" + (watch.ElapsedMilliseconds / 1000f) + " s");
			return networkBuilder;

		}

		public virtual SumoRouteBuilder BuildRoutes ()
		{
			GameObject go = new GameObject ("SumoRouteBuilder");
			if (useJSONFiles) {
				routeBuilder = go.AddComponent<SumoJSONRouteBuilder> ();
				((SumoJSONRouteBuilder)routeBuilder).jsPath = pathToRoutes;
			} else {
				routeBuilder = go.AddComponent<SumoRouteBuilder> ();
				routeBuilder.pathToRoutes = pathToRoutes;
			}
			go.transform.parent = transform;
			routeBuilder.BuildRoutes (this);
			return routeBuilder;
		}

		public virtual SumoEnvironmentBuilder BuildPolygons ()
		{
			
			GameObject go = new GameObject ("SumoEnvironmentBuilder");
			if (useJSONFiles) {
				envBuilder = go.AddComponent<SumoJSONEnvironmentBuilder> ();
				((SumoJSONEnvironmentBuilder)envBuilder).jsPath = pathToPolys;
				envBuilder.pathToOSMJSON = pathToOSMJSON;
			} else {
				envBuilder = go.AddComponent<SumoEnvironmentBuilder> ();
				envBuilder.pathToPolys = pathToPolys;
				envBuilder.pathToOSMJSON = pathToOSMJSON;
			}
			go.transform.parent = transform;
			if (buildOnlyTrafficLights) {
				envBuilder.BuildOnlyTrafficLights (this);
			} else {
				envBuilder.BuildPolygons (this);
			}
			return envBuilder;

		}


		public List<VenerisRoad> GetRoads ()
		{
			if (networkBuilder != null) {
				return networkBuilder.GetRoads ();
			} else {
				VenerisRoad[] roads = GameObject.FindObjectsOfType (typeof(VenerisRoad)) as VenerisRoad[];
				return new List<VenerisRoad> (roads);
			}

		}

		//Recursively get all the roads connected to a path, through path connectors
		public void GetConnectedRoadsFromPath (Path p, PathConnector pc, List<VenerisRoad> roadlist)
		{
			networkBuilder.GetConnectedRoadsFromPath (p, pc, roadlist);
		}


		public virtual void DestroyGameObject (GameObject o)
		{
			//In order to have an editor version of this component
			Destroy (o);
		}


	}
}
