/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Veneris {
public abstract class Spline  {
		
		//Derivation of normal and curvature formula from N. Patrikalakis et al. "Shape Interogation for Computer Aid Design and Manufacturing" http://web.mit.edu/hyperbook/Patrikalakis-Maekawa-Cho

		public Vector3[] points;

		public float[] GetParameterFractionAtPoints() {
			int total = points.Length - 2; //Without control points
			float[] f = new float[total];
			for (int i = 0; i<total; i++)
			{ 
				f[i]=((float) i)/(total-1);


			}
			return f;
		}

		abstract public  Vector3 Interpolate (float t);
		abstract public  Vector3 Derivative (float t, int order = 1);

		public float Curvature (float t) {
			//k=|r'(u)x r''(u)|/|r'(u)|^3
			Vector3 dr=Derivative(t,1);
			Vector3 d2r=Derivative(t,2);
			return ((Vector3.Cross (dr, d2r).magnitude) / Mathf.Pow (dr.magnitude, 3));
		}
		public Vector3 Normal (float t) {
			//n=b x t

			return Vector3.Cross (Binormal(t), Tangent(t));
		}
		public Vector3 Tangent (float t) {
			return Derivative (t, 1).normalized;
		}
		public Vector3 Binormal(float t) {
			Vector3 dr=Derivative(t,1);
			Vector3 d2r=Derivative(t,2);
			return Vector3.Normalize(Vector3.Cross (dr, d2r));
		}
}
}
