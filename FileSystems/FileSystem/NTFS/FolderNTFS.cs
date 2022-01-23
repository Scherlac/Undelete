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

namespace KFS.FileSystems.NTFS {
	/// <summary>
	/// A folder (directory) node in the NTFS filesystem.
	/// </summary>
	public class FolderNTFS : Folder, IDescribable {
		private class IndexBuffer {
			List<IndexEntry> _entries = null;
			ulong _clusterStart;
			UInt16 _entriesStart;
			UInt16 _entriesEnd;
			FolderNTFS _folder;
			IDataStream _stream;
			public IndexBuffer(IDataStream stream, ulong vcn, FolderNTFS folder) {
				_folder = folder;
				_clusterStart = vcn * (ulong)(_folder._record.SectorsPerCluster * _folder._record.BytesPerSector);
				String magic = Util.GetASCIIString(stream, _clusterStart + 0x0, 4);

				if (!magic.Equals("INDX")) {
					throw new Exception("Magic INDX value not present");
				}

				_entriesStart = (ushort)(Util.GetUInt16(stream, _clusterStart + 0x18) + 0x18);
				_entriesEnd = (ushort)(Util.GetUInt16(stream, _clusterStart + 0x1c) + _entriesStart);

				ushort updateSequenceOffset = Util.GetUInt16(stream, _clusterStart + 0x04);
				ushort updateSequenceLength = Util.GetUInt16(stream, _clusterStart + 0x06);

				ushort updateSequenceNumber = Util.GetUInt16(stream, _clusterStart + updateSequenceOffset);
				ushort[] updateSequenceArray = new ushort[updateSequenceLength - 1];
				ushort read = 1;
				while (read < updateSequenceLength) {
					updateSequenceArray[read - 1] = Util.GetUInt16(stream, _clusterStart + updateSequenceOffset + (ushort)(read * 2));
					read++;
				}

				_stream = new FixupStream(stream, _clusterStart, _entriesEnd, updateSequenceNumber, updateSequenceArray, (ulong)folder.BytesPerSector);

				if (_entriesEnd == _entriesStart) {
					throw new Exception("Entry size was 0");
				}
			}

			private void LoadEntries() {
				_entries = new List<IndexEntry>();
				HashSet<ulong> recordNumbers = new HashSet<ulong>(); // to check for dupes
				ulong offset = _entriesStart;
				IndexEntry entry;
				do {
					entry = new IndexEntry(_stream, offset, _folder);
					if (!recordNumbers.Contains(entry.RecordNum)) {
						// check for dupes
						_entries.Add(entry);
						if (!entry.DummyEntry) {
							recordNumbers.Add(entry.RecordNum);
						}
					}
					offset += entry.EntryLength;
				} while (!entry.LastEntry && offset < _entriesEnd);
			}

			public List<IndexEntry> GetEntries() {
				if (_entries == null) {
					LoadEntries();
				}
				return _entries;
			}
		}
		private class IndexEntry {
			private UInt64 _indexedFile;
			private UInt16 _indexEntryLength;
			private UInt16 _filenameOffset;
			private Byte _filenameLength;
			private UInt16 _flags;
			private UInt64 _recordNum;

			private IndexBuffer _child = null;
			private FileSystemNode _node = null;

			private FolderNTFS _folder;
			private ulong _offset;
			private IDataStream _stream;

			public IndexEntry(IDataStream stream, ulong offset, FolderNTFS folder) {
				_folder = folder;
				_stream = stream;
				_offset = offset;
				ulong mask = 0x0000ffffffffffff; // This is a hack

				_indexedFile = Util.GetUInt64(stream, offset);
				_indexEntryLength = Util.GetUInt16(stream, offset + 8);
				_filenameOffset = Util.GetUInt16(stream, offset + 10);
				_flags = Util.GetByte(stream, offset + 12);
				if (_indexEntryLength > 0x50) {
					_filenameLength = Util.GetByte(stream, offset + 0x50);
					Name = Util.GetUnicodeString(stream, offset + 0x52, (ulong)_filenameLength * 2);
					DummyEntry = false;
				} else {
					// no filename, dummy entry
					DummyEntry = true;
				}

				_recordNum = (_indexedFile & mask);
			}

