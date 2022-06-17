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
	public class WebGLBuilder : SumoBuilder
	{
		//public float cameraHeight = 70f;


		// Use this for initialization
		void Start ()
		{

			Debug.Log ("WebGLBuilder Start ");
			//ReadPaths ();
			BuildScenario ();
			//Create simulation manager
			Debug.Log ("WebGLBuilder Creating SimulationManager ");
			CreateSimulationManager ();
			CreateUICanvas ();
			//Make roads visible
			Debug.Log ("Translating the floor");
			networkBuilder.floor.transform.Translate (new Vector3 (0f, -0.01f, 0f));
			//SetUpCamera ();


		}

		public void CreateSimulationManager ()
		{
			Debug.Log ("WebGLBuilder creating WebGLSimulationManager");
			GameObject.Instantiate (Resources.Load<GameObject> ("Prefabs/WebGLSimulationManager"));
		
		}

		public void CreateUICanvas ()
		{
			GameObject.Instantiate (Resources.Load<GameObject> ("Prefabs/UI/SimInformationCanvas"));
		}

		public void ReadPaths ()
		{
			Debug.Log ("WebGLBuilder reading paths ");
			Debug.Log (pathToNet);

			pathToNet = JavaScriptInterface.ReadKeyProxy ("pathToNet");
			Debug.Log ("pathToNet=" + pathToNet);
			pathToRoutes = JavaScriptInterface.ReadKeyProxy ("pathToRoutes");
			Debug.Log ("pathToRoutes=" + pathToRoutes);
			pathToPolys = JavaScriptInterface.ReadKeyProxy ("pathToPolys");
			Debug.Log ("pathToPolys=" + pathToPolys);
			pathToOSMJSON = JavaScriptInterface.ReadKeyProxy ("pathToOSMJSON");
			Debug.Log ("pathToOSMJSON=" + pathToOSMJSON);
		}

		public override SumoNetworkBuilder BuildNetwork ()
		{
			
				if (networkBuilder == null) {
					GameObject go = new GameObject ("SumoDOMNetworkBuilder");
					networkBuilder = go.AddComponent<SumoJSONNetworkBuilder> ();
					networkBuilder.pathToNet = pathToNet;
					go.transform.parent = transform;
				} 
				var watch = System.Diagnostics.Stopwatch.StartNew ();
				Debug.Log ("BUILDING NETWORK. It may take some time...");
				networkBuilder.BuildNetwork (this);
				watch.Stop ();
				Debug.Log ("Time to build network=" + (watch.ElapsedMilliseconds / 1000f) + " s");
				return networkBuilder;

		}

		public override SumoRouteBuilder BuildRoutes ()
		{
			
				GameObject go = new GameObject ("SumoJSONRouteBuilder");
				routeBuilder = go.AddComponent<SumoJSONRouteBuilder> ();
				routeBuilder.pathToRoutes = pathToRoutes;
				go.transform.parent = transform;
				routeBuilder.BuildRoutes (this);
				return routeBuilder;


		}

		public override SumoEnvironmentBuilder BuildPolygons ()
		{
			
				GameObject go = new GameObject ("SumoEnvironmentBuilder");
				envBuilder = go.AddComponent<SumoJSONEnvironmentBuilder> ();
				envBuilder.pathToPolys = pathToPolys;
				envBuilder.pathToOSMJSON = pathToOSMJSON;
				Debug.Log ("WebGL building polygons with buildOnlyTrafficLights=" + buildOnlyTrafficLights);
				go.transform.parent = transform;
				if (buildOnlyTrafficLights) {
					envBuilder.BuildOnlyTrafficLights (this);
				} else {
					envBuilder.BuildPolygons (this);
				}
				return envBuilder;

		}
	}
}
