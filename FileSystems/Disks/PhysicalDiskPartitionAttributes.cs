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
using System.ComponentModel;
using System.Management;
using System.Text;
using System.Xml.Serialization;

namespace KFS.Disks {
	/// <summary>
	/// Attributes of a physical disk partition. Windows only.
	/// </summary>
	public class PhysicalDiskPartitionAttributes : Attributes, IDescribable {
		public PartitionType PartitionType { get; set; }
		public Access Access { get; set; }
		public Availability Availability { get; set; }
		public ulong BlockSize { get; set; }
		public bool Bootable { get; set; }
		public bool BootPartition { get; set; }
		[DefaultValue(512)]
		public uint BytesPerSector { get; set; }
		public string Caption { get; set; }
		public uint ConfigManagerErrorCode { get; set; }
		public bool ConfigManagerUserConfig { get; set; }
		public string CreationClassName { get; set; }
		public string Description { get; set; }
		public string DeviceID { get; set; }
		public uint DiskIndex { get; set; }
		public bool ErrorCleared { get; set; }
		public string ErrorDescription { get; set; }
		public string ErrorMethodology { get; set; }
		public uint HiddenSectors { get; set; }
		public uint Index { get; set; }
		public DateTime InstallDate { get; set; }
		public uint LastErrorCode { get; set; }
		public string Name { get; set; }
		public ulong NumberOfBlocks { get; set; }
		public string PNPDeviceID { get; set; }
		public PowerManagementCapability[] PowerManagementCapabilities { get; set; }
		public bool PowerManagementSupported { get; set; }
		public bool PrimaryPartition { get; set; }
		public string Purpose { get; set; }
		public bool RewritePartition { get; set; }
		public ulong Size { get; set; }
		public ulong StartingOffset { get; set; }
		public string Status { get; set; }
		public StatusInfo StatusInfo { get; set; }
		public string SystemCreationClassName { get; set; }
		public string SystemName { get; set; }
		public string Type { get; set; }

		public PhysicalDiskPartitionAttributes() { }
		public PhysicalDiskPartitionAttributes(ManagementObject mo, WinPhysicalDisk disk) {
			BytesPerSector = disk.Attributes.BytesPerSector;
			Access = GetProperty<Access>(mo, "Access", Access.Unknown);
			Availability = GetProperty<Availability>(mo, "Availability", Availability.Unknown);
			BlockSize = GetProperty<ulong>(mo, "BlockSize", 0);
			Bootable = GetProperty<bool>(mo, "Bootable", false);
			BootPartition = GetProperty<bool>(mo, "BootPartition", false);
			Caption = GetProperty<string>(mo, "Caption", "");
			ConfigManagerErrorCode = GetProperty<uint>(mo, "ConfigManagerErrorCode", 0);
			ConfigManagerUserConfig = GetProperty<bool>(mo, "ConfigManagerUserConfig", false);
			CreationClassName = GetProperty<string>(mo, "CreationClassName", "");
			Description = GetProperty<string>(mo, "Description", "");
			DeviceID = GetProperty<string>(mo, "DeviceID", "");
			DiskIndex = GetProperty<uint>(mo, "DiskIndex", 0);
			ErrorCleared = GetProperty<bool>(mo, "ErrorCleared", false);
			ErrorDescription = GetProperty<string>(mo, "ErrorDescription", "");
			ErrorMethodology = GetProperty<string>(mo, "ErrorMethodology", "");
			HiddenSectors = GetProperty<uint>(mo, "HiddenSectors", 0);
			Index = GetProperty<uint>(mo, "Index", 0);
			InstallDate = GetProperty<DateTime>(mo, "InstallDate", DateTime.MinValue);
			LastErrorCode = GetProperty<uint>(mo, "LastErrorCode", 0);
			Name = GetProperty<string>(mo, "Name", "");
			NumberOfBlocks = GetProperty<ulong>(mo, "NumberOfBlocks", 0);
			PNPDeviceID = GetProperty<string>(mo, "PNPDeviceID", "");
			PowerManagementCapabilities = GetArray<PowerManagementCapability>(mo, "PowerManagementCapabilities", new PowerManagementCapability[0]);
			PowerManagementSupported = GetProperty<bool>(mo, "PowerManagementSupported", false);
			PrimaryPartition = GetProperty<bool>(mo, "PrimaryPartition", false);
			Purpose = GetProperty<string>(mo, "Purpose", "");
			RewritePartition = GetProperty<bool>(mo, "RewritePartition", false);
			Size = GetProperty<ulong>(mo, "Size", 0);
			StartingOffset = GetProperty<ulong>(mo, "StartingOffset", 0);
			Status = GetProperty<string>(mo, "Status", "");
			StatusInfo = GetProperty<StatusInfo>(mo, "StatusInfo", 0);
			SystemCreationClassName = GetProperty<string>(mo, "SystemCreationClassName", "");
			SystemName = GetProperty<string>(mo, "SystemName", "");
			Type = GetProperty<string>(mo, "Type", "");
		}

