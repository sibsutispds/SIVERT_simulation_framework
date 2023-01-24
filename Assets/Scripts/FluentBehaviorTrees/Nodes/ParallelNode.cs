using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FluentBehaviourTree
{
    /// <summary>
    /// Runs childs nodes in parallel.
    /// </summary>
    public class ParallelNode : IParentBehaviourTreeNode
    {
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
        /// <summary>
        /// List of child nodes.
        /// </summary>
        private List<IBehaviourTreeNode> children = new List<IBehaviourTreeNode>();

        /// <summary>
        /// Number of child failures required to terminate with failure.
        /// </summary>
        private int numRequiredToFail;

        /// <summary>
        /// Number of child successess require to terminate with success.
        /// </summary>
        private int numRequiredToSucceed;

        public ParallelNode(string name, int numRequiredToFail, int numRequiredToSucceed)
        {
            this.name = name;
            this.numRequiredToFail = numRequiredToFail;
            this.numRequiredToSucceed = numRequiredToSucceed;
        }

		public BehaviourTreeStatus Tick(TimeData time, string debug)
        {
            var numChildrenSuceeded = 0;
            var numChildrenFailed = 0;

			//foreach (var child in children)
			for (int i = 0; i < children.Count; i++) 

			{
				var childStatus = children[i].Tick(time,debug);
                switch (childStatus)
                {
                    case BehaviourTreeStatus.Success: ++numChildrenSuceeded; break;
                    case BehaviourTreeStatus.Failure: ++numChildrenFailed; break;
                }
            }
			//Debug.Log (name+" failed=" + numChildrenFailed + "suc=" + numChildrenSuceeded);
            if (numRequiredToSucceed > 0 && numChildrenSuceeded >= numRequiredToSucceed)
            {
                return BehaviourTreeStatus.Success;
            }

            if (numRequiredToFail > 0 && numChildrenFailed >= numRequiredToFail)
            {
                return BehaviourTreeStatus.Failure;
            }

            return BehaviourTreeStatus.Running;
        }

		public BehaviourTreeStatus Tick(ref string debug)
		{


			debug = name;
			var numChildrenSuceeded = 0;
			var numChildrenFailed = 0;

			//foreach (var child in children)
			for (int i = 0; i < children.Count; i++) 

			{
				var childStatus = children[i].Tick(ref debug);
				switch (childStatus)
				{
				case BehaviourTreeStatus.Success: ++numChildrenSuceeded; break;
				case BehaviourTreeStatus.Failure: ++numChildrenFailed; break;
				}
			}
			//Debug.Log (name+" failed=" + numChildrenFailed + "suc=" + numChildrenSuceeded);
			if (numRequiredToSucceed > 0 && numChildrenSuceeded >= numRequiredToSucceed)
			{
				return BehaviourTreeStatus.Success;
			}

			if (numRequiredToFail > 0 && numChildrenFailed >= numRequiredToFail)
			{
				return BehaviourTreeStatus.Failure;
			}

			return BehaviourTreeStatus.Running;
		}
		public BehaviourTreeStatus Tick(List<string> log)
		{


			//log.Add(name);
			var numChildrenSuceeded = 0;
			var numChildrenFailed = 0;

			//foreach (var child in children)
			for (int i = 0; i < children.Count; i++) 

			{
				var childStatus = children[i].Tick(log);
				switch (childStatus)
				{
				case BehaviourTreeStatus.Success: ++numChildrenSuceeded; break;
				case BehaviourTreeStatus.Failure: ++numChildrenFailed; break;
				}
			}
			//Debug.Log (name+" failed=" + numChildrenFailed + "suc=" + numChildrenSuceeded);
			if (numRequiredToSucceed > 0 && numChildrenSuceeded >= numRequiredToSucceed)
			{
				return BehaviourTreeStatus.Success;
			}

			if (numRequiredToFail > 0 && numChildrenFailed >= numRequiredToFail)
			{
				return BehaviourTreeStatus.Failure;
			}

			return BehaviourTreeStatus.Running;
		}
		public BehaviourTreeStatus Tick()
		{
			var numChildrenSuceeded = 0;
			var numChildrenFailed = 0;

			//foreach (var child in children)
			for (int i = 0; i < children.Count; i++) 

			{
				var childStatus = children[i].Tick();
				switch (childStatus)
				{
				case BehaviourTreeStatus.Success: ++numChildrenSuceeded; break;
				case BehaviourTreeStatus.Failure: ++numChildrenFailed; break;
				}
			}
			//Debug.Log (name+" failed=" + numChildrenFailed + "suc=" + numChildrenSuceeded);
			if (numRequiredToSucceed > 0 && numChildrenSuceeded >= numRequiredToSucceed)
			{
				return BehaviourTreeStatus.Success;
			}

			if (numRequiredToFail > 0 && numChildrenFailed >= numRequiredToFail)
			{
				return BehaviourTreeStatus.Failure;
			}

			return BehaviourTreeStatus.Running;
		}

        public void AddChild(IBehaviourTreeNode child)
        {
            children.Add(child);
        }
    }
}
