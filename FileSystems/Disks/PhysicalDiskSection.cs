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

namespace KFS.Disks {
	/// <summary>
	/// Part of a physical disk.
	/// </summary>
	public abstract class PhysicalDiskSection : IImageable, IDescribable {
		public IPhysicalDisk PhysicalDisk { get; protected set; }
		public ulong Offset { get; protected set; }
		public ulong Length { get; protected set; }

		#region IDataStream Members

		public byte[] GetBytes(ulong offset, ulong length) {
			if ((ulong)offset + length - 1 >= Length) {
				throw new IndexOutOfRangeException("Tried to read off the end of the physical disk!");
			}
			return PhysicalDisk.GetBytes(offset + Offset, length);
		}

		public ulong StreamLength {
			get { return Length; }
		}

		public virtual String StreamName {
			get { return "Physical Disk Section"; }
		}

		public virtual IDataStream ParentStream {
			get { return PhysicalDisk; }
		}

		public ulong DeviceOffset {
			get { return ParentStream.DeviceOffset + Offset; }
		}

		public void Open() { }

		public void Close() { }

		#endregion

		#region IImageable Members

		public abstract Attributes GetAttributes();

		public abstract ulong GetSectorSize();

		public abstract SectorStatus GetSectorStatus(ulong sectorNum);

		#endregion

		#region IDescribable Members

		public abstract string TextDescription { get; }

		#endregion
	}
}
