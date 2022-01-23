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

using KFS.FileSystems.NTFS;

namespace KFA.Exceptions {
	public class InvalidFileRecordException : NTFSException {
		public InvalidFileRecordException(FileSystemNTFS fileSystem, ulong errorOffset, string expected, string found)
			: base(fileSystem, errorOffset, string.Format("Error parsing file record at {0}. Expected {1}, found {2}", errorOffset, expected, found)) {

		}
		public InvalidFileRecordException(FileSystemNTFS fileSystem, ulong errorOffset, string error)
			: base(fileSystem, errorOffset, string.Format("Error parsing file record at {0}. {1}", errorOffset, error)) {

		}
	}
}