			public bool LastEntry {
				get { return (_flags & 2) == 2; }
			}

			public ushort EntryLength {
				get { return _indexEntryLength; }
			}

			public ulong RecordNum {
				get { return _recordNum; }
			}

			public bool DummyEntry {
				get;
				private set;
			}

			public string Name {
				get;
				private set;
			}

			public FileSystemNode FileSystemNode {
				get {
					if (_node == null && !DummyEntry) {
						//Not last entry
						if ((_flags & 2) == 0) {
							//Could read index stream here (i.e. file name etc - but we can just grab the info
							//from the actual mft record at the cost of efficiency.
						}

						if (_recordNum != _folder._record.RecordNum) {
							MFTRecord record = MFTRecord.Load(_recordNum, _folder._record.FileSystem);
							if (record.Valid) {
								_node = record.GetFileSystemNode(_folder.Path);
							}
						}
					}
					return _node;
				}
			}

			public IndexBuffer Child {
				get {
					if (_child == null && _folder._indexAllocation != null && (_flags & 1) > 0) {
						//This isn't a leaf - points to more index entries
						UInt64 vcn = Util.GetUInt32(_stream, _offset + (ulong)(_indexEntryLength - 8));
						_child = new IndexBuffer(_folder._indexAllocation, vcn, _folder);
					}
					return _child;
				}
			}

			public IEnumerable<FileSystemNode> GetNodes() {
				List<FileSystemNode> res = new List<FileSystemNode>();
				if (Child != null) {
					foreach (IndexEntry entry in Child.GetEntries()) {
						res.AddRange(entry.GetNodes());
					}
				}
				if (FileSystemNode != null) {
					res.Add(FileSystemNode);
				}
				return res;
			}

			public FileSystemNode FindNode(string name) {
				if (Child != null) {
					name = name.ToUpperInvariant();
					foreach (IndexEntry entry in Child.GetEntries()) {
						if (entry.LastEntry) {
							return entry.FindNode(name);
						} else if (!string.IsNullOrEmpty(entry.Name)) {
							string currentName = entry.Name.ToUpper();
							if (name == currentName) {
								return entry.FileSystemNode;
							} else if (name.CompareTo(currentName) < 0) {
								return entry.FindNode(name);
							}
						}
					}
				}
				return null;
			}
			public override string ToString() {
				return FileSystemNode == null ? "null" : FileSystemNode.Name;
			}
		}

		private MFTRecord _record;
		private NTFSFileStream _indexRoot, _indexAllocation;
		private List<IndexEntry> _rootEntries = null;

		public FolderNTFS(MFTRecord record, string path, bool isRoot = false) {
			_record = record;
			FileSystem = record.FileSystem;
			Deleted = _record.Deleted;
			_indexRoot = new NTFSFileStream(_record.PartitionStream, _record, AttributeType.IndexRoot);

			MFTAttribute attr = _record.GetAttribute(AttributeType.IndexAllocation);
			if (attr != null) {
				_indexAllocation = new NTFSFileStream(_record.PartitionStream, _record, AttributeType.IndexAllocation);
			}
			if (isRoot) { // root
				Root = true;
				Name = FileSystem.Store.DeviceID;
				Path = FileSystem.Store.DeviceID;
				foreach (FileSystemNode node in GetChildren("$Volume")) {
					FileNTFS file = node as FileNTFS;
					if (file != null && file.VolumeLabel != "") {
						Name = file.VolumeLabel;
						break;
					}
				}
			} else {
				Name = PathUtils.MakeFileNameValid(record.FileName);
				if (!string.IsNullOrEmpty(path)) {
					Path = PathUtils.Combine(path, Name);
				} else {
					// We don't know the path
					Path = PathUtils.Combine(FileSystem.Store.DeviceID, "?", Name);
				}
			}
		}

		public long BytesPerSector {
			get { return _record.BytesPerSector; }
		}

		public override long Identifier {
			get { return (long)_record.MFTRecordNumber; }
		}

