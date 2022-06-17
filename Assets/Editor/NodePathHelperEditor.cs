/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Veneris
{
	[CustomEditor (typeof(NodePathHelper), true)]
	public class NodePathHelperEditor : Editor
	{
		NodePathHelper _target;
		GUIStyle style = new GUIStyle ();
		// Use this for initialization
		void OnEnable ()
		{

			style.fontStyle = FontStyle.Bold;
			style.normal.textColor = Color.white;
			_target = (NodePathHelper)target;


		}


		public override void OnInspectorGUI ()
		{		
			//draw the path?
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.PrefixLabel ("Path Id");
			_target.pathId = EditorGUILayout.LongField (_target.pathId);
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.PrefixLabel ("Path Visible");
			_target.pathVisible = EditorGUILayout.Toggle (_target.pathVisible);
			EditorGUILayout.EndHorizontal ();

			//path name:
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.PrefixLabel ("Path Name");
			_target.pathName = EditorGUILayout.TextField (_target.pathName);
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.PrefixLabel ("Path Length");
			EditorGUILayout.LabelField (_target.totalPathLength.ToString ());
			EditorGUILayout.EndHorizontal ();

			//path color:
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.PrefixLabel ("Path Color");
			_target.pathColor = EditorGUILayout.ColorField (_target.pathColor);
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.PrefixLabel ("Draw Normals");
			_target.drawNormals = EditorGUILayout.Toggle (_target.drawNormals);
			EditorGUILayout.EndHorizontal ();
			//exploration segment count control:
			EditorGUILayout.BeginHorizontal ();

			Mathf.Max (1, EditorGUILayout.IntField ("Node Count", _target.nodeCount));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			//EditorGUILayout.PrefixLabel("Node Count");
			_target.interpolatedPointsPerSegment = Mathf.Max (1, EditorGUILayout.IntField ("Interpolated Points per Segment", _target.interpolatedPointsPerSegment));
			EditorGUILayout.EndHorizontal ();

			/*EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Bind to GameObject Transform");
		_target.bindToTransform = EditorGUILayout.Toggle(_target.bindToTransform);
		EditorGUILayout.EndHorizontal();
		*/

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.PrefixLabel ("Bind to Terrain");
			_target.bindToTerrain = EditorGUILayout.Toggle (_target.bindToTerrain);
			EditorGUILayout.EndHorizontal ();




			if (GUILayout.Button ("Add node")) {
				//_target.nodes.Add(_target.nodes[_target.nodes.Count-1]+new Vector3(10,10,10));
				_target.AddNode ();
			}

			if (GUILayout.Button ("Make loop")) {
				//Add a new node equal to the first one
				//_target.nodes.Add(_target.nodes[0]);
				_target.MakeLoop ();

			}
			if (GUILayout.Button ("Reverse Path")) {
				_target.ReversePath ();
			}



			if (_target.bindToTerrain) {

				if (Terrain.activeTerrain == null) {
					if (EditorUtility.DisplayDialog ("Bind to Terrain", "There is no active terrain in the scene", "OK", "")) {
						_target.bindToTerrain = false;
					}
				}

			}



			//node display:
			EditorGUI.indentLevel = 4;
			if (_target.nodeCount > 0) {
				for (int i = 0; i < _target.nodes.Count; i++) {
					_target.nodes [i].transform.position = EditorGUILayout.Vector3Field ("Node " + i, _target.nodes [i].transform.position);

				}
			}
			EditorGUI.indentLevel = 1;
			DrawDefaultInspector ();
			//update and redraw:
			if (GUI.changed) {
				EditorUtility.SetDirty (_target);			
			}
		}

		void OnSceneGUI ()
		{
			if (_target.bindToTerrain) {
				_target.BindToTerrain ();
			}
			if (_target.pathVisible) {			
				if (_target.nodeCount > 0) {
					Handles.color = Color.green;
					//allow path adjustment undo:
					Undo.SetSnapshotTarget (_target, "Adjust Agent Path");

					//path begin and end labels:
					Handles.Label (_target.nodes [0].transform.position, "Begin", style);
					Handles.Label (_target.nodes [_target.nodes.Count - 1].transform.position, "End", style);


					//Tie node[0] to AgentPath transform
					//if (	_target.bindToTransform && _target.transform.hasChanged) {
					//	_target.nodes [0] = _target.transform.position;
					//}




					//node handle display:
					for (int i = 0; i < _target.nodes.Count; i++) {
						/*if (_target.bindToTerrain) {

						Vector3 aux = _target.nodes [i].transform.position;
						aux.y= Terrain.activeTerrain.SampleHeight (_target.nodes [i].transform.position);
						_target.nodes[i].transform.position = aux;




					}*/

						_target.nodes [i].transform.position = Handles.PositionHandle (_target.nodes [i].transform.position, _target.nodes [i].transform.rotation);
					}	
				}	
			}

		}
	}
}

