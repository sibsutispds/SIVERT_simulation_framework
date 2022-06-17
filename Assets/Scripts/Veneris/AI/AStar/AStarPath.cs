/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;

namespace Veneris
{
	public class AStarPath<T> : IEnumerable<T>
	{


		public T LastStep { get; private set; }

		public AStarPath<T> PreviousSteps { get; private set; }

		public float TotalCost { get; private set; }

		private AStarPath (T lastStep, AStarPath<T> previousSteps, float totalCost)
		{
			LastStep = lastStep;
			PreviousSteps = previousSteps;
			TotalCost = totalCost;
		}

		public AStarPath (T start) : this (start, null, 0)
		{
		}

		public AStarPath<T> AddStep (T step, float stepCost)
		{
			return new AStarPath<T> (step, this, TotalCost + stepCost);
		}

		public IEnumerator<T> GetEnumerator ()
		{
			for (AStarPath<T> p = this; p != null; p = p.PreviousSteps)
				yield return p.LastStep;
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return this.GetEnumerator ();
		}
	


		
	}
}
