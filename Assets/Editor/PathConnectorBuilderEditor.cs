/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;
using UnityEditor;
namespace Veneris {
	[CustomEditor(typeof(PathConnectorBuilder))]
	public class PathConnectorBuilderEditor : Editor {
		PathConnectorBuilder _target;
		GUIStyle style = new GUIStyle();
		// Use this for initialization

		SerializedObject start;
		void OnEnable(){

			style.fontStyle = FontStyle.Bold;
			style.normal.textColor = Color.white;
			_target = (PathConnectorBuilder)target;


		}
		public override void OnInspectorGUI(){	
			bool allowScene = !EditorUtility.IsPersistent (_target);
			EditorGUILayout.PrefixLabel("Segment 1");
			_target.startSegment1 = (Transform)EditorGUILayout.ObjectField ("Start Segment 1", _target.startSegment1, typeof(Transform), allowScene);
			_target.endSegment1 = (Transform)EditorGUILayout.ObjectField ("End Segment 1", _target.endSegment1, typeof(Transform), allowScene);
			EditorGUILayout.PrefixLabel("Segment 2");
			_target.startSegment2 = (Transform)EditorGUILayout.ObjectField ("Start Segment 2", _target.startSegment2, typeof(Transform), allowScene);
			_target.endSegment2 = (Transform)EditorGUILayout.ObjectField ("End Segment 1", _target.endSegment2, typeof(Transform), allowScene);
			EditorGUILayout.PrefixLabel("Connectors");
			_target.connectionPoint1 = (Transform)EditorGUILayout.ObjectField ("Connector 1", _target.connectionPoint1, typeof(Transform), allowScene);
			_target.connectionPoint2 = (Transform)EditorGUILayout.ObjectField ("Connector 2", _target.connectionPoint2, typeof(Transform), allowScene);
			_target.turningPoint = (Transform)EditorGUILayout.ObjectField ("Turning Point", _target.turningPoint, typeof(Transform), allowScene);
			if (GUILayout.Button("Build Path")) {
				_target.CreateNewPath ();
			}
		}
	
	
}
}
