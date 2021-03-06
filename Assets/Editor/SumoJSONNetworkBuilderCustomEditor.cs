using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Veneris
{
	[CustomEditor (typeof(SumoJSONNetworkBuilder))]
	public class SumoJSONNetworkBuilderCustomEditor : Editor
	{
		SumoJSONNetworkBuilder _target;
		GUIStyle style = new GUIStyle ();

		void OnEnable ()
		{

			style.fontStyle = FontStyle.Bold;
			style.normal.textColor = Color.white;
			_target = (SumoJSONNetworkBuilder)target;


		}

		protected void StartBuilding ()
		{
			var watch = System.Diagnostics.Stopwatch.StartNew ();
			Debug.Log ("BUILDING NETWORK. It may take some time...");
			_target.BuildNetwork (_target.GetComponentInParent<SumoBuilder> ());
			watch.Stop ();
			Debug.Log ("Time to build network=" + (watch.ElapsedMilliseconds / 1000f) + " s");
		}

		public override void OnInspectorGUI ()
		{
			DrawDefaultInspector ();
			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Build Network")) {


				if (_target.networkRoot != null) {
					if (EditorUtility.DisplayDialog ("Network exists", "A network already exists. It will be kept and a new one will be created and the root replaced for this component. Existing traffic lights  in Environment must be rebuilt", "Replace", "Cancel")) {

						StartBuilding ();

					}

				} else {
					StartBuilding ();
				}


			}
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
			using (new EditorGUI.DisabledScope (_target.networkRoot == null)) {
				if (GUILayout.Button ("Build Global Network Manager")) {
					_target.CreateGlobalRouteManager ();
				}
			}
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Build SumoConnection List")) {
				var watch = System.Diagnostics.Stopwatch.StartNew ();
				Debug.Log ("BUILDING SumoConnectionList. It may take some time...");
				_target.BuildConnectionList ();
				watch.Stop ();
				Debug.Log ("Time to build SumoConnectionList=" + (watch.ElapsedMilliseconds / 1000f) + " s");
			}
			EditorGUILayout.EndHorizontal ();
		}
	}
}
