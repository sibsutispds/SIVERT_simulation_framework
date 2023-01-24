using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace FluentBehaviourTree
{ 
	/// <summary>
	/// Decorator node that executes its child a maxium of N times.
	/// </summary>

	public class ExecuteUntilSuccessNTimesNode : IParentBehaviourTreeNode {

		/// <summary>
		/// Name of the node.
		/// </summary>
		private string name;
		public  string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}
		private int maxTimes;
		private int done;

		/// <summary>
		/// The child to be inverted.
		/// </summary>
		private IBehaviourTreeNode childNode;

		public ExecuteUntilSuccessNTimesNode(string name, int maxTimes)
		{
			this.name = name;
			this.maxTimes = maxTimes;
			this.done = 0;
		}

		public BehaviourTreeStatus Tick(TimeData time, string debug)
		{
			if (childNode == null)
			{
				throw new ApplicationException("ExecuteUntilSucceesN must have a child node!");
			}

			if (done < maxTimes) {
				//Debug.Log ("Executing " + done + " time");
				var result = childNode.Tick (time,debug);
				if (result == BehaviourTreeStatus.Success) {
					++done;
					return BehaviourTreeStatus.Success;
				} else {
					return result;
				}

			} else {
				return BehaviourTreeStatus.Success;
			}


		}
		public BehaviourTreeStatus Tick(ref string debug)

		{
			debug = name;
			if (childNode == null)
			{
				throw new ApplicationException("ExecuteUntilSucceesN must have a child node!");
			}

			if (done < maxTimes) {
				//Debug.Log ("Executing " + done + " time");
				var result = childNode.Tick (ref debug);
				if (result == BehaviourTreeStatus.Success) {
					++done;
					return BehaviourTreeStatus.Success;
				} else {
					return result;
				}

			} else {
				return BehaviourTreeStatus.Success;
			}


		}
		public BehaviourTreeStatus Tick(List<string> log)

		{
			//log.Add(name);
			if (childNode == null)
			{
				throw new ApplicationException("ExecuteUntilSucceesN must have a child node!");
			}

			if (done < maxTimes) {
				//Debug.Log ("Executing " + done + " time");
				var result = childNode.Tick (log);
				if (result == BehaviourTreeStatus.Success) {
					++done;
					return BehaviourTreeStatus.Success;
				} else {
					return result;
				}

			} else {
				return BehaviourTreeStatus.Success;
			}


		}
		public BehaviourTreeStatus Tick()
		{
			if (childNode == null)
			{
				throw new ApplicationException("ExecuteUntilSucceesN must have a child node!");
			}

			if (done < maxTimes) {
				//Debug.Log ("Executing " + done + " time");
				var result = childNode.Tick ();
				if (result == BehaviourTreeStatus.Success) {
					++done;
					return BehaviourTreeStatus.Success;
				} else {
					return result;
				}

			} else {
				return BehaviourTreeStatus.Success;
			}


		}

		/// <summary>
		/// Add a child to the parent node.
		/// </summary>
		public void AddChild(IBehaviourTreeNode child)
		{
			if (this.childNode != null)
			{
				throw new ApplicationException("Can't add more than a single child to InverterNode!");
			}

			this.childNode = child;
		}
	}
}
