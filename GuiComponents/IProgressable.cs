// Copyright (C) 2017  Joey Scarr
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

namespace GuiComponents {
	/// <summary>
	/// An object that reports its progress in completing some action.
	/// </summary>
	public interface IProgressable {
		/// <summary>
		/// Fires whenever a progress update is made.
		/// </summary>
		event ProgressEvent Progress;
		/// <summary>
		/// Fires when the process is complete.
		/// </summary>
		event Action Finished;
	}

	/// <summary>
	/// A type of event that occurs when progress has been made.
	/// </summary>
	/// <param name="status">A text description of the current progress.</param>
	/// <param name="progress">
	///		A number from 0.0 to 1.0 representing the current progress.
	///	</param>
	public delegate void ProgressEvent(string status, double progress);
}
