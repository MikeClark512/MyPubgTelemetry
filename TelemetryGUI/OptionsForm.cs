using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MyPubgTelemetry.GUI
{
    public partial class OptionsForm : Form
    {
        public TelemetryApp App { get; set; }

        public OptionsForm()
        {
            InitializeComponent();
        }

        private void OptionsForm_Load(object sender, EventArgs e)
        {
            App = new TelemetryApp();
            textBoxApiKey.Text = App.ApiKey;
            MinimumSize = Size;
            textBoxSettingsDir.Text = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            textBoxDataDir.Text = App.DataDir;
            int linksCount = labelApiKey.Links.Count;
            foreach (LinkLabel.Link link in labelApiKey.Links)
            {
                Debug.WriteLine($"link {link.Name} {link.Start} {link.Length}");
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            File.WriteAllText(App.DefaultApiKeyFile, textBoxApiKey.Text);
            Close();
        }

        private void ButtonSettingsDirOpenInFileExplorer_Click(object sender, EventArgs e)
        {
            TelemetryApp.SelectFileInFileExplorer(textBoxSettingsDir.Text);
        }

        private void ButtonDataDirOpenInFileExplorer_Click(object sender, EventArgs e)
        {
            TelemetryApp.OpenFolderInFileExplorer(textBoxDataDir.Text);
        }

        private void ButtonDataDirBrowse_Click(object sender, EventArgs e)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = textBoxDataDir.Text;
            //dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            CommonFileDialogResult result = dlg.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                textBoxDataDir.Text = dlg.FileName;
            }
        }

        private void LabelApiKey_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            TelemetryApp.OpenUrlInWebBrowser("https://developer.pubg.com/");
        }
    }
}
