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
	[CustomEditor(typeof(SimulationManager))]
	public class SimulationManagerEditor : Editor
	{
		SimulationManager _target;
		GUIStyle style = new GUIStyle();
		// Use this for initialization
		void OnEnable(){

			style.fontStyle = FontStyle.Bold;
			style.normal.textColor = Color.white;
			_target = (SimulationManager)target;


		}
		public override void OnInspectorGUI(){
			EditorGUILayout.BeginHorizontal();
			//EditorGUILayout.PrefixLabel("Node Count");
			_target.outputPath = EditorGUILayout.TextField("Output directory", _target.outputPath);
			EditorGUILayout.EndHorizontal();	
			if (GUILayout.Button("Select output file")) {
				_target.outputPath = EditorUtility.OpenFolderPanel ("Select output directory","","");
			}
			DrawDefaultInspector ();

		}
	}
}