		private void loadChildrenIndexRoot() {
			NTFSFileStream stream = _indexRoot;
			_rootEntries = new List<IndexEntry>();

			//Index Root
			UInt32 attrTypes = Util.GetUInt32(stream, 0x0);
			UInt32 indexBufferSize = Util.GetUInt32(stream, 0x8);
			Byte clustersPerIndexBuffer = Util.GetByte(stream, 0xC);
			UInt32 size = Util.GetUInt32(stream, 0x14);
			UInt32 size2 = Util.GetUInt32(stream, 0x18);
			UInt32 flags = Util.GetUInt32(stream, 0x1C);

			ulong offset = 0x20;
			IndexEntry entry;
			do {
				entry = new IndexEntry(stream, offset, this);
				_rootEntries.Add(entry);
				offset += entry.EntryLength;
			} while (!entry.LastEntry);
		}

		private IEnumerable<IndexEntry> RootEntries {
			get {
				if (_rootEntries == null) {
					loadChildrenIndexRoot();
				}
				return _rootEntries;
			}
		}

		public override void ReloadChildren() {
			loadChildrenIndexRoot();
		}

		public override IEnumerable<IFileSystemNode> GetChildren() {
			List<FileSystemNode> res = new List<FileSystemNode>();
			foreach (IndexEntry entry in RootEntries) {
				res.AddRange(entry.GetNodes());
			}
			return res;
		}

		public override IEnumerable<IFileSystemNode> GetChildren(string name) {
			if (name == "*") {
				return GetChildren();
			} else {
				List<IFileSystemNode> res = new List<IFileSystemNode>();
				// Use the B+ tree to efficiently find the child
				name = name.ToUpperInvariant();
				foreach (IndexEntry entry in RootEntries) {
					if (entry.LastEntry) {
						FileSystemNode node = entry.FindNode(name);
						if (node != null) {
							res.Add(node);
						}
						break;
					} else if (!string.IsNullOrEmpty(entry.Name)) {
						string currentName = entry.Name.ToUpperInvariant();
						if (name == currentName) {
							res.Add(entry.FileSystemNode);
							break;
						} else if (name.CompareTo(currentName) < 0) {
							FileSystemNode node = entry.FindNode(name);
							if (node != null) {
								res.Add(node);
							}
							break;
						}
					}
				}
				return res;
			}
		}

		public override byte[] GetBytes(ulong offset, ulong length) {
			return _indexRoot.GetBytes(offset, length);
		}

		public override ulong DeviceOffset {
			get { return _indexRoot.DeviceOffset; }
		}

		public override ulong StreamLength {
			get { return _indexRoot.StreamLength; }
		}

		public override String StreamName {
			get { return "NTFS Directory " + _record.FileName; }
		}

		public override IDataStream ParentStream {
			get { return _record.PartitionStream; }
		}

		public override void Open() { }

		public override void Close() { }

		public DateTime CreationTime {
			get { return _record.CreationTime; }
		}

		public DateTime LastAccessed {
			get { return _record.LastAccessTime; }
		}

		public override DateTime LastModified {
			get { return _record.LastDataChangeTime; }
		}

		public DateTime LastModifiedMFT {
			get { return _record.LastMFTChangeTime; }
		}

		public bool Root {
			get;
			private set;
		}

		#region IDescribable Members

		public string TextDescription {
			get {
				StringBuilder sb = new StringBuilder();
				sb.AppendFormat("{0}: {1}\r\n", "Name", Name);
				sb.AppendFormat("{0}: {1}\r\n", "Size", Util.FileSizeToHumanReadableString(StreamLength));
				sb.AppendFormat("{0}: {1}\r\n", "Deleted", Deleted);
				sb.AppendFormat("{0}: {1}\r\n", "Created", CreationTime);
				sb.AppendFormat("{0}: {1}\r\n", "Last Modified", LastModified);
				sb.AppendFormat("{0}: {1}\r\n", "MFT Record Last Modified", LastModifiedMFT);
				sb.AppendFormat("{0}: {1}\r\n", "Last Accessed", LastAccessed);
				return sb.ToString();
			}
		}

		#endregion
	}
}
