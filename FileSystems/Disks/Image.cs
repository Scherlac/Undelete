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
using KFS.FileSystems;
using System;
using System.IO;
using System.Xml.Serialization;

namespace KFS.Disks {
	/// <summary>
	/// Represents a disk image stored in a file, allowing the filesystem to be
	/// loaded as if it were a physical disk.
	/// </summary>
	public class Image : IFileSystemStore, IDescribable, IHasSectors {
		public delegate void ImageCallback(ulong x, ulong total);

		public String Path { get; set; }

		public string Name { get; set; }

		public Attributes Attributes { get; set; }

		public StorageType StorageType { get; set; }

		public ulong DeviceOffset {
			get {
				return 0;
			}
		}

		public string DeviceID {
			get { return Name; }
		}

		private IFileSystem _fileSystem;
		[XmlIgnore]
		public IFileSystem FS {
			get { return _fileSystem; }
		}

		public static Image CreateImage(IImageable stream, String path, ImageCallback callback) {
			ulong BLOCK_SIZE = 1024 * 1024; // Write 1MB at a time
			BinaryWriter bw = new BinaryWriter(System.IO.File.OpenWrite(path));
			ulong offset = 0;
			while (offset < stream.StreamLength) {
				ulong read = Math.Min(BLOCK_SIZE, stream.StreamLength - offset);
				bw.Write(stream.GetBytes(offset, read));
				callback(offset, stream.StreamLength);
				offset += read;
			}
			bw.Close();
			callback(stream.StreamLength, stream.StreamLength);

			Image result = new Image();
			result.Path = path;
			result.Name = System.IO.Path.GetFileNameWithoutExtension(path);
			result.Attributes = stream.GetAttributes();
			if (stream is IPhysicalDisk) {
				result.StorageType = StorageType.PhysicalDisk;
			} else if (stream is PhysicalDiskPartition) {
				result.StorageType = StorageType.PhysicalDiskPartition;
			} else {
				result.StorageType = StorageType.PhysicalDiskRange;
			}
			result.LoadFileSystem();
			return result;
		}

		private Image() { }

		public void LoadFileSystem() {
			_fileSystem = FileSystem.TryLoad(this);
		}

		public override string ToString() {
			return StreamName;
		}

		#region IDataStream Members

		FileDataStream fileStream = null;

		public byte[] GetBytes(ulong offset, ulong length) {
			if (fileStream == null) {
				Open();
			}
			return fileStream.GetBytes(offset, length);
		}

		public ulong StreamLength {
			get {
				if (fileStream == null) {
					Open();
				}
				return fileStream.StreamLength;
			}
		}

		public String StreamName {
			get { return Name; }
		}

		public virtual IDataStream ParentStream {
			get { return null; }
		}

		public void Open() {
			if (fileStream == null) {
				fileStream = new FileDataStream(Path, null);
				fileStream.Open();
			}
		}

		public void Close() {
			if (fileStream != null) {
				fileStream.Close();
				fileStream = null;
			}
		}

		#endregion

		#region IDescribable Members

		public string TextDescription {
			get { return Attributes.TextDescription; }
		}

		#endregion

		#region IHasSectors Members

		public ulong GetSectorSize() {
			if (StorageType == StorageType.PhysicalDisk) {
				return ((PhysicalDiskAttributes)Attributes).BytesPerSector;
			} else if (StorageType == StorageType.PhysicalDiskPartition) {
				return ((PhysicalDiskPartitionAttributes)Attributes).BlockSize;
			} else {
				return 512; // best guess
			}
		}

		public SectorStatus GetSectorStatus(ulong sectorNum) {
			if (StorageType == StorageType.PhysicalDiskPartition && FS != null) {
				return FS.GetSectorStatus(sectorNum);
			} else {
				return SectorStatus.Unknown;
			}
			// TODO: We should probably add a StorageLayer abstraction so that Images
			// can do all the same things as PhysicalDisks/Partitions/Ranges without code duplication.
			// We should probably have a subclass of Image for each of the StorageType values.
		}

		#endregion
	}
}
