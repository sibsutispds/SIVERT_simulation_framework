/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Veneris {
public class CatmullRomSpline : Spline  {
	
	

	public float tau=0.5f;
	

	private CatmullRomSpline(Vector3[] pts) {
		this.points = pts;

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

	override public  Vector3 Interpolate(float t) {
		int numSections = points.Length - 3;
		int currPt = Mathf.Min(Mathf.FloorToInt(t * (float) numSections), numSections - 1);
		float u = t * (float) numSections - (float) currPt;
		Vector3 a = points[currPt];
		Vector3 b = points[currPt + 1];
		Vector3 c = points[currPt + 2];
		Vector3 d = points[currPt + 3];
		//c3u*u*u + c2*u*u + c1*u +c0 Interpolation polynomial r(u)
		return ((-tau*a+(2f-tau)*b+(tau-2f)*c+tau*d)*(u*u*u)+ (2f*tau*a+(tau-3f)*b+(3-2f*tau)*c-tau*d)*(u*u)  + (tau*(-a+c))*u+ b );
	}
	

	override public  Vector3 Derivative(float t, int order=1) {
		int numSections = points.Length - 3;
		int currPt = Mathf.Min(Mathf.FloorToInt(t * (float) numSections), numSections - 1);
		float u = t * (float) numSections - (float) currPt;
		Vector3 a = points[currPt];
		Vector3 b = points[currPt + 1];
		Vector3 c = points[currPt + 2];
		Vector3 d = points[currPt + 3];
		if (order == 1) {
			//3*c3u*u + 2*c2*u + c1 Derivative of the above polynomial: r('u)
			return ((-tau * a + (2f - tau) * b + (tau - 2f) * c + tau * d) * 3f* (u * u) + (2f * tau * a + (tau - 3f) * b + (3 - 2f * tau) * c - tau * d) * 2f * u + (tau * (-a + c)));
		} else if (order == 2) {
			//6*c3*u + 2*c2 Second derivative: r''(u) 
			return ((-tau * a + (2f - tau) * b + (tau - 2f) * c + tau * d) * 6f * u + (2f * tau * a + (tau - 3f) * b + (3 - 2f * tau) * c - tau * d) * 2f);
		} else if (order == 3) {
			//6*c3 Third derivative: r'''(u)
			return ((-tau * a + (2f - tau) * b + (tau - 2f) * c + tau * d) * 6f * u);
		} else {
			return Vector3.zero;
		}
	}


	public static Vector3 Interp(Vector3 a, Vector3 b,Vector3 c,Vector3 d, float u, float tau) {
		return ((-tau*a+(2f-tau)*b+(tau-2f)*c+tau*d)*(u*u*u)+ (2f*tau*a+(tau-3f)*b+(3-2f*tau)*c-tau*d)*(u*u)  + (tau*(-a+c))*u+ b );
	}

}
}
