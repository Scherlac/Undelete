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
using System.Linq;

namespace KFS.FileSystems {
	/// <summary>
	/// An abstract node of the filesystem tree.
	/// </summary>
	public abstract class FileSystemNode : IFileSystemNode {
		public abstract long Identifier { get; }
		public abstract FSNodeType Type { get; }
		public string Name { get; protected set; }
		public ulong Size {
			get { return StreamLength; }
		}
		public string Path { get; set; }
		public bool Deleted { get; protected set; }
		public abstract DateTime LastModified { get; }
		public FileSystem FileSystem { get; protected set; }
		public abstract IEnumerable<IFileSystemNode> GetChildren();
		public bool Loaded { get; set; }

		public virtual void ReloadChildren() { }

		public virtual IEnumerable<IFileSystemNode> GetChildren(string name) {
			name = name.Trim('/', '\\');
			if (name == "*") {
				return GetChildren();
			} else {
				return GetChildren().Where(node => Matches(name, node.Name)).ToList();
			}
		}

		public IEnumerable<IFileSystemNode> GetChildrenAtPath(string path) {
			if (path.StartsWith("/")) path = path.Substring(1);
			string nextFile, remainingPath;
			int nextSlash = path.IndexOf('/');
			if (nextSlash > -1) {
				nextFile = path.Substring(0, nextSlash);
				remainingPath = path.Substring(nextSlash + 1);
			} else {
				nextFile = path;
				remainingPath = "";
			}
			List<IFileSystemNode> res = new List<IFileSystemNode>();
			if (remainingPath.Replace("/", "") == "") {
				res.AddRange(GetChildren(nextFile));
			} else {
				foreach (IFileSystemNode node in GetChildren(nextFile)) {
					if (node is Folder) {
						res.AddRange(node.GetChildrenAtPath(remainingPath));
					}
				}
			}
			return res;
		}

		private bool Matches(string expression, string s) {
			if (s == null) return false;
			expression = expression.ToLower();
			s = s.ToLower();
			// This could use Regexes in future
			return expression == "*" || expression == s;
		}

		private FileRecoveryStatus _recoveryStatus = FileRecoveryStatus.Unknown;
		public FileRecoveryStatus ChanceOfRecovery {
			get {
				if (_recoveryStatus == FileRecoveryStatus.Unknown) {
					return FileSystem.GetChanceOfRecovery(this);
				}
				return _recoveryStatus;
			}
			set {
				_recoveryStatus = value;
			}
		}

		#region IDataStream Members

		public abstract byte[] GetBytes(ulong offset, ulong length);

		public abstract ulong StreamLength { get; }

		public abstract ulong DeviceOffset { get; }

		public abstract String StreamName { get; }

		public abstract IDataStream ParentStream { get; }

		public abstract void Open();

		public abstract void Close();

		#endregion

		public override string ToString() {
			return Name;
		}

		#region INodeMetadata Members


		public IFileSystemNode GetFileSystemNode() {
			return this;
		}

		#endregion

		public IFile AsFile() {
			return this as File;
		}

		public IFolder AsFolder() {
			return this as Folder;
		}
	}
}
