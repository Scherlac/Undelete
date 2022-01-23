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
	/// A file node in the NTFS filesystem.
	/// </summary>
	public class FileNTFS : File, IDescribable {

		private MFTRecord _record;
		private NTFSFileStream _stream;

		public FileNTFS(MFTRecord record, string path) {
			_record = record;
			if (_record.DataAttribute != null) {
				_stream = new NTFSFileStream(_record.PartitionStream, _record, AttributeType.Data);
			}
			FileSystem = record.FileSystem;
			Deleted = _record.Deleted;
			Name = PathUtils.MakeFileNameValid(record.FileName);
			if (!string.IsNullOrEmpty(path)) {
				Path = PathUtils.Combine(path, Name);
			} else {
				// We don't know the path.
				Path = PathUtils.Combine(FileSystem.Store.DeviceID, "?", Name);
			}
		}

		/// <summary>
		/// The constructor for a file contained as a hidden data stream.
		/// </summary>
		public FileNTFS(MFTRecord record, MFTAttribute attr, string path) {
			_record = record;
			_stream = new NTFSFileStream(_record.PartitionStream, _record, attr);
			FileSystem = record.FileSystem;
			Deleted = _record.Deleted;
			Name = PathUtils.MakeFileNameValid(record.FileName + "_" + attr.Name);
			Path = PathUtils.Combine(path, Name);
		}

		/// <summary>
		/// Gets a list of the on-disk runs of this NTFSFile. Returns null if resident.
		/// </summary>
		public IEnumerable<IRun> GetRuns() {
			return _stream.GetRuns();
		}

		public override long Identifier {
			get { return (long)_record.MFTRecordNumber; }
		}

		public override byte[] GetBytes(ulong offset, ulong length) {
			return _stream.GetBytes(offset, length);
		}

		public override ulong StreamLength {
			get { return _stream == null ? 0 : _stream.StreamLength; }
		}

		public override String StreamName {
			get { return "NTFS File - " + _record.FileName; }
		}

		public override IDataStream ParentStream {
			get { return _record.PartitionStream; }
		}

		public override ulong DeviceOffset {
			get { return _stream.DeviceOffset; }
		}

		public override void Open() {
		}

		public override void Close() {
		}

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

		public string VolumeLabel {
			get { return _record.VolumeLabel ?? ""; }
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
