namespace MyPubgTelemetry.GUI
{
    partial class OptionsForm
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.labelApiKey = new System.Windows.Forms.LinkLabel();
            this.textBoxApiKey = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageGeneral = new System.Windows.Forms.TabPage();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.buttonSettingsDirOpenInFileExplorer = new System.Windows.Forms.Button();
            this.buttonDataDirOpenInFileExplorer = new System.Windows.Forms.Button();
            this.buttonDataDirBrowse = new System.Windows.Forms.Button();
            this.textBoxSettingsDir = new System.Windows.Forms.TextBox();
            this.labelSettingsDir = new System.Windows.Forms.Label();
            this.textBoxDataDir = new System.Windows.Forms.TextBox();
            this.labelDataDir = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPageGeneral.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(434, 285);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.Location = new System.Drawing.Point(353, 285);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 0;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // labelApiKey
            // 
            this.labelApiKey.AutoSize = true;
            this.labelApiKey.Location = new System.Drawing.Point(6, 7);
            this.labelApiKey.Name = "labelApiKey";
            this.labelApiKey.Size = new System.Drawing.Size(70, 13);
            this.labelApiKey.TabIndex = 2;
            this.labelApiKey.TabStop = true;
            this.labelApiKey.Text = "PUB API Key";
            this.toolTip1.SetToolTip(this.labelApiKey, "https://developer.pubg.com\r\n\r\nFor information on how to obtain a free API key.");
            this.labelApiKey.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LabelApiKey_LinkClicked);
            // 
            // textBoxApiKey
            // 
            this.textBoxApiKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxApiKey.Location = new System.Drawing.Point(9, 23);
            this.textBoxApiKey.Multiline = true;
            this.textBoxApiKey.Name = "textBoxApiKey";
            this.textBoxApiKey.Size = new System.Drawing.Size(474, 58);
            this.textBoxApiKey.TabIndex = 3;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPageGeneral);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(497, 267);
            this.tabControl1.TabIndex = 4;
            // 
            // tabPageGeneral
            // 
            this.tabPageGeneral.Controls.Add(this.buttonSettingsDirOpenInFileExplorer);
            this.tabPageGeneral.Controls.Add(this.buttonDataDirOpenInFileExplorer);
            this.tabPageGeneral.Controls.Add(this.buttonDataDirBrowse);
            this.tabPageGeneral.Controls.Add(this.textBoxSettingsDir);
            this.tabPageGeneral.Controls.Add(this.labelSettingsDir);
            this.tabPageGeneral.Controls.Add(this.textBoxDataDir);
            this.tabPageGeneral.Controls.Add(this.labelDataDir);
            this.tabPageGeneral.Controls.Add(this.textBoxApiKey);
            this.tabPageGeneral.Controls.Add(this.labelApiKey);
            this.tabPageGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabPageGeneral.Name = "tabPageGeneral";
            this.tabPageGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGeneral.Size = new System.Drawing.Size(489, 241);
            this.tabPageGeneral.TabIndex = 0;
            this.tabPageGeneral.Text = "General";
            this.tabPageGeneral.UseVisualStyleBackColor = true;
            // 
            // buttonSettingsDirOpenInFileExplorer
            // 
            this.buttonSettingsDirOpenInFileExplorer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSettingsDirOpenInFileExplorer.Image = global::MyPubgTelemetry.GUI.Properties.Resources.OpeninFileExplorer_16x;
            this.buttonSettingsDirOpenInFileExplorer.Location = new System.Drawing.Point(459, 116);
            this.buttonSettingsDirOpenInFileExplorer.Name = "buttonSettingsDirOpenInFileExplorer";
            this.buttonSettingsDirOpenInFileExplorer.Size = new System.Drawing.Size(24, 22);
            this.buttonSettingsDirOpenInFileExplorer.TabIndex = 13;
            this.toolTip1.SetToolTip(this.buttonSettingsDirOpenInFileExplorer, "Open in File Explorer");
            this.buttonSettingsDirOpenInFileExplorer.UseVisualStyleBackColor = true;
            this.buttonSettingsDirOpenInFileExplorer.Click += new System.EventHandler(this.ButtonSettingsDirOpenInFileExplorer_Click);
            // 
            // buttonDataDirOpenInFileExplorer
            // 
            this.buttonDataDirOpenInFileExplorer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDataDirOpenInFileExplorer.Image = global::MyPubgTelemetry.GUI.Properties.Resources.OpeninFileExplorer_16x;
            this.buttonDataDirOpenInFileExplorer.Location = new System.Drawing.Point(459, 90);
            this.buttonDataDirOpenInFileExplorer.Name = "buttonDataDirOpenInFileExplorer";
            this.buttonDataDirOpenInFileExplorer.Size = new System.Drawing.Size(24, 22);
            this.buttonDataDirOpenInFileExplorer.TabIndex = 12;
            this.toolTip1.SetToolTip(this.buttonDataDirOpenInFileExplorer, "Open in File Explorer");
            this.buttonDataDirOpenInFileExplorer.UseVisualStyleBackColor = true;
            this.buttonDataDirOpenInFileExplorer.Click += new System.EventHandler(this.ButtonDataDirOpenInFileExplorer_Click);
            // 
            // buttonDataDirBrowse
            // 
            this.buttonDataDirBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDataDirBrowse.Location = new System.Drawing.Point(429, 90);
            this.buttonDataDirBrowse.Name = "buttonDataDirBrowse";
            this.buttonDataDirBrowse.Size = new System.Drawing.Size(24, 22);
            this.buttonDataDirBrowse.TabIndex = 11;
            this.buttonDataDirBrowse.Text = "...";
            this.toolTip1.SetToolTip(this.buttonDataDirBrowse, "Browse...");
            this.buttonDataDirBrowse.UseVisualStyleBackColor = true;
            this.buttonDataDirBrowse.Click += new System.EventHandler(this.ButtonDataDirBrowse_Click);
            // 
            // textBoxSettingsDir
            // 
            this.textBoxSettingsDir.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSettingsDir.Location = new System.Drawing.Point(74, 117);
            this.textBoxSettingsDir.Name = "textBoxSettingsDir";
            this.textBoxSettingsDir.ReadOnly = true;
            this.textBoxSettingsDir.Size = new System.Drawing.Size(349, 20);
            this.textBoxSettingsDir.TabIndex = 10;
            // 
            // labelSettingsDir
            // 
            this.labelSettingsDir.AutoSize = true;
            this.labelSettingsDir.Location = new System.Drawing.Point(6, 120);
            this.labelSettingsDir.Name = "labelSettingsDir";
            this.labelSettingsDir.Size = new System.Drawing.Size(48, 13);
            this.labelSettingsDir.TabIndex = 9;
            this.labelSettingsDir.Text = "Settings:";
            this.toolTip1.SetToolTip(this.labelSettingsDir, "Most application preferences are stored in this file.");
            // 
            // textBoxDataDir
            // 
            this.textBoxDataDir.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDataDir.Location = new System.Drawing.Point(74, 91);
            this.textBoxDataDir.Name = "textBoxDataDir";
            this.textBoxDataDir.Size = new System.Drawing.Size(349, 20);
            this.textBoxDataDir.TabIndex = 8;
            // 
            // labelDataDir
            // 
            this.labelDataDir.AutoSize = true;
            this.labelDataDir.Location = new System.Drawing.Point(6, 94);
            this.labelDataDir.Name = "labelDataDir";
            this.labelDataDir.Size = new System.Drawing.Size(62, 13);
            this.labelDataDir.TabIndex = 7;
            this.labelDataDir.Text = "Data folder:";
            this.toolTip1.SetToolTip(this.labelDataDir, "Telemetry data files will be stored and and read from this directory.");
            // 
            // OptionsForm
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(521, 320);
            this.ControlBox = false;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.buttonCancel);
            this.Name = "OptionsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Options";
            this.Load += new System.EventHandler(this.OptionsForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPageGeneral.ResumeLayout(false);
            this.tabPageGeneral.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.TextBox textBoxApiKey;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageGeneral;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.LinkLabel labelApiKey;
        private System.Windows.Forms.Button buttonSettingsDirOpenInFileExplorer;
        private System.Windows.Forms.Button buttonDataDirOpenInFileExplorer;
        private System.Windows.Forms.Button buttonDataDirBrowse;
        private System.Windows.Forms.TextBox textBoxSettingsDir;
        private System.Windows.Forms.Label labelSettingsDir;
        private System.Windows.Forms.TextBox textBoxDataDir;
        private System.Windows.Forms.Label labelDataDir;
    }
}