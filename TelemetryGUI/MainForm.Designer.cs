using System.Windows.Forms;

namespace MyPubgTelemetry.GUI
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.contextMenuMatches = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiPubgLookup = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiOpenInFileExplorer = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCopyMatchId = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolTipBalloon = new System.Windows.Forms.ToolTip(this.components);
            this.splitContainer1 = new MyPubgTelemetry.GUI.SplitContainerEx();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.textBoxSquad = new System.Windows.Forms.ToolStripTextBox();
            this.buttonRefresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonOptions = new System.Windows.Forms.ToolStripButton();
            this.toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
            this.hideChartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hideTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButtonFile = new System.Windows.Forms.ToolStripDropDownButton();
            this.exportCSVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButtonSearch = new System.Windows.Forms.ToolStripButton();
            this.toolStripComboBoxPlayerSelect = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.contextMenuMatches.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuMatches
            // 
            this.contextMenuMatches.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiPubgLookup,
            this.toolStripSeparator1,
            this.tsmiOpenInFileExplorer,
            this.tsmiCopyMatchId});
            this.contextMenuMatches.Name = "contextMenuMatches";
            this.contextMenuMatches.Size = new System.Drawing.Size(212, 76);
            this.contextMenuMatches.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuMatches_Opening);
            // 
            // tsmiPubgLookup
            // 
            this.tsmiPubgLookup.Name = "tsmiPubgLookup";
            this.tsmiPubgLookup.Size = new System.Drawing.Size(211, 22);
            this.tsmiPubgLookup.Text = "Open in pubglookup.com";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(208, 6);
            // 
            // tsmiOpenInFileExplorer
            // 
            this.tsmiOpenInFileExplorer.Name = "tsmiOpenInFileExplorer";
            this.tsmiOpenInFileExplorer.Size = new System.Drawing.Size(211, 22);
            this.tsmiOpenInFileExplorer.Text = "Open in File Explorer";
            this.tsmiOpenInFileExplorer.Click += new System.EventHandler(this.TsmiOpenInFileExplorer_Click);
            // 
            // tsmiCopyMatchId
            // 
            this.tsmiCopyMatchId.Name = "tsmiCopyMatchId";
            this.tsmiCopyMatchId.Size = new System.Drawing.Size(211, 22);
            this.tsmiCopyMatchId.Text = "Copy Match ID";
            this.tsmiCopyMatchId.Click += new System.EventHandler(this.TsmiCopyPath_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripProgressBar1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 650);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1279, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Margin = new System.Windows.Forms.Padding(12, 3, 0, 2);
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(1032, 17);
            this.toolStripStatusLabel1.Spring = true;
            this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Margin = new System.Windows.Forms.Padding(12, 3, 1, 3);
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(189, 16);
            // 
            // toolTipBalloon
            // 
            this.toolTipBalloon.IsBalloon = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(3, 28);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.AutoScroll = true;
            this.splitContainer1.Panel1.Controls.Add(this.dataGridView1);
            this.splitContainer1.Size = new System.Drawing.Size(1276, 619);
            this.splitContainer1.SplitterDistance = 346;
            this.splitContainer1.TabIndex = 0;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.ContextMenuStrip = this.contextMenuMatches;
            this.dataGridView1.Location = new System.Drawing.Point(3, 10);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.Size = new System.Drawing.Size(1270, 333);
            this.dataGridView1.StandardTab = true;
            this.dataGridView1.TabIndex = 3;
            this.dataGridView1.SelectionChanged += new System.EventHandler(this.DataGridView1_SelectionChanged);
            // 
            // toolStrip1
            // 
            this.toolStrip1.AutoSize = false;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.textBoxSquad,
            this.buttonRefresh,
            this.toolStripSeparator2,
            this.toolStripButtonOptions,
            this.toolStripDropDownButton2,
            this.toolStripDropDownButtonFile,
            this.toolStripButtonSearch,
            this.toolStripComboBoxPlayerSelect,
            this.toolStripSeparator3,
            this.toolStripLabel1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.toolStrip1.Size = new System.Drawing.Size(1279, 25);
            this.toolStrip1.Stretch = true;
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.TabStop = true;
            // 
            // textBoxSquad
            // 
            this.textBoxSquad.Name = "textBoxSquad";
            this.textBoxSquad.Size = new System.Drawing.Size(275, 25);
            this.textBoxSquad.ToolTipText = "Player names (comma separated -- up to six -- case sensitive)";
            this.textBoxSquad.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxSquad_KeyDown);
            this.textBoxSquad.TextChanged += new System.EventHandler(this.TextBoxSquad_TextChanged);
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Image = global::MyPubgTelemetry.GUI.Properties.Resources.Refresh_greyThin_16x;
            this.buttonRefresh.Margin = new System.Windows.Forms.Padding(3, 1, 3, 2);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(81, 22);
            this.buttonRefresh.Text = "Download";
            this.buttonRefresh.ToolTipText = "Check if new data is available online and if so, download it.";
            this.buttonRefresh.Click += new System.EventHandler(this.ButtonRefresh_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonOptions
            // 
            this.toolStripButtonOptions.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButtonOptions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonOptions.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonOptions.Image")));
            this.toolStripButtonOptions.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonOptions.Name = "toolStripButtonOptions";
            this.toolStripButtonOptions.Size = new System.Drawing.Size(62, 22);
            this.toolStripButtonOptions.Text = "Options...";
            this.toolStripButtonOptions.Click += new System.EventHandler(this.ToolStripButtonOptions_Click);
            // 
            // toolStripDropDownButton2
            // 
            this.toolStripDropDownButton2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripDropDownButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.hideChartToolStripMenuItem,
            this.hideTableToolStripMenuItem});
            this.toolStripDropDownButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton2.Image")));
            this.toolStripDropDownButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton2.Name = "toolStripDropDownButton2";
            this.toolStripDropDownButton2.Size = new System.Drawing.Size(45, 22);
            this.toolStripDropDownButton2.Text = "View";
            // 
            // hideChartToolStripMenuItem
            // 
            this.hideChartToolStripMenuItem.Name = "hideChartToolStripMenuItem";
            this.hideChartToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.hideChartToolStripMenuItem.Text = "Hide Chart";
            this.hideChartToolStripMenuItem.Click += new System.EventHandler(this.HideChartToolStripMenuItem_Click);
            // 
            // hideTableToolStripMenuItem
            // 
            this.hideTableToolStripMenuItem.Name = "hideTableToolStripMenuItem";
            this.hideTableToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.hideTableToolStripMenuItem.Text = "Hide Table";
            this.hideTableToolStripMenuItem.Click += new System.EventHandler(this.HideTableToolStripMenuItem_Click);
            // 
            // toolStripDropDownButtonFile
            // 
            this.toolStripDropDownButtonFile.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripDropDownButtonFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButtonFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportCSVToolStripMenuItem});
            this.toolStripDropDownButtonFile.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButtonFile.Image")));
            this.toolStripDropDownButtonFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButtonFile.Name = "toolStripDropDownButtonFile";
            this.toolStripDropDownButtonFile.Size = new System.Drawing.Size(38, 22);
            this.toolStripDropDownButtonFile.Text = "File";
            // 
            // exportCSVToolStripMenuItem
            // 
            this.exportCSVToolStripMenuItem.Name = "exportCSVToolStripMenuItem";
            this.exportCSVToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.exportCSVToolStripMenuItem.Text = "Export CSV...";
            this.exportCSVToolStripMenuItem.Click += new System.EventHandler(this.ExportCSVToolStripMenuItem_Click);
            // 
            // toolStripButtonSearch
            // 
            this.toolStripButtonSearch.Image = global::MyPubgTelemetry.GUI.Properties.Resources.Search_16x;
            this.toolStripButtonSearch.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSearch.Name = "toolStripButtonSearch";
            this.toolStripButtonSearch.Size = new System.Drawing.Size(110, 22);
            this.toolStripButtonSearch.Text = "Search matches";
            this.toolStripButtonSearch.ToolTipText = "Search the matches list for player names, dates, ID, etc.";
            this.toolStripButtonSearch.Click += new System.EventHandler(this.ToolStripButtonSearch_Click);
            // 
            // toolStripComboBoxPlayerSelect
            // 
            this.toolStripComboBoxPlayerSelect.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripComboBoxPlayerSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxPlayerSelect.Name = "toolStripComboBoxPlayerSelect";
            this.toolStripComboBoxPlayerSelect.Size = new System.Drawing.Size(150, 25);
            this.toolStripComboBoxPlayerSelect.SelectedIndexChanged += new System.EventHandler(this.ToolStripComboBoxPlayerSelect_SelectedIndexChanged);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(76, 22);
            this.toolStripLabel1.Text = "Player Focus:";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1279, 672);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.KeyPreview = true;
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PUBG Telemetry Analyzer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.contextMenuMatches.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ContextMenuStrip contextMenuMatches;
        private System.Windows.Forms.ToolStripMenuItem tsmiOpenInFileExplorer;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopyMatchId;
        private System.Windows.Forms.ToolStripMenuItem tsmiPubgLookup;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolTip toolTipBalloon;
        private DataGridView dataGridView1;
        private ToolStrip toolStrip1;
        private ToolStripButton buttonRefresh;
        private ToolStripTextBox textBoxSquad;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripDropDownButton toolStripDropDownButton2;
        private ToolStripMenuItem hideChartToolStripMenuItem;
        private ToolStripMenuItem hideTableToolStripMenuItem;
        private ToolStripButton toolStripButtonOptions;
        private ToolStripButton toolStripButtonSearch;
        private ToolStripDropDownButton toolStripDropDownButtonFile;
        private ToolStripMenuItem exportCSVToolStripMenuItem;
        private SplitContainerEx splitContainer1;
        private ToolStripComboBox toolStripComboBoxPlayerSelect;
        private ToolStripLabel toolStripLabel1;
        private ToolStripSeparator toolStripSeparator3;
    }
}

