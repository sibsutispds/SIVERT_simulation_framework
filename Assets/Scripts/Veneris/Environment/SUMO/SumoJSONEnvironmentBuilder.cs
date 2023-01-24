/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Veneris
{
	public class SumoJSONEnvironmentBuilder : SumoEnvironmentBuilder
	{

		public JSONObject polygons;
		public string jsPath = "D:\\Users\\eegea\\MyDocs\\investigacion\\Unity\\builds\\wglbuilder\\osm.poly.json";
//		public bool GSCM = true;

		public override void BuildPolygons (SumoBuilder builder)
		{



			this.builder = builder;
			root = new GameObject ("Environment");
			osmIdToJSON = new Dictionary<long, JSONObject> ();
			var watch = System.Diagnostics.Stopwatch.StartNew ();
			Debug.Log ("Parsing JSON sumo environment");


			#if UNITY_EDITOR
			polygons = new JSONObject (System.IO.File.ReadAllText (jsPath));
			#else 
			polygons = new JSONObject (JavaScriptInterface.ReadJSONBuilderFile (3));
			#endif
			watch.Stop ();
			Debug.Log ("Time to read JSON environment file=" + (watch.ElapsedMilliseconds / 1000f) + " s");

			if (string.IsNullOrEmpty (pathToOSMJSON)) {
				DownloadOSMBuildingData ();
			} else {
				#if UNITY_EDITOR
				osmJSONData = new JSONObject (File.ReadAllText (pathToOSMJSON));
				#else
				osmJSONData = new JSONObject (JavaScriptInterface.ReadJSONBuilderFile (4));
				#endif
				CreateOSMWayDictionary ();
				ProcessPolygons ();

			}
			if (buildTrafficLights) {
				BuildTrafficLightPosts ();
			}
		}

		protected override void ProcessPolygons ()
		{
			List<JSONObject> pols;
			if (polygons ["additional"] ["poly"].IsArray) {
				pols = polygons ["additional"] ["poly"].list;
			} else if (polygons ["additional"] ["poly"].IsObject) {
				pols = new List<JSONObject> ();
				pols.Add (polygons ["additional"] ["poly"]);
			} else {
				Debug.Log ("JSON builder: no polygons found");
				throw new UnityException ("JSON builder: no polygons found");

				return;
			}
			foreach (JSONObject o in pols ) {
				string type = o ["_attributes"] ["type"].str;
				//if (type.Contains ("landuse") || type.Contains("amenity") || type.Contains("population") || type.Contains("man_made")) {
				if (InNoBuildList(type))  {
					//Do not create landuses
					continue;
				}
				GameObject polygon = CreatePolygon (o ["_attributes"] ["shape"].str, type, o ["_attributes"] ["id"].str);
				

				if (polygon != null) {
					SetPolygonMaterial (polygon, type, o ["_attributes"] ["color"].str);
					polygon.transform.SetParent (root.transform);
					if (GSCM)
					{
						polygon.tag = "GSCMmesh";
					}
					polygon.AddComponent<MeshCollider>();
				}

			}
		}
		protected override void SetPolygonMaterial (GameObject go, string type, string colorVal)
		{
			//TODO: to let it be used in the editor. Rewrite all of this
			#if UNITY_EDITOR
			//Debug.Log("SetPolygonMaterial called on editor");
			Material mat  = UnityEditor.AssetDatabase.LoadAssetAtPath<Material> ("Assets/Resources/Materials/" + type+".mat");
			if (mat == null) {
				mat = new Material (Shader.Find ("Standard"));
				mat.EnableKeyword ("_NORMALMAP");
				// Create a new 2x2 texture ARGB32 (32 bit with alpha) and no mipmaps
				Texture2D texture = new Texture2D (2, 2, TextureFormat.ARGB32, false);
				string[] colorvalues = colorVal.Split (',');
				Color32 color = Color.gray;
				if (colorvalues.Length > 1) {

					color = new Color32 (byte.Parse (colorvalues [0]), byte.Parse (colorvalues [1]), byte.Parse (colorvalues [2]), 255);
				} 
				// set the pixel values
				texture.SetPixel (0, 0, color);
				texture.SetPixel (1, 0, color);
				texture.SetPixel (0, 1, color);
				texture.SetPixel (1, 1, color);

				// Apply all SetPixel calls
				texture.Apply ();


				mat.color=color;
				UnityEditor.AssetDatabase.CreateAsset (mat, "Assets/Resources/Materials/" + type+".mat");
				UnityEngine.Debug.Log ("New material created" + UnityEditor.AssetDatabase.GetAssetPath (mat));


			} 
			go.GetComponent<MeshRenderer> ().material = mat;
			#else
			base.SetPolygonMaterial (go, type, colorVal);
			#endif
		}

		protected override void DownloadOSMBuildingData ()
		{
			string query = "[out:json];(";
			foreach (JSONObject o in polygons["additional"]["poly"].list) {

				string type = o ["_attributes"] ["type"].str;
				string idpol=o ["_attributes"] ["id"].str;
				if (type.Contains ("building")) {

					if ( idpol.Contains ("#")) {
						int position = idpol.LastIndexOf ('#');
						string id = idpol.Substring (0, position);
						query += "way(" + id + ");";
					} else {
						query += "way(" +idpol + ");";
					}
				}


			}
			query += "); (._;>;); out body;";
			StartCoroutine(RunOverpassQuery (query, DownloadOSMDataFinished));
		}
	}
}
