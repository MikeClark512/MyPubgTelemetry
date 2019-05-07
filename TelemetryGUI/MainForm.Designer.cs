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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.listBoxMatches = new System.Windows.Forms.ListBox();
            this.contextMenuMatches = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openInExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.buttonOptions = new System.Windows.Forms.Button();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.textBoxSquad = new System.Windows.Forms.TextBox();
            this.labelSquad = new System.Windows.Forms.Label();
            this.labelMatches = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.copyPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.contextMenuMatches.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // chart1
            // 
            chartArea2.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea2);
            this.chart1.Dock = System.Windows.Forms.DockStyle.Fill;
            legend2.Name = "Legend1";
            this.chart1.Legends.Add(legend2);
            this.chart1.Location = new System.Drawing.Point(0, 0);
            this.chart1.Name = "chart1";
            this.chart1.Size = new System.Drawing.Size(514, 426);
            this.chart1.TabIndex = 0;
            this.chart1.Text = "chart1";
            // 
            // listBoxMatches
            // 
            this.listBoxMatches.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxMatches.ContextMenuStrip = this.contextMenuMatches;
            this.listBoxMatches.FormattingEnabled = true;
            this.listBoxMatches.IntegralHeight = false;
            this.listBoxMatches.Location = new System.Drawing.Point(0, 52);
            this.listBoxMatches.Name = "listBoxMatches";
            this.listBoxMatches.Size = new System.Drawing.Size(258, 345);
            this.listBoxMatches.TabIndex = 2;
            this.listBoxMatches.SelectedIndexChanged += new System.EventHandler(this.ListBox1_SelectedIndexChanged);
            this.listBoxMatches.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ListBoxMatches_MouseDown);
            // 
            // contextMenuMatches
            // 
            this.contextMenuMatches.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openInExplorerToolStripMenuItem,
            this.copyPathToolStripMenuItem});
            this.contextMenuMatches.Name = "contextMenuMatches";
            this.contextMenuMatches.Size = new System.Drawing.Size(181, 70);
            // 
            // openInExplorerToolStripMenuItem
            // 
            this.openInExplorerToolStripMenuItem.Name = "openInExplorerToolStripMenuItem";
            this.openInExplorerToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.openInExplorerToolStripMenuItem.Text = "Open in Explorer";
            this.openInExplorerToolStripMenuItem.Click += new System.EventHandler(this.OpenInExplorerToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 12);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.buttonOptions);
            this.splitContainer1.Panel1.Controls.Add(this.buttonRefresh);
            this.splitContainer1.Panel1.Controls.Add(this.textBoxSquad);
            this.splitContainer1.Panel1.Controls.Add(this.labelSquad);
            this.splitContainer1.Panel1.Controls.Add(this.labelMatches);
            this.splitContainer1.Panel1.Controls.Add(this.listBoxMatches);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.chart1);
            this.splitContainer1.Size = new System.Drawing.Size(776, 426);
            this.splitContainer1.SplitterDistance = 258;
            this.splitContainer1.TabIndex = 2;
            // 
            // buttonOptions
            // 
            this.buttonOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOptions.Location = new System.Drawing.Point(0, 403);
            this.buttonOptions.Name = "buttonOptions";
            this.buttonOptions.Size = new System.Drawing.Size(256, 23);
            this.buttonOptions.TabIndex = 3;
            this.buttonOptions.Text = "&Options...";
            this.buttonOptions.UseVisualStyleBackColor = true;
            this.buttonOptions.Click += new System.EventHandler(this.ButtonOptions_Click);
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRefresh.Font = new System.Drawing.Font("Wingdings 3", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.buttonRefresh.Location = new System.Drawing.Point(232, 2);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(23, 22);
            this.buttonRefresh.TabIndex = 1;
            this.buttonRefresh.Text = "Q";
            this.buttonRefresh.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.toolTip1.SetToolTip(this.buttonRefresh, "Refresh/Reload");
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.Button1_Click);
            // 
            // textBoxSquad
            // 
            this.textBoxSquad.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSquad.Location = new System.Drawing.Point(50, 3);
            this.textBoxSquad.Name = "textBoxSquad";
            this.textBoxSquad.Size = new System.Drawing.Size(176, 20);
            this.textBoxSquad.TabIndex = 0;
            this.textBoxSquad.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBox1_KeyDown);
            // 
            // labelSquad
            // 
            this.labelSquad.AutoSize = true;
            this.labelSquad.Location = new System.Drawing.Point(0, 6);
            this.labelSquad.Name = "labelSquad";
            this.labelSquad.Size = new System.Drawing.Size(44, 13);
            this.labelSquad.TabIndex = 4;
            this.labelSquad.Text = "Squad: ";
            this.toolTip1.SetToolTip(this.labelSquad, "Comma separated list of player names");
            // 
            // labelMatches
            // 
            this.labelMatches.AutoSize = true;
            this.labelMatches.Location = new System.Drawing.Point(0, 36);
            this.labelMatches.Name = "labelMatches";
            this.labelMatches.Size = new System.Drawing.Size(51, 13);
            this.labelMatches.TabIndex = 3;
            this.labelMatches.Text = "Matches:";
            // 
            // copyPathToolStripMenuItem
            // 
            this.copyPathToolStripMenuItem.Name = "copyPathToolStripMenuItem";
            this.copyPathToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.copyPathToolStripMenuItem.Text = "Copy Path";
            this.copyPathToolStripMenuItem.Click += new System.EventHandler(this.CopyPathToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.splitContainer1);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PUBG Telemetry Analyzer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.contextMenuMatches.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.ListBox listBoxMatches;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label labelMatches;
        private System.Windows.Forms.Label labelSquad;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.TextBox textBoxSquad;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button buttonOptions;
        private System.Windows.Forms.ContextMenuStrip contextMenuMatches;
        private System.Windows.Forms.ToolStripMenuItem openInExplorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyPathToolStripMenuItem;
    }
}

