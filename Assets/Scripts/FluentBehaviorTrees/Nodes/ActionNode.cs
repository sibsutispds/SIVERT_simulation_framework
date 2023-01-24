using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FluentBehaviourTree
{
    /// <summary>
    /// A behaviour tree leaf node for running an action.
    /// </summary>
    public class ActionNode : IBehaviourTreeNode
    {
        /// <summary>
        /// The name of the node.
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

        /// <summary>
        /// Function to invoke for the action.
        /// </summary>
        private Func<BehaviourTreeStatus> fn;
        

        public ActionNode(string name, Func< BehaviourTreeStatus> fn)
        {
			
            this.name=name;
            this.fn=fn;
        }

		public BehaviourTreeStatus Tick(TimeData time,   string debug)
        {
			if (debug != null) {
				Debug.Log (debug + "-----" + name);
			}


            return fn();
        }
		public BehaviourTreeStatus Tick(ref string debug)
		{
			

			debug = name;
			return fn();
		}
		public BehaviourTreeStatus Tick(List<string> log)
		{

			log.Add(name);
			return fn();
		}
		public BehaviourTreeStatus Tick()
		{
			

			return fn();
		}
    }
}
