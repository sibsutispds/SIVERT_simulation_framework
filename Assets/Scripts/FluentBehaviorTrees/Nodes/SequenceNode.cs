using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentBehaviourTree
{
    /// <summary>
    /// Runs child nodes in sequence, until one fails.
    /// </summary>
    public class SequenceNode : IParentBehaviourTreeNode
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
        private List<IBehaviourTreeNode> children = new List<IBehaviourTreeNode>(); //todo: this could be optimized as a baked array.

        public SequenceNode(string name)
        {
            this.name = name;
        }

		public BehaviourTreeStatus Tick(TimeData time, string debug)
        {
            //foreach (var child in children)
			for (int i = 0; i < children.Count; i++) 
            {
				var childStatus = children[i].Tick(time, debug);
                if (childStatus != BehaviourTreeStatus.Success)
                {
                    return childStatus;
                }
            }

            return BehaviourTreeStatus.Success;
        }
		public BehaviourTreeStatus Tick(ref string debug)
		{
			//foreach (var child in children)
			for (int i = 0; i < children.Count; i++) 
			{
				var childStatus = children[i].Tick(ref debug);
				if (childStatus != BehaviourTreeStatus.Success)
				{
					return childStatus;
				}
			}

			return BehaviourTreeStatus.Success;
		}
		public BehaviourTreeStatus Tick(List<string> log)
		{
			//log.Add (name);
			//foreach (var child in children)
			for (int i = 0; i < children.Count; i++) 
			{
				var childStatus = children[i].Tick(log);
				if (childStatus != BehaviourTreeStatus.Success)
				{
					return childStatus;
				}
			}

			return BehaviourTreeStatus.Success;
		}
		public BehaviourTreeStatus Tick()
		{
			//foreach (var child in children)
			for (int i = 0; i < children.Count; i++) 
			{
				var childStatus = children[i].Tick();
				if (childStatus != BehaviourTreeStatus.Success)
				{
					return childStatus;
				}
			}

			return BehaviourTreeStatus.Success;
		}


        /// <summary>
        /// Add a child to the sequence.
        /// </summary>
        public void AddChild(IBehaviourTreeNode child)
        {
            children.Add(child);
        }
    }
}
