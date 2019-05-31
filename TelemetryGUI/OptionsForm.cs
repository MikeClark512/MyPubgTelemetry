using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MyPubgTelemetry.GUI
{
    public partial class OptionsForm : Form
    {
        public string ApiKeyAtLoad { get; set; }
        public string LogText { get; set; }

        public OptionsForm()
        {
            InitializeComponent();
        }

        private void OptionsForm_Load(object sender, EventArgs e)
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            textBoxApiKey.Text = TelemetryApp.App.ApiKey;
            ApiKeyAtLoad = TelemetryApp.App.ApiKey;
            MinimumSize = Size;
            textBoxSettingsDir.Text = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            textBoxDataDir.Text = TelemetryApp.App.DataDir;
            int linksCount = labelApiKey.Links.Count;
            foreach (LinkLabel.Link link in labelApiKey.Links)
            {
                Debug.WriteLine($"link {link.Name} {link.Start} {link.Length}");
            }
            textBoxLog.Text = LogText;
            richTextBoxAbout.SelectedRtf = Properties.Resources.AboutRTF;
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            File.WriteAllText(TelemetryApp.App.DefaultApiKeyFile, textBoxApiKey.Text);
            TelemetryApp.App.ReInit();
            DialogResult = DialogResult.OK;
        }

        private void ButtonSettingsDirOpenInFileExplorer_Click(object sender, EventArgs e)
        {
            TelemetryApp.App.SelectFileInFileExplorer(textBoxSettingsDir.Text);
        }

        private void ButtonDataDirOpenInFileExplorer_Click(object sender, EventArgs e)
        {
            TelemetryApp.App.OpenFolderInFileExplorer(textBoxDataDir.Text);
        }

        private void ButtonDataDirBrowse_Click(object sender, EventArgs e)
        {
            var dlg = new CommonOpenFileDialog { IsFolderPicker = true, InitialDirectory = textBoxDataDir.Text };
            //dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            CommonFileDialogResult result = dlg.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                textBoxDataDir.Text = dlg.FileName;
            }
        }

        private void LabelApiKey_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            TelemetryApp.App.OpenUrlInWebBrowser("https://developer.pubg.com/");
        }

        private void TextBoxLog_VisibleChanged(object sender, EventArgs e)
        {
            if (textBoxLog.Visible)
            {
                textBoxLog.SelectionStart = textBoxLog.TextLength;
                textBoxLog.ScrollToCaret();
            }
        }
    }
}
