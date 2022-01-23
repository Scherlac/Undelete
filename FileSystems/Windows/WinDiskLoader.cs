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
using System.Management;

namespace KFS.FileSystems {
	/// <summary>
	/// Deals with loading disks on a Windows host system.
	/// </summary>
	public class WinDiskLoader : DiskLoader {
		protected override List<Disk> LoadDisksInternal() {
			List<Disk> res = new List<Disk>();
			try {
				ManagementScope ms = new ManagementScope();
				ObjectQuery oq = new ObjectQuery("SELECT * FROM Win32_DiskDrive");
				ManagementObjectSearcher mos = new ManagementObjectSearcher(ms, oq);
				ManagementObjectCollection moc = mos.Get();
				foreach (ManagementObject mo in moc) {
					WinPhysicalDisk disk = new WinPhysicalDisk(mo);
					res.Add(disk);
				}

			} catch (Exception e) { Console.Error.WriteLine(e); }
			return res;
		}

		protected override List<Disk> LoadLogicalVolumesInternal() {
			// TODO: Why not Environment.GetLogicalVolumes? Only mounted?
			List<Disk> res = new List<Disk>();
			try {
				ManagementScope ms = new ManagementScope();
				ObjectQuery oq = new ObjectQuery("SELECT * FROM Win32_LogicalDisk");
				ManagementObjectSearcher mos = new ManagementObjectSearcher(ms, oq);
				ManagementObjectCollection moc = mos.Get();
				foreach (ManagementObject mo in moc) {
					try {
						WinLogicalDisk disk = new WinLogicalDisk(mo);
						res.Add(disk);
					} catch (Exception e) {
						Console.Error.WriteLine(e);
					}
				}
			} catch (Exception e) { Console.Error.WriteLine(e); }
			return res;
		}
	}
}
