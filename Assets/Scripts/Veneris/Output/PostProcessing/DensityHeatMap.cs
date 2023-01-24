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
	public class DensityHeatMap : MonoBehaviour
	{

		// Use this for initialization
		public string laneFile = "D:\\Users\\eegea\\MyDocs\\investigacion\\MATLAB\\veneris\\pasubio30\\pasubio2\\environment.txt";
		VenerisLane[] lanes;

		void Start ()
		{
			lanes=GameObject.FindObjectsOfType(typeof(VenerisLane)) as VenerisLane[];
			System.IO.StreamReader file =   
				new System.IO.StreamReader(laneFile, System.Text.Encoding.ASCII);  
			string line;
			char[] separator = new char[]{ '\t' };
			//Debug.Log ("cl=" +separator.Length);
			int c = 0;
			System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.GetCultureInfo ("es-ES");
			while((line = file.ReadLine()) != null)  
			{  
				if (c > 0) {
					Debug.Log (line);
					string[] tokens = line.Split (separator);
					//Debug.Log ("tokens=" + tokens.Length);
					if (tokens[3].Equals ("NaN")) {
						continue;
					}
					CreateGradient (tokens [0], float.Parse (tokens [3],ci));
				}
				c++;
			}  

			file.Close(); 
		}
		public void CreateGradient(string lane, float d) {
			for (int i = 0; i < lanes.Length; i++) {
				if (lanes [i].sumoId.Equals (lane)) {
					Debug.Log ("Found " + lane + "d=" + d);
					Color c = Color.Lerp (Color.green, Color.red, d); 
					Renderer r = lanes [i].GetComponent<Renderer> ();
					r.material.color = c;
					break;
				}
				
			}


		}

	}
}
