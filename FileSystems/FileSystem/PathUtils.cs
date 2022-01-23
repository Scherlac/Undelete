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

using System.IO;
using System.Text;

namespace KFS.FileSystems {
	public class PathUtils {
		/// <summary>
		/// Combines a list of paths or directory names into a path. Similar to Path.Combine,
		/// but deals with the case when the first component is a drive letter with
		/// colon, e.g. 'C:'. Note that this method may throw an exception if there are invalid
		/// filename chars in the path list.
		/// </summary>
		public static string Combine(params string[] paths) {
			// If the first path is a drive letter and colon (e.g. "C:"), append a
			// separator before calling Path.Combine.
			if (paths[0].Length == 2 && paths[0][1] == ':') {
				paths[0] += Path.DirectorySeparatorChar;
			}
			return Path.Combine(paths);
		}

		/// <summary>
		/// Sanitizes a given filename by replacing invalid chars with '_'.
		/// This method assumes the input is a filename only, not a path.
		/// </summary>
		public static string MakeFileNameValid(string filename) {
			StringBuilder sb = new StringBuilder(filename);
			foreach (char c in Path.GetInvalidPathChars()) {
				sb.Replace(c, '_');
			}
			return sb.ToString();
		}
	}
}
