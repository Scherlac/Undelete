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
using KFS.FileSystems.FAT;
using System;
using System.Text;

namespace KFS.FileSystems {
	/// <summary>
	/// Attributes of a FAT file.
	/// </summary>
	public class FileAttributesFAT : IDescribable {
		public string Name { get; private set; }
		public ulong Size { get; private set; }
		public bool Deleted { get; private set; }
		public bool Hidden { get; private set; }
		public bool System { get; private set; }
		public bool Archive { get; private set; }
		public bool ReadOnly { get; private set; }
		public DateTime Created { get; private set; }
		public DateTime LastModified { get; private set; }
		public DateTime LastAccessed { get; private set; }

		public FileAttributesFAT(FolderFAT.DirectoryEntry entry) {
			Name = entry.FileName;
			Size = (ulong)entry.Length;
			Deleted = entry.Free;
			Hidden = (entry.Attributes & FATDirectoryAttributes.ATTR_HIDDEN) != 0;
			System = (entry.Attributes & FATDirectoryAttributes.ATTR_SYSTEM) != 0;
			Archive = (entry.Attributes & FATDirectoryAttributes.ATTR_ARCHIVE) != 0;
			ReadOnly = (entry.Attributes & FATDirectoryAttributes.ATTR_READ_ONLY) != 0;
			Created = entry.CreationTime;
			LastModified = entry.LastWrite;
			LastAccessed = entry.LastAccess;
		}

		public FileAttributesFAT() {
			Name = "Root Directory";
		}

		public string TextDescription {
			get {
				StringBuilder sb = new StringBuilder();
				sb.AppendFormat("{0}: {1}\r\n", "Name", Name);
				sb.AppendFormat("{0}: {1}\r\n", "Size", Util.FileSizeToHumanReadableString(Size));
				sb.AppendFormat("{0}: {1}\r\n", "Deleted", Deleted);
				sb.AppendFormat("{0}: {1}\r\n", "Hidden", Hidden);
				sb.AppendFormat("{0}: {1}\r\n", "System", System);
				sb.AppendFormat("{0}: {1}\r\n", "Archive", Archive);
				sb.AppendFormat("{0}: {1}\r\n", "Read Only", ReadOnly);
				sb.AppendFormat("{0}: {1}\r\n", "Created", Created);
				sb.AppendFormat("{0}: {1}\r\n", "Last Modified", LastModified);
				sb.AppendFormat("{0}: {1}\r\n", "Last Accessed", LastAccessed.ToShortDateString());
				return sb.ToString();
			}
		}
	}
}
