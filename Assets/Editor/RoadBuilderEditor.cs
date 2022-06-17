/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;
using UnityEditor;
namespace Veneris {
[CustomEditor(typeof(RoadBuilder))]
public class RoadBuilderEditor : Editor {
	RoadBuilder _target;
	GUIStyle style = new GUIStyle();
	// Use this for initialization
	void OnEnable(){

		style.fontStyle = FontStyle.Bold;
		style.normal.textColor = Color.white;
		_target = (RoadBuilder)target;


	}
	

	public override void OnInspectorGUI(){	
		EditorGUILayout.BeginHorizontal();
		//EditorGUILayout.PrefixLabel("Node Count");
		_target.roadId = EditorGUILayout.IntField("Road ID", _target.roadId);
		EditorGUILayout.EndHorizontal();



		EditorGUILayout.BeginHorizontal();
		//EditorGUILayout.PrefixLabel("Node Count");
		_target.ways = (VenerisRoad.Ways) EditorGUILayout.EnumPopup("Ways", _target.ways);
		EditorGUILayout.EndHorizontal();

	
		EditorGUILayout.BeginHorizontal();
		//EditorGUILayout.PrefixLabel("Node Count");
		_target.lanesPerWay = EditorGUILayout.IntField("Lanes per Way", _target.lanesPerWay);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		//EditorGUILayout.PrefixLabel("Node Count");
		_target.laneWidth = Mathf.Max(3, EditorGUILayout.FloatField("Lane width", _target.laneWidth));
		EditorGUILayout.EndHorizontal();

		//EditorGUILayout.BeginHorizontal();
		//EditorGUILayout.PrefixLabel("Node Count");
		//_target.addLanePaths =  EditorGUILayout.Toggle("Add lane paths", _target.addLanePaths);
		//EditorGUILayout.EndHorizontal();



		EditorGUILayout.BeginHorizontal();
		//EditorGUILayout.PrefixLabel("Node Count");
		_target.roadHeight = Mathf.Max(0f, EditorGUILayout.FloatField("Road height", _target.roadHeight));
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		//EditorGUILayout.PrefixLabel("Node Count");
		_target.addShoulders =  EditorGUILayout.Toggle("Add shoulders", _target.addShoulders);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		//EditorGUILayout.PrefixLabel("Node Count");
		_target.shoulderWidth = Mathf.Max(0f, EditorGUILayout.FloatField("Shoulder width", _target.shoulderWidth));
		EditorGUILayout.EndHorizontal();


		EditorGUILayout.BeginHorizontal();
		//EditorGUILayout.PrefixLabel("Node Count");
		_target.meshSections = EditorGUILayout.IntField("Road sections", _target.meshSections);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		//EditorGUILayout.PrefixLabel("Node Count");
		_target.flattenTerrain =  EditorGUILayout.Toggle("Flatten terrain", _target.flattenTerrain);
		EditorGUILayout.EndHorizontal();


		if (GUILayout.Button("Build Road")) {
			_target.Build ();
		}


		//update and redraw:
		if(GUI.changed){
			EditorUtility.SetDirty(_target);			
		}
	}

	}

}
