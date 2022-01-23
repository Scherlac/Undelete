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

namespace KFA.GUI.Explorers {
	partial class FileExplorer {
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileExplorer));
			this.tbDescription = new System.Windows.Forms.TextBox();
			this.treeFiles = new System.Windows.Forms.TreeView();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tbDescription
			// 
			this.tbDescription.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tbDescription.Location = new System.Drawing.Point(0, 0);
			this.tbDescription.Multiline = true;
			this.tbDescription.Name = "tbDescription";
			this.tbDescription.ReadOnly = true;
			this.tbDescription.Size = new System.Drawing.Size(210, 479);
			this.tbDescription.TabIndex = 3;
			// 
			// treeFiles
			// 
			this.treeFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeFiles.HideSelection = false;
			this.treeFiles.Location = new System.Drawing.Point(0, 0);
			this.treeFiles.Name = "treeFiles";
			this.treeFiles.Size = new System.Drawing.Size(442, 479);
			this.treeFiles.TabIndex = 2;
			this.treeFiles.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeFiles_BeforeExpand);
			this.treeFiles.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeFiles_AfterSelect);
			this.treeFiles.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeFiles_NodeMouseClick);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.treeFiles);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.tbDescription);
			this.splitContainer1.Size = new System.Drawing.Size(656, 479);
			this.splitContainer1.SplitterDistance = 442;
			this.splitContainer1.TabIndex = 4;
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "DeletedFile");
			this.imageList1.Images.SetKeyName(1, "File");
			this.imageList1.Images.SetKeyName(2, "Directory");
			// 
			// FileExplorer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
			this.Name = "FileExplorer";
			this.Size = new System.Drawing.Size(656, 479);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TextBox tbDescription;
		private System.Windows.Forms.TreeView treeFiles;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ImageList imageList1;
	}
}
