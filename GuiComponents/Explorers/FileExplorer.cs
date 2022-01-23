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
using KFS.Disks;
using KFS.FileSystems;
using KFS.FileSystems.NTFS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using File = KFS.FileSystems.File;

namespace KFA.GUI.Explorers {
	/// <summary>
	/// Builds a treeview of the filesystem in a given IFileSystemStore.
	/// </summary>
	public partial class FileExplorer : UserControl, IExplorer {
		private IDataStream _currentStream;

		/// <summary>
		/// Constructs the file explorer.
		/// </summary>
		public FileExplorer() {
			InitializeComponent();
			treeFiles.ImageList = imageList1;
			treeFiles.SelectedImageKey = null;
		}

		/// <summary>
		/// Creates TreeNodes for a list of given FileSystemNodes and appends them to
		/// the specified TreeNode as children.
		/// </summary>
		/// <param name="node">The node to which the new children will be appended.</param>
		/// <param name="children">The filesystem nodes to create TreeNodes for.</param>
		private static void AppendChildren(TreeNode node, IEnumerable<IFileSystemNode> children) {
			node.Nodes.Clear();
			foreach (IFileSystemNode child in children) {
				TreeNode treeNode = new TreeNode(child.ToString());
				treeNode.Tag = child;
				if (child.Deleted) {
					treeNode.ImageKey = "Deleted";
					treeNode.ForeColor = Color.Red;
				} else if (child is File || child is HiddenDataStreamFileNTFS) {
					treeNode.ImageKey = "File";
				} else {
					treeNode.ImageKey = "Directory";
				}

				if (child is Folder || (child is File && ((File)child).IsZip)) {
					treeNode.Nodes.Add(new TreeNode("dummy"));
					child.Loaded = false;
				}
				treeNode.SelectedImageKey = treeNode.ImageKey;
				node.Nodes.Add(treeNode);
			}
		}

		private void treeFiles_BeforeExpand(object sender, TreeViewCancelEventArgs e) {
			IFileSystemNode fsNode = e.Node.Tag as IFileSystemNode;
			if (fsNode != null && !fsNode.Loaded) {
				AppendChildren(e.Node, fsNode.GetChildren());
				fsNode.Loaded = true; // TODO: Set this in GetChildren, then TEST
			}
		}

		private void treeFiles_AfterSelect(object sender, TreeViewEventArgs e) {
			IDescribable fsNode = e.Node.Tag as IDescribable;
			if (fsNode != null) {
				tbDescription.Text = fsNode.TextDescription;
			}
			IDataStream stream = e.Node.Tag as IDataStream;
			if (stream != null) {
				OnStreamSelected(stream);
			}
		}

		#region IExplorer Members

		/// <summary>
		/// Gets whether this explorer can view a particular stream.
		/// </summary>
		/// <param name="stream">The stream to view.</param>
		/// <returns>Whether the stream can be viewed by this explorer.</returns>
		public bool CanView(IDataStream stream) {
			return stream is IFileSystemStore;
		}

		/// <summary>
		/// Views a data stream. Clients should check that CanView(stream)
		/// returns true before calling this method.
		/// </summary>
		/// <param name="stream">The data stream to view.</param>
		public void View(IDataStream stream) {
			if (stream != _currentStream) {
				_currentStream = stream;
				treeFiles.Nodes.Clear();
				IFileSystemStore store = stream as IFileSystemStore;
				if (store != null) {
					IFileSystem fs = store.FS;
					FileSystemNode fsRoot;
					if (fs != null) {
						fsRoot = fs.GetRoot();
						TreeNode root = new TreeNode(fsRoot.ToString());
						root.Tag = fsRoot;
						root.SelectedImageKey = root.ImageKey = "Directory";
						treeFiles.Nodes.Add(root);
						root.Nodes.Add(new TreeNode("dummy"));
						((Folder)fsRoot).Loaded = false;
					} else {
						treeFiles.Nodes.Add("Unknown file system");
					}
				}
			}
		}

