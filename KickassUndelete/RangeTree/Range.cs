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

namespace MB.Algodat {
	/// <summary>
	/// Represents a range of values. 
	/// Both values must be of the same type and comparable.
	/// </summary>
	/// <typeparam name="T">Type of the values.</typeparam>
	public struct Range<T> : IComparable<Range<T>>
			where T : IComparable<T> {
		public T From;
		public T To;

		/// <summary>
		/// Initializes a new <see cref="Range&lt;T&gt;"/> instance.
		/// </summary>
		public Range(T value)
			: this() {
			From = value;
			To = value;
		}

		/// <summary>
		/// Initializes a new <see cref="Range&lt;T&gt;"/> instance.
		/// </summary>
		public Range(T from, T to)
			: this() {
			From = from;
			To = to;
		}

		/// <summary>
		/// Whether the value is contained in the range. 
		/// Border values are considered inside.
		/// </summary>
		public bool Contains(T value) {
			return value.CompareTo(From) >= 0 && value.CompareTo(To) <= 0;
		}

		/// <summary>
		/// Whether the value is contained in the range. 
		/// Border values are considered outside.
		/// </summary>
		public bool ContainsExclusive(T value) {
			return value.CompareTo(From) > 0 && value.CompareTo(To) < 0;
		}

		/// <summary>
		/// Whether two ranges intersect each other.
		/// </summary>
		public bool Intersects(Range<T> other) {
			return other.To.CompareTo(From) >= 0 && other.From.CompareTo(To) <= 0;
		}

		/// <summary>
		/// Whether two ranges intersect each other.
		/// </summary>
		public bool IntersectsExclusive(Range<T> other) {
			return other.To.CompareTo(From) > 0 && other.From.CompareTo(To) < 0;
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString() {
			return string.Format("{0} - {1}", From, To);
		}

		public override int GetHashCode() {
			int hash = 23;
			hash = hash * 37 + From.GetHashCode();
			hash = hash * 37 + To.GetHashCode();
			return hash;
		}

		#region IComparable<Range<T>> Members

		/// <summary>
		/// Returns -1 if this range's From is less than the other, 1 if greater.
		/// If both are equal, To is compared, 1 if greater, -1 if less.
		/// 0 if both ranges are equal.
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns></returns>
		public int CompareTo(Range<T> other) {
			if (From.CompareTo(other.From) < 0)
				return -1;
			else if (From.CompareTo(other.From) > 0)
				return 1;
			else if (To.CompareTo(other.To) < 0)
				return -1;
			else if (To.CompareTo(other.To) > 0)
				return 1;
			else
				return 0;
		}

		#endregion
	}

	/// <summary>
	/// Static helper class to create Range instances.
	/// </summary>
	public static class Range {
		/// <summary>
		/// Creates and returns a new <see cref="Range&lt;T&gt;"/> instance.
		/// </summary>
		public static Range<T> Create<T>(T from, T to)
				where T : IComparable<T> {
			return new Range<T>(from, to);
		}
	}

	/// <summary>
	/// Interface for classes which provide a range.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IRangeProvider<T> where T : IComparable<T> {
		Range<T> Range { get; }
	}
}
