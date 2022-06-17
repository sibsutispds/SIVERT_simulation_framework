/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet;

using UnityEditor;
using UnityEngine.Assertions;
using System.Xml;
using System.Xml.Serialization;

namespace Veneris
{
	
	public class SumoBuilderOnEditor : SumoBuilder
	{
		[MenuItem ("Veneris/Load SUMO Network in Editor")]
		static public void CreateLoadSumoNetworkToEditor ()
		{
			GameObject go = new GameObject ("SumoBuilderOnEditor");
			SumoBuilderOnEditor sb = go.AddComponent<SumoBuilderOnEditor> ();
			go.tag = "EditorOnly";

		}
		/*public override void BuildScenario ()
		{
			if (CheckScenarioFiles ()) {
				
				SumoNetworkBuilderOnEditor nb = LoadSumoNetworkToEditor ();
				nb.BuildNetwork (this);
				nb.CreateGlobalRouteManager ();
				SumoRouteBuilderOnEditor rb=LoadSumoRouteBuilderToEditor ();
				rb.BuildRoutes (this);
				SumoEnvironmentBuilderOnEditor eb = LoadPolygonsToEditor ();
				if (string.IsNullOrEmpty (pathToPolys)) {
					if (buildOnlyTrafficLights) {
						eb.BuildOnlyTrafficLights (this);
					} 
				} else {
					
					eb.BuildPolygons (this);
				}
			}
		}
	*/
		public void LoadSumoNetworkToEditor ()
		{
			GameObject go = new GameObject ("SumoNetworkBuilderOnEditor");
			if (useJSONFiles) {
				networkBuilder = go.AddComponent<SumoJSONNetworkBuilder> ();
				((SumoJSONNetworkBuilder)networkBuilder).jsPath = pathToNet;
			} else {
				networkBuilder = go.AddComponent<SumoNetworkBuilderOnEditor> ();
				networkBuilder.pathToNet = pathToNet;
			}
			go.transform.parent = transform;
			go.tag = "EditorOnly";
			//return networkBuilder as SumoNetworkBuilderOnEditor;
			//networkBuilder.BuildNetwork (this);
		}
		public void LoadSumoRouteBuilderToEditor ()
		{
			GameObject go = new GameObject ("SumoRouteBuilderOnEditor");
			if (useJSONFiles) {
				routeBuilder = go.AddComponent<SumoJSONRouteBuilder> ();
				((SumoJSONRouteBuilder)routeBuilder).jsPath = pathToRoutes;
			} else {
				routeBuilder = go.AddComponent<SumoRouteBuilderOnEditor> ();
				routeBuilder.pathToRoutes = pathToRoutes;
			}
			go.transform.parent = transform;
			go.tag = "EditorOnly";
			//return routeBuilder as SumoRouteBuilderOnEditor;
			//routeBuilder.BuildRoutes (this);

		}

		public void LoadPolygonsToEditor ()
		{
			GameObject go = new GameObject ("SumoEnvironmentBuilderOnEditor");
			if (useJSONFiles) {
				envBuilder = go.AddComponent<SumoJSONEnvironmentBuilder> ();
				((SumoJSONEnvironmentBuilder)envBuilder).jsPath = pathToPolys;
				envBuilder.pathToOSMJSON = pathToOSMJSON;
			} else {
				envBuilder = go.AddComponent<SumoEnvironmentBuilderOnEditor> ();
				envBuilder.pathToPolys = pathToPolys;
				envBuilder.pathToOSMJSON = pathToOSMJSON;
			}
			go.transform.parent = transform;
			go.tag = "EditorOnly";
			//return envBuilder as SumoEnvironmentBuilderOnEditor;
			//envBuilder.BuildPolygons (this);
		}





	
	
		public GameObject FindPath(long pathid) {
			Path[] paths=GameObject.FindObjectsOfType(typeof(Path)) as Path[];
			foreach (Path p in paths) {
				if (p.pathId == pathid) {
					return p.gameObject;
				}
			}
			return null;
		}
	


		public GameObject FindRoad(string  roadid) {
			VenerisRoad[] roads=GameObject.FindObjectsOfType(typeof(VenerisRoad)) as VenerisRoad[];
			foreach (VenerisRoad r in roads) {
				if (r.sumoId.Equals(roadid)) {
					return r.gameObject;
				}
			}
			return null;
		}

	
		public override void DestroyGameObject (GameObject o)
		{
			DestroyImmediate (o);
		}


	}
}
#endif
