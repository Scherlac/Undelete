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
using KFS.DataStream;
using KFS.Disks;

namespace KFS.FileSystems.FAT {
	/// <summary>
	/// Represents the File Allocation Table (FAT) in a FAT filesystem.
	/// </summary>
	public class FileAllocationTable {
		private uint[] _fatEntries;
		private bool _inMemory;
		private int _entrySize;
		private long _fatOffset;
		private long _fatSizeInBytes;
		private int _numEntries;
		private IFileSystemStore _store;

		/// <summary>
		/// Reads the File Allocation Table of a FAT filesystem.
		/// </summary>
		public FileAllocationTable(IFileSystemStore store, long fatOffset, long fatSizeInBytes, PartitionType type) {
			_store = store;
			_fatOffset = fatOffset;
			_fatSizeInBytes = fatSizeInBytes;
			if (type == PartitionType.FAT16) {
				_entrySize = 2;
			} else { // FAT32
				_entrySize = 4;
			}
			_numEntries = (int)(fatSizeInBytes / _entrySize);

			// If the FAT is small enough, just load it into memory.
			// Otherwise, we'll read from the disk on demand.
			if (fatSizeInBytes < 100 * 1024 * 1024) { // < 100 MB
				_fatEntries = new uint[_numEntries];
				int i = 0;
				for (long offset = 0; offset < fatSizeInBytes; offset += _entrySize) {
					_fatEntries[i] = (uint)Util.GetArbitraryUInt(store, (ulong)(fatOffset + offset), (int)_entrySize);
					++i;
				}
				_inMemory = true;
			} else {
				_inMemory = false;
			}
		}

		public uint GetEntry(long N) {
			if (N < 0 || N >= _numEntries) {
				Console.Error.WriteLine("Error: Tried to read off the end of the FAT. Requested entry: {0}; FAT size: {1}", N, _numEntries);
				return 0;
			}

			if (_inMemory) {
				return _fatEntries[N];
			} else {
				return GetEntryFromDisk(N);
			}
		}

		private uint GetEntryFromDisk(long N) {
			long offset = N * _entrySize;

			long entryLoc = _fatOffset + offset;
			return (uint)Util.GetArbitraryUInt(_store, (ulong)entryLoc, _entrySize);
		}
	}
}
