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
using System.Management;
using System.Runtime.InteropServices;

namespace KFS.Disks {
	/// <summary>
	/// A logical disk on a Windows host system.
	/// </summary>
	public class WinLogicalDisk : WinDisk, ILogicalDisk {
		public LogicalDiskAttributes Attributes { get; private set; }

		private ulong _size;
		private IFileSystem _fileSystem;
		public WinLogicalDisk(ManagementObject mo) {
			Attributes = new LogicalDiskAttributes(mo);
			Handle = Win32.CreateFile(@"\\.\" + Attributes.DeviceID, EFileAccess.GenericRead, EFileShare.Read | EFileShare.Write | EFileShare.Delete, IntPtr.Zero, ECreationDisposition.OpenExisting, EFileAttributes.None, IntPtr.Zero);

			if (Handle.IsInvalid) {
				throw new Exception("Failed to get a handle to the logical volume. " + Marshal.GetLastWin32Error());
			}
			_size = Util.GetDiskSize(Handle);
			_fileSystem = FileSystem.TryLoad(this);
		}

		public string TextDescription {
			get { return Attributes.TextDescription; }
		}

		public override ulong StreamLength {
			get { return _size; }
		}

		public override string StreamName {
			get { return "Logical Volume " + Attributes.VolumeName; }
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
			get { return Attributes.DeviceID; }
		}

		public override string ToString() {
			string volume;
			if (string.IsNullOrEmpty(Attributes.VolumeName.Trim())) {
				volume = Attributes.DriveType.ToString();
			} else {
				volume = Attributes.VolumeName;
			}
			if (string.IsNullOrEmpty(Attributes.FileSystem)) {
				return string.Format("{0} {1}", Attributes.DeviceID, volume);
			} else {
				return string.Format("{0} {1} ({2})", Attributes.DeviceID, volume, Attributes.FileSystem);
			}
		}
	}
}
