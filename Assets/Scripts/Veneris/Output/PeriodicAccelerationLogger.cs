/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;

namespace Veneris
{
	
	public struct AccelerationTracker
	{
		public float lastSpeed;
		public float lastTime;
	}

	public class PeriodicAccelerationLogger : MonoBehaviour
	{

		public VehicleInfo vi = null;
		public AccelerationTracker at;
		public float acceleration;
		[SerializeField]
		private float _interval=1f;
		public float interval {
			get {return _interval;}
			set { _interval = value; intervalWait=new WaitForSeconds (interval);}
		}
		protected WaitForSeconds intervalWait = null;
		public string variableName="acc";
		protected Coroutine logRoutine = null;
		protected IResultLogger resultLogger = null;
		protected float lastTime=0f;

		void Awake ()
		{
			if (vi == null) {
				vi = transform.root.GetComponentInChildren<VehicleInfo> ();
			}

			resultLogger = SimulationManager.Instance.GetResultLogger (SimulationManager.ResultType.Acceleration);
			//resultLogger = SimulationManager.Instance.GetResultLogger (vi.vehicleId, variableName);
		}

		void Start() {
			//intervalWait=new WaitForSeconds (interval);
		}

		void OnEnable ()
		{
			//logRoutine = StartCoroutine (Collect ());
			lastTime = Time.time;
		}

		/*void OnDisable ()
		{
			//StopCoroutine (logRoutine);
		}*/

		/*void OnDestroy ()
		{
			resultLogger.Close ();
		}
		*/
		IEnumerator Collect ()
		{
			while (true) {
				if (vi != null ) {
					float speed = vi.speed;
					float deltaTime = Time.time - at.lastTime;
					acceleration = (speed - at.lastSpeed) / deltaTime;
					if (deltaTime > 0) {
						resultLogger.RecordWithTimestamp<float> (acceleration,vi.vehicleId);
					}
					at.lastTime = Time.time;
					at.lastSpeed = speed;
				}
				yield return intervalWait;
			}

		}
		void Update() {
			if ((Time.time - lastTime) >= interval) {
				if (vi != null ) {
					float speed = vi.speed;
					float deltaTime = Time.time - at.lastTime;
					acceleration = (speed - at.lastSpeed) / deltaTime;
					if (deltaTime > 0) {
						resultLogger.RecordWithTimestamp<float> (acceleration, vi.vehicleId);
					}
					at.lastTime = Time.time;
					at.lastSpeed = speed;
				}
				lastTime = Time.time;
				
			}
		}
	}
}

