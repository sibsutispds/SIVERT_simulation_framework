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
	public interface IResultLogger 
	{
		void RecordWithTimestamp<T> (T t, int id);
		void RecordVariableWithTimestamp<T>  (string name,T t, int id);
		void RecordWithTimestamp<T> (T t);
		void RecordVariableWithTimestamp<T>  (string name,T t);
		void Record (string r);


		void Close () ;

	}
}
