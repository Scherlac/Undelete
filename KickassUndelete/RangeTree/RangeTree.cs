﻿// Copyright (c) 2012, Matthias Buchetics
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;

namespace MB.Algodat {
	/// <summary>
	/// Range tree interface.
	/// </summary>
	/// <typeparam name="TKey">The type of the range.</typeparam>
	/// <typeparam name="T">The type of the data items.</typeparam>
	public interface IRangeTree<TKey, T>
		where TKey : IComparable<TKey>
		where T : IRangeProvider<TKey> {
		IEnumerable<T> Items { get; }
		int Count { get; }

		List<T> Query(TKey value);
		List<T> Query(Range<TKey> range);

		void Rebuild();
		void Add(T item);
		void Add(IEnumerable<T> items);
		void Remove(T item);
		void Remove(IEnumerable<T> items);
		void Clear();
	}

	/// <summary>
	/// The standard range tree implementation. Keeps a root node and
	/// forwards all queries to it.
	/// Whenenver new items are added or items are removed, the tree 
	/// goes "out of sync" and is rebuild when it's queried next.
	/// </summary>
	/// <typeparam name="TKey">The type of the range.</typeparam>
	/// <typeparam name="T">The type of the data items.</typeparam>
	public class RangeTree<TKey, T> : IRangeTree<TKey, T>
		where TKey : IComparable<TKey>
		where T : IRangeProvider<TKey> {
		private RangeTreeNode<TKey, T> _root;
		private List<T> _items;
		private bool _isInSync;
		private bool _autoRebuild;
		private IComparer<T> _rangeComparer;

		/// <summary>
		/// Whether the tree is currently in sync or not. If it is "out of sync"
		/// you can either rebuild it manually (call Rebuild) or let it rebuild
		/// automatically when you query it next.
		/// </summary>
		public bool IsInSync {
			get { return _isInSync; }
		}

		/// <summary>
		/// All items of the tree.
		/// </summary>
		public IEnumerable<T> Items {
			get { return _items; }
		}

		/// <summary>
		/// Count of all items.
		/// </summary>
		public int Count {
			get { return _items.Count; }
		}

		/// <summary>
		/// Whether the tree should be rebuild automatically. Defaults to true.
		/// </summary>
		public bool AutoRebuild {
			get { return _autoRebuild; }
			set { _autoRebuild = value; }
		}

		/// <summary>
		/// Initializes an empty tree.
		/// </summary>
		public RangeTree(IComparer<T> rangeComparer) {
			_rangeComparer = rangeComparer;
			_root = new RangeTreeNode<TKey, T>(rangeComparer);
			_items = new List<T>();
			_isInSync = true;
			_autoRebuild = true;

		}

		/// <summary>
		/// Initializes a tree with a list of items to be added.
		/// </summary>
		public RangeTree(IEnumerable<T> items, IComparer<T> rangeComparer) {
			_rangeComparer = rangeComparer;
			_root = new RangeTreeNode<TKey, T>(items, rangeComparer);
			_items = items.ToList();
			_isInSync = true;
			_autoRebuild = true;
		}

		/// <summary>
		/// Performans a "stab" query with a single value.
		/// All items with overlapping ranges are returned.
		/// </summary>
		public List<T> Query(TKey value) {
			if (!_isInSync && _autoRebuild)
				Rebuild();

			return _root.Query(value);
		}

		/// <summary>
		/// Performans a range query.
		/// All items with overlapping ranges are returned.
		/// </summary>
		public List<T> Query(Range<TKey> range) {
			if (!_isInSync && _autoRebuild)
				Rebuild();

			return _root.Query(range);
		}

		/// <summary>
		/// Rebuilds the tree if it is out of sync.
		/// </summary>
		public void Rebuild() {
			if (_isInSync)
				return;

			_root = new RangeTreeNode<TKey, T>(_items, _rangeComparer);
			_isInSync = true;
		}

		/// <summary>
		/// Adds the specified item. Tree will go out of sync.
		/// </summary>
		public void Add(T item) {
			_isInSync = false;
			_items.Add(item);
		}

		/// <summary>
		/// Adds the specified items. Tree will go out of sync.
		/// </summary>
		public void Add(IEnumerable<T> items) {
			_isInSync = false;
			_items.AddRange(items);
		}

		/// <summary>
		/// Removes the specified item. Tree will go out of sync.
		/// </summary>
		public void Remove(T item) {
			_isInSync = false;
			_items.Remove(item);
		}

		/// <summary>
		/// Removes the specified items. Tree will go out of sync.
		/// </summary>
		public void Remove(IEnumerable<T> items) {
			_isInSync = false;

			foreach (var item in items)
				_items.Remove(item);
		}

		/// <summary>
		/// Clears the tree (removes all items).
		/// </summary>
		public void Clear() {
			_root = new RangeTreeNode<TKey, T>(_rangeComparer);
			_items = new List<T>();
			_isInSync = true;
		}
	}


}
