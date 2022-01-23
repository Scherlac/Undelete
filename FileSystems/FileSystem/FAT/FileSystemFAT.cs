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
using System.Text;

namespace KFS.FileSystems.FAT {
	/// <summary>
	/// A FAT16 or FAT32 filesystem.
	/// </summary>
	public class FileSystemFAT : FileSystem {
		private const int BPB_SIZE = 128;
		#region BPB fields
		string BS_OEMName;
		ushort BPB_BytsPerSec;
		byte BPB_SecPerClus;
		ushort BPB_RsvdSecCnt;
		byte BPB_NumFATs;
		ushort BPB_RootEntCnt;
		ushort BPB_TotSec16;
		byte BPB_Media;
		ushort BPB_FATSz16;
		ushort BPB_SecPerTrk;
		ushort BPB_NumHeads;
		uint BPB_HiddSec;
		uint BPB_TotSec32;

		uint BPB_FATSz32;
		ushort BPB_ExtFlags;
		ushort BPB_FSVer;
		uint BPB_RootClus;
		ushort BPB_FSInfo;
		ushort BPB_BkBootSec;

		private void LoadBPB() {
			byte[] bpb = Store.GetBytes(0, BPB_SIZE);
			BS_OEMName = ASCIIEncoding.ASCII.GetString(bpb, 3, 8);
			BPB_BytsPerSec = BitConverter.ToUInt16(bpb, 11);
			BPB_SecPerClus = bpb[13];
			BPB_RsvdSecCnt = BitConverter.ToUInt16(bpb, 14);
			BPB_NumFATs = bpb[16];
			BPB_RootEntCnt = BitConverter.ToUInt16(bpb, 17);
			BPB_TotSec16 = BitConverter.ToUInt16(bpb, 19);
			BPB_Media = bpb[21];
			BPB_FATSz16 = BitConverter.ToUInt16(bpb, 22);
			BPB_SecPerTrk = BitConverter.ToUInt16(bpb, 24);
			BPB_NumHeads = BitConverter.ToUInt16(bpb, 26);
			BPB_HiddSec = BitConverter.ToUInt32(bpb, 28);
			BPB_TotSec32 = BitConverter.ToUInt32(bpb, 32);
			if (Type == PartitionType.FAT32) {
				BPB_FATSz32 = BitConverter.ToUInt32(bpb, 36);
				BPB_ExtFlags = BitConverter.ToUInt16(bpb, 40);
				BPB_FSVer = BitConverter.ToUInt16(bpb, 42);
				BPB_RootClus = BitConverter.ToUInt32(bpb, 44);
				BPB_FSInfo = BitConverter.ToUInt16(bpb, 48);
				BPB_BkBootSec = BitConverter.ToUInt16(bpb, 50);
			}
		}
		#endregion

		long _FATLocation; // in bytes
		long _rootDirLocation; // in bytes
		long _dataLocation; // in bytes

		private FileAllocationTable _fileAllocationTable;

		FileSystemNode _root = null;
		public FileSystemFAT(IFileSystemStore store, PartitionType type) {
			Store = store;
			Type = type;
			LoadBPB();
			_FATLocation = BPB_RsvdSecCnt * BPB_BytsPerSec;
			// Load the FAT.
			_fileAllocationTable = new FileAllocationTable(store, _FATLocation, FATSize * BytesPerSector, type);
			long rootDirSectors = ((BPB_RootEntCnt * 32) + (BPB_BytsPerSec - 1)) / BPB_BytsPerSec;
			long afterFAT = _FATLocation + BPB_NumFATs * FATSize * BPB_BytsPerSec;
			_dataLocation = afterFAT + rootDirSectors * BPB_BytsPerSec;
			if (Type == PartitionType.FAT32) {
				_rootDirLocation = GetDiskOffsetOfFATCluster(BPB_RootClus);
			} else {
				_rootDirLocation = afterFAT;
			}
			_root = new FolderFAT(this, _rootDirLocation, 2);
		}

