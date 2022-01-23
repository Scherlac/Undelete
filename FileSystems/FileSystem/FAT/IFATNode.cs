﻿// Copyright (C) 2017  Joey Scarr
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

namespace KFS.FileSystems.FAT {
	/// <summary>
	/// Represents a node in a FAT filesystem. This interface is used to
	/// calculate whether deleted files have been overwritten.
	/// </summary>
	internal interface IFATNode {
		long FirstCluster { get; }
		DateTime LastModified { get; }
		string Name { get; }
	}
}
