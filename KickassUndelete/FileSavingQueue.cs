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

using GuiComponents;
using KFS.FileSystems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace KickassUndelete {
	public class FileSavingQueue : IProgressable {
		private bool _saving = false;
		private Thread _processingThread;
		private Queue<KeyValuePair<string, IFileSystemNode>> _queue = new Queue<KeyValuePair<string, IFileSystemNode>>();

		public FileSavingQueue() { }

		public void Push(string filepath, IFileSystemNode fileNode) {
			lock (_queue) {
				_queue.Enqueue(new KeyValuePair<string, IFileSystemNode>(filepath, fileNode));
				if (!_saving) {
					_saving = true;
					_processingThread = new Thread(delegate() {
						int remaining = 0;
						lock (_queue) {
							remaining = _queue.Count;
						}
						while (remaining > 0) {
							KeyValuePair<string, IFileSystemNode> nextFile;
							lock (_queue) {
								nextFile = _queue.Dequeue();
							}
							var filePath = nextFile.Key;
							var node = nextFile.Value;
							WriteFileToDisk(filePath, node);
							lock (_queue) {
								remaining = _queue.Count;
								if (remaining == 0) {
									_saving = false;
									OnFinished();
									return;
								}
							}
						}
					});
					_processingThread.Start();
				}
			}
		}

		private static bool ValidateFilename(string fileName)
		{
			System.IO.FileInfo fi = null;
			try
			{
				fi = new System.IO.FileInfo(fileName);
			}
			catch (ArgumentException) { }
			catch (System.IO.PathTooLongException) { }
			catch (NotSupportedException) { }

			if (ReferenceEquals(fi, null))
			{
				return false;
			}
			else
			{
				System.IO.DirectoryInfo di = null;
				try
				{
					di = Directory.CreateDirectory(fi.DirectoryName);
				}
				catch (Exception)
				{
				}

				if (ReferenceEquals(di, null))
				{
					return false;
				}
				else
				{
					return true;

				}

			}

		}

		private void WriteFileToDisk(string filePath, IFileSystemNode node) {

			ValidateFilename(filePath);

			using (BinaryWriter bw = new BinaryWriter(new FileStream(filePath, FileMode.Create))) {
				ulong BLOCK_SIZE = 1024 * 1024; // 1MB
				ulong offset = 0;
				while (offset < node.StreamLength) {
					if (offset + BLOCK_SIZE < node.StreamLength) {
						bw.Write(node.GetBytes(offset, BLOCK_SIZE));
					} else {
						bw.Write(node.GetBytes(offset, node.StreamLength - offset));
					}
					offset += BLOCK_SIZE;

					// Notify the progress listeners that bytes have been saved to disk.
					string filename = Path.GetFileName(filePath);
					double progress = Math.Min(1, (double)offset / (double)node.StreamLength);
					OnProgress(string.Concat("Recovering ", filename, "..."), progress);
				}
			}
		}

		private void OnProgress(string status, double progress) {
			if (Progress != null) {
				Progress(status, progress);
			}
		}
		public event ProgressEvent Progress;

		private void OnFinished() {
			if (Finished != null) {
				Finished();
			}
		}
		public event Action Finished;
	}
}
