// Copyright (C) 2017  Joey Scarr
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using KFS.FileSystems;
using MB.Algodat;
using System.Collections.Generic;

namespace KickassUndelete {
	/// <summary>
	/// A range item wrapping an IRun.
	/// </summary>
	public class RangeItem : IRangeProvider<ulong> {
		public RangeItem(IRun run, LightweightMFTRecord record) {
			// We subtract 1 below because ranges are inclusive.
			Range = new Range<ulong>(run.LCN, run.LCN + run.LengthInClusters - 1);
			Record = record;
		}

		public Range<ulong> Range { get; private set; }
		public LightweightMFTRecord Record { get; private set; }
	}

	/// <summary>
	/// Compares two range items by comparing their ranges.
	/// </summary>
	public class RangeItemComparer : IComparer<RangeItem> {
		#region IComparer<RangeItem> Members

		public int Compare(RangeItem x, RangeItem y) {
			return x.Range.CompareTo(y.Range);
		}

		#endregion
	}
}
