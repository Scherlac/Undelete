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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KFS.DataStream;
using KFS.Disks;
using KFS.FileSystems;

namespace FileSystemsTests
{
	/// <summary>
	/// This represents a test data stream to recover data from. For now this loads an image, 
	/// making it redundant with ImageDiskStream. In the future it's possibly going to be more 
	/// of a "builder", so it's a separate class. 
    ///
    /// Make sure to set "Copy to Output Dir" on any image files you add to the project. 
	/// </summary>
	class TestDiskDataStream : IFileSystemStore
	{
		public string _testName { get; protected set; }
		public string _imageName { get; protected set; }
		protected FileDataStream _file { get; set; }
		protected IFileSystem _fileSystem;

		public TestDiskDataStream(string testName, string imageName, string fsType)
		{
			_testName = testName;
			_imageName = imageName;
			Open();
			_fileSystem = FileSystem.TryLoad(this as IFileSystemStore, fsType);
		}

		public StorageType StorageType => StorageType.DiskImage;

		public Attributes Attributes => throw new NotImplementedException();

		public IFileSystem FS => _fileSystem;

		public string DeviceID => "TestResult:";

		public ulong DeviceOffset => 0;

		public ulong StreamLength => (ulong)_file.StreamLength;

		public string StreamName => "Test " + _testName + " data stream";

		public IDataStream ParentStream => null;

		public void Close()
		{
			_file.Close();
			_file = null;
		}

		public byte[] GetBytes(ulong offset, ulong length)
		{
			if (_file == null) {
				Open();
			}
			return _file.GetBytes(offset, length);
		}

		public void Open()
		{
			if (_file == null)
			    _file = new FileDataStream(_imageName, null);
		}
	}
}
