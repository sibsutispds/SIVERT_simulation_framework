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
	public class GenerationInfo : MonoBehaviour
	{

		public string generationInfo;
		public void SetGenerationInfo (string g) {
			generationInfo = g;
		}
		public string GetGenerationInfo() {
			return generationInfo;
		}

	}
}
