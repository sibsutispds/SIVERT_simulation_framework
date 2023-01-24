/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;


namespace Veneris {
	//We want it to be serialized in the editor. If not derived from object it serializes everything it knows how to serialize
	[System.Serializable]
public class LaneSection 
{

	public Vector3[] vertices;
	public LaneSection(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
		vertices =new Vector3[5];
		vertices[0]=vertices[4]=p0;
		vertices[1]=p1;
		vertices[2]=p2;
		vertices[3]=p3;
	}
	public int PointInSection(Vector3 point) {
		return wn_PnPoly (point);
	}
	// Copyright 2000 softSurfer, 2012 Dan Sunday
	// This code may be freely used and modified for any purpose
	// providing that this copyright notice is included with it.
	// SoftSurfer makes no warranty for this code, and cannot be held
	// liable for any real or imagined damage resulting from its use.
	// Users of this code must verify correctness for their application.
	//http://geomalgorithms.com/a03-_inclusion.html


	// a Point is defined by its coordinates {int x, z;}
	//===================================================================


	// isLeft(): tests if a point is Left|On|Right of an infinite line.
	//    Input:  three points P0, P1, and P2
	//    Return: >0 for P2 left of the line through P0 and P1
	//            =0 for P2  on the line
	//            <0 for P2  right of the line
	//    See: Algorithm 1 "Area of Triangles and Polygons"
	private float	isLeft( Vector3 P0, Vector3 P1, Vector3 P2 )
	{
		return ( (P1.x - P0.x) * (P2.z - P0.z)	- (P2.x -  P0.x) * (P1.z - P0.z) );
	}
	//===================================================================


	// cn_PnPoly(): crossing number test for a point in a polygon
	//      Input:   P = a point,
	//               V[] = vertex points of a polygon V[n+1] with V[n]=V[0]
	//      Return:  0 = outside, 1 = inside
	// This code is patterned after [Franklin, 2000]
	private int	cn_PnPoly( Vector3 P)
	{
		int    cn = 0;    // the  crossing number counter

		// loop through all edges of the polygon
		for (int i=0; i<vertices.Length-1; i++) {    // edge from V[i]  to V[i+1]
			if (((vertices[i].z <= P.z) && (vertices[i+1].z > P.z))     // an upward crossing
				|| ((vertices[i].z > P.z) && (vertices[i+1].z <=  P.z))) { // a downward crossing
				// compute  the actual edge-ray intersect x-coordinate
				float vt = (float)(P.z  - vertices[i].z) / (vertices[i+1].z - vertices[i].z);
				if (P.x <  vertices[i].x + vt * (vertices[i+1].x - vertices[i].x)) // P.x < intersect
					++cn;   // a valid crossing of y=P.y right of P.x
			}
		}
		return (cn&1);    // 0 if even (out), and 1 if  odd (in)

	}
	//===================================================================


	// wn_PnPoly(): winding number test for a point in a polygon
	//      Input:   P = a point,
	//               V[] = vertex points of a polygon V[n+1] with V[n]=V[0]
	//      Return:  wn = the winding number (=0 only when P is outside)
	private int	wn_PnPoly( Vector3 P )
	{
		
		int    wn = 0;    // the  winding number counter

		// loop through all edges of the polygon
		for (int i=0; i<vertices.Length-1; i++) {   // edge from V[i] to  V[i+1]
			
			if (vertices[i].z <= P.z) {          // start y <= P.y
				if (vertices[i+1].z  > P.z)      // an upward crossing
				if (isLeft( vertices[i], vertices[i+1], P) > 0)  // P left of  edge
					++wn;            // have  a valid up intersect
			}
			else {                        // start y > P.y (no test needed)
				if (vertices[i+1].z  <= P.z)     // a downward crossing
				if (isLeft( vertices[i], vertices[i+1], P) < 0)  // P right of  edge
					--wn;            // have  a valid down intersect
			}
		}
	
		return wn;
	}
	}
}