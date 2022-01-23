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

using KFS.DataStream;
using System;
using System.Collections.Generic;
using System.IO;

namespace KFS.FileSystems {
	/// <summary>
	/// Allows a folder on the host system to be treated as an IFolder.
	/// </summary>
	public class FolderMounted : Folder {
		private string _path;
		private IDataStream _parent;
		private DirectoryInfo _info;

		public FolderMounted(string filePath, IDataStream parent) {
			_path = filePath;
			_parent = parent;
			_info = new DirectoryInfo(_path);
			Name = _info.Name;
		}

		public override DateTime LastModified {
			get { return _info.LastWriteTime; }
		}

		public override long Identifier {
			get { return 0; /* no-op */ }
		}

		public override IEnumerable<IFileSystemNode> GetChildren() {
			foreach (FileSystemInfo entry in _info.GetFileSystemInfos()) {
				if ((entry.Attributes & FileAttributes.Directory) != 0) {
					yield return new FolderMounted(entry.FullName, this);
				} else {
					yield return new FileFromHostSystem(entry.FullName, this);
				}
			}
		}

		public override byte[] GetBytes(ulong offset, ulong length) {
			return new byte[length];
		}

		public override ulong StreamLength {
			get { return 0; }
		}

		public override ulong DeviceOffset {
			get { return 0; }
		}

		public override string StreamName {
			get { return "Temporary Folder " + Name; }
		}

		public override IDataStream ParentStream {
			get { return _parent; }
		}

		public override void Open() { }

		public override void Close() { }
	}
}