		#endregion

		private void treeFiles_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) {
			if (e.Button == MouseButtons.Right) {
				ShowContextMenu(e);
			}
		}

		private void ShowContextMenu(TreeNodeMouseClickEventArgs e) {
			FileSystemNode fsnode = e.Node.Tag as FileSystemNode;
			if (fsnode != null) {
				ContextMenu menu = new ContextMenu();
				if (fsnode is Folder) {
					Folder f = fsnode as Folder;
					menu.MenuItems.Add(new MenuItem("Refresh", new EventHandler(delegate(object o, EventArgs ea) {
						fsnode.ReloadChildren();
						AppendChildren(e.Node, fsnode.GetChildren());
					})));
					menu.MenuItems.Add(new MenuItem("Save Folder...", new EventHandler(delegate(object o, EventArgs ea) {
						SaveFileDialog saveFileDialog = new SaveFileDialog();
						saveFileDialog.Filter = "Any Files|*.*";
						saveFileDialog.Title = "Select a Location";
						saveFileDialog.FileName = f.Name;
						saveFileDialog.OverwritePrompt = true;

						if (saveFileDialog.ShowDialog() == DialogResult.OK) {
							SaveFolder(f, saveFileDialog.FileName);
						}
					})));
				}
				if (fsnode is File) {
					File f = fsnode as File;
					menu.MenuItems.Add(new MenuItem("Save File...", new EventHandler(delegate(object o, EventArgs ea) {
						SaveFileDialog saveFileDialog = new SaveFileDialog();
						saveFileDialog.Filter = "Any Files|*.*";
						saveFileDialog.Title = "Select a Location";
						saveFileDialog.FileName = f.Name;
						saveFileDialog.OverwritePrompt = true;

						if (saveFileDialog.ShowDialog() == DialogResult.OK) {
							SaveFile(f, saveFileDialog.FileName);
						}
					})));
				}
				menu.Show(e.Node.TreeView, e.Location);
			}
		}

		/// <summary>
		/// Saves a folder full of recovered files to the specified path.
		/// </summary>
		/// <param name="folder">The folder to save.</param>
		/// <param name="path">The path to save the folder to.</param>
		private void SaveFolder(Folder folder, string path) {
			Directory.CreateDirectory(path);
			foreach (FileSystemNode node in folder.GetChildren()) {
				if (node is File || node is HiddenDataStreamFileNTFS) {
					SaveFile(node, Path.Combine(path, node.Name));
				} else {
					Folder folderNode = node as Folder;
					if (folderNode != null) {
						SaveFolder(folderNode, Path.Combine(path, node.Name));
					}
				}
			}
		}

		/// <summary>
		/// Saves a single recovered file to disk.
		/// </summary>
		/// <param name="file">The file to save.</param>
		/// <param name="filePath">The path to save the file to.</param>
		private static void SaveFile(FileSystemNode file, string filePath) {
			if (!System.IO.File.Exists(filePath)) {
				using (ForensicsAppStream fas = new ForensicsAppStream(file)) {
					using (Stream output = new FileStream(filePath, FileMode.Create)) {
						byte[] buffer = new byte[32 * 1024];
						int read;

						while ((read = fas.Read(buffer, 0, buffer.Length)) > 0) {
							output.Write(buffer, 0, read);
						}
					}
				}
			}
		}

		/// <summary>
		/// This event is fired when an IDataStream is selected in this explorer by the user.
		/// </summary>
		public event StreamSelectedEventHandler StreamSelected;

		private void OnStreamSelected(IDataStream stream) {
			if (StreamSelected != null) {
				StreamSelected(this, stream);
			}
		}
	}

	/// <summary>
	/// An event handler to be used when a stream is selected.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="stream"></param>
	public delegate void StreamSelectedEventHandler(object sender, IDataStream stream);
}
