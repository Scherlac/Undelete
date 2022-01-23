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
	/// An NTFS data run that contains only zeroes. Stored on disk as metadata
	/// only (i.e. the zeroes are not actually stored) in order to save space.
	/// </summary>
	public class SparseRun : IRun {
		public SparseRun(ulong vcn, ulong lengthInClusters, MFTRecord record) {
			VCN = vcn;
			LengthInClusters = lengthInClusters;
			ulong clusterSize = (ulong)record.BytesPerSector * (ulong)record.SectorsPerCluster;
			StreamLength = lengthInClusters * clusterSize;
		}

		public byte[] GetBytes(ulong offset, ulong length) {
			return new byte[length];
		}

		public ulong DeviceOffset { get; private set; }

		public ulong StreamLength { get; private set; }

		public string StreamName { get; private set; }

		public IDataStream ParentStream { get; private set; }

		public void Open() {
		}

		public void Close() {
		}

		public bool HasRealClusters {
			get { return false; }
		}

		public ulong VCN { get; private set; }

		public ulong LCN {
			get { return 0; }
		}

		public ulong LengthInClusters { get; private set; }

		public override string ToString() {
			return "Sparse " + base.ToString();
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
