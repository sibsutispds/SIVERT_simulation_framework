using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentBehaviourTree
{
    /// <summary>
    /// Selects the first node that succeeds. Tries successive nodes until it finds one that doesn't fail.
    /// </summary>
    public class SelectorNode : IParentBehaviourTreeNode
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
        /// List of child nodes.
        /// </summary>
        private List<IBehaviourTreeNode> children = new List<IBehaviourTreeNode>(); //todo: optimization, bake this to an array.

        public SelectorNode(string name)
        {
            this.name = name;
        }

		public BehaviourTreeStatus Tick(TimeData time, string debug)
        {
            //foreach (var child in children)
			for (int i = 0; i < children.Count; i++) 

			{
				var childStatus = children[i].Tick(time,debug);
                if (childStatus != BehaviourTreeStatus.Failure)
                {
                    return childStatus;
                }
            }

            return BehaviourTreeStatus.Failure;
        }
		public BehaviourTreeStatus Tick(ref string debug)
		{
			debug = name;
			//foreach (var child in children)
			for (int i = 0; i < children.Count; i++) 

			{
				var childStatus = children[i].Tick(ref debug);
				if (childStatus != BehaviourTreeStatus.Failure)
				{
					return childStatus;
				}
			}

			return BehaviourTreeStatus.Failure;
		}
		public BehaviourTreeStatus Tick(List<string> log)
		{
			//log.Add (name);
			//foreach (var child in children)
			for (int i = 0; i < children.Count; i++) 

			{
				var childStatus = children[i].Tick(log);
				if (childStatus != BehaviourTreeStatus.Failure)
				{
					return childStatus;
				}
			}

			return BehaviourTreeStatus.Failure;
		}
		public BehaviourTreeStatus Tick()
		{
			//foreach (var child in children)
			for (int i = 0; i < children.Count; i++) 

			{
				var childStatus = children[i].Tick();
				if (childStatus != BehaviourTreeStatus.Failure)
				{
					return childStatus;
				}
			}

			return BehaviourTreeStatus.Failure;
		}

        /// <summary>
        /// Add a child node to the selector.
        /// </summary>
        public void AddChild(IBehaviourTreeNode child)
        {
            children.Add(child);
        }
    }
}
