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
using System.Text;

namespace KFS.Disks {
	/// <summary>
	/// Whether a partition is active or inactive.
	/// </summary>
	public enum PartitionState {
		Inactive = 0x00,
		Active = 0x80
	}
	/// <summary>
	/// The type of filesystem contained on a partition.
	/// </summary>
	public enum PartitionType : byte {
		Unknown = 0x00,
		FAT12 = 0x01,
		XENIXRoot = 0x02,
		XENIXusr = 0x03,
		FAT16Old = 0x04,
		Extended = 0x05,
		FAT16 = 0x06,
		NTFS = 0x07,
		AIXBoot = 0x08,
		QNX = 0x09,
		OS2BootManager = 0x0A,
		FAT32 = 0x0B,
		FAT32WithInt13Support = 0x0C,
		QuestionMark = 0x0D,
		FAT16LBAMapped = 0x0E,
		Win95ExtendedWithLBA = 0x0F
	}
	/// <summary>
	/// The master boot record of a physical disk.
	/// </summary>
	public class MasterBootRecord : PhysicalDiskSection {
		public const int MBR_SIZE = 0x200;
		public const uint MAX_PARTITIONS = 4;

		public class PartitionEntry {
			public PartitionState State { get; private set; }
			public PartitionType PartitionType { get; private set; }
			public ulong PartitionOffset { get; private set; }
			public ulong PartitionLength { get; private set; }
			public int Index { get; private set; }
			public PartitionEntry(byte[] mbr, int offset, int sectorSize, int index) {
				State = (PartitionState)mbr[offset];
				SetPartitionType(mbr[offset + 0x4]);
				uint firstSector = BitConverter.ToUInt32(mbr, offset + 0x8);
				uint numSectorsInPartition = BitConverter.ToUInt32(mbr, offset + 0xC);
				PartitionOffset = (ulong)(firstSector * sectorSize);
				PartitionLength = (ulong)(numSectorsInPartition * sectorSize);
				Index = index;
			}

			private void SetPartitionType(byte val) {
				PartitionType = Enum.IsDefined(typeof(PartitionType), val) ? (PartitionType)val : PartitionType.Unknown;
			}
		}

		private byte[] _data;

		public MasterBootRecord(WinPhysicalDisk disk) {
			PhysicalDisk = disk;
			Offset = 0;
			Length = MBR_SIZE;

			_partitionEntries = new List<PartitionEntry>();

			try {
				_data = disk.GetBytes(0, MBR_SIZE);
			} catch (Exception e) {
				throw new Exception("Failed to load the Master Boot Record!", e);
			}

			for (int i = 0; i < MAX_PARTITIONS; i++) {
				int offset = 0x1BE + 16 * i;
				if (PartitionEntryExistsAt(offset, 16)) {
					_partitionEntries.Add(new PartitionEntry(_data, offset, (int)disk.Attributes.BytesPerSector, i));
				}
			}

			_partitionEntries.Sort(new Comparison<PartitionEntry>(delegate(PartitionEntry a, PartitionEntry b) {
				return a.PartitionOffset.CompareTo(b.PartitionOffset);
			}));
		}

		private bool PartitionEntryExistsAt(int offset, int len) {
			byte total = 0;
			for (int i = offset; i < offset + len; i++) {
				total |= _data[i];
			}
			return total != 0;
		}

		private List<PartitionEntry> _partitionEntries;
		public List<PartitionEntry> PartitionEntries {
			get { return new List<PartitionEntry>(_partitionEntries); }
		}

		public override string ToString() {
			return StreamName;
		}

		public override string StreamName {
			get {
				return "Master Boot Record";
			}
		}

		#region IDescribable Members

		public override string TextDescription {
			get {
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("Master Boot Record");
				sb.AppendFormat("{0}: {1}\r\n", "Offset", Offset);
				sb.AppendFormat("{0}: {1}\r\n", "Length", Length);
				return sb.ToString();
			}
		}

		#endregion

		public override Attributes GetAttributes() {
			return new MasterBootRecordAttributes("Master Boot Record");
		}

		public override ulong GetSectorSize() {
			return PhysicalDisk.Attributes.BytesPerSector;
		}

		public override SectorStatus GetSectorStatus(ulong sectorNum) {
			return SectorStatus.MasterBootRecord;
		}
	}
}
