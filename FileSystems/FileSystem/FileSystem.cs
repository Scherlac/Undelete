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
using KFS.FileSystems.FAT;
using KFS.FileSystems.NTFS;
using System.Collections.Generic;
using System.Linq;

namespace KFS.FileSystems {
	/// <summary>
	/// The abstract FileSystem class. Call FileSystem.TryLoad() to load a
	/// filesystem from an IFileSystemStore.
	/// </summary>
	public abstract class FileSystem : IFileSystem {
		public delegate bool NodeVisitCallback(INodeMetadata node, ulong current, ulong total);

		public static IFileSystem TryLoad(IFileSystemStore store, string FSType = null) {
			if (store == null || store.StreamLength == 0) return null;
			if (FSType == "NTFS") {
				return new FileSystemNTFS(store);
			} else if (FSType == "FAT16") {
				return new FileSystemFAT(store, PartitionType.FAT16);
			} else if (FSType == "FAT32") {
				return new FileSystemFAT(store, PartitionType.FAT32);
			}

			// TODO: Autodetect based on FS signatures if possible?
			switch (store.StorageType) {
				case StorageType.PhysicalDiskPartition: {
						PhysicalDiskPartitionAttributes attributes = (PhysicalDiskPartitionAttributes)store.Attributes;
						if (attributes.PartitionType == PartitionType.NTFS) {
							return new FileSystemNTFS(store);
						} else if (attributes.PartitionType == PartitionType.FAT16
								|| attributes.PartitionType == PartitionType.FAT32) {
							return new FileSystemFAT(store, attributes.PartitionType);
						} else if (attributes.PartitionType == PartitionType.FAT32WithInt13Support) {
							return new FileSystemFAT(store, PartitionType.FAT32);
						} else {
							return null;
						}
					}
				case StorageType.LogicalVolume: {
						LogicalDiskAttributes attributes = (LogicalDiskAttributes)store.Attributes;
						if (attributes.FileSystem == "NTFS") {
							return new FileSystemNTFS(store);
						} else if (attributes.FileSystem == "FAT16") {
							return new FileSystemFAT(store, PartitionType.FAT16);
						} else if (attributes.FileSystem == "FAT32") {
							return new FileSystemFAT(store, PartitionType.FAT32);
						} else {
							return null;
						}
					}
				default:
					return null;
			}
		}

		public static bool HasFileSystem(IFileSystemStore store) {
			if (store == null || store.StreamLength == 0) return false;
			switch (store.StorageType) {
				case StorageType.PhysicalDiskPartition: {
						PhysicalDiskPartitionAttributes attributes = (PhysicalDiskPartitionAttributes)store.Attributes;
						if (attributes.PartitionType == PartitionType.NTFS) {
							return true;
						} else if (attributes.PartitionType == PartitionType.FAT16
								|| attributes.PartitionType == PartitionType.FAT32) {
							return true;
						} else if (attributes.PartitionType == PartitionType.FAT32WithInt13Support) {
							return true;
						} else {
							return false;
						}
					}
				case StorageType.LogicalVolume: {
						LogicalDiskAttributes attributes = (LogicalDiskAttributes)store.Attributes;
						if (attributes.FileSystem == "NTFS") {
							return true;
						} else if (attributes.FileSystem == "FAT16") {
							return true;
						} else if (attributes.FileSystem == "FAT32") {
							return true;
						} else {
							return false;
						}
					}
				default:
					return false;
			}
		}

		public abstract FileSystemNode GetRoot();

		public IFileSystemStore Store { get; protected set; }

		public IEnumerable<FileSystemNode> GetFile(string path) {
			if (GetRoot() != null) {
				// GetChildrenAtPath requires forward slashes.
				path = path.Replace('\\', '/');
				foreach (FileSystemNode node in GetRoot().GetChildrenAtPath(path)) {
					yield return node;
				}
			}
		}

		public abstract string FileSystemType { get; }

		public FileSystemNode GetFirstFile(string path) {
			IEnumerator<FileSystemNode> en = this.GetFile(path).GetEnumerator();
			if (en.MoveNext()) {
				return en.Current;
			}
			return null;
		}

		public virtual SectorStatus GetSectorStatus(ulong sectorNum) {
			return SectorStatus.Unknown;
		}

		internal virtual FileRecoveryStatus GetChanceOfRecovery(FileSystemNode node) {
			return FileRecoveryStatus.Unknown;
		}

		public abstract List<ISearchStrategy> GetSearchStrategies();

		public virtual ISearchStrategy GetDefaultSearchStrategy() {
			return GetSearchStrategies().First();
		}
	}
}
