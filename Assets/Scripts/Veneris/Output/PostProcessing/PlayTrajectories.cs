/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Veneris
{
	public class PlayTrajectories : MonoBehaviour
	{



		public string posFile = "D:\\Users\\eegea\\MyDocs\\investigacion\\MATLAB\\veneris\\valencia\\pruebaLights\\position.txt";
		public string sumoFile = "D:\\Users\\eegea\\MyDocs\\investigacion\\MATLAB\\veneris\\valencia\\pruebaLights\\fcd.gps";

		public int vehicleId = 5;
		public GameObject sumoV;
		public GameObject venV;
		protected List<Vector3> positionsVen=null;
		protected List<float> timesVen=null ;
		protected List<Vector3> positionsSumo=null;
		protected List<float> timesSumo =null;
		// Use this for initialization
		void Start ()
		{

			if (!string.IsNullOrEmpty (posFile)) {
				FillVenerisTrajectory ();
				if (positionsVen.Count > 0) {
					venV = GameObject.CreatePrimitive (PrimitiveType.Capsule);
					venV.name = "Veneris "+vehicleId;



				}
			}
			if (!string.IsNullOrEmpty (sumoFile)) {
				FillSumoTrajectory ();
				if (positionsSumo.Count > 0) {
					sumoV = GameObject.CreatePrimitive (PrimitiveType.Cylinder);
					sumoV.name = "SUMO";
				}
			}
		
			Time.timeScale = 2.0f;
		
		}
	
		void FillVenerisTrajectory() {
			Debug.Log ("Veneris positions for " + vehicleId);
			positionsVen = new List<Vector3> ();
			timesVen = new List<float> (); 
			string line;
			char[] separator = new char[]{ '\t' };
			System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.GetCultureInfo ("en-US");

			float totalLength=0;
			int i = 0;
			using (System.IO.StreamReader file = new System.IO.StreamReader (posFile,System.Text.Encoding.ASCII)) {
				while ((line = file.ReadLine ()) != null) {  

					//Debug.Log (line);
					string[] tokens = line.Split (separator);
					//Debug.Log ("tokens=" + tokens.Length);
					if (int.Parse (tokens [0]) == vehicleId) {
						Vector3 p = new Vector3 (float.Parse (tokens [2],ci), float.Parse (tokens [3],ci), float.Parse (tokens [4],ci));
						positionsVen.Add (p);
						Debug.Log (p);
						timesVen.Add(float.Parse(tokens[1],ci));
						Debug.Log (float.Parse (tokens [1], ci));
						if (i>= 1) {
							totalLength += Vector3.Magnitude (positionsVen [i] - positionsVen [i - 1]);
						}
						i++;
					}


				}  


			}
			Debug.Log ("Veneris: "+positionsVen.Count + " positions added. Route length="+totalLength);
			Debug.Log ("Veneris: "+timesVen.Count + " times added. Route length="+totalLength);
		}
		void FillSumoTrajectory() {
			positionsSumo = new List<Vector3> ();
			timesSumo = new List<float> (); 
			string line;
			char[] separator = new char[]{ '\t' };
			float totalLength=0;
			int i = 0;
			using (System.IO.StreamReader file = new System.IO.StreamReader (sumoFile,System.Text.Encoding.ASCII)) {
				while ((line = file.ReadLine ()) != null) {  

					//Debug.Log (line);
					string[] tokens = line.Split (separator);
					//Debug.Log ("tokens=" + tokens.Length);
					if (int.Parse (tokens [0]) == vehicleId) {
						Vector3 p = new Vector3 (float.Parse (tokens [2]), 0.6f, float.Parse (tokens [3]));
						positionsSumo.Add (p);
						timesSumo.Add(float.Parse(tokens[1]));
						if (i>= 1) {
							totalLength += Vector3.Magnitude (positionsSumo [i] - positionsSumo [i - 1]);
						}
						i++;
					}


				}  


			}
		}
	
		void FixedUpdate ()
		{
			
			if (timesVen != null) {
				if (timesVen.Count > 0) {
				
					if (Time.time >= timesVen [0]) {
						venV.transform.position = positionsVen [0];
						positionsVen.RemoveAt (0);
						timesVen.RemoveAt (0);
				

				
						Debug.Log ("ven=" + Time.time);
					}


				}
			}
			if (timesSumo != null) {
				if (timesSumo.Count > 0) {
				
					if (Time.time >= timesSumo [0]) {
					
						sumoV.transform.position = positionsSumo [0];
						positionsSumo.RemoveAt (0);
						timesSumo.RemoveAt (0);


						Debug.Log ("sumo=" + Time.time);
				
					}
			


		
				}
			}
		}
	}

}