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

using Microsoft.Win32.SafeHandles;
using System;

namespace KFS.Disks {
	/// <summary>
	/// Represents a disk, and uses the Win32 API to read bytes from it.
	/// Can only be used when running on Windows.
	/// </summary>
	public abstract class WinDisk : Disk {
		protected SafeFileHandle Handle { get; set; }

		#region Disk Members
		protected override void ForceReadBytes(byte[] result, ulong offset, ulong length) {
			uint bytesRead = 0;

			long filePtr;
			Win32.SetFilePointerEx(Handle, (long)offset, out filePtr, EMoveMethod.Begin);
			bool readSuccess = Win32.ReadFile(Handle, result, (uint)length - bytesRead, out bytesRead, IntPtr.Zero);
			if (!readSuccess) {
				Console.Error.Write("Read failed: {0}", Handle);
			}
		}
		#endregion
	}
}

