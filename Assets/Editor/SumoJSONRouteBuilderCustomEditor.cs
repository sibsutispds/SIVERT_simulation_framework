using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Veneris
{
	[CustomEditor (typeof(SumoJSONRouteBuilder))]
	public class SumoJSONRouteBuilderCustomEditor : Editor
	{

		SumoJSONRouteBuilder _target;
		GUIStyle style = new GUIStyle ();
		bool networkExists = false;
		SumoBuilder builder = null;
		//long pathSought = -1;
		// Use this for initialization
		void OnEnable ()
		{

			style.fontStyle = FontStyle.Bold;
			style.normal.textColor = Color.white;
			_target = (SumoJSONRouteBuilder)target;
			builder = _target.GetComponentInParent<SumoBuilder> ();
			if (builder.networkBuilder != null) {
				if (builder.networkBuilder.networkRoot != null) {
					networkExists = true;
				}
			}


		}

		public override void OnInspectorGUI ()
		{
			DrawDefaultInspector ();

			using (new EditorGUI.DisabledScope (networkExists == false)) {
				EditorGUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Build Sumo Vehicle Manager")) {

					_target.BuildRoutes (builder);

				}
				EditorGUILayout.EndHorizontal ();
			}

		}
	}
}
