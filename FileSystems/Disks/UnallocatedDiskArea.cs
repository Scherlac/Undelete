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

using System.Text;

namespace KFS.Disks {
	/// <summary>
	/// An unallocated section of the disk (also known as slack space).
	/// </summary>
	public class UnallocatedDiskArea : PhysicalDiskSection {
		public UnallocatedDiskArea(IPhysicalDisk disk, ulong offset, ulong len) {
			PhysicalDisk = disk;
			Offset = offset;
			Length = len;
		}

		public override string ToString() {
			return StreamName;
		}

		public override string StreamName {
			get {
				return "Unallocated Disk Space";
			}
		}

		#region IDescribable Members

		public override string TextDescription {
			get {
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("Unallocated disk space");
				sb.AppendFormat("{0}: {1}\r\n", "Offset", Offset);
				sb.AppendFormat("{0}: {1}\r\n", "Length", Length);
				return sb.ToString();
			}
		}

		public override Attributes GetAttributes() {
			return new UnallocatedDiskAreaAttributes();
		}

		#endregion

		public override ulong GetSectorSize() {
			return PhysicalDisk.Attributes.BytesPerSector;
		}

		public override SectorStatus GetSectorStatus(ulong sectorNum) {
			return SectorStatus.SlackSpace;
		}
	}
}
