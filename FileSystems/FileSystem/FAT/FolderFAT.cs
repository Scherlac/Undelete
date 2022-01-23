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
using KFS.Disks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace KFS.FileSystems.FAT {
	/// <summary>
	/// Attribute flags for a FAT directory.
	/// </summary>
	[Flags]
	public enum FATDirectoryAttributes : byte {
		ATTR_READ_ONLY = 0x01,
		ATTR_HIDDEN = 0x02,
		ATTR_SYSTEM = 0x04,
		ATTR_VOLUME_ID = 0x08,
		ATTR_DIRECTORY = 0x10,
		ATTR_ARCHIVE = 0x20,
		ATTR_LONG_NAME = 0x0F
	}
	/// <summary>
	/// A folder (directory) node in the FAT filesystem.
	/// </summary>
	public class FolderFAT : Folder, IFATNode, IDescribable {
		public long Offset { get; protected set; }
		private const int DIR_ENTRY_SIZE = 32;
		public class DirectoryEntry {
			string DIR_Name;
			FATDirectoryAttributes DIR_Attr;
			byte DIR_NTRes;
			byte DIR_CrtTimeTenth;
			ushort DIR_CrtTime;
			ushort DIR_CrtDate;
			ushort DIR_LstAccDate;
			ushort DIR_FstClusHI;
			ushort DIR_WrtTime;
			ushort DIR_WrtDate;
			ushort DIR_FstClusLO;
			uint DIR_FileSize;

			public bool Free { get; private set; }
			public bool Last { get; private set; }
			public string FileName { get; set; }
			public long Offset { get; private set; }
			public long Length { get { return DIR_FileSize; } }
			public long ClusterNum { get { return (DIR_FstClusHI << 16) + DIR_FstClusLO; } }
			public FATDirectoryAttributes Attributes {
				get { return DIR_Attr; }
			}
			public bool LongNameEntry {
				get { return DIR_Attr == FATDirectoryAttributes.ATTR_LONG_NAME; }
			}
			public string LongName { get; private set; }
			public DateTime LastWrite {
				get {
					return GetDate(DIR_WrtDate) + GetTime(DIR_WrtTime);
				}
			}
			public DateTime CreationTime {
				get {
					return GetDate(DIR_CrtDate) + GetTime(DIR_CrtTime) + new TimeSpan(0, 0, 0, 0, DIR_CrtTimeTenth * 100);
				}
			}
			public DateTime LastAccess {
				get {
					return GetDate(DIR_LstAccDate);
				}
			}
			private bool ValidAttribute(FATDirectoryAttributes attr) {
				return attr == FATDirectoryAttributes.ATTR_LONG_NAME
						|| attr == FATDirectoryAttributes.ATTR_VOLUME_ID
						|| (attr & (FATDirectoryAttributes.ATTR_ARCHIVE |
												FATDirectoryAttributes.ATTR_DIRECTORY |
												FATDirectoryAttributes.ATTR_HIDDEN |
												FATDirectoryAttributes.ATTR_READ_ONLY |
												FATDirectoryAttributes.ATTR_SYSTEM)) == attr;
			}
			public bool Invalid {
				get {
					return !Free
							&& (Attributes != FATDirectoryAttributes.ATTR_VOLUME_ID)
							&& (Attributes != (FATDirectoryAttributes.ATTR_VOLUME_ID | FATDirectoryAttributes.ATTR_ARCHIVE))
							&& !LongNameEntry
							&& (DIR_CrtTimeTenth >= 200
									|| GetDate(DIR_LstAccDate) == DateTime.MinValue
									|| GetDate(DIR_WrtDate) == DateTime.MinValue
									|| GetDate(DIR_CrtDate) == DateTime.MinValue
									|| GetTime(DIR_WrtTime).Hours >= 24
									|| GetTime(DIR_CrtTime).Hours >= 24
									|| !ValidAttribute(Attributes));
				}
			}

			private DateTime GetDate(ushort val) {
				byte[] bytes = BitConverter.GetBytes(val);
				int dayOfMonth = (bytes[0] & 31); // the low 5 bits
				int month = ((bytes[0] & 224) >> 5) + ((bytes[1] & 0x1) << 3);
				int year = 1980 + ((bytes[1] & 254) >> 1);
				if (1 <= year && year <= 9999 && 1 <= month && month <= 12 && 1 <= dayOfMonth
								&& dayOfMonth <= DateTime.DaysInMonth(year, month)) {
					return new DateTime(year, month, dayOfMonth);
				} else {
					return DateTime.MinValue;
				}
			}

			private TimeSpan GetTime(ushort val) {
				byte[] bytes = BitConverter.GetBytes(val);
				int seconds = 2 * (bytes[0] & 31);
				int minutes = ((bytes[0] & 224) >> 5) + ((bytes[1] & 0x7) << 3);
				int hours = ((bytes[1] & 248) >> 3);
				return new TimeSpan(hours, minutes, seconds);
			}

			public DirectoryEntry(FileSystemFAT fileSystem, long offset) {
				byte[] data = fileSystem.Store.GetBytes((ulong)offset, DIR_ENTRY_SIZE);
				DIR_Name = ASCIIEncoding.ASCII.GetString(data, 0, 11);
				DIR_Attr = (FATDirectoryAttributes)data[11];
				DIR_NTRes = data[12];
				DIR_CrtTimeTenth = data[13];
				DIR_CrtTime = BitConverter.ToUInt16(data, 14);
				DIR_CrtDate = BitConverter.ToUInt16(data, 16);
				DIR_LstAccDate = BitConverter.ToUInt16(data, 18);
				DIR_FstClusHI = BitConverter.ToUInt16(data, 20);
				DIR_WrtTime = BitConverter.ToUInt16(data, 22);
				DIR_WrtDate = BitConverter.ToUInt16(data, 24);
				DIR_FstClusLO = BitConverter.ToUInt16(data, 26);
				DIR_FileSize = BitConverter.ToUInt32(data, 28);

				Free = data[0] == 0x0 || data[0] == 0x05 || data[0] == 0xE5;
				Last = data[0] == 0x0;
				string filename = DIR_Name.Substring(0, 8).Trim().ToLower();
				string ext = DIR_Name.Substring(8).Trim().ToLower();
				FileName = filename;
				if (ext != "") {
					if ((Attributes & FATDirectoryAttributes.ATTR_VOLUME_ID) == 0) {
						FileName += ".";
					}
					FileName += ext;
				}
				Offset = fileSystem.GetDiskOffsetOfFATCluster(ClusterNum);

				if (LongNameEntry) {
					LongName = string.Concat(Encoding.Unicode.GetString(data, 1, 10),
																	 Encoding.Unicode.GetString(data, 14, 12),
																	 Encoding.Unicode.GetString(data, 28, 4));
				}
			}
		}

		public FileAttributesFAT Attributes { get; private set; }

		public override DateTime LastModified {
			get { return Attributes.LastModified; }
		}
		public long FirstCluster { get; private set; }
		public override long Identifier {
			get { return FirstCluster; }
		}
		public new FileSystemFAT FileSystem {
			get {
				return (FileSystemFAT)base.FileSystem;
			}
			private set {
				base.FileSystem = value;
			}
		}

		private bool _root;

		public FolderFAT(FileSystemFAT fileSystem, long offset, long cluster) {
			_root = true;
			Offset = offset;
			FileSystem = fileSystem;
			FirstCluster = cluster;
			Name = GetVolumeName();
			Path = FileSystem.Store.DeviceID;
			Loaded = false;
			Attributes = new FileAttributesFAT();
			Deleted = Attributes.Deleted;
		}
		public FolderFAT(FileSystemFAT fileSystem, DirectoryEntry entry, string path) {
			_root = false;
			FileSystem = fileSystem;
			Offset = entry.Offset;
			FirstCluster = entry.ClusterNum;
			Name = PathUtils.MakeFileNameValid(entry.FileName);
			Path = PathUtils.Combine(path, Name);
			Loaded = false;
			Attributes = new FileAttributesFAT(entry);
			Deleted = Attributes.Deleted;
		}
		public override IEnumerable<IFileSystemNode> GetChildren() {
			List<FileSystemNode> res = new List<FileSystemNode>();
			foreach (DirectoryEntry entry in GetDirectoryEntries()) {
				if (entry.FileName.Replace(".", "") != ""
						&& (entry.Attributes & FATDirectoryAttributes.ATTR_VOLUME_ID) == 0
						&& entry.ClusterNum != FirstCluster) {
					if ((entry.Attributes & FATDirectoryAttributes.ATTR_DIRECTORY) != 0) {
						res.Add(new FolderFAT(FileSystem, entry, Path));
					} else {
						res.Add(new FileFAT(FileSystem, entry, Path));
					}
				}
			}
			return res;
		}
		private string GetVolumeName() {
			foreach (DirectoryEntry entry in GetDirectoryEntries()) {
				if ((entry.Attributes & FATDirectoryAttributes.ATTR_VOLUME_ID) != 0) {
					return entry.FileName.ToUpper();
				}
			}
			return FileSystem.Store.DeviceID;
		}
		private IEnumerable<DirectoryEntry> GetDirectoryEntries() {
			// read all directory entries
			long pos = Offset;
			long clusterStart = Offset;
			long currentCluster = FirstCluster;
			string longName = "";
			List<DirectoryEntry> res = new List<DirectoryEntry>();

			// check if this is pointing at the root dir and is obviously not supposed to be
			if (_root || FirstCluster != FileSystem.RootCluster) {

				DirectoryEntry current = new DirectoryEntry(FileSystem, pos);
				while (!current.Last) {
					if (current.LongNameEntry) {
						longName = current.LongName + longName;
					} else {
						if (!(_root && FileSystem.Type == PartitionType.FAT16) && current.Invalid) {
							// this is an invalid entry, so the whole directory is probably invalid
							break;
						}
						if (longName.Contains("\0")) {
							longName = longName.Remove(longName.IndexOf("\0"));
						}
						if (longName != "") {
							current.FileName = longName;
						}
						//yield return current;
						res.Add(current);
						longName = "";
					}

					if (_root && FileSystem.Type == PartitionType.FAT16 &&
							pos - clusterStart >= FileSystem.RootEntryCount * DIR_ENTRY_SIZE) {
						break;
					} else if ((_root && FileSystem.Type == PartitionType.FAT16)
							|| pos - clusterStart < FileSystem.BytesPerCluster) {
						pos += DIR_ENTRY_SIZE;
					} else {
						// check the FAT
						currentCluster = GetNextCluster(currentCluster);
						if (currentCluster < 0) break;
						pos = clusterStart = FileSystem.GetDiskOffsetOfFATCluster(currentCluster);
					}
					current = new DirectoryEntry(FileSystem, pos);
				}
			}
			return res;
		}

		#region IDescribable Members

		public string TextDescription {
			get { return Attributes.TextDescription; }
		}

		#endregion


		private Dictionary<long, byte[]> _clusterCache = new Dictionary<long, byte[]>();

		public override byte[] GetBytes(ulong _offset, ulong _length) {
			long offset = (long)_offset;
			long length = (long)_length;
			long currentCluster = FirstCluster;

			lock (_clusterCache) {
				byte[] res = new byte[length];

				// First, handle the case in FAT16 where the root directory is fixed.
				if (_root && FileSystem.Type == PartitionType.FAT16) {
					if (!_clusterCache.ContainsKey(currentCluster)) {
						_clusterCache[currentCluster] = FileSystem.Store.GetBytes(
									(ulong)FileSystem.GetDiskOffsetOfFATCluster(currentCluster),
									(ulong)FileSystem.RootEntryCount * DIR_ENTRY_SIZE);
					}
					// Read the cached data.
					Array.Copy(_clusterCache[currentCluster], offset, res, 0, length);
				} else { // Now handle the general case.
					long resindex = 0;
					// Find the first cluster we want to read.
					while (offset >= FileSystem.BytesPerCluster && currentCluster >= 0) {
						currentCluster = GetNextCluster(currentCluster);
						offset -= FileSystem.BytesPerCluster;
					}
					// Cache and retrieve the data for each cluster until we get all we need.
					while (length > 0 && currentCluster >= 0) {
						// Cache the current cluster.
						if (!_clusterCache.ContainsKey(currentCluster)) {
							_clusterCache[currentCluster] = FileSystem.Store.GetBytes(
									(ulong)FileSystem.GetDiskOffsetOfFATCluster(currentCluster),
									(ulong)FileSystem.BytesPerCluster);
						}

						// Read the cached data.
						long read = Math.Min(length, FileSystem.BytesPerCluster - offset);
						Array.Copy(_clusterCache[currentCluster], offset, res, resindex, read);
						offset = 0;
						length -= read;
						resindex += read;
						currentCluster = GetNextCluster(currentCluster);
					}
				}
				return res;
			}
		}

		private long GetNextCluster(long currentCluster) {
			var nextCluster = FileSystem.GetNextCluster(currentCluster);
			if (nextCluster < 0) {
				// Just try reading contiguous blocks until we run out of space.
				// When a file is deleted in FAT, its entries are removed from
				// the allocation table. This code allows us to recover contiguous
				// deleted files.
				return currentCluster + 1;
			} else {
				return nextCluster;
			}
		}

		ulong _streamLength = ulong.MaxValue;

		public override ulong StreamLength {
			get {
				if (_streamLength == ulong.MaxValue) {
					// read all directory entries
					long pos = Offset;
					long clusterStart = Offset;
					long currentCluster = FirstCluster;
					ulong count = DIR_ENTRY_SIZE;

					// check if this is pointing at the root dir and is obviously not supposed to be
					if (_root || FirstCluster != FileSystem.RootCluster) {

						DirectoryEntry current = new DirectoryEntry(FileSystem, pos);
						while (!current.Last) {
							if (!(_root && FileSystem.Type == PartitionType.FAT16) && current.Invalid) {
								// this is an invalid entry, so the whole directory is probably invalid
								break;
							}
							count += DIR_ENTRY_SIZE;

							if (_root && FileSystem.Type == PartitionType.FAT16 &&
									pos - clusterStart >= FileSystem.RootEntryCount * DIR_ENTRY_SIZE) {
								break;
							} else if ((_root && FileSystem.Type == PartitionType.FAT16)
									|| pos - clusterStart < FileSystem.BytesPerCluster) {
								pos += DIR_ENTRY_SIZE;
							} else {
								// check the FAT
								currentCluster = GetNextCluster(currentCluster);
								if (currentCluster < 0) break;
								pos = clusterStart = FileSystem.GetDiskOffsetOfFATCluster(currentCluster);
							}
							current = new DirectoryEntry(FileSystem, pos);
						}
					}
					_streamLength = count;
				}
				return _streamLength;
			}
		}

		public override String StreamName {
			get { return "FAT Folder"; }
		}

		public override IDataStream ParentStream {
			get { return this.FileSystem.Store; }
		}

		public override ulong DeviceOffset {
			get { return (ulong)FileSystem.GetDiskOffsetOfFATCluster(FirstCluster); }
		}

		public override void Open() {
			FileSystem.Store.Open();
		}

		public override void Close() {
			FileSystem.Store.Close();
		}
	}
}
