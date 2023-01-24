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
	[CustomEditor(typeof(SumoBuilder))]
public class SumoBuilderEditor : Editor {

		SumoBuilder _target;
		GUIStyle style = new GUIStyle();
		// Use this for initialization
		void OnEnable(){

			style.fontStyle = FontStyle.Bold;
			style.normal.textColor = Color.white;
			_target = (SumoBuilder)target;


		}
		public override void OnInspectorGUI(){
			EditorGUILayout.BeginHorizontal();
			//EditorGUILayout.PrefixLabel("Node Count");
			_target.pathToNet = EditorGUILayout.TextField("Network File", _target.pathToNet);
			EditorGUILayout.EndHorizontal();	
			if (GUILayout.Button("Select network file")) {
				_target.pathToNet = EditorUtility.OpenFilePanel ("Select network", "", "net.xml");
			}
		

		}

}
}
