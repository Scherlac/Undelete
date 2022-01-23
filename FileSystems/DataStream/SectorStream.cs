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

namespace KFS.DataStream {
	/// <summary>
	/// A data stream representing a single disk sector.
	/// </summary>
	public class SectorStream : SubStream {
		private ulong _sectorNum;

		public SectorStream(IDataStream stream, ulong start, ulong length, ulong sectorNum) :
			base(stream, start, length) {
			_sectorNum = sectorNum;
		}

		public override String StreamName {
			get { return string.Concat("Sector ", _sectorNum, " of ", ParentStream.StreamName); }
		}
	}
}
