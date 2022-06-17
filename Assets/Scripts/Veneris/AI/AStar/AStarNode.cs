/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;

namespace Veneris
{
	public interface IHasNodeNeighbors<N> {	
		IEnumerable<N> Neighbors {get;}
	}
	[System.Serializable]
	public class AStarNode
	{
		
		public List<AStarNode> Neighbors=null;
		public AStarNode()  {
			Neighbors = new List<AStarNode> ();
		}
		public virtual string ToString() {
			return "AStarNode";
			
		}
		public virtual bool IsEqualNode(AStarNode other) {
			return this == other;
		}
	}
}
