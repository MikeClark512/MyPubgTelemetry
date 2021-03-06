﻿namespace MyPubgTelemetry.GUI
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
            this.tabControlOptions = new System.Windows.Forms.TabControl();
            this.tabPageGeneral = new System.Windows.Forms.TabPage();
            this.buttonSettingsDirOpenInFileExplorer = new System.Windows.Forms.Button();
            this.buttonDataDirOpenInFileExplorer = new System.Windows.Forms.Button();
            this.buttonDataDirBrowse = new System.Windows.Forms.Button();
            this.textBoxSettingsDir = new System.Windows.Forms.TextBox();
            this.labelSettingsDir = new System.Windows.Forms.Label();
            this.textBoxDataDir = new System.Windows.Forms.TextBox();
            this.labelDataDir = new System.Windows.Forms.Label();
            this.tabPageLog = new System.Windows.Forms.TabPage();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.tabPageAbout = new System.Windows.Forms.TabPage();
            this.richTextBoxAbout = new System.Windows.Forms.RichTextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tabControlOptions.SuspendLayout();
            this.tabPageGeneral.SuspendLayout();
            this.tabPageLog.SuspendLayout();
            this.tabPageAbout.SuspendLayout();
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
            // tabControlOptions
            // 
            this.tabControlOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlOptions.Controls.Add(this.tabPageGeneral);
            this.tabControlOptions.Controls.Add(this.tabPageLog);
            this.tabControlOptions.Controls.Add(this.tabPageAbout);
            this.tabControlOptions.Location = new System.Drawing.Point(12, 12);
            this.tabControlOptions.Name = "tabControlOptions";
            this.tabControlOptions.SelectedIndex = 0;
            this.tabControlOptions.Size = new System.Drawing.Size(497, 267);
            this.tabControlOptions.TabIndex = 4;
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
            // tabPageLog
            // 
            this.tabPageLog.Controls.Add(this.textBoxLog);
            this.tabPageLog.Location = new System.Drawing.Point(4, 22);
            this.tabPageLog.Name = "tabPageLog";
            this.tabPageLog.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLog.Size = new System.Drawing.Size(489, 241);
            this.tabPageLog.TabIndex = 1;
            this.tabPageLog.Text = "Log";
            this.tabPageLog.UseVisualStyleBackColor = true;
            // 
            // textBoxLog
            // 
            this.textBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLog.Location = new System.Drawing.Point(6, 6);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxLog.Size = new System.Drawing.Size(480, 229);
            this.textBoxLog.TabIndex = 0;
            this.textBoxLog.WordWrap = false;
            this.textBoxLog.VisibleChanged += new System.EventHandler(this.TextBoxLog_VisibleChanged);
            // 
            // tabPageAbout
            // 
            this.tabPageAbout.Controls.Add(this.richTextBoxAbout);
            this.tabPageAbout.Location = new System.Drawing.Point(4, 22);
            this.tabPageAbout.Name = "tabPageAbout";
            this.tabPageAbout.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAbout.Size = new System.Drawing.Size(489, 241);
            this.tabPageAbout.TabIndex = 2;
            this.tabPageAbout.Text = "About";
            this.tabPageAbout.UseVisualStyleBackColor = true;
            // 
            // richTextBoxAbout
            // 
            this.richTextBoxAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxAbout.BackColor = System.Drawing.Color.White;
            this.richTextBoxAbout.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxAbout.Location = new System.Drawing.Point(6, 6);
            this.richTextBoxAbout.Name = "richTextBoxAbout";
            this.richTextBoxAbout.ReadOnly = true;
            this.richTextBoxAbout.Size = new System.Drawing.Size(477, 229);
            this.richTextBoxAbout.TabIndex = 0;
            this.richTextBoxAbout.Text = "";
            // 
            // OptionsForm
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(521, 320);
            this.Controls.Add(this.tabControlOptions);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.buttonCancel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Options";
            this.Load += new System.EventHandler(this.OptionsForm_Load);
            this.tabControlOptions.ResumeLayout(false);
            this.tabPageGeneral.ResumeLayout(false);
            this.tabPageGeneral.PerformLayout();
            this.tabPageLog.ResumeLayout(false);
            this.tabPageLog.PerformLayout();
            this.tabPageAbout.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.TextBox textBoxApiKey;
        private System.Windows.Forms.TabControl tabControlOptions;
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
        private System.Windows.Forms.TabPage tabPageLog;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.TabPage tabPageAbout;
        private System.Windows.Forms.RichTextBox richTextBoxAbout;
    }
}