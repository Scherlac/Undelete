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
using System.IO;

namespace KFS.Disks {
	/// <summary>
	/// Used when reading a disk from a Linux host system.
	/// </summary>
	public abstract class LinDisk : Disk {
		protected FileStream Handle { get; set; }

		#region Disk Members
		protected override void ForceReadBytes(byte[] result, ulong offset, ulong length) {
			Handle.Position = (long)offset;
			int bytes_read = Handle.Read(result, 0, (int)length);
			if (bytes_read != (int)length) {
				throw new Exception("IO Error. Bug in Linux version: Tried to read O:" + offset + ", L:" + length);
			}
		}
		#endregion
	}
}
