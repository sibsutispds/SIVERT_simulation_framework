/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Veneris
{
	
	public class SumoEnvironmentBuilderOnEditor : SumoEnvironmentBuilder
	{
		//Use the assetdatabase if we are on the editor
	
		protected override void SetPolygonMaterial (GameObject go, string type, string colorVal)
		{
			Material mat  = AssetDatabase.LoadAssetAtPath<Material> ("Assets/Resources/Materials/" + type+".mat");
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
				AssetDatabase.CreateAsset (mat, "Assets/Resources/Materials/" + type+".mat");
				UnityEngine.Debug.Log ("New material created" + AssetDatabase.GetAssetPath (mat));


			} 
			go.GetComponent<MeshRenderer> ().material = mat;

		}

		//Write to file if we are in editor

		public override JSONObject GetOSMDataAsJSON (string text)
		{
			if (!string.IsNullOrEmpty (pathToPolys)) {
				string directoryPath = pathToPolys.Substring(0,pathToPolys.LastIndexOf ('/'));
				UnityEngine.Debug.Log (text);
				pathToOSMJSON = directoryPath + "/osm_bbox.json";
				UnityEngine.Debug.Log ("Writing JSON data to " + pathToOSMJSON);
				System.IO.FileStream m_FileStream = new System.IO.FileStream (pathToOSMJSON, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite);
				System.IO.StreamWriter m_StreamWriter = new System.IO.StreamWriter (m_FileStream);
				m_StreamWriter.WriteLine (text);

				m_StreamWriter.Flush ();
				m_StreamWriter.Close ();


				m_FileStream.Close ();

			}
			return new JSONObject (text);

		}

		protected override UnityEngine.Mesh GetComponentMesh (GameObject go)
		{
			return go.GetComponent<MeshFilter> ().sharedMesh;
		}

		protected override GameObject LoadPostPrefab (int lanes)
		{
			
			if (lanes > 1) {
				//return AssetDatabase.LoadAssetAtPath<GameObject> ("Assets/Prefabs/Signs/BasicTrafficLightPost"+lanes+".prefab");
				return AssetDatabase.LoadAssetAtPath<GameObject> ("Assets/Resources/Prefabs/Signs/BasicTrafficLightPost2.prefab");
			} else {
				return AssetDatabase.LoadAssetAtPath<GameObject> ("Assets/Resources/Prefabs/Signs/BasicTrafficLightPost.prefab");
			}
		}

		protected override GameObject LoadTrafficLightPrefab ()
		{
			return AssetDatabase.LoadAssetAtPath<GameObject> ("Assets/Resources/Prefabs/Signs/BasicTrafficLight.prefab");
		}
	}
}
#endif
