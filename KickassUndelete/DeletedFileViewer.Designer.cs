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

namespace KickassUndelete {
	partial class DeletedFileViewer {
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
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.bScan = new System.Windows.Forms.Button();
			this.UpdateTimer = new System.Windows.Forms.Timer(this.components);
			this.tbFilter = new System.Windows.Forms.TextBox();
			this.bRestoreFiles = new System.Windows.Forms.Button();
			this.cbShowUnknownFiles = new System.Windows.Forms.CheckBox();
			this.fileView = new KickassUndelete.ListViewNoFlicker();
			this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colModified = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colRecovery = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.SuspendLayout();
			// 
			// progressBar
			// 
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar.Location = new System.Drawing.Point(0, 586);
			this.progressBar.Maximum = 10000;
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(702, 34);
			this.progressBar.TabIndex = 5;
			// 
			// bScan
			// 
			this.bScan.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.bScan.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.bScan.Location = new System.Drawing.Point(0, 0);
			this.bScan.Name = "bScan";
			this.bScan.Size = new System.Drawing.Size(702, 90);
			this.bScan.TabIndex = 3;
			this.bScan.Text = "Scan";
			this.bScan.UseVisualStyleBackColor = true;
			this.bScan.Click += new System.EventHandler(this.bScan_Click);
			// 
			// UpdateTimer
			// 
			this.UpdateTimer.Interval = 200;
			this.UpdateTimer.Tick += new System.EventHandler(this.UpdateTimer_Tick);
			// 
			// tbFilter
			// 
			this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbFilter.Location = new System.Drawing.Point(0, 100);
			this.tbFilter.Name = "tbFilter";
			this.tbFilter.Size = new System.Drawing.Size(475, 20);
			this.tbFilter.TabIndex = 6;
			this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
			this.tbFilter.Enter += new System.EventHandler(this.tbFilter_Enter);
			this.tbFilter.Leave += new System.EventHandler(this.tbFilter_Leave);
			// 
			// bRestoreFiles
			// 
			this.bRestoreFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.bRestoreFiles.Enabled = false;
			this.bRestoreFiles.Location = new System.Drawing.Point(491, 586);
			this.bRestoreFiles.Name = "bRestoreFiles";
			this.bRestoreFiles.Size = new System.Drawing.Size(211, 34);
			this.bRestoreFiles.TabIndex = 5;
			this.bRestoreFiles.Text = "Restore Files...";
			this.bRestoreFiles.UseVisualStyleBackColor = true;
			this.bRestoreFiles.Visible = false;
			this.bRestoreFiles.Click += new System.EventHandler(this.bRestoreFiles_Click);
			// 
			// cbShowUnknownFiles
			// 
			this.cbShowUnknownFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cbShowUnknownFiles.AutoSize = true;
			this.cbShowUnknownFiles.Location = new System.Drawing.Point(481, 102);
			this.cbShowUnknownFiles.Name = "cbShowUnknownFiles";
			this.cbShowUnknownFiles.Size = new System.Drawing.Size(221, 17);
			this.cbShowUnknownFiles.TabIndex = 7;
			this.cbShowUnknownFiles.Text = "Show system files and unknown file types";
			this.cbShowUnknownFiles.UseVisualStyleBackColor = true;
			this.cbShowUnknownFiles.CheckedChanged += new System.EventHandler(this.cbShowUnknownFiles_CheckedChanged);
			// 
			// fileView
			// 
			this.fileView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.fileView.CheckBoxes = true;
			this.fileView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colType,
            this.colSize,
            this.colModified,
            this.colPath,
            this.colRecovery});
			this.fileView.Location = new System.Drawing.Point(0, 124);
			this.fileView.Margin = new System.Windows.Forms.Padding(10);
			this.fileView.Name = "fileView";
			this.fileView.Size = new System.Drawing.Size(702, 449);
			this.fileView.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.fileView.TabIndex = 4;
			this.fileView.UseCompatibleStateImageBehavior = false;
			this.fileView.View = System.Windows.Forms.View.Details;
			this.fileView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.fileView_ColumnClick);
			this.fileView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.fileView_ItemCheck);
			this.fileView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.fileView_ItemSelectionChanged);
			this.fileView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.fileView_MouseClick);
			// 
			// colName
			// 
			this.colName.Text = "Name";
			this.colName.Width = 193;
			// 
			// colType
			// 
			this.colType.Text = "Type";
			this.colType.Width = 100;
			// 
			// colSize
			// 
			this.colSize.Text = "Size";
			// 
			// colModified
			// 
			this.colModified.Text = "Last Modified";
			this.colModified.Width = 89;
			// 
			// colPath
			// 
			this.colPath.Text = "Path";
			this.colPath.Width = 100;
			// 
			// colRecovery
			// 
			this.colRecovery.Text = "Chance of Recovery";
			this.colRecovery.Width = 160;
			// 
			// DeletedFileViewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.cbShowUnknownFiles);
			this.Controls.Add(this.fileView);
			this.Controls.Add(this.bRestoreFiles);
			this.Controls.Add(this.tbFilter);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.bScan);
			this.Name = "DeletedFileViewer";
			this.Size = new System.Drawing.Size(702, 620);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.ColumnHeader colName;
		private System.Windows.Forms.ColumnHeader colSize;
		private System.Windows.Forms.Button bScan;
		private System.Windows.Forms.Timer UpdateTimer;
		private System.Windows.Forms.ColumnHeader colType;
		private System.Windows.Forms.TextBox tbFilter;
		private System.Windows.Forms.Button bRestoreFiles;
		private System.Windows.Forms.ColumnHeader colModified;
		private System.Windows.Forms.ColumnHeader colRecovery;
		private System.Windows.Forms.CheckBox cbShowUnknownFiles;
		private System.Windows.Forms.ColumnHeader colPath;
		private ListViewNoFlicker fileView;

	}
}
