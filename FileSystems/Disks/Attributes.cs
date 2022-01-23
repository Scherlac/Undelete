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
using System.Collections;
using System.Linq;
using System.Management;
using System.Xml.Serialization;

namespace KFS.Disks {

	#region Enums
	public enum Access : ushort {
		Unknown = 0,
		Readable = 1,
		Writable = 2,
		ReadWriteSupported = 3,
		WriteOnce = 4
	}
	public enum Availability : int {
		None = 0,
		Other = 1,
		Unknown = 2,
		RunningOrFullPower = 3,
		Warning = 4,
		InTest = 5,
		NotApplicable = 6,
		PowerOff = 7,
		OffLine = 8,
		OffDuty = 9,
		Degraded = 10,
		NotInstalled = 11,
		InstallError = 12,
		PowerSaveUnknown = 13,
		PowerSaveLowPowerMode = 14,
		PowerSaveStandby = 15,
		PowerCycle = 16,
		PowerSaveWarning = 17
	}
	public enum Capability : ushort {
		Unknown = 0,
		Other = 1,
		SequentialAccess = 2,
		RandomAccess = 3,
		SupportsWriting = 4,
		Encryption = 5,
		Compression = 6,
		SupportsRemovableMedia = 7,
		ManualCleaning = 8,
		AutomaticCleaning = 9,
		SMARTNotification = 10,
		SupportsDualSidedMedia = 11,
		EjectionPriorToDriveDismountNotRequired = 12
	}
	public enum PowerManagementCapability : int {
		Unknown = 0,
		NotSupported = 1,
		Disabled = 2,
		Enabled = 3,
		PowerSavingModesEnteredAutomatically = 4,
		PowerStateSettable = 5,
		PowerCyclingSupported = 6,
		TimedPowerOnSupported = 7
	}
	public enum StatusInfo : int {
		None = 0,
		Other = 1,
		Unknown = 2,
		Enabled = 3,
		Disabled = 4,
		NotApplicable = 5
	}
	#endregion

	/// <summary>
	/// Describes the attributes of a disk or disk section. Windows only.
	/// </summary>
	[XmlInclude(typeof(MasterBootRecordAttributes))]
	[XmlInclude(typeof(PhysicalDiskAttributes))]
	[XmlInclude(typeof(PhysicalDiskPartitionAttributes))]
	[XmlInclude(typeof(UnallocatedDiskAreaAttributes))]
	public abstract class Attributes : IDescribable {
		protected T GetProperty<T>(ManagementObject mo, string name, T def) {
			try {
				object o = mo[name];
				if (o != null) {
					if (typeof(T).IsEnum) {
						return (T)Enum.ToObject(typeof(T), o);
					} else {
						return (T)Convert.ChangeType(o, typeof(T));
					}
				}
			} catch (ManagementException) { }
			return def;
		}

		protected T[] GetArray<T>(ManagementObject mo, string name, T[] def) {
			try {
				object o = mo[name];
				if (o != null && o.GetType().IsArray) {
					ArrayList list = new ArrayList((ICollection)o);
					T[] res = new T[list.Count];
					for (int i = 0; i < list.Count; i++) {
						res[i] = (T)list[i];
					}
					return res;
				}
			} catch (ManagementException) { }
			return def;
		}

		protected string ArrayToString<T>(T[] a) {
			return string.Join(", ", a.Select(x => x.ToString()));
		}

		public abstract string TextDescription { get; }
	}
}
