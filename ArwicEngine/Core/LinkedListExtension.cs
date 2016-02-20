// Dominion - Copyright (C) Timothy Ings
// LinkedListExtensions.cs
// This file defines extension methods for linked lists

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArwicEngine.Core
{
    public static class LinkedListExtension
    {
        /// <summary>
        /// Removes and returns the first element of the linked list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ll"></param>
        /// <returns></returns>
        public static T Dequeue<T>(this LinkedList<T> ll)
        {
            T val = ll.First.Value;
            ll.RemoveFirst();
            return val;
        }

        /// <summary>
        /// Adds an element to the end of the linked list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ll"></param>
        /// <param name="node"></param>
        public static void Enqueue<T>(this LinkedList<T> ll, T node)
        {
            ll.AddLast(node);
        }

        /// <summary>
        /// Swaps two elements of a linked list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ll"></param>
        /// <param name="node0"></param>
        /// <param name="node1"></param>
        public static void Swap<T>(this LinkedList<T> ll, LinkedListNode<T> node0, LinkedListNode<T> node1)
        {
            T val0 = node0.Value;
            T val1 = node1.Value;
            node0.Value = val1;
            node1.Value = val0;
        }

        /// <summary>
        /// Removes the element of the linked list at a given index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ll"></param>
        /// <param name="index"></param>
        public static void Remove<T>(this LinkedList<T> ll, int index)
        {
            if (ll.Count <= 1)
            {
                ll.Clear();
                return;
            }
            LinkedList<T> newll = new LinkedList<T>();
            int i = 0;
            LinkedListNode<T> currentNode = ll.First;
            while (currentNode != null)
            {
                if (i != index)
                    newll.Enqueue(currentNode.Value);
                i++;
                currentNode = currentNode.Next;
            }
            ll.Clear();
            foreach (T node in newll)
                ll.Enqueue(node);
        }

        /// <summary>
        /// Returns the element of a linked list at the given index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ll"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T Get<T>(this LinkedList<T> ll, int index)
        {
            return ll.GetNode(index).Value;
        }

        /// <summary>
        /// Returns the linked list node of the linked list at the given index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ll"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static LinkedListNode<T> GetNode<T>(this LinkedList<T> ll, int index)
        {
            int i = 0;
            LinkedListNode<T> node = ll.First;
            while (i != index)
            {
                if (node == null || node.Next == null)
                    throw new IndexOutOfRangeException();
                node = node.Next;
                i++;
            }
            return node;
        }
    }
}
