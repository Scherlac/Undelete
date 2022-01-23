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
using System;

namespace KFS.Disks {
	/// <summary>
	/// Represents a disk. Caches the most recent block read.
	/// </summary>
	public abstract class Disk : IDataStream {
		#region IDataStream Members

		private object _padlock = new object();
		private const ulong CACHE_LINE_SIZE = 64 * 1024; // Read 64KB at a time from disk
		private ulong _currentCacheLine = ulong.MaxValue;
		private byte[] _cache = new byte[CACHE_LINE_SIZE];
		public byte[] GetBytes(ulong offset, ulong length) {
			byte[] result = new byte[length];
			if (length > 0) {
				ulong bytes_read = 0;
				while (bytes_read < length) {
					ulong cache_line = offset / CACHE_LINE_SIZE;
					lock (_padlock) {
						if (_currentCacheLine != cache_line) {
							LoadCacheLine(cache_line);
						}
						ulong offset_in_cache_line = offset % CACHE_LINE_SIZE;
						ulong num_to_read = Math.Min(CACHE_LINE_SIZE - offset_in_cache_line, length - bytes_read);
						Array.Copy(_cache, (int)offset_in_cache_line, result, (int)bytes_read, (int)num_to_read);
						bytes_read += num_to_read;
						offset += num_to_read;
					}
				}
			}
			return result;
		}

		protected void LoadCacheLine(ulong cache_line) {
			_currentCacheLine = cache_line;
			ForceReadBytes(_cache, cache_line * CACHE_LINE_SIZE, CACHE_LINE_SIZE);
		}

		protected abstract void ForceReadBytes(byte[] buffer, ulong offset, ulong length);

		public abstract ulong StreamLength {
			get;
		}

		public ulong DeviceOffset {
			get { return 0; }
		}

		public abstract string StreamName {
			get;
		}

		public IDataStream ParentStream {
			get { return null; }
		}

		public void Open() { }

		public void Close() { }

		#endregion
	}
}
