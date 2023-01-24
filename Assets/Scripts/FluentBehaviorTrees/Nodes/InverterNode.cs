using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentBehaviourTree
{
    /// <summary>
    /// Decorator node that inverts the success/failure of its child.
    /// </summary>
    public class InverterNode : IParentBehaviourTreeNode
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
        /// The child to be inverted.
        /// </summary>
        private IBehaviourTreeNode childNode;

        public InverterNode(string name)
        {
            this.name = name;
        }

		public BehaviourTreeStatus Tick(TimeData time, string debug)
        {
            if (childNode == null)
            {
                throw new ApplicationException("InverterNode must have a child node!");
            }

			var result = childNode.Tick(time,debug);
            if (result == BehaviourTreeStatus.Failure)
            {
                return BehaviourTreeStatus.Success;
            }
            else if (result == BehaviourTreeStatus.Success)
            {
                return BehaviourTreeStatus.Failure;
            }
            else
            {
                return result;
            }
        }
		public BehaviourTreeStatus Tick()
		{
			if (childNode == null)
			{
				throw new ApplicationException("InverterNode must have a child node!");
			}

			var result = childNode.Tick();
			if (result == BehaviourTreeStatus.Failure)
			{
				return BehaviourTreeStatus.Success;
			}
			else if (result == BehaviourTreeStatus.Success)
			{
				return BehaviourTreeStatus.Failure;
			}
			else
			{
				return result;
			}
		}
		public BehaviourTreeStatus Tick(ref string debug)
		{
			debug = name;
			if (childNode == null)
			{
				throw new ApplicationException("InverterNode must have a child node!");
			}

			var result = childNode.Tick(ref debug);
			if (result == BehaviourTreeStatus.Failure)
			{
				return BehaviourTreeStatus.Success;
			}
			else if (result == BehaviourTreeStatus.Success)
			{
				return BehaviourTreeStatus.Failure;
			}
			else
			{
				return result;
			}
		}
		public BehaviourTreeStatus Tick(List<string> log)
		{
			//log.Add(name);
			if (childNode == null)
			{
				throw new ApplicationException("InverterNode must have a child node!");
			}

			var result = childNode.Tick(log);
			if (result == BehaviourTreeStatus.Failure)
			{
				return BehaviourTreeStatus.Success;
			}
			else if (result == BehaviourTreeStatus.Success)
			{
				return BehaviourTreeStatus.Failure;
			}
			else
			{
				return result;
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