		[XmlIgnore]
		public override string TextDescription {
			get {
				StringBuilder sb = new StringBuilder();
				sb.AppendFormat("{0}: {1}\r\n", "PartitionType", PartitionType);
				sb.AppendLine();
				sb.AppendFormat("{0}: {1}\r\n", "Access", Access);
				sb.AppendFormat("{0}: {1}\r\n", "Availability", Availability);
				sb.AppendFormat("{0}: {1}\r\n", "BlockSize", BlockSize);
				sb.AppendFormat("{0}: {1}\r\n", "Bootable", Bootable);
				sb.AppendFormat("{0}: {1}\r\n", "BootPartition", BootPartition);
				sb.AppendFormat("{0}: {1}\r\n", "Caption", Caption);
				sb.AppendFormat("{0}: {1}\r\n", "ConfigManagerErrorCode", ConfigManagerErrorCode);
				sb.AppendFormat("{0}: {1}\r\n", "ConfigManagerUserConfig", ConfigManagerUserConfig);
				sb.AppendFormat("{0}: {1}\r\n", "CreationClassName", CreationClassName);
				sb.AppendFormat("{0}: {1}\r\n", "Description", Description);
				sb.AppendFormat("{0}: {1}\r\n", "DeviceID", DeviceID);
				sb.AppendFormat("{0}: {1}\r\n", "DiskIndex", DiskIndex);
				sb.AppendFormat("{0}: {1}\r\n", "ErrorCleared", ErrorCleared);
				sb.AppendFormat("{0}: {1}\r\n", "ErrorDescription", ErrorDescription);
				sb.AppendFormat("{0}: {1}\r\n", "ErrorMethodology", ErrorMethodology);
				sb.AppendFormat("{0}: {1}\r\n", "HiddenSectors", HiddenSectors);
				sb.AppendFormat("{0}: {1}\r\n", "Index", Index);
				sb.AppendFormat("{0}: {1}\r\n", "InstallDate", InstallDate);
				sb.AppendFormat("{0}: {1}\r\n", "LastErrorCode", LastErrorCode);
				sb.AppendFormat("{0}: {1}\r\n", "Name", Name);
				sb.AppendFormat("{0}: {1}\r\n", "NumberOfBlocks", NumberOfBlocks);
				sb.AppendFormat("{0}: {1}\r\n", "PNPDeviceID", PNPDeviceID);
				sb.AppendFormat("{0}: {1}\r\n", "PowerManagementCapabilities", ArrayToString(PowerManagementCapabilities));
				sb.AppendFormat("{0}: {1}\r\n", "PowerManagementSupported", PowerManagementSupported);
				sb.AppendFormat("{0}: {1}\r\n", "PrimaryPartition", PrimaryPartition);
				sb.AppendFormat("{0}: {1}\r\n", "Purpose", Purpose);
				sb.AppendFormat("{0}: {1}\r\n", "RewritePartition", RewritePartition);
				sb.AppendFormat("{0}: {1}\r\n", "Size", Size);
				sb.AppendFormat("{0}: {1}\r\n", "StartingOffset", StartingOffset);
				sb.AppendFormat("{0}: {1}\r\n", "Status", Status);
				sb.AppendFormat("{0}: {1}\r\n", "StatusInfo", StatusInfo);
				sb.AppendFormat("{0}: {1}\r\n", "SystemCreationClassName", SystemCreationClassName);
				sb.AppendFormat("{0}: {1}\r\n", "SystemName", SystemName);
				sb.AppendFormat("{0}: {1}\r\n", "Type", Type);
				return sb.ToString();
			}
		}
	}
}
