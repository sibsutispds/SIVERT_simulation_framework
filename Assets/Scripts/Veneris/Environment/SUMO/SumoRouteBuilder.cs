/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using Veneris.Routes;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine.Assertions;


namespace Veneris
{
	public class SumoRouteBuilder : MonoBehaviour
	{
		public string pathToRoutes = "";
		protected routesType routes = null;
		public SumoBuilder builder = null;

		protected Dictionary<string, VenerisRoad> edgeIdToVenerisRoadDictionary = null;

		public virtual GameObject LoadVehiclePrefab ()
		{
//			return Resources.Load ("Prefabs/Vehicles/STDRSCar") as GameObject;
			return Resources.Load ("Prefabs/Vehicles/GSCMVenerisCar_s90") as GameObject;
		}

		public virtual void BuildRoutes (SumoBuilder builder)
		{
			this.builder = builder;
			Debug.Log ("Building routes. This may take time...");
			GameObject go = new GameObject ("SumoVehicleManager");
			SumoVehicleManager manager = go.AddComponent<SumoVehicleManager> ();

			XmlSerializer serializer = new XmlSerializer (typeof(routesType));
			XmlReader reader = XmlReader.Create (pathToRoutes);
			try {

				routes = (routesType)serializer.Deserialize (reader);
				//We need to recreate this in case we are in the editor
				reader.Close ();
				edgeIdToVenerisRoadDictionary = builder.GetEdgeIdToVenerisRoadDictionary ();


				var watch = System.Diagnostics.Stopwatch.StartNew ();
				//According to the SUMO xsd routes_file.xsd, only one type of the elements can be present: use of xsd:choice. However, the generated files by duarouter includes different elements..
				int vehicleCounter = 0;
				foreach (object o in routes.Items) {
					if (o.GetType () == typeof(vehicleType)) {


						ScheduleVehicle ((vehicleType)o, manager);
						++vehicleCounter;

					}
				}
				watch.Stop ();
				Debug.Log ("Using routes file: " + pathToRoutes + ". " + vehicleCounter + " vehicles scheduled. Time to build=" + (watch.ElapsedMilliseconds / 1000f) + " s");

				go.AddComponent<GenerationInfo> ().SetGenerationInfo ("SumoVehicleManager generated from " + pathToRoutes + ". " + vehicleCounter + " vehicles scheduled. Time to build=" + (watch.ElapsedMilliseconds / 1000f) + " s");
				manager.vehiclePrefab = LoadVehiclePrefab ();
			} catch (System.Exception e) {
				Debug.LogError (e);
				reader.Close ();
			}
		}

	
		protected void ScheduleVehicle (vehicleType vt, SumoVehicleManager m)
		{
			//Debug.Log ("scheduling vehicle " + vt.id);
			int id = 0;
			if (m.vehicleGenerationList != null) {
				id = m.vehicleGenerationList.Count;
			}
			//Debug.Log (" vehicle id=" + id );
			VehicleGenerationInfo i = new VehicleGenerationInfo (id, VehicleGenerationInfo.SumoVehicleTypes.passenger, SumoUtils.StringToFloat (vt.depart), vt.departLane);

			vehicleRouteType route = vt.Item as vehicleRouteType;
			if (route != null) {
				List<string> edges = SumoUtils.SumoAttributeStringToStringList (route.edges);
				List<VenerisRoad> ro = new List<VenerisRoad> ();
				foreach (string e in edges) {
					
					ro.Add (edgeIdToVenerisRoadDictionary [e]);
				}

			
				i.SetRouteRoads (ro);
			}

			m.AddVehicleGenerationInfo (i);
		}
	





	}
}
