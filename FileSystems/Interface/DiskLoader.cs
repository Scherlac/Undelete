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

using KFS.Disks;
using System;
using System.Collections.Generic;

namespace KFS.FileSystems {
	/// <summary>
	/// Provides access to the user's currently mounted disks.
	/// </summary>
	public abstract class DiskLoader {
		public static DiskLoader GetNativeLoader() {
			if (IsWindows()) {
				return new WinDiskLoader();
			} else {
				return new LinDiskLoader();
			}
		}

		static bool IsWindows() {
			int p = (int)Environment.OSVersion.Platform;
			return ((p != 4) && (p != 6) && (p != 128));
		}

		public static List<Disk> LoadDisks() {
			return GetNativeLoader().LoadDisksInternal();
		}

		public static List<Disk> LoadLogicalVolumes() {
			return GetNativeLoader().LoadLogicalVolumesInternal();
		}

		protected abstract List<Disk> LoadDisksInternal();

		protected abstract List<Disk> LoadLogicalVolumesInternal();
	}
}
