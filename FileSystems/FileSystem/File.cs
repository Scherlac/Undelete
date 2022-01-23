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

using System.Collections.Generic;
#if !KFS_LEAN_AND_MEAN
using Ionic.Zip;
using KFS.DataStream;
#endif

namespace KFS.FileSystems {
	/// <summary>
	/// A file node.
	/// </summary>
	public abstract class File : FileSystemNode, IFile {
#if !KFS_LEAN_AND_MEAN
		private bool _isZip = false;
		private bool _known = false;
#endif

		public bool IsZip {
			get {
#if KFS_LEAN_AND_MEAN
				return false;
#else
				if (!_known) {
					//_known = ZipFile.IsZipFile(new ForensicsAppStream(this), false);
					_isZip = Name.Trim().ToLower().EndsWith("zip");
					_known = true;
				}
				return _isZip;
#endif
			}
		}

		public override IEnumerable<IFileSystemNode> GetChildren() {
#if KFS_LEAN_AND_MEAN
			return new List<FileSystemNode>();
#else
			if (IsZip) {
				ZipFile f = ZipFile.Read(new ForensicsAppStream(this));
				string tempDir = Util.CreateTemporaryDirectory();
				// TODO: Add progress bar here
				f.ExtractAll(tempDir, ExtractExistingFileAction.InvokeExtractProgressEvent);
				FolderMounted folder = new FolderMounted(tempDir, this);
				return folder.GetChildren();
			} else {
				return new List<FileSystemNode>();
			}
#endif
		}

		public override string ToString() {
			return Name;
		}

		public override FSNodeType Type {
			get { return FSNodeType.File; }
		}
	}
}
