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
using System.Collections.ObjectModel;
using System.Management;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace KFS.Disks {
	/// <summary>
	/// A physical disk attached to a Windows host system.
	/// </summary>
	public class WinPhysicalDisk : WinDisk, IPhysicalDisk, IImageable, IDescribable {
		public PhysicalDiskAttributes Attributes { get; private set; }

		private ulong _size;
		public WinPhysicalDisk(ManagementObject mo) {
			Attributes = new PhysicalDiskAttributes(mo);

			Handle = Win32.CreateFile(Attributes.DeviceID, EFileAccess.GenericRead, EFileShare.Read | EFileShare.Write | EFileShare.Delete, IntPtr.Zero, ECreationDisposition.OpenExisting, EFileAttributes.None, IntPtr.Zero);

			if (Handle.IsInvalid) {
				throw new Exception("Failed to get a handle to the physical disk. " + Marshal.GetLastWin32Error());
			}
			_size = Util.GetDiskSize(Handle);

			GetDiskSections();
		}

		private List<PhysicalDiskSection> _sections;
		[XmlIgnore]
		public IList<PhysicalDiskSection> Sections {
			get { return new ReadOnlyCollection<PhysicalDiskSection>(_sections); }
		}

		private MasterBootRecord _masterBootRecord;
		private void GetDiskSections() {
			_sections = new List<PhysicalDiskSection>();
			try {
				_masterBootRecord = new MasterBootRecord(this);
				_sections.Add(_masterBootRecord);
				ulong offset = MasterBootRecord.MBR_SIZE;
				foreach (MasterBootRecord.PartitionEntry pEntry in _masterBootRecord.PartitionEntries) {
					if (pEntry.PartitionOffset > offset) {
						_sections.Add(new UnallocatedDiskArea(this, offset, pEntry.PartitionOffset - offset));
					}
					if (offset > pEntry.PartitionOffset) {
						throw new Exception("Something went wrong!");
					}

					_sections.Add(new PhysicalDiskPartition(this, pEntry));

					offset = pEntry.PartitionOffset + pEntry.PartitionLength;
				}
				if (StreamLength > offset) {
					_sections.Add(new UnallocatedDiskArea(this, offset, StreamLength - offset));
				}
			} catch (Exception) { }
		}

		public override string ToString() {
			return StreamName;
		}

		#region IDataStream Members

		public override ulong StreamLength {
			get { return _size; }
		}

		public override String StreamName {
			get { return Attributes.Caption; }
		}

		#endregion

		#region IDescribable Members

		public string TextDescription {
			get {
				return Attributes.TextDescription;
			}
		}

		#endregion

		#region IImageable Members

		public Attributes GetAttributes() {
			return Attributes;
		}

		public ulong GetSectorSize() {
			return Attributes.BytesPerSector;
		}

		public SectorStatus GetSectorStatus(ulong sectorNum) {
			foreach (PhysicalDiskSection section in _sections) {
				if (section.Offset / Attributes.BytesPerSector <= sectorNum
						&& (section.Offset + section.Length) / Attributes.BytesPerSector > sectorNum) {
					return section.GetSectorStatus(sectorNum - section.Offset / Attributes.BytesPerSector);
				}
			}
			return SectorStatus.Unknown;
		}

		#endregion
	}
}
