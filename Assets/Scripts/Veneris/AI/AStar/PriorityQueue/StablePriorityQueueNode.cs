/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



namespace Priority_Queue
{
    public class StablePriorityQueueNode : FastPriorityQueueNode
    {
        /// <summary>
        /// Represents the order the node was inserted in
        /// </summary>
        public long InsertionIndex { get; internal set; }
    }
}
