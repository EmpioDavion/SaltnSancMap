namespace SaltMapEdit
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			pnlMap = new System.Windows.Forms.Panel();
			tbEdit = new System.Windows.Forms.TextBox();
			pnlConnections = new System.Windows.Forms.Panel();
			lbConnections = new System.Windows.Forms.ListBox();
			lblConnections = new System.Windows.Forms.Label();
			btnAddConnection = new System.Windows.Forms.Button();
			btnAddRegion = new System.Windows.Forms.Button();
			btnRedo = new System.Windows.Forms.Button();
			btnUndo = new System.Windows.Forms.Button();
			btnSave = new System.Windows.Forms.Button();
			lbLocations = new System.Windows.Forms.ListBox();
			btnAddItem = new System.Windows.Forms.Button();
			lbItems = new System.Windows.Forms.ListBox();
			lbRegions = new System.Windows.Forms.ListBox();
			lblRegions = new System.Windows.Forms.Label();
			lbSelectRegion = new System.Windows.Forms.ListBox();
			pnlRegions = new System.Windows.Forms.Panel();
			btnClearFilter = new System.Windows.Forms.Button();
			pnlLocations = new System.Windows.Forms.Panel();
			lblLocations = new System.Windows.Forms.Label();
			pnlItems = new System.Windows.Forms.Panel();
			lblItems = new System.Windows.Forms.Label();
			lbSelectCheck = new System.Windows.Forms.ListBox();
			pnlProgress = new System.Windows.Forms.Panel();
			btnAddProgress = new System.Windows.Forms.Button();
			lbProgress = new System.Windows.Forms.ListBox();
			lblProgress = new System.Windows.Forms.Label();
			pnlConnections.SuspendLayout();
			pnlRegions.SuspendLayout();
			pnlLocations.SuspendLayout();
			pnlItems.SuspendLayout();
			pnlProgress.SuspendLayout();
			SuspendLayout();
			// 
			// pnlMap
			// 
			pnlMap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			pnlMap.Location = new System.Drawing.Point(12, 12);
			pnlMap.Name = "pnlMap";
			pnlMap.Size = new System.Drawing.Size(1240, 577);
			pnlMap.TabIndex = 0;
			pnlMap.TabStop = true;
			pnlMap.KeyDown += pnlMap_KeyDown;
			pnlMap.MouseClick += pnlMap_MouseClick;
			pnlMap.MouseDown += pnlMap_MouseDown;
			pnlMap.MouseMove += pnlMap_MouseMove;
			pnlMap.MouseWheel += pnlMap_MouseWheel;
			// 
			// tbEdit
			// 
			tbEdit.AcceptsReturn = true;
			tbEdit.BackColor = System.Drawing.Color.Gray;
			tbEdit.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			tbEdit.Location = new System.Drawing.Point(150, 20);
			tbEdit.Name = "tbEdit";
			tbEdit.Size = new System.Drawing.Size(100, 23);
			tbEdit.TabIndex = 0;
			tbEdit.TabStop = false;
			tbEdit.Text = "tbEdit";
			tbEdit.Visible = false;
			tbEdit.KeyDown += tbEdit_KeyDown;
			tbEdit.Leave += tbEdit_Leave;
			// 
			// pnlConnections
			// 
			pnlConnections.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			pnlConnections.Controls.Add(lbConnections);
			pnlConnections.Controls.Add(lblConnections);
			pnlConnections.Controls.Add(btnAddConnection);
			pnlConnections.Location = new System.Drawing.Point(225, 595);
			pnlConnections.Name = "pnlConnections";
			pnlConnections.Size = new System.Drawing.Size(210, 294);
			pnlConnections.TabIndex = 1;
			// 
			// lbConnections
			// 
			lbConnections.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			lbConnections.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			lbConnections.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			lbConnections.Location = new System.Drawing.Point(3, 18);
			lbConnections.Name = "lbConnections";
			lbConnections.Size = new System.Drawing.Size(204, 244);
			lbConnections.TabIndex = 4;
			lbConnections.SelectedValueChanged += lbConnections_SelectedValueChanged;
			lbConnections.DoubleClick += lbConnections_DoubleClick;
			lbConnections.KeyDown += lbConnections_KeyDown;
			// 
			// lblConnections
			// 
			lblConnections.AutoSize = true;
			lblConnections.BackColor = System.Drawing.Color.Transparent;
			lblConnections.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			lblConnections.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			lblConnections.Location = new System.Drawing.Point(0, 0);
			lblConnections.Name = "lblConnections";
			lblConnections.Size = new System.Drawing.Size(75, 15);
			lblConnections.TabIndex = 0;
			lblConnections.Text = "Connections";
			// 
			// btnAddConnection
			// 
			btnAddConnection.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			btnAddConnection.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			btnAddConnection.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			btnAddConnection.Location = new System.Drawing.Point(45, 267);
			btnAddConnection.Name = "btnAddConnection";
			btnAddConnection.Size = new System.Drawing.Size(120, 24);
			btnAddConnection.TabIndex = 5;
			btnAddConnection.Text = "Add";
			btnAddConnection.UseVisualStyleBackColor = false;
			btnAddConnection.Click += btnAddConnection_Click;
			// 
			// btnAddRegion
			// 
			btnAddRegion.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			btnAddRegion.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			btnAddRegion.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			btnAddRegion.Location = new System.Drawing.Point(87, 267);
			btnAddRegion.Name = "btnAddRegion";
			btnAddRegion.Size = new System.Drawing.Size(120, 24);
			btnAddRegion.TabIndex = 3;
			btnAddRegion.Text = "Add";
			btnAddRegion.UseVisualStyleBackColor = false;
			btnAddRegion.Click += btnAddRegion_Click;
			// 
			// btnRedo
			// 
			btnRedo.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			btnRedo.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			btnRedo.Enabled = false;
			btnRedo.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			btnRedo.Location = new System.Drawing.Point(1092, 823);
			btnRedo.Name = "btnRedo";
			btnRedo.Size = new System.Drawing.Size(160, 34);
			btnRedo.TabIndex = 13;
			btnRedo.Text = "Redo";
			btnRedo.UseVisualStyleBackColor = false;
			btnRedo.Click += btnRedo_Click;
			// 
			// btnUndo
			// 
			btnUndo.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			btnUndo.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			btnUndo.Enabled = false;
			btnUndo.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			btnUndo.Location = new System.Drawing.Point(1092, 718);
			btnUndo.Name = "btnUndo";
			btnUndo.Size = new System.Drawing.Size(160, 34);
			btnUndo.TabIndex = 12;
			btnUndo.Text = "Undo";
			btnUndo.UseVisualStyleBackColor = false;
			btnUndo.Click += btnUndo_Click;
			// 
			// btnSave
			// 
			btnSave.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			btnSave.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			btnSave.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			btnSave.Location = new System.Drawing.Point(1092, 613);
			btnSave.Name = "btnSave";
			btnSave.Size = new System.Drawing.Size(160, 34);
			btnSave.TabIndex = 11;
			btnSave.Text = "Save";
			btnSave.UseVisualStyleBackColor = false;
			btnSave.Click += btnSave_Click;
			// 
			// lbLocations
			// 
			lbLocations.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			lbLocations.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			lbLocations.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			lbLocations.Location = new System.Drawing.Point(3, 18);
			lbLocations.Name = "lbLocations";
			lbLocations.Size = new System.Drawing.Size(203, 244);
			lbLocations.TabIndex = 6;
			lbLocations.SelectedValueChanged += lbLocations_SelectedValueChanged;
			// 
			// btnAddItem
			// 
			btnAddItem.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			btnAddItem.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			btnAddItem.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			btnAddItem.Location = new System.Drawing.Point(45, 267);
			btnAddItem.Name = "btnAddItem";
			btnAddItem.Size = new System.Drawing.Size(120, 24);
			btnAddItem.TabIndex = 8;
			btnAddItem.Text = "Add";
			btnAddItem.UseVisualStyleBackColor = false;
			btnAddItem.Click += btnAddItem_Click;
			// 
			// lbItems
			// 
			lbItems.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			lbItems.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			lbItems.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			lbItems.Location = new System.Drawing.Point(3, 18);
			lbItems.Name = "lbItems";
			lbItems.Size = new System.Drawing.Size(204, 244);
			lbItems.TabIndex = 7;
			lbItems.DoubleClick += lbItems_DoubleClick;
			lbItems.KeyDown += lbItems_KeyDown;
			// 
			// lbRegions
			// 
			lbRegions.AllowDrop = true;
			lbRegions.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			lbRegions.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			lbRegions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			lbRegions.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			lbRegions.FormattingEnabled = true;
			lbRegions.Location = new System.Drawing.Point(3, 18);
			lbRegions.Name = "lbRegions";
			lbRegions.Size = new System.Drawing.Size(204, 244);
			lbRegions.TabIndex = 1;
			lbRegions.SelectedValueChanged += lbRegions_SelectedValueChanged;
			lbRegions.DragDrop += lbRegions_DragDrop;
			lbRegions.DragOver += lbRegions_DragOver;
			lbRegions.DoubleClick += lbRegions_DoubleClick;
			lbRegions.KeyDown += lbRegions_KeyDown;
			lbRegions.MouseDown += lbRegions_MouseDown;
			lbRegions.MouseMove += lbRegions_MouseMove;
			// 
			// lblRegions
			// 
			lblRegions.AutoSize = true;
			lblRegions.BackColor = System.Drawing.Color.Transparent;
			lblRegions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			lblRegions.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			lblRegions.Location = new System.Drawing.Point(3, 0);
			lblRegions.Name = "lblRegions";
			lblRegions.Size = new System.Drawing.Size(51, 15);
			lblRegions.TabIndex = 0;
			lblRegions.Text = "Regions";
			// 
			// lbSelectRegion
			// 
			lbSelectRegion.BackColor = System.Drawing.Color.Gray;
			lbSelectRegion.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			lbSelectRegion.FormattingEnabled = true;
			lbSelectRegion.Location = new System.Drawing.Point(20, 20);
			lbSelectRegion.Name = "lbSelectRegion";
			lbSelectRegion.Size = new System.Drawing.Size(120, 94);
			lbSelectRegion.TabIndex = 0;
			lbSelectRegion.TabStop = false;
			lbSelectRegion.Visible = false;
			lbSelectRegion.Click += lbSelectRegion_Click;
			lbSelectRegion.SelectedIndexChanged += lbSelectRegion_SelectedIndexChanged;
			lbSelectRegion.KeyDown += lbSelectRegion_KeyDown;
			lbSelectRegion.Leave += lbSelectRegion_Leave;
			// 
			// pnlRegions
			// 
			pnlRegions.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			pnlRegions.Controls.Add(btnClearFilter);
			pnlRegions.Controls.Add(btnAddRegion);
			pnlRegions.Controls.Add(lbRegions);
			pnlRegions.Controls.Add(lblRegions);
			pnlRegions.Location = new System.Drawing.Point(12, 595);
			pnlRegions.Name = "pnlRegions";
			pnlRegions.Size = new System.Drawing.Size(210, 294);
			pnlRegions.TabIndex = 11;
			// 
			// btnClearFilter
			// 
			btnClearFilter.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			btnClearFilter.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			btnClearFilter.Location = new System.Drawing.Point(3, 267);
			btnClearFilter.Name = "btnClearFilter";
			btnClearFilter.Size = new System.Drawing.Size(75, 23);
			btnClearFilter.TabIndex = 2;
			btnClearFilter.Text = "Clear Filter";
			btnClearFilter.UseVisualStyleBackColor = false;
			btnClearFilter.Click += btnClearFilter_Click;
			// 
			// pnlLocations
			// 
			pnlLocations.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			pnlLocations.Controls.Add(lbLocations);
			pnlLocations.Controls.Add(lblLocations);
			pnlLocations.Location = new System.Drawing.Point(438, 595);
			pnlLocations.Name = "pnlLocations";
			pnlLocations.Size = new System.Drawing.Size(210, 294);
			pnlLocations.TabIndex = 10;
			// 
			// lblLocations
			// 
			lblLocations.AutoSize = true;
			lblLocations.BackColor = System.Drawing.Color.Transparent;
			lblLocations.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			lblLocations.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			lblLocations.Location = new System.Drawing.Point(0, 0);
			lblLocations.Name = "lblLocations";
			lblLocations.Size = new System.Drawing.Size(59, 15);
			lblLocations.TabIndex = 0;
			lblLocations.Text = "Locations";
			// 
			// pnlItems
			// 
			pnlItems.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			pnlItems.Controls.Add(lbItems);
			pnlItems.Controls.Add(lblItems);
			pnlItems.Controls.Add(btnAddItem);
			pnlItems.Location = new System.Drawing.Point(651, 595);
			pnlItems.Name = "pnlItems";
			pnlItems.Size = new System.Drawing.Size(210, 294);
			pnlItems.TabIndex = 7;
			// 
			// lblItems
			// 
			lblItems.AutoSize = true;
			lblItems.BackColor = System.Drawing.Color.Transparent;
			lblItems.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			lblItems.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			lblItems.Location = new System.Drawing.Point(0, 0);
			lblItems.Name = "lblItems";
			lblItems.Size = new System.Drawing.Size(39, 15);
			lblItems.TabIndex = 0;
			lblItems.Text = "Items";
			// 
			// lbSelectCheck
			// 
			lbSelectCheck.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			lbSelectCheck.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			lbSelectCheck.FormattingEnabled = true;
			lbSelectCheck.Location = new System.Drawing.Point(260, 20);
			lbSelectCheck.Name = "lbSelectCheck";
			lbSelectCheck.Size = new System.Drawing.Size(120, 94);
			lbSelectCheck.TabIndex = 0;
			lbSelectCheck.TabStop = false;
			lbSelectCheck.Visible = false;
			lbSelectCheck.Click += lbSelectCheck_Click;
			lbSelectCheck.SelectedIndexChanged += lbSelectCheck_SelectedIndexChanged;
			lbSelectCheck.KeyDown += lbSelectCheck_KeyDown;
			lbSelectCheck.Leave += lbSelectCheck_Leave;
			// 
			// pnlProgress
			// 
			pnlProgress.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			pnlProgress.Controls.Add(btnAddProgress);
			pnlProgress.Controls.Add(lbProgress);
			pnlProgress.Controls.Add(lblProgress);
			pnlProgress.Location = new System.Drawing.Point(867, 595);
			pnlProgress.Name = "pnlProgress";
			pnlProgress.Size = new System.Drawing.Size(210, 294);
			pnlProgress.TabIndex = 12;
			// 
			// btnAddProgress
			// 
			btnAddProgress.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			btnAddProgress.Location = new System.Drawing.Point(45, 267);
			btnAddProgress.Name = "btnAddProgress";
			btnAddProgress.Size = new System.Drawing.Size(120, 24);
			btnAddProgress.TabIndex = 10;
			btnAddProgress.Text = "Add";
			btnAddProgress.UseVisualStyleBackColor = false;
			btnAddProgress.Click += btnAddProgress_Click;
			// 
			// lbProgress
			// 
			lbProgress.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			lbProgress.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			lbProgress.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			lbProgress.Location = new System.Drawing.Point(3, 18);
			lbProgress.Name = "lbProgress";
			lbProgress.Size = new System.Drawing.Size(204, 244);
			lbProgress.TabIndex = 9;
			lbProgress.DoubleClick += lbProgress_DoubleClick;
			lbProgress.KeyDown += lbProgress_KeyDown;
			// 
			// lblProgress
			// 
			lblProgress.AutoSize = true;
			lblProgress.BackColor = System.Drawing.Color.Transparent;
			lblProgress.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			lblProgress.Location = new System.Drawing.Point(0, 0);
			lblProgress.Name = "lblProgress";
			lblProgress.Size = new System.Drawing.Size(55, 15);
			lblProgress.TabIndex = 0;
			lblProgress.Text = "Progress";
			// 
			// MainForm
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			BackColor = System.Drawing.Color.FromArgb(74, 88, 104);
			ClientSize = new System.Drawing.Size(1264, 901);
			Controls.Add(lbSelectCheck);
			Controls.Add(lbSelectRegion);
			Controls.Add(tbEdit);
			Controls.Add(pnlRegions);
			Controls.Add(pnlConnections);
			Controls.Add(pnlLocations);
			Controls.Add(pnlItems);
			Controls.Add(pnlProgress);
			Controls.Add(btnSave);
			Controls.Add(btnUndo);
			Controls.Add(btnRedo);
			Controls.Add(pnlMap);
			ForeColor = System.Drawing.SystemColors.ControlLightLight;
			Name = "MainForm";
			Text = "MainForm";
			pnlConnections.ResumeLayout(false);
			pnlConnections.PerformLayout();
			pnlRegions.ResumeLayout(false);
			pnlRegions.PerformLayout();
			pnlLocations.ResumeLayout(false);
			pnlLocations.PerformLayout();
			pnlItems.ResumeLayout(false);
			pnlItems.PerformLayout();
			pnlProgress.ResumeLayout(false);
			pnlProgress.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private System.Windows.Forms.Panel pnlConnections;
		private System.Windows.Forms.Label lblRegions;
		public System.Windows.Forms.Panel pnlMap;
		public System.Windows.Forms.ListBox lbRegions;
		public System.Windows.Forms.ListBox lbLocations;
		private System.Windows.Forms.Button btnSave;
		public System.Windows.Forms.ListBox lbConnections;
		private System.Windows.Forms.Button btnAddItem;
		private System.Windows.Forms.Button btnAddConnection;
		public System.Windows.Forms.ListBox lbItems;
		private System.Windows.Forms.Button btnRedo;
		private System.Windows.Forms.Button btnUndo;
		private System.Windows.Forms.Button btnAddRegion;
		private System.Windows.Forms.TextBox tbEdit;
		private System.Windows.Forms.ListBox lbSelectRegion;
		private System.Windows.Forms.Panel pnlRegions;
		private System.Windows.Forms.Panel pnlLocations;
		private System.Windows.Forms.Panel pnlItems;
		private System.Windows.Forms.Label lblConnections;
		private System.Windows.Forms.Label lblLocations;
		private System.Windows.Forms.Label lblItems;
		private System.Windows.Forms.ListBox lbSelectCheck;
		private System.Windows.Forms.Button btnClearFilter;
		private System.Windows.Forms.Panel pnlProgress;
		private System.Windows.Forms.Label lblProgress;
		private System.Windows.Forms.Button btnAddProgress;
		public System.Windows.Forms.ListBox lbProgress;
	}
}