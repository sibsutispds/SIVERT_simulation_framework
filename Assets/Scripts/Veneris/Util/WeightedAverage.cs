/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Veneris
{
	
	[System.Serializable]
	public class WeightedAverage : Veneris.Average
	{

		public double sumWeightedValues=0.0;
		public double sumWeightedSquaredValues=0.0;
		public double sumSquaredWeights =0.0;
		public double sumWeights = 0;
		public float lastTime=0f;
		public WeightedAverage() {
			
		}
	
		public void Collect(float v, float t) {
			Collect (v);
			sumWeights += t;
			sumWeightedValues += (t * v);
			sumSquaredWeights += (t * t);
			sumWeightedSquaredValues += (t * v * v);
		}
		public void CollectWithLastTime(float v) {
			Collect (v, (Time.time - lastTime));
			lastTime = Time.time;
		}
		public double ComputeWeightedAverage() {
			return (sumWeights==0 ? double.NaN : sumWeightedValues/sumWeights);
		}
		public override double Variance() {
			if (samples <= 1) {
				return double.NaN;
			} else {
				double den = sumWeights * sumWeights - sumSquaredWeights;
				double sig = (sumWeights * sumWeightedSquaredValues - sumWeightedValues * sumWeightedValues) / den;
				return (sig < 0 ? 0 : sig);
			}
		}
		public override void ResetValues() {
			base.ResetValues ();
			 sumWeightedValues=0.0;
			sumWeightedSquaredValues=0.0;
			 sumSquaredWeights =0.0;
			 sumWeights = 0;
			lastTime = Time.time;
		}
	}
}
