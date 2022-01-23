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
using System.Collections;
using System.Collections.Generic;

namespace KFS.FileSystems.NTFS {
	/// <summary>
	/// Encapsulates an NTFS filesystem.
	/// </summary>
	public class FileSystemNTFS : FileSystem {
		private const int BPB_SIZE = 84;
		ushort BPB_BytsPerSec;
		byte BPB_SecPerClus;
		ushort BPB_SecPerTrk;
		ushort BPB_NumHeads;
		uint BPB_HiddSec;
		ulong BPB_TotSec64;
		ulong BPB_MFTStartCluster64;
		ulong BPB_MFTMirrorStartCluster64;
		ushort BPB_SectorsPerMFTRecord;
		ulong BPB_SerialNumber;

		private void LoadBPB() {
			byte[] bpb = Store.GetBytes((ulong)0x0B, (ulong)BPB_SIZE);

			BPB_BytsPerSec = BitConverter.ToUInt16(bpb, 0);
			BPB_SecPerClus = bpb[2];
			BPB_SecPerTrk = bpb[13];
			BPB_NumHeads = bpb[15];
			BPB_HiddSec = BitConverter.ToUInt32(bpb, 17);
			BPB_TotSec64 = BitConverter.ToUInt64(bpb, 29);
			BPB_MFTStartCluster64 = BitConverter.ToUInt64(bpb, 37);
			BPB_MFTMirrorStartCluster64 = BitConverter.ToUInt64(bpb, 45);

			byte b = bpb[53];
			if (b > 0x80) {
				BPB_SectorsPerMFTRecord = (ushort)(Math.Pow(2, Math.Abs(256 - b)) / BPB_BytsPerSec);
			} else {
				BPB_SectorsPerMFTRecord = (ushort)(BPB_SecPerClus * b);
			}
			BPB_SerialNumber = BitConverter.ToUInt64(bpb, 57);
		}

		private FileSystemNode _root = null;
		private FileNTFS _MFT = null;
		private UInt64 _mftSector;
		private FileNTFS _bitmapFile;
		private byte[] _bitmap;

		public FileSystemNTFS(IFileSystemStore store) {
			Store = store;

			LoadBPB();

			_mftSector = (BPB_MFTStartCluster64 * BPB_SecPerClus);
			_MFT = new FileNTFS(MFTRecord.Load(0, this), "");
			_root = new FolderNTFS(MFTRecord.Load(5, this), "", true);

			_bitmapFile = new FileNTFS(MFTRecord.Load(6, this), "");
			_bitmap = _bitmapFile.GetBytes(0, _bitmapFile.StreamLength);
		}

		public override FileSystemNode GetRoot() {
			return _root;
		}

		public override string FileSystemType {
			get {
				return "NTFS";
			}
		}

		public override SectorStatus GetSectorStatus(ulong sectorNum) {
			ulong lcn = sectorNum / (ulong)(BPB_SecPerClus);
			return GetClusterStatus(lcn);
		}

		private SectorStatus GetClusterStatus(ulong lcn) {
			if (lcn / 8 >= (ulong)_bitmap.Length) {
				Console.Error.WriteLine(string.Format("ERROR: Tried to read off the end of " +
					"the $Bitmap file. $Bitmap length = {0}, lcn = {1}, lcn / 8 = {2}",
					_bitmap.Length, lcn, lcn / 8));
				return SectorStatus.Unknown;
			}
			Byte b = _bitmap[lcn / 8];
			Byte mask = (byte)(0x1 << (int)(lcn % 8));
			if ((b & mask) > 0) {
				return SectorStatus.Used;
			} else {
				return SectorStatus.Free;
			}
		}

		internal override FileRecoveryStatus GetChanceOfRecovery(FileSystemNode node) {
			FileNTFS file = node as FileNTFS;
			if (file == null) {
				return FileRecoveryStatus.Unknown;
			} else {
				IEnumerable<IRun> runs = file.GetRuns();
				if (runs == null) {
					// The data stream is resident, so recovery is trivial.
					return FileRecoveryStatus.Resident;
				} else {
					ulong totalClusters = 0;
					ulong usedClusters = 0;
					// Check the status of each cluster in the runs.
					foreach (IRun run in runs) {
						if (run.HasRealClusters) {
							totalClusters += run.LengthInClusters;
							for (ulong i = run.LCN; i < run.LengthInClusters; i++) {
								if (GetClusterStatus(run.LCN + i) == SectorStatus.Used
										|| GetClusterStatus(run.LCN + i) == SectorStatus.Bad) {
									usedClusters++;
								}
							}
						}
					}
					if (usedClusters == 0) {
						return FileRecoveryStatus.MaybeOverwritten;
					} else if (usedClusters < totalClusters) {
						return FileRecoveryStatus.PartiallyOverwritten;
					} else {
						return FileRecoveryStatus.Overwritten;
					}
				}
			}
		}

		public FileNTFS MFT {
			get { return _MFT; }
		}

		public ulong MFTSector {
			get { return _mftSector; }
		}

		public long BytesPerSector {
			get { return BPB_BytsPerSec; }
		}

		public long SectorsPerMFTRecord {
			get { return BPB_SectorsPerMFTRecord; }
		}

		public long BytesPerMFTRecord {
			get { return BytesPerSector * SectorsPerMFTRecord; }
		}

		public long SectorsPerCluster {
			get { return BPB_SecPerClus; }
		}

		public long BytesPerCluster {
			get { return BytesPerSector * SectorsPerCluster; }
		}

		public void SearchByTree(FileSystem.NodeVisitCallback callback, string searchPath) {
			FileSystemNode searchRoot = this._root;
			if (!string.IsNullOrEmpty(searchPath)) {
				searchRoot = this.GetFirstFile(searchPath) ?? searchRoot;
			}
			Visit(callback, searchRoot);
		}

		public void SearchByMFT(FileSystem.NodeVisitCallback callback, string searchPath) {
			MftScan(callback);
		}

		public override List<ISearchStrategy> GetSearchStrategies() {
			List<ISearchStrategy> res = new List<ISearchStrategy>();

			// Add the MFT search strategy (default)
			res.Add(new SearchStrategy("MFT scan", SearchByMFT));

			// Add the tree search strategy
			res.Add(new SearchStrategy("Folder hierarchy scan", SearchByTree));

			return res;
		}

		private void MftScan(FileSystem.NodeVisitCallback callback) {
			ulong numFiles = _MFT.StreamLength / (ulong)(SectorsPerMFTRecord * BytesPerSector);

			for (ulong i = 0; i < numFiles; i++) {
				MFTRecord record = MFTRecord.Load(i, this, MFTLoadDepth.NameAttributeOnly);

				if (record.Valid) {
					if (!callback(record, i, numFiles)) {
						return;
					}
				}
			}
		}

		private void Visit(FileSystem.NodeVisitCallback callback, FileSystemNode node) {
			if (!callback(node, 0, 1)) {
				return;
			}
			if (node is Folder && !(node is HiddenDataStreamFileNTFS)) {  //No zip support yet
				foreach (FileSystemNode child in node.GetChildren()) {
					Visit(callback, child);
				}
			}
		}
	}
}
