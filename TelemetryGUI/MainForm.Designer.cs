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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.contextMenuMatches = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiPubgLookup = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiOpenInFileExplorer = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCopyMatchId = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.labelStatusFocus = new System.Windows.Forms.Label();
            this.comboBoxStatsFocus = new System.Windows.Forms.ComboBox();
            this.buttonExportCsv = new System.Windows.Forms.Button();
            this.buttonOptions = new System.Windows.Forms.Button();
            this.buttonToggle = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.buttonSearch = new System.Windows.Forms.Button();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.textBoxSquad = new System.Windows.Forms.TextBox();
            this.labelSquad = new System.Windows.Forms.Label();
            this.labelMatches = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolTipBalloon = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.contextMenuMatches.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // chart1
            // 
            this.chart1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.Name = "Default";
            this.chart1.ChartAreas.Add(chartArea1);
            legend1.Alignment = System.Drawing.StringAlignment.Center;
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend1.LegendStyle = System.Windows.Forms.DataVisualization.Charting.LegendStyle.Row;
            legend1.Name = "Legend1";
            legend1.TableStyle = System.Windows.Forms.DataVisualization.Charting.LegendTableStyle.Wide;
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(6, -1);
            this.chart1.Name = "chart1";
            this.chart1.Size = new System.Drawing.Size(990, 326);
            this.chart1.TabIndex = 6;
            this.chart1.Text = "chart1";
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
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(12, 12);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.labelStatusFocus);
            this.splitContainer1.Panel1.Controls.Add(this.comboBoxStatsFocus);
            this.splitContainer1.Panel1.Controls.Add(this.buttonExportCsv);
            this.splitContainer1.Panel1.Controls.Add(this.buttonOptions);
            this.splitContainer1.Panel1.Controls.Add(this.buttonToggle);
            this.splitContainer1.Panel1.Controls.Add(this.dataGridView1);
            this.splitContainer1.Panel1.Controls.Add(this.buttonSearch);
            this.splitContainer1.Panel1.Controls.Add(this.buttonRefresh);
            this.splitContainer1.Panel1.Controls.Add(this.textBoxSquad);
            this.splitContainer1.Panel1.Controls.Add(this.labelSquad);
            this.splitContainer1.Panel1.Controls.Add(this.labelMatches);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.chart1);
            this.splitContainer1.Size = new System.Drawing.Size(996, 668);
            this.splitContainer1.SplitterDistance = 325;
            this.splitContainer1.TabIndex = 5;
            // 
            // labelStatusFocus
            // 
            this.labelStatusFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelStatusFocus.AutoSize = true;
            this.labelStatusFocus.Location = new System.Drawing.Point(569, 44);
            this.labelStatusFocus.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.labelStatusFocus.Name = "labelStatusFocus";
            this.labelStatusFocus.Size = new System.Drawing.Size(49, 13);
            this.labelStatusFocus.TabIndex = 10;
            this.labelStatusFocus.Text = "Stats for:";
            // 
            // comboBoxStatsFocus
            // 
            this.comboBoxStatsFocus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxStatsFocus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxStatsFocus.FormattingEnabled = true;
            this.comboBoxStatsFocus.Items.AddRange(new object[] {
            "Squad (sum)"});
            this.comboBoxStatsFocus.Location = new System.Drawing.Point(621, 40);
            this.comboBoxStatsFocus.Name = "comboBoxStatsFocus";
            this.comboBoxStatsFocus.Size = new System.Drawing.Size(107, 21);
            this.comboBoxStatsFocus.TabIndex = 9;
            // 
            // buttonExportCsv
            // 
            this.buttonExportCsv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExportCsv.Location = new System.Drawing.Point(823, 39);
            this.buttonExportCsv.Name = "buttonExportCsv";
            this.buttonExportCsv.Size = new System.Drawing.Size(83, 23);
            this.buttonExportCsv.TabIndex = 8;
            this.buttonExportCsv.Text = "Export CSV...";
            this.buttonExportCsv.UseVisualStyleBackColor = true;
            this.buttonExportCsv.Click += new System.EventHandler(this.ButtonExportCsv_Click);
            // 
            // buttonOptions
            // 
            this.buttonOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOptions.Location = new System.Drawing.Point(912, 39);
            this.buttonOptions.Name = "buttonOptions";
            this.buttonOptions.Size = new System.Drawing.Size(81, 23);
            this.buttonOptions.TabIndex = 2;
            this.buttonOptions.Text = "&Options...";
            this.buttonOptions.UseVisualStyleBackColor = true;
            this.buttonOptions.Click += new System.EventHandler(this.ButtonOptions_Click);
            // 
            // buttonToggle
            // 
            this.buttonToggle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonToggle.Location = new System.Drawing.Point(734, 40);
            this.buttonToggle.Name = "buttonToggle";
            this.buttonToggle.Size = new System.Drawing.Size(83, 23);
            this.buttonToggle.TabIndex = 7;
            this.buttonToggle.Text = "Hide Chart";
            this.buttonToggle.UseVisualStyleBackColor = true;
            this.buttonToggle.Click += new System.EventHandler(this.ButtonNext_Click);
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
            this.dataGridView1.Location = new System.Drawing.Point(0, 68);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.Size = new System.Drawing.Size(993, 254);
            this.dataGridView1.StandardTab = true;
            this.dataGridView1.TabIndex = 1;
            this.dataGridView1.SelectionChanged += new System.EventHandler(this.DataGridView1_SelectionChanged);
            // 
            // buttonSearch
            // 
            this.buttonSearch.Image = global::MyPubgTelemetry.GUI.Properties.Resources.Search_16x;
            this.buttonSearch.Location = new System.Drawing.Point(0, 40);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(23, 22);
            this.buttonSearch.TabIndex = 3;
            this.buttonSearch.Text = " ";
            this.toolTip1.SetToolTip(this.buttonSearch, "Search match metadata\r\nShortcut = Ctrl+F\r\nFind Next = F3");
            this.buttonSearch.Click += new System.EventHandler(this.ButtonSearch_Click);
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Image = global::MyPubgTelemetry.GUI.Properties.Resources.Refresh_greyThin_16x;
            this.buttonRefresh.Location = new System.Drawing.Point(0, 2);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(23, 22);
            this.buttonRefresh.TabIndex = 4;
            this.buttonRefresh.Text = " ";
            this.buttonRefresh.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.toolTip1.SetToolTip(this.buttonRefresh, "Refresh matches list using squad");
            this.buttonRefresh.Click += new System.EventHandler(this.ButtonRefresh_Click);
            // 
            // textBoxSquad
            // 
            this.textBoxSquad.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSquad.Location = new System.Drawing.Point(63, 4);
            this.textBoxSquad.Name = "textBoxSquad";
            this.textBoxSquad.Size = new System.Drawing.Size(930, 20);
            this.textBoxSquad.TabIndex = 0;
            this.textBoxSquad.TextChanged += new System.EventHandler(this.TextBoxSquad_TextChanged);
            this.textBoxSquad.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxSquad_KeyDown);
            // 
            // labelSquad
            // 
            this.labelSquad.AutoSize = true;
            this.labelSquad.Location = new System.Drawing.Point(22, 6);
            this.labelSquad.Name = "labelSquad";
            this.labelSquad.Size = new System.Drawing.Size(44, 13);
            this.labelSquad.TabIndex = 2;
            this.labelSquad.Text = "Squad: ";
            this.toolTip1.SetToolTip(this.labelSquad, "Comma separated list of player names");
            // 
            // labelMatches
            // 
            this.labelMatches.AutoSize = true;
            this.labelMatches.Location = new System.Drawing.Point(22, 45);
            this.labelMatches.Name = "labelMatches";
            this.labelMatches.Size = new System.Drawing.Size(51, 13);
            this.labelMatches.TabIndex = 3;
            this.labelMatches.Text = "Matches:";
            this.labelMatches.Click += new System.EventHandler(this.LabelMatches_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripProgressBar1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 670);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1020, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Margin = new System.Windows.Forms.Padding(12, 3, 0, 2);
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(791, 17);
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
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1020, 692);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.splitContainer1);
            this.KeyPreview = true;
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PUBG Telemetry Analyzer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.contextMenuMatches.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label labelMatches;
        private System.Windows.Forms.Label labelSquad;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.TextBox textBoxSquad;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button buttonOptions;
        private System.Windows.Forms.ContextMenuStrip contextMenuMatches;
        private System.Windows.Forms.ToolStripMenuItem tsmiOpenInFileExplorer;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopyMatchId;
        private System.Windows.Forms.Button buttonSearch;
        private System.Windows.Forms.Button buttonToggle;
        private System.Windows.Forms.ToolStripMenuItem tsmiPubgLookup;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Button buttonExportCsv;
        private System.Windows.Forms.Label labelStatusFocus;
        private System.Windows.Forms.ComboBox comboBoxStatsFocus;
        private System.Windows.Forms.ToolTip toolTipBalloon;
    }
}

