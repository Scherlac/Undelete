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
using System.Collections.Generic;
using System.Text;

using KFS.FileSystems;

namespace FileSystemsTests
{
	class DiskImageTestUtil
	{
		internal static string ScanImage(string imagePath, string fileSystemType)
		{
			var stream = new TestDiskDataStream("TestImage", imagePath, fileSystemType);
			if (stream.FS == null)
				throw new Exception();
			var strat = stream.FS.GetDefaultSearchStrategy();
			var metadataEntries = new List<INodeMetadata>();
			strat.Search(new FileSystem.NodeVisitCallback(delegate(INodeMetadata metadata, ulong current, ulong total) {
				if (metadata.Deleted)
					metadataEntries.Add(metadata);
				return true;
			}));

			var sb = new StringBuilder();
			foreach (var metadata in metadataEntries)
			{
				var node = metadata.GetFileSystemNode();
				var file = node.AsFile();
				if (file == null)
					continue;
				if (file.Size > 0) {
					sb.AppendLine(file.Name);
					sb.AppendLine(file.Path);
					sb.AppendLine(file.StreamLength.ToString());
					sb.AppendLine(Convert.ToBase64String(file.GetBytes(0, Math.Min(4096, file.StreamLength))));
					if (file.StreamLength > 4096) {
						sb.AppendLine(Convert.ToBase64String(file.GetBytes(file.StreamLength - 4096, 4096)));
					} else {
						sb.AppendLine();
					}
				}
			}
			return sb.ToString();
		}
	}
}
