using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentBehaviourTree
{
    /// <summary>
    /// Represents time. Used to pass time values to behaviour tree nodes.
    /// </summary>
    public class TimeData
    {
        public TimeData(float deltaTime)
        {
            this.deltaTime = deltaTime;
        }

        public float deltaTime;
    }
}
