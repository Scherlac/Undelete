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
	/// Allows a file on the host system to be treated as an IFile.
	/// </summary>
	public class FileFromHostSystem : FileDataStream, IFile {
		private FileInfo _info;

		public FileFromHostSystem(string filePath, IDataStream parent)
			: base(filePath, parent) {
			_info = new FileInfo(filePath);
			Name = _info.Name;
			Path = ""; // TODO
		}

		public string Name { get; private set; }

		public ulong Size { get; private set; }

		public DateTime LastModified {
			get { return _info.LastWriteTime; }
		}

		public bool Deleted { get; private set; }

		public IFileSystemNode GetFileSystemNode() {
			return this;
		}

		public FileRecoveryStatus ChanceOfRecovery {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public override String StreamName {
			get { return "Temporary File " + Name; }
		}

		public IFile AsFile() {
			return this;
		}

		public IFolder AsFolder() {
			return null;
		}

		public IEnumerable<IFileSystemNode> GetChildren() {
			return new List<IFileSystemNode>();
		}

		public IEnumerable<IFileSystemNode> GetChildrenAtPath(string path) {
			throw new NotImplementedException();
		}

		public bool Loaded { get; set; }

		public string Path { get; set; }

		public FSNodeType Type { get { return FSNodeType.Folder; } }
	}
}
