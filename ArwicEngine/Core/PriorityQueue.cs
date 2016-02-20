// Dominion - Copyright (C) Timothy Ings
// PriorityQueue.cs
// This file defines classes that define a priority queue

using System.Collections.Generic;

namespace ArwicEngine.Core
{
    public class PriorityQueue<T>
    {
        private class PriorityQueueElement
        {
            public T Item { get; set; }
            public int Priority { get; set; }

            public PriorityQueueElement(T item, int priority)
            {
                Item = item;
                Priority = priority;
            }
        }

        private List<PriorityQueueElement> elements = new List<PriorityQueueElement>();

        /// <summary>
        /// Gets the number of elements in the priority queue 
        /// </summary>
        public int Count => elements.Count;

        /// <summary>
        /// Adds an item to the queue at the given priority
        /// </summary>
        /// <param name="item"></param>
        /// <param name="priority"></param>
        public void Enqueue(T item, int priority)
        {
            elements.Add(new PriorityQueueElement(item, priority));
        }

        /// <summary>
        /// Removes the item with the highest priority from the queue
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            int bestIndex = 0;

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Priority < elements[bestIndex].Priority)
                {
                    bestIndex = i;
                }
            }

            T bestItem = elements[bestIndex].Item;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }
    }

}
