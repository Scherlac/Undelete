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

using KFA.Exceptions;
using KFS.DataStream;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace KFS.FileSystems.NTFS {
	/// <summary>
	/// Tells an MFT record whether to load all, none, or some of its attributes.
	/// </summary>
	public enum MFTLoadDepth {
		Full = 2,
		NameAttributeOnly = 1,
		None = 0
	}

	[Flags]
	public enum FilenameType {
		Posix = 0,
		Win32 = 1,
		Dos = 2,
	}

	[Flags]
	public enum RecordFlags {
		InUse = 1,
		Directory = 2,
		ViewIndex = 8,
		SpaceFiller = 16
	}

	[Flags]
	public enum FilePermissions {
		ReadOnly = 0x1,
		Hidden = 0x2,
		System = 0x4,
		Archive = 0x20,
		Device = 0x40,
		Normal = 0x80,
		Temporary = 0x100,
		SparseFile = 0x200,
		ReparsePoint = 0x400,
		Compressed = 0x800,
		Offline = 0x1000,
		NotContentIndexed = 0x2000,
		Encrypted = 0x4000
	}

	/// <summary>
	/// A record in the NTFS Master File Table. Stores metadata about a single file.
	/// </summary>
	public class MFTRecord : INodeMetadata {

		#region Header Fields

		public UInt64 LogSequenceNumber;
		public UInt16 SequenceNumber;
		public UInt16 HardLinkCount;
		public UInt16 AttributeOffset;
		public RecordFlags Flags;
		public UInt32 BytesInUse;
		public UInt32 BytesAllocated;
		public UInt64 BaseMFTRecord;
		public UInt16 NextAttrInstance;
		public UInt32 MFTRecordNumber;

		#endregion

		#region Attribute Fields

		public UInt64 ParentDirectory = 0;
		public DateTime CreationTime;
		public DateTime LastDataChangeTime;
		public DateTime LastMFTChangeTime;
		public DateTime LastAccessTime;
		public UInt64 AllocatedSize;
		public UInt64 ActualSize;
		public FilePermissions FilePermissions;
		public Byte FileNameLength;
		public FilenameType FileNameType;
		public String FileName;
		public string VolumeLabel;

		#endregion

		public long BytesPerSector;
		public long SectorsPerCluster;
		public ulong RecordNum, StartOffset;
		public FileSystemNTFS FileSystem;
		public IDataStream PartitionStream;

		// Attributes stored on this MFT Record
		private List<MFTAttribute> _attributes;
		public IList<MFTAttribute> Attributes {
			get { return new ReadOnlyCollection<MFTAttribute>(_attributes); }
		}
		private MFTAttribute _dataAttribute;
		public MFTAttribute DataAttribute {
			get {
				LoadData(MFTLoadDepth.Full);
				return _dataAttribute;
			}
		}
		public IList<IRun> Runs {
			get {
				var dataAttribute = DataAttribute;
				if (dataAttribute != null && dataAttribute.Runs != null) {
					return dataAttribute.Runs;
				}
				return new List<IRun>();
			}
		}
		private List<MFTAttribute> _namedDataAttributes = new List<MFTAttribute>();
		public IList<MFTAttribute> NamedDataAttributes {
			get {
				LoadData(MFTLoadDepth.Full);
				return new ReadOnlyCollection<MFTAttribute>(_namedDataAttributes);
			}
		}

		public bool Valid { get; private set; }
		private FileSystemNode _node = null;
		private byte[] _data;
		private MFTLoadDepth _dataLoaded = MFTLoadDepth.None;
		private string _path = "";
		private FileRecoveryStatus _chanceOfRecovery = FileRecoveryStatus.Unknown;

		public static MFTRecord Load(ulong recordNum, FileSystemNTFS fileSystem, MFTLoadDepth loadDepth = MFTLoadDepth.Full, string path = "") {
			ulong startOffset = recordNum * (ulong)fileSystem.SectorsPerMFTRecord * (ulong)fileSystem.BytesPerSector;

			IDataStream stream;

			//Special case for MFT - can't read itself
			if (recordNum == 0) {
				stream = new SubStream(fileSystem.Store, fileSystem.MFTSector * (ulong)fileSystem.BytesPerSector, (ulong)(fileSystem.SectorsPerMFTRecord * fileSystem.BytesPerSector));
			} else {
				stream = new SubStream(fileSystem.MFT, startOffset, (ulong)(fileSystem.SectorsPerMFTRecord * fileSystem.BytesPerSector));
			}

			// Read the whole record into memory
			byte[] data = stream.GetBytes(0, stream.StreamLength);

			return new MFTRecord(recordNum, fileSystem, data, loadDepth, path);
		}

		private MFTRecord(ulong recordNum, FileSystemNTFS fileSystem, byte[] data,
				MFTLoadDepth loadDepth, string path) {
			this.RecordNum = recordNum;
			this.FileSystem = fileSystem;
			this.BytesPerSector = fileSystem.BytesPerSector;
			this.SectorsPerCluster = fileSystem.SectorsPerCluster;
			this.PartitionStream = fileSystem.Store;

			Valid = true;

			_data = data;
			_path = path;

			Flags = (RecordFlags)BitConverter.ToUInt16(_data, 22);

			if (loadDepth != MFTLoadDepth.None) {
				LoadData(loadDepth);
			}
		}

		private void LoadData(MFTLoadDepth loadDepth = MFTLoadDepth.Full) {
			if (loadDepth.CompareTo(_dataLoaded) <= 0) {
				// If we're not loading more than we already have, just stop here.
				return;
			}

			string Magic = Encoding.ASCII.GetString(_data, 0, 4);
			if (!Magic.Equals("FILE")) {
				Console.Error.WriteLine("Warning: MFT record number {0} was missing the 'FILE' header. Skipping.", RecordNum);
				// This record is invalid, so don't read any more.
				Valid = false;
				return;
			}

			ushort updateSequenceOffset = BitConverter.ToUInt16(_data, 4);
			ushort updateSequenceLength = BitConverter.ToUInt16(_data, 6);

			ushort updateSequenceNumber = BitConverter.ToUInt16(_data, updateSequenceOffset);
			ushort[] updateSequenceArray = new ushort[updateSequenceLength - 1];
			ushort read = 1;
			while (read < updateSequenceLength) {
				updateSequenceArray[read - 1] = BitConverter.ToUInt16(_data, (ushort)(updateSequenceOffset + read * 2));
				read++;
			}

			// Apply fixups to the in-memory array
			try {
				FixupStream.FixArray(_data, updateSequenceNumber, updateSequenceArray, (int)BytesPerSector);
			} catch (NTFSFixupException e) {
				Console.Error.WriteLine(e);
				// This record is invalid, so don't read any more.
				Valid = false;
				return;
			}

			LoadHeader();
			LoadAttributes(AttributeOffset, loadDepth);

			if (_attributes.Count == 0) {
				Console.Error.WriteLine("Warning: MFT record number {0} had no attributes.", RecordNum);
			}

			_dataLoaded = loadDepth;

			// If we've loaded everything, dispose of the underlying byte array.
			if (_dataLoaded == MFTLoadDepth.Full) {
				_data = null;
			}
		}

		public static DateTime fromNTFS(ulong time) {
			try {
				return (new DateTime(1601, 1, 1)).AddMilliseconds((double)time / 10000.0);
			} catch (Exception) {
				return new DateTime(1601, 1, 1);
			}
		}

		public bool Deleted {
			get {
				return (Flags & RecordFlags.InUse) == 0;
			}
		}

		public bool IsDirectory {
			get {
				return (Flags & RecordFlags.Directory) > 0;
			}
		}

		public DateTime LastModified {
			get {
				return LastDataChangeTime;
			}
		}

		public IFileSystemNode GetFileSystemNode() {
			if (_node == null) {
				return GetFileSystemNode(_path);
			}
			return _node;
		}

		public FileSystemNode GetFileSystemNode(String path) {
			LoadData();
			if (_node == null) {
				if (IsDirectory) {
					_node = new FolderNTFS(this, path);
				} else if (_namedDataAttributes.Count > 0) {
					_node = new HiddenDataStreamFileNTFS(this, path);
				} else {
					_node = new FileNTFS(this, path);
				}
			}
			return _node;
		}

		public MFTAttribute GetAttribute(AttributeType type) {
			LoadData();
			try {
				foreach (MFTAttribute attr in _attributes) {
					if (attr.Type == type) {
						return attr;
					}
				}
			} catch (Exception e) {
				Console.Error.WriteLine(e);
			}
			return null;
		}

		private void LoadHeader() {
			LogSequenceNumber = BitConverter.ToUInt64(_data, 8);
			SequenceNumber = BitConverter.ToUInt16(_data, 16);
			HardLinkCount = BitConverter.ToUInt16(_data, 18);
			AttributeOffset = BitConverter.ToUInt16(_data, 20);
			BytesInUse = BitConverter.ToUInt32(_data, 24);
			BytesAllocated = BitConverter.ToUInt32(_data, 28);
			BaseMFTRecord = BitConverter.ToUInt64(_data, 32);
			NextAttrInstance = BitConverter.ToUInt16(_data, 40);
			MFTRecordNumber = BitConverter.ToUInt32(_data, 44);
		}

		private void LoadAttributes(int startOffset, MFTLoadDepth loadDepth) {
			_attributes = new List<MFTAttribute>();
			while (true) {
				//Align to 8 byte boundary
				if (startOffset % 8 != 0) {
					startOffset = (startOffset / 8 + 1) * 8;
				}

				// Read the attribute type and length and determine whether we care about this attribute.
				AttributeType type = (AttributeType)BitConverter.ToUInt32(_data, startOffset);
				if (type == AttributeType.End) {
					break;
				}
				int length = BitConverter.ToUInt16(_data, startOffset + 4);
				if (loadDepth == MFTLoadDepth.NameAttributeOnly && type != AttributeType.FileName) {
					// Skip this attribute if we're only loading the filename
					startOffset += length;
					continue;
				}

				MFTAttribute attribute = MFTAttribute.Load(_data, startOffset, this);
				if (!attribute.NonResident) {
					switch (attribute.Type) {
						case AttributeType.StandardInformation:
							LoadStandardAttribute(startOffset + attribute.ValueOffset);
							break;
						case AttributeType.FileName:
							LoadNameAttribute(startOffset + attribute.ValueOffset);
							break;
						case AttributeType.AttributeList:
							LoadExternalAttributeList(startOffset + attribute.ValueOffset, attribute);
							break;
						case AttributeType.VolumeLabel:
							LoadVolumeLabelAttribute(startOffset + attribute.ValueOffset, (int)attribute.ValueLength);
							break;
					}
				}
				if (attribute.Valid) {
					if (attribute.Type == AttributeType.Data) {
						if (attribute.Name == null) {
							if (_dataAttribute != null) {
								Console.Error.WriteLine("Warning: multiple unnamed data streams found on MFT record {0}.", RecordNum);
							}
							_dataAttribute = attribute;
						} else {
							_namedDataAttributes.Add(attribute);
						}
					}
					_attributes.Add(attribute);
				}

				startOffset += (int)attribute.Length;
			}
		}

		private void LoadStandardAttribute(int startOffset) {
			CreationTime = fromNTFS(BitConverter.ToUInt64(_data, startOffset));
			LastDataChangeTime = fromNTFS(BitConverter.ToUInt64(_data, startOffset + 8));
			LastMFTChangeTime = fromNTFS(BitConverter.ToUInt64(_data, startOffset + 16));
			LastAccessTime = fromNTFS(BitConverter.ToUInt64(_data, startOffset + 24));
			FilePermissions = (FilePermissions)BitConverter.ToInt32(_data, startOffset + 32);
		}

		private void LoadNameAttribute(int startOffset) {
			// Read in the bytes, then parse them.
			ParentDirectory = BitConverter.ToUInt64(_data, startOffset) & 0xFFFFFF;
			AllocatedSize = BitConverter.ToUInt64(_data, startOffset + 40);
			ActualSize = BitConverter.ToUInt64(_data, startOffset + 48);
			FileNameLength = _data[startOffset + 64];
			FileNameType = (FilenameType)_data[startOffset + 65];
			if (FileName == null && FileNameType != FilenameType.Dos) { // Don't bother reading DOS (8.3) filenames
				FileName = Encoding.Unicode.GetString(_data, startOffset + 66, FileNameLength * 2);
			}
		}

		private void LoadVolumeLabelAttribute(int startOffset, int length) {
			VolumeLabel = Encoding.Unicode.GetString(_data, startOffset, length);
		}

		private void LoadExternalAttributeList(int startOffset, MFTAttribute attrList) {
			int offset = 0;
			while (true) {
				//Align to 8 byte boundary
				if (offset % 8 != 0) {
					offset = (offset / 8 + 1) * 8;
				}

				// Load the header for this external attribute reference.
				AttributeType type = (AttributeType)BitConverter.ToUInt32(_data, offset + startOffset + 0x0);
				// 0xFFFFFFFF marks end of attributes.
				if (offset == attrList.ValueLength || type == AttributeType.End) {
					break;
				}
				ushort length = BitConverter.ToUInt16(_data, offset + startOffset + 0x4);
				byte nameLength = _data[offset + startOffset + 0x6];
				ushort id = BitConverter.ToUInt16(_data, offset + startOffset + 0x18);
				ulong vcn = BitConverter.ToUInt64(_data, offset + startOffset + 0x8);
				ulong extensionRecordNumber = (BitConverter.ToUInt64(_data, offset + startOffset + 0x10) & 0x00000000FFFFFFFF);

				if (extensionRecordNumber != RecordNum && extensionRecordNumber != MFTRecordNumber) { // TODO: Are these ever different?
					// Load the MFT extension record, locate the attribute we want, and copy it over.
					MFTRecord extensionRecord = MFTRecord.Load(extensionRecordNumber, this.FileSystem);
					if (extensionRecord != null && extensionRecord.Valid) {
						foreach (MFTAttribute externalAttribute in extensionRecord._attributes) {
							if (id == externalAttribute.Id) {
								if (externalAttribute.NonResident && externalAttribute.Type == AttributeType.Data) {
									// Find the corresponding data attribute on this record and merge the runlists
									bool merged = false;
									foreach (MFTAttribute attribute in _attributes) {
										if (attribute.Type == AttributeType.Data && externalAttribute.Name == attribute.Name) {
											if (attribute.Runs == null)
												attribute.Runs = new List<IRun>();
											MergeRunLists(ref attribute.Runs, externalAttribute.Runs);
											merged = true;
											break;
										}
									}
									if (!merged) {
										this._attributes.Add(externalAttribute);
									}
								} else {
									this._attributes.Add(externalAttribute);
								}
							}
						}
					}
				}

				offset += 0x1A + (nameLength * 2);
			}
		}

		private void MergeRunLists(ref List<IRun> list1, List<IRun> list2) {
			if (list2 != null)
			{
				list1.AddRange(list2);
			}
			// TODO: Verify that the runlists don't overlap
		}

		public string Name {
			get {
				if (string.IsNullOrEmpty(FileName)) {
					LoadData(MFTLoadDepth.NameAttributeOnly);
				}
				return FileName;
			}
		}

		public ulong Size {
			get {
				LoadData();
				return ActualSize;
			}
		}

		public FileRecoveryStatus ChanceOfRecovery {
			get {
				if (_chanceOfRecovery == FileRecoveryStatus.Unknown) {
					_chanceOfRecovery = GetFileSystemNode().ChanceOfRecovery;
				}
				return _chanceOfRecovery;
			}
			set {
				_chanceOfRecovery = value;
			}
		}
	}
}

