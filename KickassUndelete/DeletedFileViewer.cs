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

using GuiComponents;
using KFS.DataStream;
using KFS.FileSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace KickassUndelete {
	/// <summary>
	/// A custom GUI control for viewing a list of deleted files.
	/// Acts as the View to a Scanner object.
	/// </summary>
	public partial class DeletedFileViewer : UserControl {
		private const string EMPTY_FILTER_TEXT = "Enter filter text here...";
		private Dictionary<FileRecoveryStatus, string> _recoveryDescriptions =
				new Dictionary<FileRecoveryStatus, string>() {
                {FileRecoveryStatus.Unknown,"Unknown"},
                {FileRecoveryStatus.Resident,"Excellent"},
                {FileRecoveryStatus.Recoverable,"Good (no data loss detected)"},
                {FileRecoveryStatus.MaybeOverwritten,"Calculating..."},
                {FileRecoveryStatus.PartiallyOverwritten,"Bad (data partially overwritten)"},
                {FileRecoveryStatus.Overwritten,"Bad (data completely overwritten)"}};
		private Dictionary<FileRecoveryStatus, Color> _recoveryColors =
				new Dictionary<FileRecoveryStatus, Color>() {
                {FileRecoveryStatus.Unknown,Color.FromArgb(255,222,168)}, // Orange
                {FileRecoveryStatus.Resident,Color.FromArgb(190,255,180)}, // Green
                {FileRecoveryStatus.Recoverable,Color.FromArgb(190,255,180)}, // Green
                {FileRecoveryStatus.MaybeOverwritten,Color.FromArgb(255,222,168)}, // Orange
                {FileRecoveryStatus.PartiallyOverwritten,Color.FromArgb(255,222,168)}, // Orange
                {FileRecoveryStatus.Overwritten,Color.FromArgb(255,130,130)}}; // Orange

		private HashSet<string> _systemFileExtensions =
				new HashSet<string>() { ".DLL", ".TMP", ".CAB", ".LNK", ".LOG", ".EXE", ".XML", ".INI" };

		private Scanner _scanner;
		private FileSavingQueue _fileSavingQueue;
		private ProgressPopup _progressPopup;
		private string _mostRecentlySavedFile;
		private string _filter = "";
		private int _numCheckedItems = 0;
		private int _numSelectedItems = 0;
		private bool _matchUnknownFileTypes = false;
		private bool _scanning;

		private List<ListViewItem> _files = new List<ListViewItem>();

		private ListViewColumnSorter _lvwColumnSorter;

		private Dictionary<string, ExtensionInfo> _extensionMap;
		private ImageList _imageList;

		/// <summary>
		/// Constructs a DeletedFileViewer, using a given Scanner.
		/// </summary>
		/// <param name="scanner">The Scanner that will be the model for this DeletedFileViewer.</param>
		public DeletedFileViewer(Scanner scanner) {
			InitializeComponent();

			_lvwColumnSorter = new ListViewColumnSorter();
			fileView.ListViewItemSorter = _lvwColumnSorter;
			_extensionMap = new Dictionary<string, ExtensionInfo>();
			_imageList = new ImageList();
			fileView.SmallImageList = _imageList;

			_scanner = scanner;
			scanner.ProgressUpdated += new EventHandler(state_ProgressUpdated);
			scanner.ScanStarted += new EventHandler(state_ScanStarted);
			scanner.ScanFinished += new EventHandler(state_ScanFinished);

			_fileSavingQueue = new FileSavingQueue();
			_fileSavingQueue.Finished += FileSavingQueue_Finished;
			_progressPopup = new ProgressPopup(_fileSavingQueue);

			UpdateFilterTextBox();
		}

		/// <summary>
		/// Updates the filter textbox to show the "Enter filter text" message.
		/// </summary>
		private void UpdateFilterTextBox() {
			if (tbFilter.Text.Length == 0 || tbFilter.Text == EMPTY_FILTER_TEXT) {
				tbFilter.Text = EMPTY_FILTER_TEXT;
				tbFilter.ForeColor = Color.Gray;
				tbFilter.Font = new Font(tbFilter.Font, FontStyle.Italic);
			} else {
				tbFilter.ForeColor = Color.Black;
				tbFilter.Font = new Font(tbFilter.Font, FontStyle.Regular);
			}
		}

		/// <summary>
		/// Handles a scan starting.
		/// </summary>
		void state_ScanStarted(object sender, EventArgs ea) {
			_scanning = true;
			try {
				this.Invoke(new Action(() => {
					SetScanButtonScanning();
					UpdateTimer.Start();
				}));
			} catch (InvalidOperationException exc) { Console.WriteLine(exc); }
		}

		/// <summary>
		/// Handles a scan finishing.
		/// </summary>
		void state_ScanFinished(object sender, EventArgs ea) {
			try {
				this.Invoke(new Action(() => {
					foreach (ListViewItem item in _files) {
						item.SubItems[4].Text = ((INodeMetadata)item.Tag).GetFileSystemNode().Path;
						item.SubItems[5].Text = _recoveryDescriptions[((INodeMetadata)item.Tag).ChanceOfRecovery];
						item.BackColor = _recoveryColors[((INodeMetadata)item.Tag).ChanceOfRecovery];
					}

					SetScanButtonFinished();
					UpdateTimer.Stop();
					UpdateTimer_Tick(null, null);
					fileView.BeginUpdate();
					fileView.Items.Clear();
					fileView.Items.AddRange(_files.Where(FilterMatches).ToArray());
					fileView.EndUpdate();
				}));
				_scanning = false;
			} catch (InvalidOperationException exc) { Console.WriteLine(exc); }
		}

		/// <summary>
		/// Handles a progress report from the underlying Scanner.
		/// </summary>
		void state_ProgressUpdated(object sender, EventArgs ea) {
			try {
				this.BeginInvoke(new Action(() => {
					SetProgress(_scanner.Progress);
				}));
			} catch (InvalidOperationException exc) { Console.WriteLine(exc); }
		}

		/// <summary>
		/// Disables the scan button and shows the progress bar.
		/// </summary>
		public void SetScanButtonScanning() {
			bScan.Enabled = false;
			bScan.Text = "Scanning...";
			progressBar.Show();
		}

		/// <summary>
		/// Hides the progress bar and sets the scan button to "Finished".
		/// </summary>
		public void SetScanButtonFinished() {
			bScan.Enabled = false;
			bScan.Text = "Finished Scanning!";
			progressBar.Hide();
			bRestoreFiles.Show();
		}

		private void bScan_Click(object sender, EventArgs e) {
			_scanner.StartScan();
		}

		/// <summary>
		/// Constructs a list of ListViewItems based on the files retrieved by the
		/// underlying Scanner.
		/// </summary>
		/// <param name="metadatas">A list of the metadata for each deleted file found.</param>
		/// <returns>An array of ListViewItems.</returns>
		private List<ListViewItem> MakeListItems(IList<INodeMetadata> metadatas) {
			List<ListViewItem> items = new List<ListViewItem>(metadatas.Count);
			for (int i = 0; i < metadatas.Count; i++) {
				ListViewItem item = MakeListItem(metadatas[i]);
				items.Add(item);
			}
			return items;
		}

		/// <summary>
		/// Constructs a ListViewItem from an underlying INodeMetadata model.
		/// </summary>
		/// <param name="metadata">The metadata to create a view for.</param>
		/// <returns>The constructed ListViewItem.</returns>
		private ListViewItem MakeListItem(INodeMetadata metadata) {
			IFileSystemNode node = metadata.GetFileSystemNode();
			string ext = "";
			try {
				ext = Path.GetExtension(metadata.Name);
			} catch (ArgumentException exc) { Console.WriteLine(exc); }
			if (!_extensionMap.ContainsKey(ext)) {
				_extensionMap[ext] = new ExtensionInfo(ext);
			}
			ExtensionInfo extInfo = _extensionMap[ext];
			if (extInfo.Image != null && !extInfo.Image.Size.IsEmpty) {
				if (!_imageList.Images.ContainsKey(ext)) {
					_imageList.Images.Add(ext, extInfo.Image);
				}
			}
			ListViewItem lvi = new ListViewItem(new string[] {
                metadata.Name,
                extInfo.FriendlyName,
                Util.FileSizeToHumanReadableString(node.Size),
                metadata.LastModified.ToString(CultureInfo.CurrentCulture),
								node.Path,
                _recoveryDescriptions[metadata.ChanceOfRecovery]
            });
			lvi.BackColor = _recoveryColors[metadata.ChanceOfRecovery];

			lvi.ImageKey = ext;
			lvi.Tag = metadata;
			return lvi;
		}

		/// <summary>
		/// Sets the progress of the scan progress bar.
		/// </summary>
		/// <param name="progress"></param>
		private void SetProgress(double progress) {
			progressBar.Value = (int)(progress * progressBar.Maximum);
		}

		/// <summary>
		/// Filter the ListView by a filter string.
		/// </summary>
		/// <param name="filter">The string to filter by.</param>
		/// <param name="showUnknownFileTypes">Whether to show unknown file types.</param>
		private void FilterBy(string filter, bool showUnknownFileTypes) {
			string upperFilter = filter.ToUpperInvariant();
			if (_filter != upperFilter
					|| showUnknownFileTypes != _matchUnknownFileTypes) {
				// Check whether the new filter is more restrictive than the old filter.
				// If so, only iterate over the displayed list items and remove the ones that don't match.
				if (upperFilter.StartsWith(_filter) && (showUnknownFileTypes == _matchUnknownFileTypes || !showUnknownFileTypes)) {
					_filter = upperFilter;
					_matchUnknownFileTypes = showUnknownFileTypes;

					fileView.BeginUpdate();
					// THis is premature optimization
					for (int i = 0; i < fileView.Items.Count; i++) {
						if (!FilterMatches(fileView.Items[i])) {
							fileView.Items.RemoveAt(i);
							i--;
						}
					}
					fileView.EndUpdate();
				} else {

					_filter = upperFilter;
					_matchUnknownFileTypes = showUnknownFileTypes;

					fileView.BeginUpdate();
					fileView.Items.Clear();
					fileView.Items.AddRange(_files.Where(FilterMatches).ToArray());
					fileView.EndUpdate();
				}
			}
		}

		/// <summary>
		/// Returns whether the current filter text matches a list view item.
		/// </summary>
		/// <param name="item">The list item to check.</param>
		/// <returns>Whether this list item matches the filter text.</returns>
		private bool FilterMatches(ListViewItem item) {
			return (item.SubItems[0].Text.ToUpperInvariant().Contains(_filter)
							|| item.SubItems[1].Text.ToUpperInvariant().Contains(_filter))
					&& (_matchUnknownFileTypes
							|| !IsSystemOrUnknownFile(item));
		}

		private bool IsSystemOrUnknownFile(ListViewItem item) {
			try {
				string ext = Path.GetExtension(item.SubItems[0].Text);
				return !_extensionMap.ContainsKey(ext)
						|| _extensionMap[ext].UnrecognisedExtension
						|| _systemFileExtensions.Contains(ext.ToUpper());
			} catch (ArgumentException) {
				return true;
			}
		}

		private void UpdateTimer_Tick(object sender, EventArgs e) {
			IList<INodeMetadata> deletedFiles = _scanner.GetDeletedFiles();
			int fileCount = deletedFiles.Count;
			if (fileCount > _files.Count) {
				var items = MakeListItems(deletedFiles.GetRange(_files.Count, fileCount - _files.Count));
				_files.AddRange(items);
				fileView.BeginUpdate();
				fileView.Items.AddRange(items.Where(FilterMatches).ToArray());
				fileView.EndUpdate();
			}
		}

		private void fileView_MouseClick(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Right) {
				if (fileView.SelectedItems.Count == 1) {
					INodeMetadata metadata = fileView.SelectedItems[0].Tag as INodeMetadata;
					if (metadata != null) {
						ContextMenu menu = new ContextMenu();
						MenuItem recoverFile = new MenuItem("Recover File...", new EventHandler(delegate(object o, EventArgs ea) {
							PromptUserToSaveFile(metadata);
						}));
						recoverFile.Enabled = !_scanning;
						menu.MenuItems.Add(recoverFile);
						menu.Show(fileView, e.Location);
					}
				} else if (fileView.SelectedItems.Count > 1) {
					// We need slightly different behaviour to save multiple files.
					ContextMenu menu = new ContextMenu();
					MenuItem recoverFiles = new MenuItem("Recover Files...", new EventHandler(delegate(object o, EventArgs ea) {
						PromptUserToSaveFiles(fileView.SelectedItems);
					}));
					recoverFiles.Enabled = !_scanning;
					menu.MenuItems.Add(recoverFiles);
					menu.Show(fileView, e.Location);
				}
			}
		}

		private void PromptUserToSaveFile(INodeMetadata metadata) {
			if (metadata != null) {
				SaveFileDialog saveFileDialog = new SaveFileDialog();
				saveFileDialog.OverwritePrompt = true;
				saveFileDialog.InitialDirectory = Environment.ExpandEnvironmentVariables("%SystemDrive");
				saveFileDialog.FileName = metadata.Name;
				saveFileDialog.Filter = "Any Files|*.*";
				saveFileDialog.Title = "Select a Location";

				if (saveFileDialog.ShowDialog() == DialogResult.OK) {
					// Check that the drive isn't the same as the drive being copied from.
					if (saveFileDialog.FileName[0] != _scanner.DiskName[0]
						|| MessageBox.Show("WARNING: You are about to save this file to the same disk you are " +
						"trying to recover from. This may cause recovery to fail, and overwrite your data " +
						"permanently! Are you sure you wish to continue?", "Warning!",
						MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {

						IFileSystemNode node = metadata.GetFileSystemNode();
						SaveSingleFile(node, saveFileDialog.FileName);
					}
				}
			}
		}

		/// <summary>
		/// Recovers a single file to the specified filepath.
		/// </summary>
		/// <param name="node">The file to recover.</param>
		/// <param name="filePath">The path to save the file to.</param>
		private void SaveSingleFile(IFileSystemNode node, string filePath) {
			_mostRecentlySavedFile = filePath;
			if (!_progressPopup.Visible) {
				_progressPopup.Show(this);
			}
			_fileSavingQueue.Push(filePath, node);
		}

		private void PromptUserToSaveFiles(IEnumerable items) {
			FolderBrowserDialog folderDialog = new FolderBrowserDialog();

			if (folderDialog.ShowDialog() == DialogResult.OK) {
				// Check that the drive isn't the same as the drive being copied from.
				if (folderDialog.SelectedPath[0] != _scanner.DiskName[0]
					|| MessageBox.Show("WARNING: You are about to save this file to the same disk you are " +
					"trying to recover from. This may cause recovery to fail, and overwrite your data " +
					"permanently! Are you sure you wish to continue?", "Warning!",
					MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {

					List<IFileSystemNode> nodes = new List<IFileSystemNode>();
					foreach (ListViewItem item in items) {
						INodeMetadata metadata = item.Tag as INodeMetadata;
						if (metadata != null) {
							nodes.Add(metadata.GetFileSystemNode());
						}
					}
					SaveMultipleFiles(nodes, folderDialog.SelectedPath);
				}
			}
		}

		/// <summary>
		/// Recovers multiple files into the specified folder.
		/// </summary>
		/// <param name="nodes">The files to recover.</param>
		/// <param name="folderPath">The folder in which to save the recovered files.</param>
		private void SaveMultipleFiles(IEnumerable<IFileSystemNode> nodes, string folderPath) {
			foreach (IFileSystemNode node in nodes) {
				var reg = new Regex("[A-Z]:\\\\");
				var path = reg.Replace(node.Path, "");
				string file = PathUtils.MakeFileNameValid(path);
				string fileName = Path.Combine(folderPath, file);
				if (System.IO.File.Exists(fileName)) {
					int copyNum = 1;
					string newFileName;
					do {
						newFileName = Path.Combine(Path.GetDirectoryName(fileName),
							string.Format("{0} ({1}){2}", Path.GetFileNameWithoutExtension(fileName),
							copyNum, Path.GetExtension(fileName)));
						copyNum++;
					} while (System.IO.File.Exists(newFileName));
					fileName = newFileName;
				}
				SaveSingleFile(node, fileName);
			}
		}

		private void FileSavingQueue_Finished() {
			if (!string.IsNullOrEmpty(_mostRecentlySavedFile)) {
				Process.Start("explorer.exe", "/select, \"" + _mostRecentlySavedFile + '"');
			}
		}

		private void fileView_ColumnClick(object sender, ColumnClickEventArgs e) {
			// Determine if clicked column is already the column that is being sorted.
			if (e.Column == _lvwColumnSorter.SortColumn) {
				// Reverse the current sort direction for this column.
				if (_lvwColumnSorter.Order == SortOrder.Ascending) {
					_lvwColumnSorter.Order = SortOrder.Descending;
				} else {
					_lvwColumnSorter.Order = SortOrder.Ascending;
				}
			} else {
				// Set the column number that is to be sorted; default to ascending.
				_lvwColumnSorter.SortColumn = e.Column;
				_lvwColumnSorter.Order = SortOrder.Ascending;
			}

			// Perform the sort with these new sort options.
			this.fileView.Sort();
		}

		private void tbFilter_Enter(object sender, EventArgs e) {
			if (tbFilter.Text.Length == 0 || tbFilter.Text == EMPTY_FILTER_TEXT) {
				tbFilter.Text = "";
				tbFilter.ForeColor = Color.Black;
				tbFilter.Font = new Font(tbFilter.Font, FontStyle.Regular);
			}
		}

		private void tbFilter_Leave(object sender, EventArgs e) {
			UpdateFilterTextBox();
		}

		private void Filter() {
			if (tbFilter.Text.Length > 0 && tbFilter.Text != EMPTY_FILTER_TEXT) {
				FilterBy(tbFilter.Text, cbShowUnknownFiles.Checked);
			} else {
				FilterBy("", cbShowUnknownFiles.Checked);
			}
		}

		private void tbFilter_TextChanged(object sender, EventArgs e) {
			Filter();
		}

		private void cbShowUnknownFiles_CheckedChanged(object sender, EventArgs e) {
			Filter();
		}

		private void bRestoreFiles_Click(object sender, EventArgs e) {
			if (_numCheckedItems == 1) {
				PromptUserToSaveFile(fileView.CheckedItems[0].Tag as INodeMetadata);
			} else if (_numCheckedItems > 1) {
				PromptUserToSaveFiles(fileView.CheckedItems);
			} else if (_numSelectedItems == 1) {
				PromptUserToSaveFile(fileView.SelectedItems[0].Tag as INodeMetadata);
			} else if (_numSelectedItems > 1) {
				PromptUserToSaveFiles(fileView.SelectedItems);
			}
		}

		/// <summary>
		/// Sets the restore button to be enabled if there are list items checked.
		/// </summary>
		private void UpdateRestoreButton() {
			if (!_scanning) {
				bRestoreFiles.Enabled = _numCheckedItems > 0 || _numSelectedItems > 0;
			}
		}

		private void fileView_ItemCheck(object sender, ItemCheckEventArgs e) {
			// Update the number of checked items
			_numCheckedItems += e.NewValue == CheckState.Checked ? 1 : -1;
			UpdateRestoreButton();
		}

		private void fileView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e) {
			_numSelectedItems += e.IsSelected ? 1 : -1;
			UpdateRestoreButton();
		}
	}
}
