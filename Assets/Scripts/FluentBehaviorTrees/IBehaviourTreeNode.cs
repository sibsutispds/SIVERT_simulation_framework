using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentBehaviourTree
{
    /// <summary>
    /// Interface for behaviour tree nodes.
    /// </summary>
    public interface IBehaviourTreeNode
    {

		 string Name {
			get;
			set;
		}

        /// <summary>
        /// Update the time of the behaviour tree.
        /// </summary>
		BehaviourTreeStatus Tick(TimeData t, string debug);
		BehaviourTreeStatus Tick( ref string debug);
		BehaviourTreeStatus Tick( List<string> log);
		BehaviourTreeStatus Tick();
    }
}
