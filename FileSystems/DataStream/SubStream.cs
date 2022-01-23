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

namespace KFS.DataStream {
	/// <summary>
	/// A data stream that allows access to a subset of another stream.
	/// </summary>
	public class SubStream : IDataStream {
		private IDataStream _stream;
		private ulong _start, _length;

		public SubStream(IDataStream stream, ulong start, ulong length) {
			_stream = stream;
			_start = start;
			_length = length;
		}

		public virtual byte[] GetBytes(ulong offset, ulong length) {
			return _stream.GetBytes(_start + offset, length);
		}

		public ulong StreamLength {
			get {
				return _length;
			}
		}

		public ulong DeviceOffset {
			get {
				return _stream.DeviceOffset + _start;
			}
		}

		public virtual String StreamName {
			get { return "Substream of " + _stream.StreamName; }
		}

		public virtual IDataStream ParentStream {
			get { return _stream; }
		}

		public void Open() {
			_stream.Open();
		}

		public void Close() {
			_stream.Close();
		}

		public override string ToString() {
			return StreamName;
		}
	}
}
