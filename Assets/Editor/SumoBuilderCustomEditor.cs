/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Veneris {
	[CustomEditor(typeof(SumoBuilderOnEditor))]
public class SumoBuilderCustomEditor : Editor {

		SumoBuilderOnEditor _target;
		GUIStyle style = new GUIStyle();
		long pid=0;
		string roadid="";
		//long pathSought = -1;
		// Use this for initialization
		void OnEnable(){

			style.fontStyle = FontStyle.Bold;
			style.normal.textColor = Color.white;
			_target = (SumoBuilderOnEditor)target;


		}
		public override void OnInspectorGUI(){

			EditorGUILayout.BeginHorizontal();
			_target.useJSONFiles = EditorGUILayout.Toggle ("Use JSON files",_target.useJSONFiles);
			EditorGUILayout.EndHorizontal();	
			EditorGUILayout.BeginHorizontal();

			_target.pathToNet = EditorGUILayout.TextField("Network File", _target.pathToNet);

			EditorGUILayout.EndHorizontal();	

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Select network file")) {
				_target.pathToNet = EditorUtility.OpenFilePanel ("Select network", "","xml,json");
			}
			EditorGUILayout.EndHorizontal ();





			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Create NetworkBuilder")) {
				_target.LoadSumoNetworkToEditor ();
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal();
			//EditorGUILayout.PrefixLabel("Node Count");
			_target.pathToRoutes = EditorGUILayout.TextField("Routes File", _target.pathToRoutes);
			EditorGUILayout.EndHorizontal();	

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Select routes file")) {
				_target.pathToRoutes = EditorUtility.OpenFilePanel ("Select routes", "","");
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Create RouteBuilder")) {
				_target.LoadSumoRouteBuilderToEditor ();
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal();
			//EditorGUILayout.PrefixLabel("Node Count");
			_target.pathToPolys = EditorGUILayout.TextField("Polygons File", _target.pathToPolys);
			EditorGUILayout.EndHorizontal();	

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Select polygons file")) {
				_target.pathToPolys = EditorUtility.OpenFilePanel ("Select polygons", "","");
			}
			EditorGUILayout.EndHorizontal ();


			/*EditorGUILayout.BeginHorizontal();
			//EditorGUILayout.PrefixLabel("Node Count");
			_target.pathToOSMJSON= EditorGUILayout.TextField("OSM File", _target.pathToOSMJSON);
			EditorGUILayout.EndHorizontal();


			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Select OSM file")) {
				_target.pathToOSMJSON = EditorUtility.OpenFilePanel ("Select OSM", "","*");
			}
			EditorGUILayout.EndHorizontal ();
			*/

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Create EnvironmentBuilder")) {
				_target.LoadPolygonsToEditor ();
			}

			EditorGUILayout.EndHorizontal ();

			/*EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Build complete scenario")) {
				_target.BuildScenario ();
			}

			EditorGUILayout.EndHorizontal ();

*/
			EditorGUILayout.BeginHorizontal();
			 pid = EditorGUILayout.LongField ("Path id", pid);
			if (GUILayout.Button("Find Path")) {
				Selection.activeGameObject=_target.FindPath (pid);
			}

			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal();
			roadid = EditorGUILayout.TextField ("Road Sumo Id", roadid);
			if (GUILayout.Button("Find Road")) {
				Selection.activeGameObject=_target.FindRoad (roadid);
			}

			EditorGUILayout.EndHorizontal ();
			/*if (GUILayout.Button("DownloadOSMJSONData")) {
					_target.ReadPolygons ();
					_target.DownloadOSMData (_target.GetLocationElement ().origBoundary);

			}*/
			/*EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Find Path")) {
				Path[] paths = UnityEngine.GameObject.FindObjectsOfType (typeof(Path)) as Path[]; 
				foreach (Path p in paths) {
					if (p.pathId == pathSought) {
						Selection.activeGameObject = p.gameObject;
					}
				}


			}
			EditorGUILayout.EndHorizontal ();
			pathSought = EditorGUILayout.LongField ("Path id to find", pathSought);
			*/


		}
}
}
