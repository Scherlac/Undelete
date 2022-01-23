// Copyright (C) 2017  Joey Scarr, Lukas Korsika
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

using KFS.FileSystems;
using KFS.FileSystems.NTFS;
using MB.Algodat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace KickassUndelete {
	/// <summary>
	/// Encapsulates the state of a scan for deleted files.
	/// </summary>
	public class Scanner {
		private List<INodeMetadata> _deletedFiles = new List<INodeMetadata>();
		private double _progress;
		private DateTime _startTime;
		private Thread _thread;
		private bool _scanCancelled;
		private IFileSystem _fileSystem;
		private string _diskName;

		/// <summary>
		/// Constructs a Scanner on the specified filesystem.
		/// </summary>
		/// <param name="fileSystem">The filesystem to scan.</param>
		public Scanner(string diskName, IFileSystem fileSystem) {
			_fileSystem = fileSystem;
			_diskName = diskName;
		}

		/// <summary>
		/// Gets the deleted files found by the scan.
		/// </summary>
		public IList<INodeMetadata> GetDeletedFiles() {
			lock (_deletedFiles) {
				return new List<INodeMetadata>(_deletedFiles);
			}
		}

		/// <summary>
		/// The human-readable device label, e.g. "C: Local Disk (NTFS)".
		/// </summary>
		public string DiskName {
			get { return _diskName; }
		}

		/// <summary>
		/// The Device ID, e.g. "C:".
		/// </summary>
		public string DeviceID {
			get { return _fileSystem.Store.DeviceID; }
		}

		/// <summary>
		/// Gets the current progress of the scan (between 0 and 1).
		/// </summary>
		public double Progress {
			get { return _progress; }
		}

		/// <summary>
		/// Starts a scan on the filesystem.
		/// </summary>
		public void StartScan() {
			_scanCancelled = false;
			_thread = new Thread(Run);
			_thread.Start();
		}

		/// <summary>
		/// Cancels the currently running scan.
		/// </summary>
		public void CancelScan() {
			_scanCancelled = true;
		}

		/// <summary>
		/// Runs a scan.
		/// </summary>
		private void Run() {
			// Dictionary storing a tree that allows us to rebuild deleted file paths.
			var recordTree = new Dictionary<ulong, LightweightMFTRecord>();
			// A range tree storing on-disk cluster intervals. Allows us to tell whether files are overwritten.
			var runIndex = new RangeTree<ulong, RangeItem>(new RangeItemComparer());

			ulong numFiles;

			OnScanStarted();
			_progress = 0;
			OnProgressUpdated();

			// TODO: Replace me with a search strategy selected from a text box!
			ISearchStrategy strat = _fileSystem.GetDefaultSearchStrategy();

			if (_fileSystem is FileSystemNTFS) {
				var ntfsFS = _fileSystem as FileSystemNTFS;
				numFiles = ntfsFS.MFT.StreamLength / (ulong)(ntfsFS.SectorsPerMFTRecord * ntfsFS.BytesPerSector);
			}

			Console.WriteLine("Beginning scan...");
			_startTime = DateTime.Now;

			strat.Search(new FileSystem.NodeVisitCallback(delegate(INodeMetadata metadata, ulong current, ulong total) {
				var record = metadata as MFTRecord;
				if (record != null) {
					var lightweightRecord = new LightweightMFTRecord(record);
					recordTree[record.RecordNum] = lightweightRecord;

					foreach (IRun run in record.Runs) {
						runIndex.Add(new RangeItem(run, lightweightRecord));
					}
				}

				if (metadata != null && metadata.Deleted && metadata.Name != null
						&& !metadata.Name.EndsWith(".manifest", StringComparison.OrdinalIgnoreCase)
						&& !metadata.Name.EndsWith(".cat", StringComparison.OrdinalIgnoreCase)
						&& !metadata.Name.EndsWith(".mum", StringComparison.OrdinalIgnoreCase)) {
					IFileSystemNode node = metadata.GetFileSystemNode();
					if (node.Type == FSNodeType.File && node.Size > 0) {
						lock (_deletedFiles) {
							_deletedFiles.Add(metadata);
						}
					}
				}

				if (current % 100 == 0) {
					_progress = (double)current / (double)total;
					OnProgressUpdated();
				}
				return !_scanCancelled;
			}));

			if (_fileSystem is FileSystemNTFS) {
				List<INodeMetadata> fileList;
				lock (_deletedFiles) {
					fileList = _deletedFiles;
				}
				foreach (var file in fileList) {
					var record = file as MFTRecord;
					var node = file.GetFileSystemNode();
					node.Path = PathUtils.Combine(GetPathForRecord(recordTree, record.ParentDirectory), node.Name);
					if (record.ChanceOfRecovery == FileRecoveryStatus.MaybeOverwritten) {
						record.ChanceOfRecovery = FileRecoveryStatus.Recoverable;
						// Query all the runs for this node.
						foreach (IRun run in record.Runs) {
							List<RangeItem> overlapping = runIndex.Query(new Range<ulong>(run.LCN, run.LCN + run.LengthInClusters - 1));

							if (overlapping.Count(x => x.Record.RecordNumber != record.RecordNum) > 0) {
								record.ChanceOfRecovery = FileRecoveryStatus.PartiallyOverwritten;
								break;
							}
						}
					}
				}
			}

			runIndex.Clear();
			recordTree.Clear();
			GC.Collect();

			TimeSpan timeTaken = DateTime.Now - _startTime;
			if (!_scanCancelled) {
				Console.WriteLine("Scan complete! Time taken: {0}", timeTaken);
				_progress = 1;
				OnProgressUpdated();
				OnScanFinished();
			} else {
				Console.WriteLine("Scan cancelled! Time taken: {0}", timeTaken);
			}
		}

		private string GetPathForRecord(Dictionary<ulong, LightweightMFTRecord> recordTree, ulong recordNum) {
			if (recordNum == 0 || !recordTree.ContainsKey(recordNum)
					|| recordTree[recordNum].ParentRecord == recordNum) {
				// This is the root record
				return DeviceID;
			} else {
				var record = recordTree[recordNum];
				if (record.Path == null) {
					if (!record.IsDirectory || record.FileName == null) {
						// This isn't a directory, or we can't read the directory name, so the path must have been broken.
						return PathUtils.Combine(DeviceID, "?");
					} else {
						var fileName = PathUtils.MakeFileNameValid(record.FileName);
						record.Path = PathUtils.Combine(GetPathForRecord(recordTree, record.ParentRecord), fileName);
					}
				}
				return record.Path;
			}
		}

		/// <summary>
		/// This event fires repeatedly as the scan progresses.
		/// </summary>
		public event EventHandler ProgressUpdated;
		private void OnProgressUpdated() {
			if (ProgressUpdated != null) {
				ProgressUpdated(this, null);
			}
		}

		/// <summary>
		/// This event fires when the scan is started.
		/// </summary>
		public event EventHandler ScanStarted;
		private void OnScanStarted() {
			if (ScanStarted != null) {
				ScanStarted(this, null);
			}
		}

		/// <summary>
		/// This event fires when the scan finishes.
		/// </summary>
		public event EventHandler ScanFinished;
		private void OnScanFinished() {
			if (ScanFinished != null) {
				ScanFinished(this, null);
			}
		}
	}
}
