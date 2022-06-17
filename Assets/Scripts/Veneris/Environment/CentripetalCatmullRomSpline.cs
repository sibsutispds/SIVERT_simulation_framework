/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Veneris {
	public class CentripetalCatmullRomSpline : Spline {

		//Implementation derived from cfh at  http://stackoverflow.com/questions/9489736/catmull-rom-curve-with-no-cusps-and-no-self-intersections 

		//If alpha=0, we get a uniform CatmullRom spline (common), if alpha=1 we get a chordal parameterization
		//http://www.cemyuksel.com/research/catmullrom_param/catmullrom.pdf
		//If just a uniform CatmullRomSpline is necessary, use a CatmullRomSpline, it is much more efficient

		public float alpha=0.5f;

		private Vector3 c0,c1,c2,c3;

		public CentripetalCatmullRomSpline(Vector3[] pts) {
			this.points = pts;

		}
	

		public void ComputeCoefficients(Vector3 p0, Vector3 tan0, Vector3 p1, Vector3 tan1) {
			c0 = p0;
			c1 = tan0;
			c2 = -3f*p0 + 3f*p1 - 2f*tan0 - tan1;
			c3 = 2f*p0 - 2f*p1 + tan0 + tan1;
		}
		private Vector3 Eval(float t) {
			float t2 = t*t;
			float t3 = t2 * t;
			return (c0 + c1*t + c2*t2 + c3*t3);
		}
	
		private  Vector3 EvalDerivative(float t, int order=1) {
			float t2 = t*t;

			if (order == 1) {
				return (c1 + 2f * c2 * t + 3f * c3 * t2);
			} else if (order == 2) {
				return (2f * c2 + 6f * c3 * t);
			} else if (order == 3) {
				return (6f * c3);
			} else {
				return Vector3.zero;
			}
		}
	

		//Interpolate points uniformly per section of the path, INDEPENDENT OF THE SEPARATION OF THE NODES
		//t in [0,1] parameter is for the total path. 
		//u in [0,1] is the corresponding fraction in a given segment (section)
		//With u=0.5 we get the middle point in a given segment
		//Examples: with 2 segments and t=0.75 we should get u=0.5, that is, the middle point in the second segment: X----X--|--X
		//So all t<0.5 goes to the first section and the t>0.5 goes to the second section
		//If we have 3 segments, t<0.33 goes to the first section, 0.33<t<0.66 goes to the second section and so on
		//To get a uniform density of points (points/meter) we would need arc-length parameterization, which is expensive, see for instance http://algorithmist.net/docs/arcparam.pdf or https://doi.org/10.1109/TVCG.2006.53
		//so it is not implemented
		override	public  Vector3 Interpolate(float t) {
			
			int numSections = points.Length - 3;
			int currPt = Mathf.Min(Mathf.FloorToInt(t * (float) numSections), numSections - 1);
			float u = t * (float) numSections - (float) currPt;

			return InterpolateAtSegment (currPt, u);
		
		
		}
		//currPt is the first of the control points: we have P0,P1,P2,P3
		//Interpolation is made for the segment P1-P2, with u in [0,1] the fraction of the normalized segment between P1-P2. 
		//currPt is the index of P0
		public Vector3 InterpolateAtSegment(int currPt, float u) {
			
			Vector3 x0 = points[currPt];
			Vector3 x1 = points[currPt + 1];
			Vector3 x2 = points[currPt + 2];
			Vector3 x3 = points[currPt + 3];
			float dt0 = Mathf.Pow((x0-x1).sqrMagnitude, alpha/2f);
			float dt1 = Mathf.Pow((x1-x2).sqrMagnitude, alpha/2f);
			float dt2 = Mathf.Pow((x2-x3).sqrMagnitude, alpha/2f);
			// safety check for repeated points
			if (dt1 < 1e-5f)    dt1 = 1.0f;
			if (dt0 < 1e-5f)    dt0 = dt1;
			if (dt2 < 1e-5f)    dt2 = dt1;
			// compute tangents when parameterized in [t1,t2]
			Vector3 t1 = (x1 - x0) / dt0 - (x2 - x0) / (dt0 + dt1) + (x2 - x1) / dt1;
			Vector3 t2 = (x2 - x1) / dt1 - (x3 - x1) / (dt1 + dt2) + (x3 - x2) / dt2;

			// rescale tangents for parametrization in [0,1]
			t1 *= dt1;
			t2 *= dt1;
			ComputeCoefficients (x1, t1, x2, t2);
			return Eval (u);
		}
		override public  Vector3 Derivative(float t, int order=1) {
			int numSections = points.Length - 3;
			int currPt = Mathf.Min(Mathf.FloorToInt(t * (float) numSections), numSections - 1);
			float u = t * (float) numSections - (float) currPt;
			return DerivativeAtSegment (currPt, u, order);

		}
		//u in [0,1] is the normalized fracrtion in the segment
		public Vector3 DerivativeAtSegment(int currPt, float u, int order) {
			Vector3 x0 = points[currPt];
			Vector3 x1 = points[currPt + 1];
			Vector3 x2 = points[currPt + 2];
			Vector3 x3 = points[currPt + 3];
			float dt0 = Mathf.Pow((x0-x1).sqrMagnitude, alpha/2f);
			float dt1 = Mathf.Pow((x1-x2).sqrMagnitude, alpha/2f);
			float dt2 = Mathf.Pow((x2-x3).sqrMagnitude, alpha/2f);
			// safety check for repeated points
			if (dt1 < 1e-5f)    dt1 = 1.0f;
			if (dt0 < 1e-5f)    dt0 = dt1;
			if (dt2 < 1e-5f)    dt2 = dt1;
			// compute tangents when parameterized in [t1,t2]
			Vector3 t1 = (x1 - x0) / dt0 - (x2 - x0) / (dt0 + dt1) + (x2 - x1) / dt1;
			Vector3 t2 = (x2 - x1) / dt1 - (x3 - x1) / (dt1 + dt2) + (x3 - x2) / dt2;

			// rescale tangents for parametrization in [0,1]
			t1 *= dt1;
			t2 *= dt1;
			ComputeCoefficients (x1, t1, x2, t2);
			return EvalDerivative (u, order);
		}
		//t here is the normalized fraction in the segment
		public float CurvatureAtSegment (int currPt, float u) {
			//k=|r'(u)x r''(u)|/|r'(u)|^3
			Vector3 dr=DerivativeAtSegment(currPt,u,1);
			Vector3 d2r=DerivativeAtSegment(currPt,u,2);
			return ((Vector3.Cross (dr, d2r).magnitude) / Mathf.Pow (dr.magnitude, 3));
		}
		public Vector3 NormalAtSegment (int currPt, float u) {
			//n=b x t

			return Vector3.Cross (BinormalAtSegment(currPt,u), TangentAtSegment(currPt,u));
		}
		public Vector3 TangentAtSegment (int currPt, float u) {
			return DerivativeAtSegment (currPt,u, 1).normalized;
		}
		public Vector3 BinormalAtSegment(int currPt, float u) {
			Vector3 dr=DerivativeAtSegment(currPt,u,1);
			Vector3 d2r=DerivativeAtSegment(currPt,u,2);
			return Vector3.Normalize(Vector3.Cross (dr, d2r));
		}
	}
}

