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

using KFS.FileSystems;
using System;
using System.Management;
using System.Text;

namespace KFS.Disks {
	public class PhysicalDiskPartition : PhysicalDiskSection, IFileSystemStore {
		public PhysicalDiskPartitionAttributes Attributes { get; private set; }
		private IFileSystem _fileSystem;

		public PhysicalDiskPartition(WinPhysicalDisk disk, MasterBootRecord.PartitionEntry pEntry) {
			PhysicalDisk = disk;
			Offset = pEntry.PartitionOffset;
			Length = pEntry.PartitionLength;

			ManagementScope ms = new ManagementScope();
			ObjectQuery oq = new ObjectQuery(
					string.Format("SELECT * FROM Win32_DiskPartition WHERE DiskIndex = {0} AND Index = {1}",
					disk.Attributes.Index, pEntry.Index));
			ManagementObjectSearcher mos = new ManagementObjectSearcher(ms, oq);
			ManagementObjectCollection moc = mos.Get();
			if (moc.Count != 1) {
				throw new Exception("Unable to get partition data from WMI");
			}
			foreach (ManagementObject mo in moc) {
				Attributes = new PhysicalDiskPartitionAttributes(mo, disk);
				break;
			}
			Attributes.PartitionType = pEntry.PartitionType;

			_fileSystem = FileSystem.TryLoad(this as IFileSystemStore);
		}

		public override string ToString() {
			return StreamName;
		}

		public override String StreamName {
			get { return Attributes.PartitionType.ToString() + " Partition"; }
		}

		#region IDescribable Members

		public override string TextDescription {
			get {
				StringBuilder sb = new StringBuilder();
				sb.AppendFormat("{0}: {1}\r\n", "Offset", Offset);
				sb.AppendFormat("{0}: {1}\r\n", "Length", Length);
				sb.Append(Attributes.TextDescription);
				return sb.ToString();
			}
		}

		#endregion

		public override Attributes GetAttributes() {
			return Attributes;
		}

		public override ulong GetSectorSize() {
			return PhysicalDisk.Attributes.BytesPerSector;
		}

		public override SectorStatus GetSectorStatus(ulong sectorNum) {
			if (_fileSystem != null) {
				return _fileSystem.GetSectorStatus(sectorNum);
			} else {
				return SectorStatus.UnknownFilesystem;
			}
		}

		public string DeviceID {
			get { return Attributes.DeviceID; }
		}

		public StorageType StorageType {
			get { return StorageType.PhysicalDiskPartition; }
		}

		Attributes IFileSystemStore.Attributes {
			get { return Attributes; }
		}

		public IFileSystem FS {
			get { return _fileSystem; }
		}
	}
}