		public long GetNextCluster(long N) {
			uint fatContent = _fileAllocationTable.GetEntry(N);

			bool eof = false;
			bool bad = false;
			if (fatContent == 0) {
				eof = true;
			}
			if (Type == PartitionType.FAT12) {
				if (fatContent >= 0x0FF8)
					eof = true;
				if (fatContent == 0x0FF7)
					bad = true;
			} else if (Type == PartitionType.FAT16) {
				if (fatContent >= 0xFFF8)
					eof = true;
				if (fatContent == 0xFFF7)
					bad = true;
			} else if (Type == PartitionType.FAT32) {
				if (fatContent >= 0x0FFFFFF8)
					eof = true;
				if (fatContent == 0x0FFFFFF7)
					bad = true;
			}
			if (eof || bad) {
				return -1;
			} else {
				return fatContent;
			}
		}

		public long GetDiskOffsetOfFATCluster(long N) {
			return DataSectionOffset + (N - 2) * BytesPerCluster;
		}

		public long GetFATClusterFromOffset(long diskOffset) {
			return (diskOffset - DataSectionOffset) / BytesPerCluster + 2;
		}

		public PartitionType Type { get; private set; }
		public override FileSystemNode GetRoot() {
			return _root;
		}
		public override string FileSystemType {
			get {
				return Type == PartitionType.FAT32 ? "FAT32" :
										 Type == PartitionType.FAT16 ? "FAT16" :
										 "Unknown";
			}
		}
		public long TotalSectors {
			get { return BPB_TotSec16 == 0 ? BPB_TotSec32 : BPB_TotSec16; }
		}
		public long BytesPerSector {
			get { return BPB_BytsPerSec; }
		}
		public long SectorsPerCluster {
			get { return BPB_SecPerClus; }
		}
		public long BytesPerCluster {
			get { return BytesPerSector * SectorsPerCluster; }
		}
		public long DataSectionOffset {
			get { return _dataLocation; }
		}
		public long FATOffset {
			get { return _FATLocation; }
		}
		public long FATSize {
			get {
				if (BPB_FATSz16 == 0) {
					return BPB_FATSz32;
				} else {
					return BPB_FATSz16;
				}
			}
		}
		public ushort RootEntryCount {
			get { return BPB_RootEntCnt; }
		}
		public uint RootCluster {
			get { return BPB_RootClus; }
		}

		public void SearchByTree(FileSystem.NodeVisitCallback callback, string searchPath) {
			FileSystemNode searchRoot = this._root;
			if (!string.IsNullOrEmpty(searchPath)) {
				searchRoot = this.GetFirstFile(searchPath) ?? searchRoot;
			}
			Visit(callback, searchRoot, new HashSet<long>(), 0, 1);
		}

		public void SearchByCluster(FileSystem.NodeVisitCallback callback, string searchPath) {
			SectorSearch(callback);
		}

		public override List<ISearchStrategy> GetSearchStrategies() {
			List<ISearchStrategy> res = new List<ISearchStrategy>();

			// Add the tree search strategy (default)
			res.Add(new SearchStrategy("Folder hierarchy scan", SearchByTree));

			// Add the cluster search strategy
			res.Add(new SearchStrategy("Cluster scan", SearchByCluster));

			return res;
		}

		private void SectorSearch(FileSystem.NodeVisitCallback callback) {
			long clusterNum = BPB_RootClus;
			ulong progress = 0;
			ulong total = (ulong)(TotalSectors * SectorsPerCluster - (_rootDirLocation + BPB_RootClus * BytesPerCluster));
			while (_rootDirLocation + clusterNum * BytesPerCluster < TotalSectors * SectorsPerCluster) {
				progress++;
				clusterNum++;
				if (!callback(new FileFAT(this, clusterNum), progress, total)) {
					break;
				}
			}
		}

