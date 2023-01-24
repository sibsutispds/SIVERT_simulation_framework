/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.UI;
using Slider = UnityEngine.UIElements.Slider;
using Toggle = UnityEngine.UIElements.Toggle;

namespace Veneris
{
	public class TimeScaleUIControl : MonoBehaviour
	{

		public Toggle automaticTimeScaleControl = null;
		public Slider timeScale=null;
		public InputField timeScaleInput=null;
		// public MediaTypeNames.Text timeScaleText=null;
	
		// public MediaTypeNames.Text timeScaleControlInfoText=null;
		public UIManager manager =null;
		protected string timeScaleControlInfo =null;
		// Use this for initialization
		void Awake ()
		{

			if (manager == null) {
				manager = transform.parent.GetComponent<UIManager> ();
			}
			if (timeScale == null) {
				timeScale =transform.Find("TimeScale").GetComponent<Slider> ();
			}
			// if (timeScaleText == null) {
			// 	timeScaleText = transform.Find("TimeScaleText").GetComponent<MediaTypeNames.Text>();
			// 	timeScaleText.text = "TimeScale Control";
			// }
			if (timeScaleInput == null) {
				timeScaleInput = transform.Find("TimeScaleInputField").GetComponent<InputField>();
				timeScaleInput.text = timeScale.value.ToString();
			}
			if (automaticTimeScaleControl == null) {
				automaticTimeScaleControl = transform.Find("AutomaticTimeScaleControl").GetComponent<Toggle>();

			}
			// if (timeScaleControlInfoText == null) {
			// 	timeScaleControlInfoText= transform.Find("TimeScaleControlInfo").GetComponent<MediaTypeNames.Text>();
			// 	timeScaleControlInfoText.text = "";
			// }
			// timeScale.onValueChanged.AddListener (delegate {
			// 	TimeScaleChanged ();
			// });
			timeScaleInput.onEndEdit.AddListener (delegate {
				TimeScaleInputChanged ();
			});
			// automaticTimeScaleControl.onValueChanged.AddListener (delegate {
			// 	ToggleAutomaticTimeScaleControl ();
			// });
			// if (SimulationManager.Instance.IsTimeScaleControlActive ()) {
			// 	automaticTimeScaleControl.isOn = true;
			// } else {
			// 	automaticTimeScaleControl.isOn = false;
			// }
		
		}
	

		void OnEnable() {
			if (SimulationManager.Instance.IsTimeScaleControlActive ()) {
				// automaticTimeScaleControl.isOn = true;
				OnTimeScaleControlValue ();
			} else {
				// automaticTimeScaleControl.isOn = false;
			}
		}
		void TimeScaleChanged() {
			Time.timeScale = timeScale.value;
			//timeScaleText.text = "Time Scale: " +timeScale.value.ToString("F2");
			timeScaleInput.text = timeScale.value.ToString();
		}
		void TimeScaleInputChanged() {
			float ts=float.Parse(timeScaleInput.text);
			Time.timeScale = ts;
			//timeScaleText.text = "Time Scale: " +timeScale.value.ToString("F2");
			timeScale.value = ts;
		}
		// void ToggleAutomaticTimeScaleControl() {
		// 	if (automaticTimeScaleControl.isOn) {
		// 		SimulationManager.Instance.ActivateTimeScaleControl ();
		// 		SimulationManager.Instance.RegisterTimeScaleControlListener (OnTimeScaleControlValue);
		// 	} else {
		// 		SimulationManager.Instance.DeactivateTimeScaleControl ();
		// 	}
		// }
		void OnTimeScaleControlValue() {
			
			timeScaleInput.text = Time.timeScale.ToString ();
			timeScale.value =Time.timeScale;
			// timeScaleControlInfoText.text  = SimulationManager.Instance.GetTimeScaleControlInfoText ();
			//RefreshGeneralTextInfo ();
		}
	}
}
