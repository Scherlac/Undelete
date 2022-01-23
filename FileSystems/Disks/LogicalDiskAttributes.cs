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
using System.Management;
using System.Text;
using System.Xml.Serialization;

namespace KFS.Disks {
	/// <summary>
	/// Types of logical drives.
	/// </summary>
	public enum DriveType : uint {
		Unknown = 0,
		NoRootDirectory = 1,
		RemovableDisk = 2,
		LocalDisk = 3,
		NetworkDrive = 4,
		CompactDisc = 5,
		RAMDisk = 6
	}
	/// <summary>
	/// The attributes of a logical disk. Windows only.
	/// </summary>
	public class LogicalDiskAttributes : Attributes {
		public Access Access { get; set; }
		public Availability Availability { get; set; }
		public UInt64 BlockSize { get; set; }
		public string Caption { get; set; }
		public bool Compressed { get; set; }
		public UInt32 ConfigManagerErrorCode { get; set; }
		public bool ConfigManagerUserConfig { get; set; }
		public string CreationClassName { get; set; }
		public string Description { get; set; }
		public string DeviceID { get; set; }
		public DriveType DriveType { get; set; }
		public bool ErrorCleared { get; set; }
		public string ErrorDescription { get; set; }
		public string ErrorMethodology { get; set; }
		public string FileSystem { get; set; }
		public UInt64 FreeSpace { get; set; }
		public DateTime InstallDate { get; set; }
		public UInt32 LastErrorCode { get; set; }
		public UInt32 MaximumComponentLength { get; set; }
		public UInt32 MediaType { get; set; }
		public string Name { get; set; }
		public UInt64 NumberOfBlocks { get; set; }
		public string PNPDeviceID { get; set; }
		public PowerManagementCapability[] PowerManagementCapabilities { get; set; }
		public bool PowerManagementSupported { get; set; }
		public string ProviderName { get; set; }
		public string Purpose { get; set; }
		public bool QuotasDisabled { get; set; }
		public bool QuotasIncomplete { get; set; }
		public bool QuotasRebuilding { get; set; }
		public UInt64 Size { get; set; }
		public string Status { get; set; }
		public StatusInfo StatusInfo { get; set; }
		public bool SupportsDiskQuotas { get; set; }
		public bool SupportsFileBasedCompression { get; set; }
		public string SystemCreationClassName { get; set; }
		public string SystemName { get; set; }
		public bool VolumeDirty { get; set; }
		public string VolumeName { get; set; }
		public string VolumeSerialNumber { get; set; }

		public LogicalDiskAttributes() { }
		public LogicalDiskAttributes(ManagementObject mo) {
			Access = GetProperty<Access>(mo, "Access", Access.Unknown);
			Availability = GetProperty<Availability>(mo, "Availability", Availability.Unknown);
			BlockSize = GetProperty<UInt64>(mo, "BlockSize", 0);
			Caption = GetProperty<string>(mo, "Caption", "");
			Compressed = GetProperty<bool>(mo, "Compressed", false);
			ConfigManagerErrorCode = GetProperty<uint>(mo, "ConfigManagerErrorCode", 0);
			ConfigManagerUserConfig = GetProperty<bool>(mo, "ConfigManagerUserConfig", false);
			CreationClassName = GetProperty<string>(mo, "CreationClassName", "");
			Description = GetProperty<string>(mo, "Description", "");
			DeviceID = GetProperty<string>(mo, "DeviceID", "");
			DriveType = GetProperty<DriveType>(mo, "DriveType", DriveType.Unknown);
			ErrorCleared = GetProperty<bool>(mo, "ErrorCleared", false);
			ErrorDescription = GetProperty<string>(mo, "ErrorDescription", "");
			ErrorMethodology = GetProperty<string>(mo, "ErrorMethodology", "");
			FileSystem = GetProperty<string>(mo, "FileSystem", "");
			FreeSpace = GetProperty<UInt64>(mo, "FreeSpace", 0);
			InstallDate = GetProperty<DateTime>(mo, "InstallDate", DateTime.MinValue);
			LastErrorCode = GetProperty<uint>(mo, "LastErrorCode", 0);
			MaximumComponentLength = GetProperty<uint>(mo, "MaximumComponentLength", 0);
			MediaType = GetProperty<uint>(mo, "MediaType", 0);
			Name = GetProperty<string>(mo, "Name", "");
			NumberOfBlocks = GetProperty<UInt64>(mo, "NumberOfBlocks", 0);
			PNPDeviceID = GetProperty<string>(mo, "PNPDeviceID", "");
			PowerManagementCapabilities = GetArray<PowerManagementCapability>(mo, "PowerManagementCapabilities", new PowerManagementCapability[0]);
			PowerManagementSupported = GetProperty<bool>(mo, "PowerManagementSupported", false);
			ProviderName = GetProperty<string>(mo, "ProviderName", "");
			Purpose = GetProperty<string>(mo, "Purpose", "");
			QuotasDisabled = GetProperty<bool>(mo, "QuotasDisabled", false);
			QuotasIncomplete = GetProperty<bool>(mo, "QuotasIncomplete", false);
			QuotasRebuilding = GetProperty<bool>(mo, "QuotasRebuilding", false);
			Size = GetProperty<ulong>(mo, "Size", 0);
			Status = GetProperty<string>(mo, "Status", "");
			StatusInfo = GetProperty<StatusInfo>(mo, "StatusInfo", 0);
			SupportsDiskQuotas = GetProperty<bool>(mo, "SupportsDiskQuotas", false);
			SupportsFileBasedCompression = GetProperty<bool>(mo, "SupportsFileBasedCompression", false);
			SystemCreationClassName = GetProperty<string>(mo, "SystemCreationClassName", "");
			SystemName = GetProperty<string>(mo, "SystemName", "");
			VolumeDirty = GetProperty<bool>(mo, "VolumeDirty", false);
			VolumeName = GetProperty<string>(mo, "VolumeName", "");
			VolumeSerialNumber = GetProperty<string>(mo, "VolumeSerialNumber", "");
		}

