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
			this.pnlMap = new System.Windows.Forms.Panel();
			this.tbEdit = new System.Windows.Forms.TextBox();
			this.pnlConnections = new System.Windows.Forms.Panel();
			this.lbConnections = new System.Windows.Forms.ListBox();
			this.lblConnections = new System.Windows.Forms.Label();
			this.pnlGapConnections = new System.Windows.Forms.Panel();
			this.btnAddConnection = new System.Windows.Forms.Button();
			this.btnAddRegion = new System.Windows.Forms.Button();
			this.btnRedo = new System.Windows.Forms.Button();
			this.btnUndo = new System.Windows.Forms.Button();
			this.btnSave = new System.Windows.Forms.Button();
			this.lbLocations = new System.Windows.Forms.ListBox();
			this.btnAddItem = new System.Windows.Forms.Button();
			this.lbItems = new System.Windows.Forms.ListBox();
			this.lbRegions = new System.Windows.Forms.ListBox();
			this.lblRegions = new System.Windows.Forms.Label();
			this.lbSelectRegion = new System.Windows.Forms.ListBox();
			this.pnlRegions = new System.Windows.Forms.Panel();
			this.btnClearFilter = new System.Windows.Forms.Button();
			this.pnlLocations = new System.Windows.Forms.Panel();
			this.lblLocations = new System.Windows.Forms.Label();
			this.pnlGapLocations = new System.Windows.Forms.Panel();
			this.pnlItems = new System.Windows.Forms.Panel();
			this.lblItems = new System.Windows.Forms.Label();
			this.pnlGapItems = new System.Windows.Forms.Panel();
			this.lbSelectCheck = new System.Windows.Forms.ListBox();
			this.pnlProgress = new System.Windows.Forms.Panel();
			this.lbProgress = new System.Windows.Forms.ListBox();
			this.lblProgress = new System.Windows.Forms.Label();
			this.pnlGapProgress = new System.Windows.Forms.Panel();
			this.btnAddProgress = new System.Windows.Forms.Button();
			this.pnlSplit = new System.Windows.Forms.SplitContainer();
			this.pnlData = new System.Windows.Forms.TableLayoutPanel();
			this.pnlManage = new System.Windows.Forms.Panel();
			this.pnlConnections.SuspendLayout();
			this.pnlRegions.SuspendLayout();
			this.pnlLocations.SuspendLayout();
			this.pnlItems.SuspendLayout();
			this.pnlProgress.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pnlSplit)).BeginInit();
			this.pnlSplit.Panel1.SuspendLayout();
			this.pnlSplit.Panel2.SuspendLayout();
			this.pnlSplit.SuspendLayout();
			this.pnlData.SuspendLayout();
			this.pnlManage.SuspendLayout();
			this.SuspendLayout();
			// 
			// pnlMap
			// 
			this.pnlMap.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlMap.Location = new System.Drawing.Point(0, 0);
			this.pnlMap.Name = "pnlMap";
			this.pnlMap.Size = new System.Drawing.Size(1116, 514);
			this.pnlMap.TabIndex = 0;
			this.pnlMap.TabStop = true;
			this.pnlMap.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlMap_MouseClick);
			this.pnlMap.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlMap_MouseDown);
			this.pnlMap.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnlMap_MouseMove);
			// 
			// tbEdit
			// 
			this.tbEdit.AcceptsReturn = true;
			this.tbEdit.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.tbEdit.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.tbEdit.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			this.tbEdit.Location = new System.Drawing.Point(241, 17);
			this.tbEdit.Name = "tbEdit";
			this.tbEdit.Size = new System.Drawing.Size(86, 23);
			this.tbEdit.TabIndex = 0;
			this.tbEdit.TabStop = false;
			this.tbEdit.Text = "tbEdit";
			this.tbEdit.Visible = false;
			this.tbEdit.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbEdit_KeyDown);
			this.tbEdit.Leave += new System.EventHandler(this.tbEdit_Leave);
			// 
			// pnlConnections
			// 
			this.pnlConnections.Controls.Add(this.lbConnections);
			this.pnlConnections.Controls.Add(this.lblConnections);
			this.pnlConnections.Controls.Add(this.pnlGapConnections);
			this.pnlConnections.Controls.Add(this.btnAddConnection);
			this.pnlConnections.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlConnections.Location = new System.Drawing.Point(189, 3);
			this.pnlConnections.MinimumSize = new System.Drawing.Size(0, 255);
			this.pnlConnections.Name = "pnlConnections";
			this.pnlConnections.Size = new System.Drawing.Size(180, 257);
			this.pnlConnections.TabIndex = 1;
			// 
			// lbConnections
			// 
			this.lbConnections.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.lbConnections.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lbConnections.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lbConnections.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			this.lbConnections.IntegralHeight = false;
			this.lbConnections.ItemHeight = 15;
			this.lbConnections.Location = new System.Drawing.Point(0, 15);
			this.lbConnections.Name = "lbConnections";
			this.lbConnections.Size = new System.Drawing.Size(180, 201);
			this.lbConnections.TabIndex = 4;
			this.lbConnections.SelectedValueChanged += new System.EventHandler(this.lbConnections_SelectedValueChanged);
			this.lbConnections.DoubleClick += new System.EventHandler(this.lbConnections_DoubleClick);
			this.lbConnections.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lbConnections_KeyDown);
			// 
			// lblConnections
			// 
			this.lblConnections.AutoSize = true;
			this.lblConnections.BackColor = System.Drawing.Color.Transparent;
			this.lblConnections.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblConnections.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblConnections.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			this.lblConnections.Location = new System.Drawing.Point(0, 0);
			this.lblConnections.Name = "lblConnections";
			this.lblConnections.Size = new System.Drawing.Size(75, 15);
			this.lblConnections.TabIndex = 0;
			this.lblConnections.Text = "Connections";
			// 
			// pnlGapConnections
			// 
			this.pnlGapConnections.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlGapConnections.Location = new System.Drawing.Point(0, 216);
			this.pnlGapConnections.Name = "pnlGapConnections";
			this.pnlGapConnections.Size = new System.Drawing.Size(180, 20);
			this.pnlGapConnections.TabIndex = 6;
			// 
			// btnAddConnection
			// 
			this.btnAddConnection.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.btnAddConnection.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.btnAddConnection.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			this.btnAddConnection.Location = new System.Drawing.Point(0, 236);
			this.btnAddConnection.Name = "btnAddConnection";
			this.btnAddConnection.Size = new System.Drawing.Size(180, 21);
			this.btnAddConnection.TabIndex = 5;
			this.btnAddConnection.Text = "Add";
			this.btnAddConnection.UseVisualStyleBackColor = false;
			this.btnAddConnection.Click += new System.EventHandler(this.btnAddConnection_Click);
			// 
			// btnAddRegion
			// 
			this.btnAddRegion.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.btnAddRegion.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.btnAddRegion.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			this.btnAddRegion.Location = new System.Drawing.Point(0, 236);
			this.btnAddRegion.Name = "btnAddRegion";
			this.btnAddRegion.Size = new System.Drawing.Size(180, 21);
			this.btnAddRegion.TabIndex = 3;
			this.btnAddRegion.Text = "Add";
			this.btnAddRegion.UseVisualStyleBackColor = false;
			this.btnAddRegion.Click += new System.EventHandler(this.btnAddRegion_Click);
			// 
			// btnRedo
			// 
			this.btnRedo.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.btnRedo.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.btnRedo.Enabled = false;
			this.btnRedo.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			this.btnRedo.Location = new System.Drawing.Point(0, 228);
			this.btnRedo.Name = "btnRedo";
			this.btnRedo.Size = new System.Drawing.Size(180, 29);
			this.btnRedo.TabIndex = 13;
			this.btnRedo.Text = "Redo";
			this.btnRedo.UseVisualStyleBackColor = false;
			this.btnRedo.Click += new System.EventHandler(this.btnRedo_Click);
			// 
			// btnUndo
			// 
			this.btnUndo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.btnUndo.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.btnUndo.Enabled = false;
			this.btnUndo.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			this.btnUndo.Location = new System.Drawing.Point(0, 113);
			this.btnUndo.Name = "btnUndo";
			this.btnUndo.Size = new System.Drawing.Size(180, 29);
			this.btnUndo.TabIndex = 12;
			this.btnUndo.Text = "Undo";
			this.btnUndo.UseVisualStyleBackColor = false;
			this.btnUndo.Click += new System.EventHandler(this.btnUndo_Click);
			// 
			// btnSave
			// 
			this.btnSave.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.btnSave.Dock = System.Windows.Forms.DockStyle.Top;
			this.btnSave.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			this.btnSave.Location = new System.Drawing.Point(0, 0);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(180, 29);
			this.btnSave.TabIndex = 11;
			this.btnSave.Text = "Save";
			this.btnSave.UseVisualStyleBackColor = false;
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			// 
			// lbLocations
			// 
			this.lbLocations.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.lbLocations.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lbLocations.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lbLocations.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			this.lbLocations.IntegralHeight = false;
			this.lbLocations.ItemHeight = 15;
			this.lbLocations.Location = new System.Drawing.Point(0, 15);
			this.lbLocations.Name = "lbLocations";
			this.lbLocations.Size = new System.Drawing.Size(180, 201);
			this.lbLocations.TabIndex = 6;
			this.lbLocations.Click += new System.EventHandler(this.lbLocations_Click);
			// 
			// btnAddItem
			// 
			this.btnAddItem.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.btnAddItem.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.btnAddItem.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			this.btnAddItem.Location = new System.Drawing.Point(0, 236);
			this.btnAddItem.Name = "btnAddItem";
			this.btnAddItem.Size = new System.Drawing.Size(180, 21);
			this.btnAddItem.TabIndex = 8;
			this.btnAddItem.Text = "Add";
			this.btnAddItem.UseVisualStyleBackColor = false;
			this.btnAddItem.Click += new System.EventHandler(this.btnAddItem_Click);
			// 
			// lbItems
			// 
			this.lbItems.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.lbItems.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lbItems.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lbItems.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			this.lbItems.IntegralHeight = false;
			this.lbItems.ItemHeight = 15;
			this.lbItems.Location = new System.Drawing.Point(0, 15);
			this.lbItems.Name = "lbItems";
			this.lbItems.Size = new System.Drawing.Size(180, 201);
			this.lbItems.TabIndex = 7;
			this.lbItems.DoubleClick += new System.EventHandler(this.lbItems_DoubleClick);
			this.lbItems.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lbItems_KeyDown);
			// 
			// lbRegions
			// 
			this.lbRegions.AllowDrop = true;
			this.lbRegions.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.lbRegions.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lbRegions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbRegions.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			this.lbRegions.FormattingEnabled = true;
			this.lbRegions.IntegralHeight = false;
			this.lbRegions.ItemHeight = 15;
			this.lbRegions.Location = new System.Drawing.Point(0, 15);
			this.lbRegions.Name = "lbRegions";
			this.lbRegions.Size = new System.Drawing.Size(180, 201);
			this.lbRegions.TabIndex = 1;
			this.lbRegions.Click += new System.EventHandler(this.lbRegions_Click);
			this.lbRegions.DragDrop += new System.Windows.Forms.DragEventHandler(this.lbRegions_DragDrop);
			this.lbRegions.DragOver += new System.Windows.Forms.DragEventHandler(this.lbRegions_DragOver);
			this.lbRegions.DoubleClick += new System.EventHandler(this.lbRegions_DoubleClick);
			this.lbRegions.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lbRegions_KeyDown);
			this.lbRegions.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lbRegions_DoubleClick);
			this.lbRegions.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lbRegions_MouseDown);
			this.lbRegions.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lbRegions_MouseMove);
			this.lbRegions.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lbRegions_MouseUp);
			// 
			// lblRegions
			// 
			this.lblRegions.AutoSize = true;
			this.lblRegions.BackColor = System.Drawing.Color.Transparent;
			this.lblRegions.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblRegions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblRegions.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			this.lblRegions.Location = new System.Drawing.Point(0, 0);
			this.lblRegions.Name = "lblRegions";
			this.lblRegions.Size = new System.Drawing.Size(51, 15);
			this.lblRegions.TabIndex = 0;
			this.lblRegions.Text = "Regions";
			// 
			// lbSelectRegion
			// 
			this.lbSelectRegion.BackColor = System.Drawing.Color.Gray;
			this.lbSelectRegion.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lbSelectRegion.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			this.lbSelectRegion.FormattingEnabled = true;
			this.lbSelectRegion.ItemHeight = 15;
			this.lbSelectRegion.Location = new System.Drawing.Point(17, 17);
			this.lbSelectRegion.Name = "lbSelectRegion";
			this.lbSelectRegion.Size = new System.Drawing.Size(103, 154);
			this.lbSelectRegion.TabIndex = 0;
			this.lbSelectRegion.TabStop = false;
			this.lbSelectRegion.Visible = false;
			this.lbSelectRegion.Click += new System.EventHandler(this.lbSelectRegion_Click);
			this.lbSelectRegion.SelectedIndexChanged += new System.EventHandler(this.lbSelectRegion_SelectedIndexChanged);
			this.lbSelectRegion.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lbSelectRegion_KeyDown);
			this.lbSelectRegion.Leave += new System.EventHandler(this.lbSelectRegion_Leave);
			// 
			// pnlRegions
			// 
			this.pnlRegions.Controls.Add(this.lbRegions);
			this.pnlRegions.Controls.Add(this.lblRegions);
			this.pnlRegions.Controls.Add(this.btnClearFilter);
			this.pnlRegions.Controls.Add(this.btnAddRegion);
			this.pnlRegions.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlRegions.Location = new System.Drawing.Point(3, 3);
			this.pnlRegions.MinimumSize = new System.Drawing.Size(0, 255);
			this.pnlRegions.Name = "pnlRegions";
			this.pnlRegions.Size = new System.Drawing.Size(180, 257);
			this.pnlRegions.TabIndex = 11;
			// 
			// btnClearFilter
			// 
			this.btnClearFilter.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.btnClearFilter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.btnClearFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.btnClearFilter.Location = new System.Drawing.Point(0, 216);
			this.btnClearFilter.Name = "btnClearFilter";
			this.btnClearFilter.Size = new System.Drawing.Size(180, 20);
			this.btnClearFilter.TabIndex = 2;
			this.btnClearFilter.Text = "Clear Filter";
			this.btnClearFilter.UseVisualStyleBackColor = false;
			this.btnClearFilter.Click += new System.EventHandler(this.btnClearFilter_Click);
			// 
			// pnlLocations
			// 
			this.pnlLocations.Controls.Add(this.lbLocations);
			this.pnlLocations.Controls.Add(this.lblLocations);
			this.pnlLocations.Controls.Add(this.pnlGapLocations);
			this.pnlLocations.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlLocations.Location = new System.Drawing.Point(375, 3);
			this.pnlLocations.MinimumSize = new System.Drawing.Size(0, 255);
			this.pnlLocations.Name = "pnlLocations";
			this.pnlLocations.Size = new System.Drawing.Size(180, 257);
			this.pnlLocations.TabIndex = 10;
			// 
			// lblLocations
			// 
			this.lblLocations.AutoSize = true;
			this.lblLocations.BackColor = System.Drawing.Color.Transparent;
			this.lblLocations.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblLocations.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblLocations.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			this.lblLocations.Location = new System.Drawing.Point(0, 0);
			this.lblLocations.Name = "lblLocations";
			this.lblLocations.Size = new System.Drawing.Size(59, 15);
			this.lblLocations.TabIndex = 0;
			this.lblLocations.Text = "Locations";
			// 
			// pnlGapLocations
			// 
			this.pnlGapLocations.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlGapLocations.Location = new System.Drawing.Point(0, 216);
			this.pnlGapLocations.Name = "pnlGapLocations";
			this.pnlGapLocations.Size = new System.Drawing.Size(180, 41);
			this.pnlGapLocations.TabIndex = 7;
			// 
			// pnlItems
			// 
			this.pnlItems.Controls.Add(this.lbItems);
			this.pnlItems.Controls.Add(this.lblItems);
			this.pnlItems.Controls.Add(this.pnlGapItems);
			this.pnlItems.Controls.Add(this.btnAddItem);
			this.pnlItems.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlItems.Location = new System.Drawing.Point(561, 3);
			this.pnlItems.MinimumSize = new System.Drawing.Size(0, 255);
			this.pnlItems.Name = "pnlItems";
			this.pnlItems.Size = new System.Drawing.Size(180, 257);
			this.pnlItems.TabIndex = 7;
			// 
			// lblItems
			// 
			this.lblItems.AutoSize = true;
			this.lblItems.BackColor = System.Drawing.Color.Transparent;
			this.lblItems.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblItems.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblItems.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			this.lblItems.Location = new System.Drawing.Point(0, 0);
			this.lblItems.Name = "lblItems";
			this.lblItems.Size = new System.Drawing.Size(39, 15);
			this.lblItems.TabIndex = 0;
			this.lblItems.Text = "Items";
			// 
			// pnlGapItems
			// 
			this.pnlGapItems.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlGapItems.Location = new System.Drawing.Point(0, 216);
			this.pnlGapItems.Name = "pnlGapItems";
			this.pnlGapItems.Size = new System.Drawing.Size(180, 20);
			this.pnlGapItems.TabIndex = 9;
			// 
			// lbSelectCheck
			// 
			this.lbSelectCheck.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.lbSelectCheck.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lbSelectCheck.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			this.lbSelectCheck.FormattingEnabled = true;
			this.lbSelectCheck.ItemHeight = 15;
			this.lbSelectCheck.Location = new System.Drawing.Point(129, 17);
			this.lbSelectCheck.Name = "lbSelectCheck";
			this.lbSelectCheck.Size = new System.Drawing.Size(103, 154);
			this.lbSelectCheck.TabIndex = 0;
			this.lbSelectCheck.TabStop = false;
			this.lbSelectCheck.Visible = false;
			this.lbSelectCheck.Click += new System.EventHandler(this.lbSelectCheck_Click);
			this.lbSelectCheck.SelectedIndexChanged += new System.EventHandler(this.lbSelectCheck_SelectedIndexChanged);
			this.lbSelectCheck.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lbSelectCheck_KeyDown);
			this.lbSelectCheck.Leave += new System.EventHandler(this.lbSelectCheck_Leave);
			// 
			// pnlProgress
			// 
			this.pnlProgress.Controls.Add(this.lbProgress);
			this.pnlProgress.Controls.Add(this.lblProgress);
			this.pnlProgress.Controls.Add(this.pnlGapProgress);
			this.pnlProgress.Controls.Add(this.btnAddProgress);
			this.pnlProgress.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlProgress.Location = new System.Drawing.Point(747, 3);
			this.pnlProgress.MinimumSize = new System.Drawing.Size(0, 255);
			this.pnlProgress.Name = "pnlProgress";
			this.pnlProgress.Size = new System.Drawing.Size(180, 257);
			this.pnlProgress.TabIndex = 12;
			// 
			// lbProgress
			// 
			this.lbProgress.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.lbProgress.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lbProgress.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lbProgress.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			this.lbProgress.IntegralHeight = false;
			this.lbProgress.ItemHeight = 15;
			this.lbProgress.Location = new System.Drawing.Point(0, 15);
			this.lbProgress.Name = "lbProgress";
			this.lbProgress.Size = new System.Drawing.Size(180, 201);
			this.lbProgress.TabIndex = 9;
			this.lbProgress.DoubleClick += new System.EventHandler(this.lbProgress_DoubleClick);
			this.lbProgress.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lbProgress_KeyDown);
			// 
			// lblProgress
			// 
			this.lblProgress.AutoSize = true;
			this.lblProgress.BackColor = System.Drawing.Color.Transparent;
			this.lblProgress.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblProgress.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.lblProgress.Location = new System.Drawing.Point(0, 0);
			this.lblProgress.Name = "lblProgress";
			this.lblProgress.Size = new System.Drawing.Size(55, 15);
			this.lblProgress.TabIndex = 0;
			this.lblProgress.Text = "Progress";
			// 
			// pnlGapProgress
			// 
			this.pnlGapProgress.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlGapProgress.Location = new System.Drawing.Point(0, 216);
			this.pnlGapProgress.Name = "pnlGapProgress";
			this.pnlGapProgress.Size = new System.Drawing.Size(180, 20);
			this.pnlGapProgress.TabIndex = 11;
			// 
			// btnAddProgress
			// 
			this.btnAddProgress.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.btnAddProgress.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.btnAddProgress.Location = new System.Drawing.Point(0, 236);
			this.btnAddProgress.Name = "btnAddProgress";
			this.btnAddProgress.Size = new System.Drawing.Size(180, 21);
			this.btnAddProgress.TabIndex = 10;
			this.btnAddProgress.Text = "Add";
			this.btnAddProgress.UseVisualStyleBackColor = false;
			this.btnAddProgress.Click += new System.EventHandler(this.btnAddProgress_Click);
			// 
			// pnlSplit
			// 
			this.pnlSplit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlSplit.Location = new System.Drawing.Point(0, 0);
			this.pnlSplit.Name = "pnlSplit";
			this.pnlSplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// pnlSplit.Panel1
			// 
			this.pnlSplit.Panel1.Controls.Add(this.pnlMap);
			this.pnlSplit.Panel1MinSize = 260;
			// 
			// pnlSplit.Panel2
			// 
			this.pnlSplit.Panel2.Controls.Add(this.pnlData);
			this.pnlSplit.Panel2MinSize = 260;
			this.pnlSplit.Size = new System.Drawing.Size(1116, 781);
			this.pnlSplit.SplitterDistance = 514;
			this.pnlSplit.TabIndex = 0;
			// 
			// pnlData
			// 
			this.pnlData.ColumnCount = 6;
			this.pnlData.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.pnlData.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.pnlData.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.pnlData.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.pnlData.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.pnlData.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.pnlData.Controls.Add(this.pnlRegions);
			this.pnlData.Controls.Add(this.pnlConnections);
			this.pnlData.Controls.Add(this.pnlLocations);
			this.pnlData.Controls.Add(this.pnlItems);
			this.pnlData.Controls.Add(this.pnlProgress);
			this.pnlData.Controls.Add(this.pnlManage);
			this.pnlData.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlData.Location = new System.Drawing.Point(0, 0);
			this.pnlData.MinimumSize = new System.Drawing.Size(0, 260);
			this.pnlData.Name = "pnlData";
			this.pnlData.RowCount = 1;
			this.pnlData.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.pnlData.Size = new System.Drawing.Size(1116, 263);
			this.pnlData.TabIndex = 0;
			// 
			// pnlManage
			// 
			this.pnlManage.Controls.Add(this.btnSave);
			this.pnlManage.Controls.Add(this.btnUndo);
			this.pnlManage.Controls.Add(this.btnRedo);
			this.pnlManage.Dock = System.Windows.Forms.DockStyle.Right;
			this.pnlManage.Location = new System.Drawing.Point(933, 3);
			this.pnlManage.MinimumSize = new System.Drawing.Size(0, 255);
			this.pnlManage.Name = "pnlManage";
			this.pnlManage.Size = new System.Drawing.Size(180, 257);
			this.pnlManage.TabIndex = 14;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(88)))), ((int)(((byte)(104)))));
			this.ClientSize = new System.Drawing.Size(1116, 781);
			this.Controls.Add(this.lbSelectCheck);
			this.Controls.Add(this.lbSelectRegion);
			this.Controls.Add(this.tbEdit);
			this.Controls.Add(this.pnlSplit);
			this.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			this.Name = "MainForm";
			this.Text = "MainForm";
			this.Click += new System.EventHandler(this.MainForm_Click);
			this.pnlConnections.ResumeLayout(false);
			this.pnlConnections.PerformLayout();
			this.pnlRegions.ResumeLayout(false);
			this.pnlRegions.PerformLayout();
			this.pnlLocations.ResumeLayout(false);
			this.pnlLocations.PerformLayout();
			this.pnlItems.ResumeLayout(false);
			this.pnlItems.PerformLayout();
			this.pnlProgress.ResumeLayout(false);
			this.pnlProgress.PerformLayout();
			this.pnlSplit.Panel1.ResumeLayout(false);
			this.pnlSplit.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pnlSplit)).EndInit();
			this.pnlSplit.ResumeLayout(false);
			this.pnlData.ResumeLayout(false);
			this.pnlManage.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

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
		private System.Windows.Forms.Panel pnlRegions;
		private System.Windows.Forms.Panel pnlLocations;
		private System.Windows.Forms.Panel pnlItems;
		private System.Windows.Forms.Label lblConnections;
		private System.Windows.Forms.Label lblLocations;
		private System.Windows.Forms.Label lblItems;
		private System.Windows.Forms.Button btnClearFilter;
		private System.Windows.Forms.Panel pnlProgress;
		private System.Windows.Forms.Label lblProgress;
		private System.Windows.Forms.Button btnAddProgress;
		public System.Windows.Forms.ListBox lbProgress;
		public System.Windows.Forms.ListBox lbSelectRegion;
		public System.Windows.Forms.ListBox lbSelectCheck;
		private System.Windows.Forms.SplitContainer pnlSplit;
		private System.Windows.Forms.Panel pnlManage;
		private System.Windows.Forms.TableLayoutPanel pnlData;
		private System.Windows.Forms.Panel pnlGapConnections;
		private System.Windows.Forms.Panel pnlGapLocations;
		private System.Windows.Forms.Panel pnlGapItems;
		private System.Windows.Forms.Panel pnlGapProgress;
	}
}