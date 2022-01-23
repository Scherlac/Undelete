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
using System.Collections.Generic;

namespace KFS.FileSystems {
	/// <summary>
	/// An interface representing a node in the filesystem.
	/// </summary>
	public interface IFileSystemNode : INodeMetadata, IDataStream {
		IFile AsFile();
		IFolder AsFolder();

		IEnumerable<IFileSystemNode> GetChildren();

		IEnumerable<IFileSystemNode> GetChildrenAtPath(string path);

		long Identifier { get; }

		bool Loaded { get; set; }

		string Path { get; set; }

		FSNodeType Type { get; }
	}

	public enum FSNodeType {
		File,
		Folder
	}
}