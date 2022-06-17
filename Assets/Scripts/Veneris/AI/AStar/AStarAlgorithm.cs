/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using Priority_Queue;
using System;
using UnityEngine;
namespace Veneris
{
	//From https://blogs.msdn.microsoft.com/ericlippert/2007/10/10/path-finding-using-a-in-c-3-0-part-four/
	public class AStarAlgorithm
	{
		//Generic version
		static public AStarPath<T> FindPath<T> (T start,T destination,Func<T, T, float> distance,	Func<T, float> estimate) where T:IHasNodeNeighbors<T>

		{
			

			var closed = new HashSet<T>();
			var queue = new SimplePriorityQueue<AStarPath<T>>();
			queue.Enqueue(new AStarPath<T>(start),0f);
			while (queue.Count > 0) {

				var path = queue.Dequeue ();
				if (closed.Contains (path.LastStep)) {
					
					continue;
				}
				if (path.LastStep.Equals (destination)) {
					
					return path;
				}

				closed.Add(path.LastStep);

				//for (int i = 0; i < path.LastStep.neighbors.Count; i++) 
				foreach(T n in path.LastStep.Neighbors)
				{
					
					//AStarNode n = path.LastStep.neighbors [i];
					float d = distance(path.LastStep, n);


					var newAStarPath = path.AddStep(n, d);
					queue.Enqueue(newAStarPath, newAStarPath.TotalCost + estimate(n));
				}
			}
			return null;
		}
		//Restricted version. This works as long as the route is feasible, that is, it has been precomputed and it is a valid route
		static public AStarPath<AStarLaneNode> FindPathOnRouteRoads (AStarLaneNode start,AStarLaneNode destination,Func<AStarLaneNode, AStarLaneNode, float> distance,	Func<AStarLaneNode, float> estimate, List<VenerisRoad> roads) 

		{


			//Debug.Log ("star=" + start.ToString () + "end=" + destination.ToString ());

			var closed = new HashSet<AStarLaneNode>();
			var queue = new SimplePriorityQueue<AStarPath<AStarLaneNode>>();
			queue.Enqueue(new AStarPath<AStarLaneNode>(start),0f);
			while (queue.Count > 0) {
				//Debug.Log ("running AStart " + queue.Count);
				var path = queue.Dequeue ();
				if (closed.Contains (path.LastStep)) {
				//	Debug.Log ("Already at closed" + path.LastStep.ToString ());
					continue;
				}
				if (path.LastStep.IsEqualNode (destination)) {
				//	Debug.Log ("ended at " + destination.ToString ());
					return path;
				}
				//Debug.Log ("Adding " + path.LastStep.ToString ());
				closed.Add(path.LastStep);

				for (int i = 0; i < path.LastStep.neighbors.Count; i++) 

				{

					AStarLaneNode n = path.LastStep.neighbors [i];
					if (roads.IndexOf (n.road) >= 0) {
						float d = distance (path.LastStep, n);
				//		Debug.Log ("Adding neighbor " + n.ToString () + "with cost" + d);
				//		Debug.Log ("neighbors  " + n.neighbors.Count);

						var newAStarPath = path.AddStep (n, d);
						queue.Enqueue (newAStarPath, newAStarPath.TotalCost + estimate (n));
					}
				}
			}
			return null;
		}
	}
		

}

