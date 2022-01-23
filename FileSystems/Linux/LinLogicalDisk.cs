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
using KFS.FileSystems;
using System;
using System.IO;

namespace KFS.Disks {
	/// <summary>
	/// A logical disk attached to a Linux host system.
	/// </summary>
	public class LinLogicalDisk : LinDisk, ILogicalDisk, IFileSystemStore, IDescribable {
		public LinLogicalDiskAttributes Attributes { get; private set; }

		private readonly string _devName;
		private readonly ulong _size;
		private readonly IFileSystem _fileSystem;

		public LinLogicalDisk(string dev) {
			_devName = dev;
			Handle = System.IO.File.Open(dev, FileMode.Open, FileAccess.Read);
			if (Handle == null) {
				throw new Exception("Linux Bug!");
			}
			_size = (ulong)Handle.Length;
			Attributes = new LinLogicalDiskAttributes();
			Attributes.FileSystem = Util.DetectFSType(this);
			_fileSystem = FileSystem.TryLoad(this as IFileSystemStore);
		}

		public string TextDescription {
			get { return "LinLogicalDisk::TextDescription not implemented."; }
		}

		public override ulong StreamLength {
			get { return _size; }
		}

		public override string StreamName {
			get { return "LinLogicalDisk::StreamName not implemented."; }
		}

		public StorageType StorageType {
			get { return StorageType.LogicalVolume; }
		}

		Attributes IFileSystemStore.Attributes {
			get { return Attributes; }
		}

		public IFileSystem FS {
			get { return _fileSystem; }
		}

		public string DeviceID {
			get { return _devName; }
		}

		public override string ToString() {
			return _devName;
		}
	}
}