		[XmlIgnore]
		public override string TextDescription {
			get {
				StringBuilder sb = new StringBuilder();
				sb.AppendFormat("{0}: {1}\r\n", "Access", Access);
				sb.AppendFormat("{0}: {1}\r\n", "Availability", Availability);
				sb.AppendFormat("{0}: {1}\r\n", "BlockSize", BlockSize);
				sb.AppendFormat("{0}: {1}\r\n", "Caption", Caption);
				sb.AppendFormat("{0}: {1}\r\n", "Compressed", Compressed);
				sb.AppendFormat("{0}: {1}\r\n", "ConfigManagerErrorCode", ConfigManagerErrorCode);
				sb.AppendFormat("{0}: {1}\r\n", "ConfigManagerUserConfig", ConfigManagerUserConfig);
				sb.AppendFormat("{0}: {1}\r\n", "CreationClassName", CreationClassName);
				sb.AppendFormat("{0}: {1}\r\n", "Description", Description);
				sb.AppendFormat("{0}: {1}\r\n", "DeviceID", DeviceID);
				sb.AppendFormat("{0}: {1}\r\n", "DriveType", DriveType);
				sb.AppendFormat("{0}: {1}\r\n", "ErrorCleared", ErrorCleared);
				sb.AppendFormat("{0}: {1}\r\n", "ErrorDescription", ErrorDescription);
				sb.AppendFormat("{0}: {1}\r\n", "ErrorMethodology", ErrorMethodology);
				sb.AppendFormat("{0}: {1}\r\n", "FileSystem", FileSystem);
				sb.AppendFormat("{0}: {1}\r\n", "FreeSpace", FreeSpace);
				sb.AppendFormat("{0}: {1}\r\n", "InstallDate", InstallDate);
				sb.AppendFormat("{0}: {1}\r\n", "LastErrorCode", LastErrorCode);
				sb.AppendFormat("{0}: {1}\r\n", "MaximumComponentLength", MaximumComponentLength);
				sb.AppendFormat("{0}: {1}\r\n", "MediaType", MediaType);
				sb.AppendFormat("{0}: {1}\r\n", "Name", Name);
				sb.AppendFormat("{0}: {1}\r\n", "NumberOfBlocks", NumberOfBlocks);
				sb.AppendFormat("{0}: {1}\r\n", "PNPDeviceID", PNPDeviceID);
				sb.AppendFormat("{0}: {1}\r\n", "PowerManagementCapabilities", ArrayToString(PowerManagementCapabilities));
				sb.AppendFormat("{0}: {1}\r\n", "PowerManagementSupported", PowerManagementSupported);
				sb.AppendFormat("{0}: {1}\r\n", "ProviderName", ProviderName);
				sb.AppendFormat("{0}: {1}\r\n", "Purpose", Purpose);
				sb.AppendFormat("{0}: {1}\r\n", "QuotasDisabled", QuotasDisabled);
				sb.AppendFormat("{0}: {1}\r\n", "QuotasIncomplete", QuotasIncomplete);
				sb.AppendFormat("{0}: {1}\r\n", "QuotasRebuilding", QuotasRebuilding);
				sb.AppendFormat("{0}: {1}\r\n", "Size", Size);
				sb.AppendFormat("{0}: {1}\r\n", "Status", Status);
				sb.AppendFormat("{0}: {1}\r\n", "StatusInfo", StatusInfo);
				sb.AppendFormat("{0}: {1}\r\n", "SupportsDiskQuotas", SupportsDiskQuotas);
				sb.AppendFormat("{0}: {1}\r\n", "SupportsFileBasedCompression", SupportsFileBasedCompression);
				sb.AppendFormat("{0}: {1}\r\n", "SystemCreationClassName", SystemCreationClassName);
				sb.AppendFormat("{0}: {1}\r\n", "SystemName", SystemName);
				sb.AppendFormat("{0}: {1}\r\n", "VolumeDirty", VolumeDirty);
				sb.AppendFormat("{0}: {1}\r\n", "VolumeName", VolumeName);
				sb.AppendFormat("{0}: {1}\r\n", "VolumeSerialNumber", VolumeSerialNumber);
				return sb.ToString();
			}
		}
	}
}
