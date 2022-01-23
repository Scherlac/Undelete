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

namespace KFS.FileSystems {
	public delegate void SearchFunction(FileSystem.NodeVisitCallback callback, string searchPath);

	/// <summary>
	/// A generic search strategy that uses a search function passed into the
	/// constructor.
	/// </summary>
	public class SearchStrategy : ISearchStrategy {
		private SearchFunction _func;

		public SearchStrategy(string name, SearchFunction func) {
			Name = name;
			_func = func;
		}

		public string Name { get; set; }

		public void Search(FileSystem.NodeVisitCallback callback, string searchPath) {
			_func(callback, searchPath);
		}

		public void Search(FileSystem.NodeVisitCallback callback) {
			_func(callback, null);
		}
	}
}
