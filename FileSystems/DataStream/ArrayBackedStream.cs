// Copyright (C) 2017  Joey Scarr, Josh Oosterman, Lukas Korsika
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KFS.DataStream {
	/// <summary>
	/// A data stream wrapper for an array of bytes.
	/// </summary>
	public class ArrayBackedStream : IDataStream {
		private uint _offset;
		private uint _length;
		private byte[] _data;

		public ArrayBackedStream(byte[] data, uint offset, uint length) {
			_data = data;
			_offset = offset;
			_length = length;
		}

		public byte[] GetBytes(ulong offset, ulong length) {
			byte[] result = new byte[length];
			Array.Copy(_data, (int)(_offset + offset), result, 0, (int)length);
			return result;
		}

		public ulong DeviceOffset {
			get { return _offset; }
		}

		public ulong StreamLength {
			get { return _length; }
		}

		public string StreamName {
			get { return "Array-backed stream"; }
		}

		public IDataStream ParentStream {
			get { return null; }
		}

		public void Open() { }

		public void Close() {
			// Remove reference to the array.
			_data = null;
		}
	}
}
