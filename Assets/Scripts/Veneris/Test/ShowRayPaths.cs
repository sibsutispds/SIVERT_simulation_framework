/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Opal
{
	public class ShowRayPaths : MonoBehaviour
	{

		public enum Mode
		{
			Range,
			Ray,
			All}

		;
		public Color c=Color.red;

		public Mode mode;
		public string pFile = "D:\\Users\\eegea\\MyDocs\\investigacion\\MATLAB\\veneris\\opal\\LoS\\shits.txt";
		public string init = "";
		public string end = "";
		public GameObject rayPrefab;
		public string ray;
		//public Vector3 transmitterPosition;
		public GameObject transmitter;
		// Use this for initialization
		void Start ()
		{
			Debug.Log ("Start ShowRayPaths");


			if (mode == Mode.Ray) {
				FindRay ();
			} else if (mode == Mode.Range) {
				ShowRange ();
			} else {
				ShowAll ();
			}


		}

		public void ShowRay (string line)
		{
			List<Vector3> positions = new List<Vector3> ();
			string[] rayDir = line.Split (':');
			char[] separator = new char[]{ '\t' };
			System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.GetCultureInfo ("en-US");
			//GameObject go=new GameObject("ray");
			GameObject go = Instantiate (rayPrefab);
			LineRenderer lr = go.GetComponent<LineRenderer> ();
			lr.startColor = c;
			lr.endColor = c;

			positions.Add (transmitter.transform.position);
			string[] hitpoints = rayDir [1].Split ('|');
			for (int j = 0; j < hitpoints.Length; j++) {
				string[] tokens = hitpoints [j].Split (separator);
				//Debug.Log ("tokens=" + tokens.Length);

				Vector3 p = new Vector3 (float.Parse (tokens [0], ci), float.Parse (tokens [1], ci), float.Parse (tokens [2], ci));
				//Debug.Log ("p=" + p);

				positions.Add (p);
			}

			if (positions.Count > 0) { 

				lr.positionCount = positions.Count;
				lr.SetPositions (positions.ToArray ());
				lr.enabled = true;
				go.name = "ray " + rayDir [0];
					

			}



			  

		}

		public void FindRay ()
		{
			string line;
			Debug.DrawRay (new Vector3 (10f, 3.7164f, 85.8302f), new Vector3(-0.5755f,0.2923f,0.7637f)*30f, Color.blue,30f);
			using (System.IO.StreamReader file = new System.IO.StreamReader (pFile, System.Text.Encoding.ASCII)) {
				while ((line = file.ReadLine ()) != null) {  

					//Debug.Log (line);
					string[] rayDir = line.Split (':');
					if (rayDir [0].Equals (ray)) {
						ShowRay (line);
			

						break;

					}  
	
				}
			}
		}

		public void ShowRange ()
		{
			Debug.Log ("Showing range from " + init);
			/*if (string.IsNullOrEmpty (ray)) {
				Debug.Log ("No ray set");
				return;
			}*/


			string line;
			int i = 0;
			bool found = false;

			using (System.IO.StreamReader file = new System.IO.StreamReader (pFile, System.Text.Encoding.ASCII)) {

				while ((line = file.ReadLine ()) != null) {
					string[] rayDir = line.Split (':');
					if (rayDir [0].Equals (init)) {
						found = true;
					} 
					if (found) {
						i++;
						ShowRay (line);
					}

					if (rayDir [0].Equals (end)) {
						break;
					}

				}

				
			}

			Debug.Log (i + " line renderers created");
		}

		public void ShowAll ()
		{
			string line;
			int i = 0;
			bool found = false;

			using (System.IO.StreamReader file = new System.IO.StreamReader (pFile, System.Text.Encoding.ASCII)) {

				while ((line = file.ReadLine ()) != null) {

					i++;
					ShowRay (line);


				}


			}
			Debug.Log (i + " line renderers created");

		}
	}

}