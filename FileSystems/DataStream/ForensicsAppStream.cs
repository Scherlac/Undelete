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
	/// An adapter that allows an IDataStream to be treated as a System.IO.Stream.
	/// </summary>
	public class ForensicsAppStream : System.IO.Stream {
		IDataStream _stream = null;
		ulong _position = 0;
		public ForensicsAppStream(IDataStream stream) {
			_stream = stream;
			_stream.Open();
		}

		public override bool CanRead { get { return true; } }
		public override bool CanSeek { get { return true; } }
		public override bool CanWrite { get { return false; } }

		public override void Flush() { }

		public override long Length {
			get { return (long)_stream.StreamLength; }
		}

		public override long Position {
			get { return (long)_position; }
			set { _position = (ulong)Math.Max(0, value); }
		}

		public override int Read(byte[] buffer, int offset, int count) {
			ulong read = Math.Min((ulong)count, _stream.StreamLength - _position);
			_stream.GetBytes(_position, read);
			_position += read;
			return (int)read;
		}

		public override long Seek(long offset, System.IO.SeekOrigin origin) {
			switch (origin) {
				case System.IO.SeekOrigin.Begin:
					_position = (ulong)offset;
					break;
				case System.IO.SeekOrigin.Current:
					_position = (ulong)((long)_position + offset);
					break;
				case System.IO.SeekOrigin.End:
					_position = (ulong)((long)_stream.StreamLength + offset);
					break;
			}
			return Position;
		}

		public override void SetLength(long value) { }

		public override void Write(byte[] buffer, int offset, int count) { }

		public override void Close() {
			_stream.Close();
		}
	}
}
