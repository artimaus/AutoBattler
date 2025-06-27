
using System;
using System.Collections;
using System.Collections.Generic;

namespace AutoBattlerLib
{
    /// <summary>
    /// A queue data structure with O(1) contains checking via hash lookup.
    /// Maintains FIFO order while providing fast membership testing.
    /// </summary>
    /// <typeparam name="T">The type of elements in the queue</typeparam>
    public class HashQueue<T> : IEnumerable<T>
    {
        private readonly Queue<T> _queue;
        private readonly HashSet<T> _hashSet;

        public HashQueue()
        {
            _queue = new Queue<T>();
            _hashSet = new HashSet<T>();
        }

        public HashQueue(IEqualityComparer<T> comparer)
        {
            _queue = new Queue<T>();
            _hashSet = new HashSet<T>(comparer);
        }

        public HashQueue(int capacity)
        {
            _queue = new Queue<T>(capacity);
            _hashSet = new HashSet<T>(capacity);
        }

        public HashQueue(int capacity, IEqualityComparer<T> comparer)
        {
            _queue = new Queue<T>(capacity);
            _hashSet = new HashSet<T>(capacity, comparer);
        }

        /// <summary>
        /// Gets the number of elements in the queue
        /// </summary>
        public int Count => _queue.Count;

        /// <summary>
        /// Determines whether the queue is empty
        /// </summary>
        public bool IsEmpty => _queue.Count == 0;

        /// <summary>
        /// Adds an element to the end of the queue
        /// </summary>
        /// <param name="item">The element to add</param>
        /// <returns>True if the item was added, false if it already exists</returns>
        public bool Enqueue(T item)
        {
            if (_hashSet.Add(item))
            {
                _queue.Enqueue(item);
                return true;
            }
            return false; // Item already exists
        }

        /// <summary>
        /// Removes and returns the element at the beginning of the queue
        /// </summary>
        /// <returns>The element at the beginning of the queue</returns>
        /// <exception cref="InvalidOperationException">The queue is empty</exception>
        public T Dequeue()
        {
            if (Snip())
                throw new InvalidOperationException("Queue is empty");

            var item = _queue.Dequeue();
            _hashSet.Remove(item);
            return item;
        }

        /// <summary>
        /// Tries to remove and return the element at the beginning of the queue
        /// </summary>
        /// <param name="result">The dequeued element, or default if queue is empty</param>
        /// <returns>True if an element was dequeued, false if queue was empty</returns>
        public bool TryDequeue(out T result)
        {
            if (Snip())
            {
                result = default(T);
                return false;
            }

            result = _queue.Dequeue();
            _hashSet.Remove(result);
            return true;
        }

        /// <summary>
        /// Returns the element at the beginning of the queue without removing it
        /// </summary>
        /// <returns>The element at the beginning of the queue</returns>
        /// <exception cref="InvalidOperationException">The queue is empty</exception>
        public T Peek()
        {
            Snip();
            return _queue.Peek();
        }

        /// <summary>
        /// Tries to return the element at the beginning of the queue without removing it
        /// </summary>
        /// <param name="result">The first element, or default if queue is empty</param>
        /// <returns>True if an element exists, false if queue was empty</returns>
        public bool TryPeek(out T result)
        {
            if (Snip())
            {
                result = default(T);
                return false;
            }

            result = _queue.Peek();
            return true;
        }

        public bool Snip()
        {
            while (!(_queue.Count == 0) && !_hashSet.Contains(_queue.Peek()))
            {
                _queue.Dequeue();
            }
            return ((_queue.Count == 0));
        }

        /// <summary>
        /// Determines whether the queue contains a specific element - O(1) operation
        /// </summary>
        /// <param name="item">The element to check for</param>
        /// <returns>True if the element is in the queue, false otherwise</returns>
        public bool Contains(T item)
        {
            return _hashSet.Contains(item);
        }

        /// <summary>
        /// Removes a specific element from the queue (maintains FIFO for remaining elements)
        /// Note: This is an O(n) operation as it requires queue reconstruction
        /// </summary>
        /// <param name="item">The element to remove</param>
        /// <returns>True if the element was found and removed, false otherwise</returns>
        public bool Remove(T item)
        {
            return _hashSet.Remove(item);
        }

        /// <summary>
        /// Removes all elements from the queue
        /// </summary>
        public void Clear()
        {
            _queue.Clear();
            _hashSet.Clear();
        }

        /// <summary>
        /// Copies the queue elements to an array
        /// </summary>
        /// <returns>Array containing queue elements in FIFO order</returns>
        public T[] ToArray()
        {
            if (!ValidateConsistency())
            {
                T item;
                for (int i = 0; i < _queue.Count; i++)
                {
                    if (_hashSet.Contains(item = _queue.Dequeue()))
                    {
                        _queue.Enqueue(item);
                    }
                }
            }
            return _queue.ToArray();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the queue in FIFO order
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// For debugging: validates internal consistency
        /// </summary>
        public bool ValidateConsistency()
        {
            return _queue.Count == _hashSet.Count &&
                   _hashSet.SetEquals(_queue);
        }
    }
}