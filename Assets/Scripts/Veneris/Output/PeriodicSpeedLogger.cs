/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;

namespace Veneris
{
	public class PeriodicSpeedLogger : MonoBehaviour
	{
		public VehicleInfo vi = null;

		public float speed;
		public float sqrSpeed;
		[SerializeField]
		private float _interval = 1f;

		public float interval {
			get { return _interval; }
			set {
				_interval = value;
				intervalWait = new WaitForSeconds (interval);
			}
		}

		protected WaitForSeconds intervalWait = null;
		public string variableName = "speed";
		protected Coroutine logRoutine = null;
		protected IResultLogger resultLogger = null;
		protected float lastTime = 0f;
		// Use this for initialization
		void Awake ()
		{
			if (vi == null) {
				vi = transform.root.GetComponentInChildren<VehicleInfo> ();
			}
			//resultLogger = SimulationManager.Instance.GetResultLogger (vi.vehicleId, variableName);
			resultLogger = SimulationManager.Instance.GetResultLogger (SimulationManager.ResultType.Speed);

		}

		void Start ()
		{
			//intervalWait = new WaitForSeconds (interval);
		}

		void OnEnable ()
		{
			//logRoutine = StartCoroutine(Collect());
			lastTime = Time.time;
		}

		/*void OnDisable ()
		{
			//StopCoroutine (logRoutine);
		}*/

		/*void OnDestroy ()
		{
			resultLogger.Close ();
		}*/
		// Coroutines could be used here to make this independent of the framerate
		IEnumerator Collect ()
		{
			while (true) {
				if (vi != null) {
					resultLogger.RecordWithTimestamp<float> (vi.speed,vi.vehicleId);
				}
				if (vi != null) {
					speed = vi.speed;
					sqrSpeed = vi.sqrSpeed;
				}
				yield return intervalWait;
			}

	
		}

		void Update ()
		{
			if ((Time.time - lastTime) >= interval) {
				if (vi != null) {
					resultLogger.RecordWithTimestamp<float> (vi.speed,vi.vehicleId);
					lastTime = Time.time;
				}
			}
		}
	}
}
