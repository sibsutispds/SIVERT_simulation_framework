/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Veneris
{
	[CustomEditor (typeof(SumoEnvironmentBuilderOnEditor))]
	public class SumoEnviromentBuilderCustomEditor : Editor
	{

		SumoEnvironmentBuilderOnEditor _target;
		GUIStyle style = new GUIStyle ();

		//long pathSought = -1;
		// Use this for initialization
		void OnEnable ()
		{

			style.fontStyle = FontStyle.Bold;
			style.normal.textColor = Color.white;
			_target = (SumoEnvironmentBuilderOnEditor)target;


		}

		public override void OnInspectorGUI ()
		{
			DrawDefaultInspector ();
			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Build Only Traffic Lights")) {
				var watch = System.Diagnostics.Stopwatch.StartNew ();
				Debug.Log ("Building Traffic lights...");
				_target.BuildOnlyTrafficLights (_target.GetComponentInParent<SumoBuilder>());
				watch.Stop ();
				Debug.Log ("Time to build traffic lights=" + (watch.ElapsedMilliseconds / 1000f) + " s");
			}
			if (GUILayout.Button ("Build Environment")) {
				var watch = System.Diagnostics.Stopwatch.StartNew ();
				Debug.Log ("Building Environment...");
				_target.BuildPolygons (_target.GetComponentInParent<SumoBuilder>());
				watch.Stop ();
				Debug.Log ("Time to build environment=" + (watch.ElapsedMilliseconds / 1000f) + " s");
			}
			EditorGUILayout.EndHorizontal ();
		}
	}

}