		private void Visit(NodeVisitCallback callback, IFileSystemNode node, HashSet<long> visitedClusters, double currentProgress, double outerProgress) {
			if (node is Folder) {  //No zip support yet
				List<IFileSystemNode> children = new List<IFileSystemNode>(node.GetChildren());
				for (int i = 0; i < children.Count; i++) {
					double progress = currentProgress + outerProgress * ((double)i / (double)children.Count);
					IFileSystemNode child = children[i];
					if (!callback(child, (ulong)(progress * 1000), 1000)) {
						break;
					}
					if (!visitedClusters.Contains(child.Identifier)) {
						visitedClusters.Add(child.Identifier);
						Visit(callback, child, visitedClusters, progress, outerProgress / (double)children.Count);
					}
				}
			}
		}

		public override SectorStatus GetSectorStatus(ulong sectorNum) {
			if (sectorNum < BPB_RsvdSecCnt) {
				return SectorStatus.Reserved;
			} else if ((long)sectorNum < BPB_RsvdSecCnt + BPB_NumFATs * FATSize) {
				return SectorStatus.FAT;
			} else {
				uint FATEntry = _fileAllocationTable.GetEntry(GetFATClusterFromOffset((long)sectorNum * BytesPerSector));
				if (FATEntry == 0x0) {
					return SectorStatus.Free;
				} else if (Type == PartitionType.FAT12 && FATEntry == 0x0FF7
						|| Type == PartitionType.FAT16 && FATEntry == 0xFFF7
						|| Type == PartitionType.FAT32 && FATEntry == 0x0FFFFFF7) {
					return SectorStatus.Bad;
				} else {
					return SectorStatus.Used;
				}
			}
		}

		internal override FileRecoveryStatus GetChanceOfRecovery(FileSystemNode node) {
			FileFAT file = node as FileFAT;
			if (file == null) {
				return FileRecoveryStatus.Unknown;
			} else {
				int numZeroLinks;
				if (IsFatChainOverwritten(file, out numZeroLinks)) {
					return numZeroLinks == 0 ? FileRecoveryStatus.Overwritten : FileRecoveryStatus.PartiallyOverwritten;
				} else {
					BuildFirstClusterIndex();
					// Check to see if the first cluster is also allocated to something more recent
					if (_firstClusterIndex.ContainsKey(file.FirstCluster)) {
						return _firstClusterIndex[file.FirstCluster].Name == file.Name ? FileRecoveryStatus.Recoverable : FileRecoveryStatus.Overwritten;
					} else {
						Console.Error.WriteLine("ERROR: File {0} at cluster {1} was missing from the first cluster index.", file.Name, file.FirstCluster);
						return FileRecoveryStatus.Unknown;
					}
				}
			}
		}

		private bool IsFatChainOverwritten(FileFAT file, out int numZeroLinks) {
			// Iterate through the FAT links to see if any of them are non-zero.
			long remainingLength = file.Length;
			var currentCluster = file.FirstCluster;
			numZeroLinks = 0;
			while (remainingLength > 0) {
				if (_fileAllocationTable.GetEntry(currentCluster) != 0) {
					return true;
				}
				remainingLength -= BytesPerCluster;
				currentCluster++;
				numZeroLinks++;
			}
			return false;
		}

		private Dictionary<long, IFATNode> _firstClusterIndex = null;
		private void BuildFirstClusterIndex() {
			if (_firstClusterIndex == null) {
				// Iterate over all files on the drive and record their first cluster.
				// Newer files overwrite older files.
				_firstClusterIndex = new Dictionary<long, IFATNode>();
				SearchByTree(new NodeVisitCallback(delegate(INodeMetadata metadata, ulong progress, ulong total) {
					// Handle files
					FileFAT file = metadata as FileFAT;
					if (file != null) {
						if (!_firstClusterIndex.ContainsKey(file.FirstCluster)
								|| _firstClusterIndex[file.FirstCluster].LastModified < file.LastModified) {
							_firstClusterIndex[file.FirstCluster] = file;
						}
					}
					// Handle folders
					FolderFAT folder = metadata as FolderFAT;
					if (folder != null) {
						if (!_firstClusterIndex.ContainsKey(folder.FirstCluster)
								|| _firstClusterIndex[folder.FirstCluster].LastModified < folder.LastModified) {
							_firstClusterIndex[folder.FirstCluster] = folder;
						}
					}
					return true;
				}), null);
			}
		}
	}
}
