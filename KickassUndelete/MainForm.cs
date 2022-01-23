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

using KFS.Disks;
using KFS.FileSystems;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace KickassUndelete {
	/// <summary>
	/// The main form of Kickass Undelete.
	/// </summary>
	public partial class MainForm : Form {
		IFileSystem _fileSystem;
		Dictionary<IFileSystem, Scanner> _scanners = new Dictionary<IFileSystem, Scanner>();
		Dictionary<IFileSystem, DeletedFileViewer> _deletedViewers = new Dictionary<IFileSystem, DeletedFileViewer>();

		/// <summary>
		/// Constructs the main form.
		/// </summary>
		public MainForm() {
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs e) {
			LoadLogicalDisks();
		}

		private void LoadLogicalDisks() {
			foreach (Disk disk in DiskLoader.LoadLogicalVolumes()) {
				TreeNode node = new TreeNode(disk.ToString());
				Console.WriteLine("Added disk: " + disk.ToString());
				node.Tag = disk;
				node.ImageKey = "HDD";
				if (((IFileSystemStore)disk).FS == null) {
					node.ForeColor = Color.Gray;
				}
				diskTree.Nodes.Add(node);
			}
		}

		private void diskTree_AfterSelect(object sender, TreeViewEventArgs e) {
			SetFileSystem((IFileSystemStore)e.Node.Tag);
		}

		private void SetFileSystem(IFileSystemStore logicalDisk) {
			if (logicalDisk.FS != null) {
				if (!_scanners.ContainsKey(logicalDisk.FS)) {
					_scanners[logicalDisk.FS] = new Scanner(logicalDisk.ToString(), logicalDisk.FS);
					_deletedViewers[logicalDisk.FS] = new DeletedFileViewer(_scanners[logicalDisk.FS]);
					AddDeletedFileViewer(_deletedViewers[logicalDisk.FS]);
				}
				if (_fileSystem != null && _scanners.ContainsKey(_fileSystem)) {
					_deletedViewers[_fileSystem].Hide();
				}
				_fileSystem = logicalDisk.FS;
				_deletedViewers[logicalDisk.FS].Show();
			}
		}

		private void AddDeletedFileViewer(DeletedFileViewer viewer) {
			int MARGIN = 12;
			splitContainer1.Panel2.Controls.Add(viewer);
			viewer.Top = viewer.Left = MARGIN;
			viewer.Width = splitContainer1.Panel2.Width - MARGIN * 2;
			viewer.Height = splitContainer1.Panel2.Height - MARGIN * 2;
			viewer.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
		}

		private void MainForm_FormClosed(object sender, FormClosedEventArgs e) {
			foreach (Scanner state in _scanners.Values) {
				state.CancelScan();
			}
		}

		private void diskTree_BeforeSelect(object sender, TreeViewCancelEventArgs e) {
			if (((IFileSystemStore)e.Node.Tag).FS == null) {
				e.Cancel = true;
			}
		}
	}
}
