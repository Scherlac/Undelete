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

namespace KFS.FileSystems.FAT {
	/// <summary>
	/// A file node in the FAT filesystem.
	/// </summary>
	public class FileFAT : File, IFATNode, IDescribable {
		private long _length;

		public FileAttributesFAT Attributes { get; private set; }
		public override DateTime LastModified {
			get { return Attributes.LastModified; }
		}
		public long FirstCluster { get; private set; }
		public long Length {
			get { return _length; }
		}
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
		public FileFAT(FileSystemFAT fileSystem, FolderFAT.DirectoryEntry entry, string path) {
			FileSystem = fileSystem;
			Name = PathUtils.MakeFileNameValid(entry.FileName);
			Path = PathUtils.Combine(path, Name);
			_length = entry.Length;
			Attributes = new FileAttributesFAT(entry);
			FirstCluster = entry.ClusterNum;
			Deleted = Attributes.Deleted;
		}
		public FileFAT(FileSystemFAT fileSystem, long firstCluster) {
			FileSystem = fileSystem;
			FirstCluster = firstCluster;
			Name = Util.GetRandomString(8);
			Path = PathUtils.Combine(FileSystem.Store.DeviceID, "?", Name);
			long currentCluster = FirstCluster;
			_length = 0;
			while (currentCluster >= 0) {
				// Note: This won't correctly calculate the length of a deleted file.
				currentCluster = FileSystem.GetNextCluster(currentCluster);
				_length += FileSystem.BytesPerCluster;
			}
			Attributes = new FileAttributesFAT();
			Deleted = true;
		}

		private Dictionary<long, byte[]> _clusterCache = new Dictionary<long, byte[]>();

		public override byte[] GetBytes(ulong _offset, ulong _length) {
			long offset = (long)_offset;
			long length = (long)_length;
			long currentCluster = FirstCluster;

			lock (_clusterCache) {
				byte[] res = new byte[length];
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
				return res;
			}
		}

		private long GetNextCluster(long currentCluster) {
			if (Deleted) {
				// Just try reading contiguous blocks until we run out of space.
				// When a file is deleted in FAT, its entries are removed from
				// the allocation table. This code allows us to recover contiguous
				// deleted files.
				return currentCluster + 1;
			} else {
				return FileSystem.GetNextCluster(currentCluster);
			}
		}

		public override ulong DeviceOffset {
			get { return (ulong)FileSystem.GetDiskOffsetOfFATCluster(FirstCluster); }
		}

		public override ulong StreamLength {
			get { return (ulong)_length; }
		}

		public override String StreamName {
			get { return "FAT file " + Name; }
		}

		public override IDataStream ParentStream {
			get { return this.FileSystem.Store; }
		}

		public override void Open() {
			FileSystem.Store.Open();
		}

		public override void Close() {
			FileSystem.Store.Close();
		}

		public string TextDescription {
			get { return Attributes.TextDescription; }
		}
	}
}
