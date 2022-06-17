/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayRealRoutes : MonoBehaviour {

	// Use this for initialization
	public string posFile = "D:\\Users\\eegea\\MyDocs\\investigacion\\MATLAB\\veneris\\pasubio50\\half\\position.txt";
	public string sumoFile = "D:\\Users\\eegea\\MyDocs\\investigacion\\MATLAB\\veneris\\pasubio50\\half\\sumo\\fcd.gps";
	public int vehicleId = 73;
	public LineRenderer routeRenderer = null;
	public LineRenderer routeRenderer2 = null;
	public GameObject routeRendererPrefab = null;
	void Start () {
		List<Vector3> positions = new List<Vector3> ();

		string line;
		char[] separator = new char[]{ '\t' };
		System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.GetCultureInfo ("es-ES");

		float totalLength=0;
		int i = 0;
		if (!string.IsNullOrEmpty (posFile)) {
			using (System.IO.StreamReader file = new System.IO.StreamReader (posFile, System.Text.Encoding.ASCII)) {
				while ((line = file.ReadLine ()) != null) {  
			
					//Debug.Log (line);
					string[] tokens = line.Split (separator);
					//Debug.Log ("tokens=" + tokens.Length);
					if (int.Parse (tokens [0]) == vehicleId) {
						Vector3 p = new Vector3 (float.Parse (tokens [2], ci), float.Parse (tokens [3], ci), float.Parse (tokens [4], ci));
						positions.Add (p);
						if (i >= 1) {
							totalLength += Vector3.Magnitude (positions [i] - positions [i - 1]);
						}
						i++;
					}
				

				}  

		
			}
	
			Debug.Log ("Veneris: " + positions.Count + " positions added. Route length=" + totalLength);
			if (positions.Count > 0) {
				if (routeRendererPrefab == null) {
					routeRendererPrefab = Resources.Load<GameObject> ("UI/RouteRenderer");
				}
				if (routeRenderer == null) {
					GameObject rr = Instantiate (routeRendererPrefab);
					routeRenderer = rr.GetComponent<LineRenderer> ();
				}
				//Update points
				/*
				routeRenderer = gameObject.AddComponent<LineRenderer> ();
				*/
		
				//ailogic.Log ("Diplay route points " + points.Count);
				/*
				routeRenderer.startColor = Color.blue;
				routeRenderer.endColor = Color.blue;
				*/
				routeRenderer.positionCount = positions.Count;
				routeRenderer.SetPositions (positions.ToArray ());


				routeRenderer.name = "Vehicle " + vehicleId + " trajectory";


				//Update pints

				routeRenderer.enabled = true;
			}
		}
		positions.Clear ();
		totalLength=0;
		i = 0;
		if (!string.IsNullOrEmpty (sumoFile)) {
			using (System.IO.StreamReader file = new System.IO.StreamReader (sumoFile, System.Text.Encoding.ASCII)) {
				while ((line = file.ReadLine ()) != null) {  

					//Debug.Log (line);
					string[] tokens = line.Split (separator);
					//Debug.Log ("tokens=" + tokens.Length);
					if (int.Parse (tokens [0]) == vehicleId) {
						Vector3 p = new Vector3 (float.Parse (tokens [2]), 0.6f, float.Parse (tokens [3]));
						positions.Add (p);
						if (i >= 1) {
							totalLength += Vector3.Magnitude (positions [i] - positions [i - 1]);
						}
						i++;
					}


				}  


			}
			Debug.Log ("Sumo: " + positions.Count + " positions added. Route length=" + totalLength);
			if (positions.Count > 0) {
				if (routeRendererPrefab == null) {
					routeRendererPrefab = Resources.Load<GameObject> ("UI/RouteRenderer");
				}
				if (routeRenderer2 == null) {
					GameObject sumor = Instantiate (routeRendererPrefab);
					routeRenderer2 = sumor.GetComponent<LineRenderer> ();
				}
				//Update points
				/*
				routeRenderer = gameObject.AddComponent<LineRenderer> ();
				*/

				//ailogic.Log ("Diplay route points " + points.Count);
				/*
				routeRenderer.startColor = Color.blue;
				routeRenderer.endColor = Color.blue;
				*/
				routeRenderer2.positionCount = positions.Count;
				routeRenderer2.SetPositions (positions.ToArray ());




				//Update pints

				routeRenderer2.enabled = true;
			}
		}
		
	}

	// Update is called once per frame
//	void Update () {
//		
//	}
}
