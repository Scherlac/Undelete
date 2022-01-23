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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace KFS.FileSystems.NTFS {
	/// <summary>
	/// An NTFS data stream comprising multiple data runs.
	/// </summary>
	class NTFSFileStream : IDataStream {
		private IDataStream _partitionStream, _residentStream;
		private ulong _length;
		private MFTRecord _record;
		private List<IRun> _runs;
		private bool _nonResident;

		public NTFSFileStream(IDataStream partition, MFTRecord record, MFTAttribute attr) {
			if (attr != null) {
				_nonResident = attr.NonResident;
				if (_nonResident) {
					_runs = attr.Runs;
					_length = attr.DataSize;
				} else {
					_residentStream = attr.ResidentData;
					_length = attr.ResidentData.StreamLength;
				}
			}
			_record = record;
			_partitionStream = partition;
		}

		public NTFSFileStream(IDataStream partition, MFTRecord record, AttributeType attrType) :
			this(partition, record, record.GetAttribute(attrType)) { }

		/// <summary>
		/// Gets a list of the on-disk runs of this NTFSFileStream. Returns null if resident.
		/// </summary>
		public IEnumerable<IRun> GetRuns() {
			return _nonResident ? new ReadOnlyCollection<IRun>(_runs) : null;
		}

		public byte[] GetBytes(ulong offset, ulong length) {
			if (offset + length > _length) {
				throw new ArgumentOutOfRangeException(string.Format("Tried to read off the end of the file! offset = {0}, length = {1}, file length = {2}", offset, length, _length));
			}
			if (_nonResident) {
				// Special case for only 1 run (by far the most common occurrence).
				// Avoids the loop logic and Array.Copy.
				if (_runs.Count == 1) {
					return _runs[0].GetBytes(offset, length);
				}

				byte[] res = new byte[length];
				ulong bytesPerCluster = (ulong)(_record.SectorsPerCluster * _record.BytesPerSector);
				ulong firstCluster = offset / bytesPerCluster;
				ulong lastCluster = (offset + length - 1) / bytesPerCluster;
				foreach (NTFSDataRun run in _runs) {
					// If this run doesn't overlap the cluster range we want, skip it.
					if (run.VCN + run.LengthInClusters <= firstCluster || run.VCN > lastCluster) {
						continue;
					}
					ulong offsetInRun, bytesRead, copyLength;

					if (run.Contains(firstCluster)) {
						bytesRead = 0;
						offsetInRun = offset - run.VCN * bytesPerCluster;
					} else {
						offsetInRun = 0;
						bytesRead = run.VCN * bytesPerCluster - offset;
					}
					ulong bytesLeftToRead = length - bytesRead;
					ulong bytesLeftInRun = run.LengthInClusters * bytesPerCluster - offsetInRun;

					copyLength = Math.Min(bytesLeftToRead, bytesLeftInRun);

					byte[] a = run.GetBytes(offsetInRun, copyLength);
					Array.Copy(a, 0, res, (int)bytesRead, (int)copyLength);

				}
				return res;
			} else {
				return _residentStream.GetBytes(offset, length);
			}
		}

		public ulong DeviceOffset {
			get { return 0; }
		}

		public ulong StreamLength {
			get {
				return _length;
			}
		}

		public String StreamName {
			get { return "NTFS File " + _record.FileName; }
		}

		public IDataStream ParentStream {
			get { return _record.PartitionStream; }
		}

		public void Open() {
			if (_nonResident) {
				_partitionStream.Open();
			} else {
				_residentStream.Open();
			}
		}

		public void Close() {
			if (_nonResident) {
				_partitionStream.Close();
			} else {
				_residentStream.Close();
			}
		}
	}
}
