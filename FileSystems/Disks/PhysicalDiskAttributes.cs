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
	/// Attributes of a physical disk. Windows only.
	/// </summary>
	public class PhysicalDiskAttributes : Attributes, IDescribable {
		public Availability Availability { get; set; }
		public uint BytesPerSector { get; set; }
		public Capability[] Capabilities { get; set; }
		public string[] CapabilityDescriptions { get; set; }
		public string Caption { get; set; }
		public string CompressionMethod { get; set; }
		public uint ConfigManagerErrorCode { get; set; }
		public bool ConfigManagerUserConfig { get; set; }
		public string CreationClassName { get; set; }
		public UInt64 DefaultBlockSize { get; set; }
		public string Description { get; set; }
		public string DeviceID { get; set; }
		public bool ErrorCleared { get; set; }
		public string ErrorDescription { get; set; }
		public string ErrorMethodology { get; set; }
		public string FirmwareRevision { get; set; }
		public int Index { get; set; }
		public DateTime InstallDate { get; set; }
		public string InterfaceType { get; set; }
		public int LastErrorCode { get; set; }
		public string Manufacturer { get; set; }
		public UInt64 MaxBlockSize { get; set; }
		public UInt64 MaxMediaSize { get; set; }
		public bool MediaLoaded { get; set; }
		public string MediaType { get; set; }
		public UInt64 MinBlockSize { get; set; }
		public string Model { get; set; }
		public string Name { get; set; }
		public bool NeedsCleaning { get; set; }
		public int NumberOfMediaSupported { get; set; }
		public int Partitions { get; set; }
		public string PNPDeviceID { get; set; }
		public PowerManagementCapability[] PowerManagementCapabilities { get; set; }
		public bool PowerManagementSupported { get; set; }
		public UInt32 SCSIBus { get; set; }
		public UInt16 SCSILogicalUnit { get; set; }
		public UInt16 SCSIPort { get; set; }
		public UInt16 SCSITargetId { get; set; }
		public UInt32 SectorsPerTrack { get; set; }
		public string SerialNumber { get; set; }
		public UInt32 Signature { get; set; }
		public UInt64 Size { get; set; }
		public string Status { get; set; }
		public StatusInfo StatusInfo { get; set; }
		public string SystemCreationClassName { get; set; }
		public string SystemName { get; set; }
		public UInt64 TotalCylinders { get; set; }
		public UInt32 TotalHeads { get; set; }
		public UInt64 TotalSectors { get; set; }
		public UInt64 TotalTracks { get; set; }
		public UInt32 TracksPerCylinder { get; set; }

		public PhysicalDiskAttributes() { }
		public PhysicalDiskAttributes(ManagementObject mo) {
			Availability = GetProperty<Availability>(mo, "Availability", Availability.Unknown);
			BytesPerSector = GetProperty<uint>(mo, "BytesPerSector", 512);
			Capabilities = GetArray<Capability>(mo, "Capabilities", new Capability[0]);
			CapabilityDescriptions = GetArray<string>(mo, "CapabilityDescriptions", new string[0]);
			Caption = GetProperty<string>(mo, "Caption", "");
			CompressionMethod = GetProperty<string>(mo, "CompressionMethod", "");
			ConfigManagerErrorCode = GetProperty<uint>(mo, "ConfigManagerErrorCode", 0);
			ConfigManagerUserConfig = GetProperty<bool>(mo, "ConfigManagerUserConfig", false);
			CreationClassName = GetProperty<string>(mo, "CreationClassName", "");
			DefaultBlockSize = GetProperty<ulong>(mo, "DefaultBlockSize", 0);
			Description = GetProperty<string>(mo, "Description", "");
			DeviceID = GetProperty<string>(mo, "DeviceID", "");
			ErrorCleared = GetProperty<bool>(mo, "ErrorCleared", false);
			ErrorDescription = GetProperty<string>(mo, "ErrorDescription", "");
			ErrorMethodology = GetProperty<string>(mo, "ErrorMethodology", "");
			FirmwareRevision = GetProperty<string>(mo, "FirmwareRevision", "");
			Index = GetProperty<int>(mo, "Index", 0);
			InstallDate = GetProperty<DateTime>(mo, "InstallDate", DateTime.MinValue);
			InterfaceType = GetProperty<string>(mo, "InterfaceType", "");
			LastErrorCode = GetProperty<int>(mo, "LastErrorCode", 0);
			Manufacturer = GetProperty<string>(mo, "Manufacturer", "");
			MaxBlockSize = GetProperty<ulong>(mo, "MaxBlockSize", 0);
			MaxMediaSize = GetProperty<ulong>(mo, "MaxMediaSize", 0);
			MediaLoaded = GetProperty<bool>(mo, "MediaLoaded", false);
			MediaType = GetProperty<string>(mo, "MediaType", "").Replace('\t', ' ');
			Model = GetProperty<string>(mo, "Model", "");
			Name = GetProperty<string>(mo, "Name", "");
			NeedsCleaning = GetProperty<bool>(mo, "NeedsCleaning", false);
			NumberOfMediaSupported = GetProperty<int>(mo, "NumberOfMediaSupported", 0);
			Partitions = GetProperty<int>(mo, "Partitions", 0);
			PNPDeviceID = GetProperty<string>(mo, "PNPDeviceID", "");
			PowerManagementCapabilities = GetArray<PowerManagementCapability>(mo, "PowerManagementCapabilities", new PowerManagementCapability[0]);
			PowerManagementSupported = GetProperty<bool>(mo, "PowerManagementSupported", false);
			SCSIBus = GetProperty<uint>(mo, "SCSIBus", 0);
			SCSILogicalUnit = GetProperty<ushort>(mo, "SCSILogicalUnit", 0);
			SCSIPort = GetProperty<ushort>(mo, "SCSIPort", 0);
			SCSITargetId = GetProperty<ushort>(mo, "SCSITargetId", 0);
			SectorsPerTrack = GetProperty<uint>(mo, "SectorsPerTrack", 0);
			SerialNumber = GetProperty<string>(mo, "SerialNumber", "");
			Signature = GetProperty<uint>(mo, "Signature", 0);
			Size = GetProperty<ulong>(mo, "Size", 0);
			Status = GetProperty<string>(mo, "Status", "");
			StatusInfo = GetProperty<StatusInfo>(mo, "StatusInfo", 0);
			SystemCreationClassName = GetProperty<string>(mo, "SystemCreationClassName", "");
			SystemName = GetProperty<string>(mo, "SystemName", "");
			TotalCylinders = GetProperty<ulong>(mo, "TotalCylinders", 0);
			TotalHeads = GetProperty<uint>(mo, "TotalHeads", 0);
			TotalSectors = GetProperty<ulong>(mo, "TotalSectors", 0);
			TotalTracks = GetProperty<ulong>(mo, "TotalTracks", 0);
			TracksPerCylinder = GetProperty<uint>(mo, "TracksPerCylinder", 0);
		}

		[XmlIgnore]
		public override string TextDescription {
			get {
				StringBuilder sb = new StringBuilder();
				sb.AppendFormat("{0}: {1}\r\n", "Availability", Availability);
				sb.AppendFormat("{0}: {1}\r\n", "BytesPerSector", BytesPerSector);
				sb.AppendFormat("{0}: {1}\r\n", "Capabilities", ArrayToString(Capabilities));
				sb.AppendFormat("{0}: {1}\r\n", "CapabilityDescriptions", ArrayToString(CapabilityDescriptions));
				sb.AppendFormat("{0}: {1}\r\n", "Caption", Caption);
				sb.AppendFormat("{0}: {1}\r\n", "CompressionMethod", CompressionMethod);
				sb.AppendFormat("{0}: {1}\r\n", "ConfigManagerErrorCode", ConfigManagerErrorCode);
				sb.AppendFormat("{0}: {1}\r\n", "ConfigManagerUserConfig", ConfigManagerUserConfig);
				sb.AppendFormat("{0}: {1}\r\n", "CreationClassName", CreationClassName);
				sb.AppendFormat("{0}: {1}\r\n", "DefaultBlockSize", DefaultBlockSize);
				sb.AppendFormat("{0}: {1}\r\n", "Description", Description);
				sb.AppendFormat("{0}: {1}\r\n", "DeviceID", DeviceID);
				sb.AppendFormat("{0}: {1}\r\n", "ErrorCleared", ErrorCleared);
				sb.AppendFormat("{0}: {1}\r\n", "ErrorDescription", ErrorDescription);
				sb.AppendFormat("{0}: {1}\r\n", "ErrorMethodology", ErrorMethodology);
				sb.AppendFormat("{0}: {1}\r\n", "FirmwareRevision", FirmwareRevision);
				sb.AppendFormat("{0}: {1}\r\n", "Index", Index);
				sb.AppendFormat("{0}: {1}\r\n", "InstallDate", InstallDate);
				sb.AppendFormat("{0}: {1}\r\n", "InterfaceType", InterfaceType);
				sb.AppendFormat("{0}: {1}\r\n", "LastErrorCode", LastErrorCode);
				sb.AppendFormat("{0}: {1}\r\n", "Manufacturer", Manufacturer);
				sb.AppendFormat("{0}: {1}\r\n", "MaxBlockSize", MaxBlockSize);
				sb.AppendFormat("{0}: {1}\r\n", "MaxMediaSize", MaxMediaSize);
				sb.AppendFormat("{0}: {1}\r\n", "MediaLoaded", MediaLoaded);
				sb.AppendFormat("{0}: {1}\r\n", "MediaType", MediaType);
				sb.AppendFormat("{0}: {1}\r\n", "MinBlockSize", MinBlockSize);
				sb.AppendFormat("{0}: {1}\r\n", "Model", Model);
				sb.AppendFormat("{0}: {1}\r\n", "Name", Name);
				sb.AppendFormat("{0}: {1}\r\n", "NeedsCleaning", NeedsCleaning);
				sb.AppendFormat("{0}: {1}\r\n", "NumberOfMediaSupported", NumberOfMediaSupported);
				sb.AppendFormat("{0}: {1}\r\n", "Partitions", Partitions);
				sb.AppendFormat("{0}: {1}\r\n", "PowerManagementCapabilities", ArrayToString(PowerManagementCapabilities));
				sb.AppendFormat("{0}: {1}\r\n", "PowerManagementSupported", PowerManagementSupported);
				sb.AppendFormat("{0}: {1}\r\n", "SCSIBus", SCSIBus);
				sb.AppendFormat("{0}: {1}\r\n", "SCSILogicalUnit", SCSILogicalUnit);
				sb.AppendFormat("{0}: {1}\r\n", "SCSIPort", SCSIPort);
				sb.AppendFormat("{0}: {1}\r\n", "SCSITargetId", SCSITargetId);
				sb.AppendFormat("{0}: {1}\r\n", "SectorsPerTrack", SectorsPerTrack);
				sb.AppendFormat("{0}: {1}\r\n", "SerialNumber", SerialNumber);
				sb.AppendFormat("{0}: {1}\r\n", "Signature", Signature);
				sb.AppendFormat("{0}: {1}\r\n", "Size", Size);
				sb.AppendFormat("{0}: {1}\r\n", "Status", Status);
				sb.AppendFormat("{0}: {1}\r\n", "StatusInfo", StatusInfo);
				sb.AppendFormat("{0}: {1}\r\n", "SystemCreationClassName", SystemCreationClassName);
				sb.AppendFormat("{0}: {1}\r\n", "SystemName", SystemName);
				sb.AppendFormat("{0}: {1}\r\n", "TotalCylinders", TotalCylinders);
				sb.AppendFormat("{0}: {1}\r\n", "TotalHeads", TotalHeads);
				sb.AppendFormat("{0}: {1}\r\n", "TotalSectors", TotalSectors);
				sb.AppendFormat("{0}: {1}\r\n", "TotalTracks", TotalTracks);
				sb.AppendFormat("{0}: {1}\r\n", "TracksPerCylinder", TracksPerCylinder);
				return sb.ToString();
			}
		}
	}
}
