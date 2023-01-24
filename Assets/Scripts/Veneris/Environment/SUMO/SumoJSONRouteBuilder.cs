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
	public class SumoJSONRouteBuilder : SumoRouteBuilder
	{
		public JSONObject jsr;
		public string jsPath = "D:\\Users\\eegea\\MyDocs\\investigacion\\Unity\\builds\\wglbuilder\\osm.rou.json";

		public override void BuildRoutes (SumoBuilder builder)
		{
			this.builder = builder;
			Debug.Log ("Building routes with JSON. This is going to take time...");
			GameObject go = new GameObject ("SumoVehicleManager");
			SumoVehicleManager manager = go.AddComponent<SumoVehicleManager> ();

			var watch = System.Diagnostics.Stopwatch.StartNew ();
			Debug.Log ("Parsing JSON sumo routes");
			edgeIdToVenerisRoadDictionary = builder.GetEdgeIdToVenerisRoadDictionary();
			if (edgeIdToVenerisRoadDictionary == null) {
				Debug.Log ("edgeIdToVenerisRoadDictionary is null");
			}
			#if UNITY_EDITOR
			try {
				Debug.Log ("Parsing JSON sumo routes file "+jsPath);
				jsr = new JSONObject (System.IO.File.ReadAllText (jsPath));
			} catch (System.Exception e) {
				Debug.Log("JSON file reading failed: "+e);
			}

			#else 

			jsr = new JSONObject (JavaScriptInterface.ReadJSONBuilderFile (2));
			#endif
			watch.Stop ();
			Debug.Log ("Time to read JSON routes file=" + (watch.ElapsedMilliseconds / 1000f) + " s");
			if (jsr == null) {
				Debug.Log ("jsr is null");
				return;
			}
			//connectionType[] conns = builder.GetSUMOConnections (); //Important, make this here because if we are in the editor  it loads in the loops the network xml

			//watch = System.Diagnostics.Stopwatch.StartNew ();
			//According to the SUMO xsd routes_file.xsd, only one type of the elements can be present: use of xsd:choice. However, the generated files by duarouter includes different elements..
			int vehicleCounter = 0;
			List<JSONObject> rs = new List<JSONObject> ();
		
			if (jsr.HasField ("routes")) {
				
				if (jsr ["routes"].HasField ("vehicle")) {
					if (jsr ["routes"] ["vehicle"].IsArray) {
						rs = jsr ["routes"] ["vehicle"].list;
					} else if (jsr ["routes"] ["vehicle"].IsObject) {
						rs.Add (jsr ["routes"] ["vehicle"]);
					} else {
						Debug.Log ("No vehicles  found in ");
						/*#if UNITY_WEBGL 
						JavaScriptInterface.ExecuteJS ("window.parent.showPlayerError('There was an error building the scenario. See logs and change your settings of select another area')");

						//Application.Quit();
						#else

					//throw new UnityException ("JSON builder: No vehicles found in scenario");
						#endif
						*/
						return;
					}
				} else {
					Debug.Log ("No vehicles found in scenario");
					/*#if UNITY_WEBGL 
					JavaScriptInterface.ExecuteJS ("window.parent.showPlayerError('There was an error building the scenario. See logs and change your settings of select another area')");

					//Application.Quit();
					#else

				//throw new UnityException ("JSON builder: No vehicles found in scenario");
					#endif
			
					return;
					*/
					return;
				}
			} else {
				Debug.Log ("No routes found");
			
				/*#if UNITY_WEBGL 
				JavaScriptInterface.ExecuteJS ("window.parent.showPlayerError('There was an error building the scenario. See logs and change your settings of select another area')");
				#endif
				*/
				return;

			}
			foreach (JSONObject o in rs) {


				ScheduleVehicle (o, manager);
				++vehicleCounter;

			
			}
			//watch.Stop ();
			//Debug.Log ("Using routes file: " + pathToRoutes + ". " + vehicleCounter + " vehicles scheduled. Time to build=" + (watch.ElapsedMilliseconds / 1000f) + " s");

			go.AddComponent<GenerationInfo> ().SetGenerationInfo ("SumoVehicleManager generated from " + pathToRoutes + ". " + vehicleCounter + " vehicles scheduled. Time to build=" + (watch.ElapsedMilliseconds / 1000f) + " s");
			manager.vehiclePrefab = LoadVehiclePrefab ();
		
		}

		protected void ScheduleVehicle (JSONObject vt, SumoVehicleManager m)
		{
			//Debug.Log ("scheduling vehicle " + vt.id);
			int id = 0;
			if (m.vehicleGenerationList != null) {
				id = m.vehicleGenerationList.Count;
			}
			//Debug.Log (" vehicle id=" + id );
			VehicleGenerationInfo i = new VehicleGenerationInfo (id, VehicleGenerationInfo.SumoVehicleTypes.passenger, SumoUtils.StringToFloat (vt ["_attributes"] ["depart"].str), vt ["_attributes"] ["departLane"].str);

			if (vt.HasField ("route")) {
				
				List<string> edges = SumoUtils.SumoAttributeStringToStringList (vt["route"]["_attributes"]["edges"].str);
				List<VenerisRoad> ro = new List<VenerisRoad> ();
				foreach (string e in edges) {
					//Debug.Log (e);
					ro.Add (edgeIdToVenerisRoadDictionary [e]);
				}


				i.SetRouteRoads (ro);

			}

			m.AddVehicleGenerationInfo (i);
		}
	}
}
