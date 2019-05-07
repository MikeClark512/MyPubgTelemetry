using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyPubgTelemetry.GUI
{
    public partial class MainForm : Form
    {
        public TelemetryApp App { get; set; }

        public HashSet<string> Squad { get;  } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public JToken TeamId { get; set; }

        public MainForm()
        {
            InitializeComponent();
            App = new TelemetryApp();
            chart1.Titles.Add("Hitpoints over time (one match)");

            chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart1.ChartAreas[0].CursorX.LineColor = Color.Black;
            chart1.ChartAreas[0].CursorX.SelectionColor = Color.CornflowerBlue;
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;

            chart1.AxisViewChanged += delegate (object sender, ViewEventArgs args)
            {
                RecalcPointLabels();
            };
        }

        private void RecalcPointLabels()
        {
            double svs = chart1.ChartAreas[0].AxisX.ScaleView.Size;
            bool zoomedIn = svs > 1;
            chart1.Series.ToList().ForEach(x => x.IsValueShownAsLabel = zoomedIn);
        }

        private static object GetField(object instance, string fieldName)
        {
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = instance.GetType().GetField(fieldName, bindFlags);
            return field?.GetValue(instance);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var path = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            Debug.WriteLine("config path = " + path);
            textBoxSquad.Text = Properties.Settings.Default.Squad.Trim();
            LoadMatches();
        }

        private void LoadMatches()
        {
            string[] squaddies = textBoxSquad.Text.Split(',');
            int sumLen = 0;
            for (int i = 0; i < squaddies.Length; i++)
            {
                string squaddy = squaddies[i].Trim();
                sumLen += squaddy.Length;
            }
            if (sumLen == 0)
            {
                return;
            }
            Squad.Clear();
            Squad.UnionWith(squaddies);
            DirectoryInfo di = new DirectoryInfo(App.TelemetryDir);
            List<TelFile> telFiles = di.GetFiles("*.json").Select(x => new TelFile {FileInfo = x, Title = x.Name}).ToList();
            BindingSource binding = new BindingSource {DataSource = telFiles};

            listBoxMatches.DisplayMember = "Title";
            listBoxMatches.DataSource = binding;

            Task.Run(() => UpdateTitles(squaddies));
        }

        private void UpdateTitles(string[] squaddies)
        {
            List<TelFile> telFiles = listBoxMatches.Items.Cast<TelFile>().ToList();
            foreach (TelFile file in telFiles)
            {
                Task.Run(() => UpdateTitle(file, squaddies));
            }
        }

        private void UpdateTitle(TelFile file, string[] squaddies)
        {
            using (StreamReader sr = new StreamReader(file.FileInfo.FullName))
            {
                SortedSet<string> squadTeam = new SortedSet<string>();
                JsonTextReader jtr = new JsonTextReader(sr);
                jtr.Read();
                while (jtr.Read())
                {
                    JObject jti = JObject.Load(jtr);
                    string eventType = jti["_T"].Value<string>();
                    if (eventType == "LogPlayerCreate")
                    {
                        string player = jti.SelectToken("character.name").ToString();
                        if (Squad.Contains(player))
                        {
                            TeamId = jti.SelectToken("character.teamId");
                            squadTeam.Add(player);
                        }
                    }

                    if (eventType == "LogMatchStart")
                    {
                        break;
                    }
                }

                this.Invoke((MethodInvoker) delegate()
                {
                    file.Title = string.Join(", ", squadTeam);
                    listBoxMatches.BeginUpdate();
                    int topIdx = listBoxMatches.TopIndex;
                    int selIdx = listBoxMatches.SelectedIndex;
                    ((BindingSource) listBoxMatches.DataSource).ResetBindings(false);
                    listBoxMatches.TopIndex = topIdx;
                    listBoxMatches.SelectedIndex = selIdx;
                    listBoxMatches.EndUpdate();
                });
            }
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TelFile file = (TelFile)listBoxMatches.SelectedItem;
            Task.Run(() => SwitchMatch(file));
        }

        private void SwitchMatch(TelFile file)
        {
            var dict = new Dictionary<string, List<TelemetryEvent>>();
            using (StreamReader sr = new StreamReader(file.FileInfo.FullName))
            {
                List<TelemetryEvent> events = JsonConvert.DeserializeObject<List<TelemetryEvent>>(sr.ReadToEnd());
                foreach (TelemetryEvent @event in events)
                {
                    if (@event._T == "LogPlayerTakeDamage")
                    {
                        DateTime dt = @event._D;
                        string playerName = @event.victim.name;
                        @event.character = @event.victim;
                        if (!Squad.Contains(playerName))
                            continue;
                        @event.victim.health = @event.victim.health - @event.damage;
                        dict.TryGetValue(playerName, out List<TelemetryEvent> playerEvents);
                        if (playerEvents == null)
                            dict[playerName] = playerEvents = new List<TelemetryEvent>();
                        playerEvents.Add(@event);
                    }
                    else if (@event._T == "LogPlayerPosition")
                    {
                        DateTime dt = @event._D;
                        string playerName = @event.character.name;
                        if (!Squad.Contains(playerName))
                            continue;
                        dict.TryGetValue(playerName, out List<TelemetryEvent> playerEvents);
                        if (playerEvents == null)
                            dict[playerName] = playerEvents = new List<TelemetryEvent>();
                        playerEvents.Add(@event);
                    }
                }
            }

            this.Invoke((MethodInvoker)delegate ()
            {
                foreach (Series chart1Series in chart1.Series)
                {
                    chart1Series.Points.Clear();
                }
                chart1.Series.Clear();

                // Add series.
                foreach (KeyValuePair<string, List<TelemetryEvent>> kvp in dict)
                {
                    Series series = chart1.Series.Add(kvp.Key);
                    series.ChartType = SeriesChartType.StackedColumn;
                    series.IsValueShownAsLabel = true;
                    series.SmartLabelStyle.Enabled = true;
                    series.SmartLabelStyle.MaxMovingDistance = 0;
                    series.SmartLabelStyle.MovingDirection = LabelAlignmentStyles.Center;
                    series.LabelFormat = "#";
                    series.SmartLabelStyle.IsOverlappedHidden = true;
                    foreach (TelemetryEvent @event in kvp.Value)
                    {
                        series.Points.Add(@event.character.health);
                    }
                }
                RecalcPointLabels();
            });

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                Process.Start(App.AppDir);
            }
            else
            { 
                LoadMatches();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Squad = textBoxSquad.Text;
            Properties.Settings.Default.Save();
        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonRefresh.PerformClick();
            }
        }
    }

    public class TelFile
    {
        public FileInfo FileInfo { set; get; }
        public string Title { set; get; }
    }
}
