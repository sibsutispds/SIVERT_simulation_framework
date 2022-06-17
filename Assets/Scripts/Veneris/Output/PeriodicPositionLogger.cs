/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;

namespace Veneris
{
	public class PeriodicPositionLogger : MonoBehaviour
	{

		public VehicleInfo vi = null;
		[SerializeField]
		private float _interval = 1f;

		public float interval {
			get { return _interval; }
			set {
				_interval = value;
				intervalWait = new WaitForSeconds (interval);
			}
		}

		public string variableName = "pos";
		protected Coroutine logRoutine = null;
		protected IResultLogger resultLogger = null;
		protected WaitForSeconds intervalWait = null;
		protected float lastTime=0f;

		void Awake ()
		{
			if (vi == null) {
				vi = transform.root.GetComponentInChildren<VehicleInfo> ();
			}
			//resultLogger = SimulationManager.Instance.GetResultLogger (vi.vehicleId, variableName);
			resultLogger = SimulationManager.Instance.GetResultLogger (SimulationManager.ResultType.Position);
		}

		void Start ()
		{
			//intervalWait = new WaitForSeconds (interval);
			
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

	/*	void OnDestroy ()
		{
			resultLogger.Close ();
		}
*/
		IEnumerator Collect ()
		{
			while (true) {
				resultLogger.RecordWithTimestamp<string> (vi.carBody.transform.position.x + "\t" + vi.carBody.transform.position.y + "\t" + vi.carBody.transform.position.z,vi.vehicleId);
				yield return intervalWait;
			}
		}
		void Update() {
			if ((Time.time - lastTime) >= interval) {
				resultLogger.RecordWithTimestamp<string> (vi.carBody.transform.position.x + "\t" + vi.carBody.transform.position.y + "\t" + vi.carBody.transform.position.z,vi.vehicleId);
				lastTime = Time.time;
			}
		}
	}
}
