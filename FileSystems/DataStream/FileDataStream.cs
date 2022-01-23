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
using System.IO;

namespace KFS.DataStream {
	/// <summary>
	/// A data stream wrapper for a file on the host system, such as a disk image.
	/// </summary>
	public class FileDataStream : IDataStream {
		private FileStream _fs = null;
		private string _path;

		public FileDataStream(String filePath, IDataStream parentStream) {
			_path = filePath;
			ParentStream = parentStream;
			Open();
		}

		public byte[] GetBytes(ulong offset, ulong length) {
			if (_fs != null) {
				_fs.Seek((long)offset, SeekOrigin.Begin);
				byte[] res = new byte[length];
				_fs.Read(res, 0, (int)length);
				return res;
			} else {
				throw new Exception("FileDataStream was closed");
			}
		}

		public long Identifier {
			get { return 0; /* no-op */ }
		}

		public ulong StreamLength {
			get {
				return (ulong)_fs.Length;
			}
		}

		public ulong DeviceOffset {
			get { return 0; }
		}

		public virtual String StreamName {
			get { return "Local File"; }
		}

		public IDataStream ParentStream { get; private set; }

		public void Open() {
			if (_fs == null) {
				_fs = File.OpenRead(_path);
			}
		}

		public void Close() {
			if (_fs != null) {
				_fs.Close();
				_fs = null;
			}
		}
	}
}
