// Copyright (C) 2017  Joey Scarr
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

using KFS.FileSystems.NTFS;

namespace KickassUndelete {
	/// <summary>
	/// A lightweight copy of an MFTRecord that Scanner uses in indexes.
	/// Used to reconstruct file paths and detect overwritten clusters.
	/// </summary>
	public class LightweightMFTRecord {
		public LightweightMFTRecord(MFTRecord record) {
			FileName = record.FileName;
			IsDirectory = record.IsDirectory;
			RecordNumber = record.RecordNum;
			ParentRecord = record.ParentDirectory;
		}
		public string FileName { get; private set; }
		public string Path { get; set; }
		public bool IsDirectory { get; private set; }
		public ulong RecordNumber { get; private set; }
		public ulong ParentRecord { get; private set; }
	}
}
