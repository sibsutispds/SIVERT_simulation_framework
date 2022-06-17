/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GenerateRxPoints  {
	[MenuItem ("Opal/Generate Receivers")]
	public static void GenerateReceivers ()
	{
		string path="D:\\Users\\eegea\\MyDocs\\investigacion\\MATLAB\\veneris\\opal\\validacion-articulo-juan\\Medidas\\rx2.txt";
		string line;
		char[] separator = new char[]{ '\t' };
		int i = 0;
		int id = 1;
		float radius = 1f;
		//GameObject rxpath = new GameObject ("ReceiversPath");
		//List<Vector3> positions = new List<Vector3> ();
		GameObject root = new GameObject ("Receivers");
		Opal.Transmitter bs = GameObject.FindObjectOfType<Opal.Transmitter> ();
		Debug.Log ("Found transmitter at " + bs.transform.position);
		using (System.IO.StreamReader file = new System.IO.StreamReader (path, System.Text.Encoding.UTF8)) {
		//using (System.IO.StreamReader file = new System.IO.StreamReader (path, System.Text.Encoding.ASCII)) {
			while ((line = file.ReadLine ()) != null) {
				//if (i % 4 == 0) {
					Debug.Log (line);
					string[] tokens = line.Split (separator);
					Vector3 pos = new Vector3 (float.Parse (tokens [0]), 1.7f, float.Parse (tokens [1]));
					//	positions.Add (pos);
					GameObject go = GameObject.CreatePrimitive (PrimitiveType.Sphere);
					go.name = "Receiver " + i;
					go.transform.position = pos;
					Opal.Receiver rx = go.AddComponent<Opal.Receiver> ();
					rx.id = id;
					//rx.radius = Mathf.Deg2Rad * 1f * (pos - bs.transform.position).magnitude / Mathf.Sqrt (3);
					rx.radius = radius;
					go.transform.SetParent (root.transform);
					id++;
				//}
				i++;
			}
		}
		root.name = "Receivers med corr r="+radius+ " (" + (id-1) + " elements)";
		/*LineRenderer lr = rxpath.AddComponent<LineRenderer> ();
		lr.positionCount = positions.Count;
		lr.SetPositions (positions.ToArray ());
		*/

	}

}
