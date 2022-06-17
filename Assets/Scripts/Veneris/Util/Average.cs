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
	[System.Serializable]
	public class Average 
	{
		public double sumValues=0.0;
		public double sumSquaredValues=0.0;

		public long samples=0;
		public Average() {
			
		}
		public void Init() {
			ResetValues ();
		}
		public void Collect(float v) {
			sumValues += v;
			sumSquaredValues += (v * v);
			++samples;
		}
		public double Mean() {
			return (sumValues / samples);
		}
		public virtual void ResetValues() {
			sumValues = 0.0f;
			samples = 0;
			sumSquaredValues = 0.0f;
		}
		public virtual double Variance() {
			if (samples <= 1) {
				return double.NaN;
			}
			double devsqr = (sumSquaredValues - ((sumValues * sumValues) / samples)) / (samples-1);
			return devsqr<0 ? 0.0f : devsqr;  
		}
		public double StdDev() {
			return System.Math.Sqrt (Variance ());
		}

	}
}
