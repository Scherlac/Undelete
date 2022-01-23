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
using System.Collections;
using System.Globalization;
using System.Windows.Forms;

namespace KickassUndelete {
	/// <summary>
	/// This class is an implementation of the 'IComparer' interface.
	/// </summary>
	public class ListViewColumnSorter : IComparer {
		/// <summary>
		/// Specifies the column to be sorted
		/// </summary>
		private int _columnToSort;
		/// <summary>
		/// Specifies the order in which to sort (i.e. 'Ascending').
		/// </summary>
		private SortOrder _orderOfSort;
		/// <summary>
		/// Case insensitive comparer object
		/// </summary>
		private CaseInsensitiveComparer _objectCompare;

		/// <summary>
		/// Class constructor.  Initializes various elements
		/// </summary>
		public ListViewColumnSorter() {
			// Initialize the sort order to 'none'
			_orderOfSort = SortOrder.None;

			// Initialize the CaseInsensitiveComparer object
			_objectCompare = new CaseInsensitiveComparer(CultureInfo.CurrentCulture);
		}

		/// <summary>
		/// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
		/// </summary>
		/// <param name="x">First object to be compared</param>
		/// <param name="y">Second object to be compared</param>
		/// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
		public int Compare(object x, object y) {
			int compareResult;
			ListViewItem listviewX, listviewY;

			// Cast the objects to be compared to ListViewItem objects
			listviewX = (ListViewItem)x;
			listviewY = (ListViewItem)y;

			// Compare the two items
			// TODO: Make this not depend on a hardcoded numerical index!
			if (_columnToSort == 2) {
				compareResult = _objectCompare.Compare(((INodeMetadata)listviewX.Tag).GetFileSystemNode().Size, ((INodeMetadata)listviewY.Tag).GetFileSystemNode().Size);
			} else if (_columnToSort == 3) {
				compareResult = _objectCompare.Compare(((INodeMetadata)listviewX.Tag).LastModified, ((INodeMetadata)listviewY.Tag).LastModified);
			} else if (_columnToSort == 5) {
				compareResult = _objectCompare.Compare(((INodeMetadata)listviewX.Tag).ChanceOfRecovery, ((INodeMetadata)listviewY.Tag).ChanceOfRecovery);
			} else {
				compareResult = _objectCompare.Compare(listviewX.SubItems[_columnToSort].Text, listviewY.SubItems[_columnToSort].Text);
			}

			// Calculate correct return value based on object comparison
			if (_orderOfSort == SortOrder.Ascending) {
				// Ascending sort is selected, return normal result of compare operation
				return compareResult;
			} else if (_orderOfSort == SortOrder.Descending) {
				// Descending sort is selected, return negative result of compare operation
				return (-compareResult);
			} else {
				// Return '0' to indicate they are equal
				return 0;
			}
		}

		/// <summary>
		/// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
		/// </summary>
		public int SortColumn {
			set {
				_columnToSort = value;
			}
			get {
				return _columnToSort;
			}
		}

		/// <summary>
		/// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
		/// </summary>
		public SortOrder Order {
			set {
				_orderOfSort = value;
			}
			get {
				return _orderOfSort;
			}
		}

	}
}
