// Copyright (C) 2017  Joey Scarr, Josh Oosterman
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

namespace KFA.GUI.Explorers {
	/// <summary>
	/// The interface for an explorer widget.
	/// </summary>
	public interface IExplorer {

		/// <summary>
		/// Gets whether this explorer can view a particular stream.
		/// </summary>
		/// <param name="stream">The stream to view.</param>
		/// <returns>Whether the stream can be viewed by this explorer.</returns>
		bool CanView(IDataStream stream);

		/// <summary>
		/// Views a data stream. Clients should check that CanView(stream)
		/// returns true before calling this method.
		/// </summary>
		/// <param name="stream">The data stream to view.</param>
		void View(IDataStream stream);
	}
}
