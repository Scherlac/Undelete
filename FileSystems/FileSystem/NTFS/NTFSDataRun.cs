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

namespace KFS.FileSystems.NTFS {
	/// <summary>
	/// A standard run of data on an NTFS filesystem. Represents a contiguous
	/// block of data stored on disk.
	/// </summary>
	public class NTFSDataRun : IRun {
		private ulong _vcn, _lcn, _bytesPerCluster, _lengthInBytes;
		private MFTRecord _record;
		public ulong VCN { get { return _vcn; } }
		public ulong LCN { get { return _lcn; } }
		public ulong LengthInClusters { get; private set; }
		public NTFSDataRun(ulong vcn, ulong lcn, ulong lengthInClusters, MFTRecord record) {
			_vcn = vcn;
			_lcn = lcn;
			LengthInClusters = lengthInClusters;
			_record = record;
			_bytesPerCluster = (ulong)(_record.BytesPerSector * _record.SectorsPerCluster);
			_lengthInBytes = LengthInClusters * _bytesPerCluster;
		}

		public bool Contains(ulong vcn) {
			return vcn >= VCN && vcn < VCN + LengthInClusters;
		}

		public bool HasRealClusters {
			get { return true; }
		}

		#region IDataStream Members

		public virtual byte[] GetBytes(ulong offset, ulong length) {
			if (offset + length - 1 < _lengthInBytes) {
				return _record.PartitionStream.GetBytes(LCN * _bytesPerCluster + offset, length);
			} else {
				throw new Exception("Offset does not exist in this run!");
			}
		}

		public ulong StreamLength {
			get { return _lengthInBytes; }
		}

		public string StreamName {
			get { return "Non-resident Attribute Run"; }
		}

		public IDataStream ParentStream {
			get { return _record.PartitionStream; }
		}

		public ulong DeviceOffset {
			get { return ParentStream.DeviceOffset + LCN * _bytesPerCluster; }
		}

		public void Open() {
			_record.PartitionStream.Open();
		}

		public void Close() {
			_record.PartitionStream.Close();
		}

		#endregion

		public override string ToString() {
			return string.Format("Run: VCN {0}, Length {1}, LCN {2}", VCN, LengthInClusters, LCN);
		}

		public int CompareTo(object obj) {
			if (obj == null) {
				return 1;
			}
			IRun otherRun = obj as IRun;
			if (otherRun != null) {
				return this.LCN.CompareTo(otherRun.LCN);
			} else {
				throw new ArgumentException("Object is not an IRun");
			}
		}
	}
}
