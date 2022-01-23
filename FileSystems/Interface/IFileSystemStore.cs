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

namespace KFS.Disks {
	/// <summary>
	/// Determines what type of data source an IFileSystemStore is.
	/// </summary>
	public enum StorageType {
		PhysicalDiskRange = 0,
		PhysicalDisk = 1,
		PhysicalDiskPartition = 2,
		LogicalVolume = 3,
		DiskImage = 4
	}
	/// <summary>
	/// An IFileSystemStore represents a data source, such as a logical drive
	/// or disk partition, that contains a filesystem.
	/// </summary>
	public interface IFileSystemStore : IDataStream {
		StorageType StorageType { get; }
		Attributes Attributes { get; }
		IFileSystem FS { get; }
		/// <summary>
		/// The ID of the device (such as "C:" on Windows). Used to construct
		/// file paths.
		/// </summary>
		string DeviceID { get; }
	}
}
