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

        public HashSet<string> Squad { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public int TeamId { get; set; }

        public MainForm()
        {
            InitializeComponent();
            App = new TelemetryApp();
            chart1.Titles.Add("Hitpoints over time (one match)");

            chart1.AxisViewChanged += delegate (object sender, ViewEventArgs args)
            {
                RecalcPointLabels();
            };
        }

        private void RecalcPointLabels()
        {
            double scaleViewSize = chart1.ChartAreas[0].AxisX.ScaleView.Size;
            Debug.WriteLine("scaleViewSize " + scaleViewSize);
            bool zoomedIn = scaleViewSize < 0.01;
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
            List<TelFile> telFiles = di.GetFiles("*.json").Select(x => new TelFile { FileInfo = x, Title = x.Name }).ToList();
            BindingSource binding = new BindingSource { DataSource = telFiles };

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
                var teams = new Dictionary<int, SortedSet<string>>();
                int squadTeamId = -1;
                JsonTextReader jtr = new JsonTextReader(sr);
                jtr.Read();
                while (jtr.Read())
                {
                    JObject jti = JObject.Load(jtr);
                    string eventType = jti["_T"].Value<string>();
                    if (eventType == "LogPlayerCreate")
                    {
                        string player = jti.SelectToken("character.name").ToString();
                        int teamId = jti.SelectToken("character.teamId").Value<int>();
                        teams.TryGetValue(teamId, out var team);
                        if (team == null) teams[teamId] = team = new SortedSet<string>();
                        team.Add(player);
                        if (Squad.Contains(player))
                        {
                            squadTeamId = teamId;
                        }
                    }
                    if (eventType == "LogMatchStart") break;
                }
                if (squadTeamId == -1) return;
                var squadTeam = teams[squadTeamId];
                this.Invoke((MethodInvoker)delegate ()
                {
                    file.Title = string.Join(", ", squadTeam);
                    listBoxMatches.BeginUpdate();
                    int topIdx = listBoxMatches.TopIndex;
                    int selIdx = listBoxMatches.SelectedIndex;
                    ((BindingSource)listBoxMatches.DataSource).ResetBindings(false);
                    listBoxMatches.TopIndex = topIdx;
                    listBoxMatches.SelectedIndex = selIdx;
                    listBoxMatches.EndUpdate();
                    TeamId = squadTeamId; // lazy code, race condition, oh well.
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
            var playerToEvents = new Dictionary<string, List<TelemetryEvent>>();
            var timeToPlayerToEvents = new Dictionary<DateTime, Dictionary<string, List<TelemetryEvent>>>();
            var normalizedEvents = new List<TelemetryEvent>();
            using (StreamReader sr = new StreamReader(file.FileInfo.FullName))
            {
                List<TelemetryEvent> events = JsonConvert.DeserializeObject<List<TelemetryEvent>>(sr.ReadToEnd());
                foreach (TelemetryEvent @event in events)
                {
                    if (@event._T == "LogPlayerTakeDamage")
                    {
                        @event.character = @event.victim;
                        @event.victim.health = @event.victim.health - @event.damage;
                        normalizedEvents.Add(@event);
                    }
                    else if (@event._T == "LogPlayerPosition")
                    {
                        @event.victim = @event.character;
                        normalizedEvents.Add(@event);
                    }
                }
            }
            Debug.Write("a");
            var squad = new HashSet<string>();
            foreach (var @event in normalizedEvents)
            {
                DateTime dt = @event._D;
                string name = @event.character.name;
                if (!Squad.Contains(name))
                    continue;
                squad.Add(name);
                var playerEvents = playerToEvents.GetOrAdd(name, () => new List<TelemetryEvent>());
                playerEvents.Add(@event);
                var timePlayerToEvents = timeToPlayerToEvents.GetOrAdd(@event._D, () => new Dictionary<string, List<TelemetryEvent>>());
                var timePlayerEvents = timePlayerToEvents.GetOrAdd(name, () => new List<TelemetryEvent>());
                timePlayerEvents.Add(@event);
            }

            this.Invoke((MethodInvoker)delegate ()
            {
                chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
                chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
                chart1.ChartAreas[0].CursorX.LineColor = Color.Black;
                chart1.ChartAreas[0].CursorX.SelectionColor = Color.CornflowerBlue;
                chart1.ChartAreas[0].CursorX.IntervalType = DateTimeIntervalType.Seconds;
                chart1.ChartAreas[0].CursorX.Interval = 1;
                chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
                chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
                chart1.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss";
                chart1.ChartAreas[0].AxisX.IsLabelAutoFit = true;
                chart1.ChartAreas[0].AxisX.LabelAutoFitStyle = LabelAutoFitStyles.LabelsAngleStep30;
                chart1.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Minutes;
                chart1.ChartAreas[0].AxisX.Interval = 2;

                chart1.Series.ToList().ForEach(series => series.Points.Clear());
                foreach (Series chart1Series in chart1.Series)
                {
                    chart1Series.Points.Clear();
                }
                chart1.Series.Clear();

                // Declare series first
                foreach (string playerName in playerToEvents.Keys)
                {
                    Series series = chart1.Series.Add(playerName);
                    series.ChartType = SeriesChartType.StackedArea;
                    series.IsValueShownAsLabel = true;
                    series.SmartLabelStyle.Enabled = true;
                    //series.SmartLabelStyle.MaxMovingDistance = 0;
                    series.SmartLabelStyle.MovingDirection = LabelAlignmentStyles.Center;
                    series.LabelFormat = "#";
                    series.SmartLabelStyle.IsOverlappedHidden = true;
                    series.XValueType = ChartValueType.DateTime;
                }
                // Track player's last known HP so we can fill in a reasonable value at missing time intervals
                var lastHps = new Dictionary<string, float>();
                // Then add data
                foreach (var dt in timeToPlayerToEvents)
                {
                    DateTime k = dt.Key;
                    Dictionary<string, List<TelemetryEvent>> v = dt.Value;
                    foreach (string squadMember in squad)
                    {
                        v.TryGetValue(squadMember, out var squadEvents);
                        if (squadEvents != null)
                        {
                            chart1.Series[squadMember].Points.AddXY(k, squadEvents.First().character.health);
                            lastHps[squadMember] = squadEvents.First().character.health;
                        }
                        else
                        {
                            float lastHp = lastHps.GetValueOrDefault(squadMember, () => 100);
                            chart1.Series[squadMember].Points.AddXY(k, lastHp);
                        }
                    }
                }

                //foreach (string playerName in playerToEvents.Keys)
                //{
                //    var otherPlayers = squad.Where(x => x != playerName);
                //    List<TelemetryEvent> playerEvents = playerToEvents[playerName];
                //    foreach (TelemetryEvent @event in playerEvents)
                //    {
                //        if (@event.skip) continue;
                //        Series series = chart1.Series[playerName];
                //        series.Points.AddXY(@event._D, @event.character.health);
                //        var timePlayerEvents = timeToPlayerToEvents[@event._D];
                //        foreach (var otherPlayer in otherPlayers)
                //        {
                //            Series otherSeries = chart1.Series[otherPlayer];
                //            timePlayerEvents.TryGetValue(otherPlayer, out var tpes);
                //            if (tpes != null)
                //            {
                //                Debug.WriteLine("op " + otherPlayer + " tpes" + tpes.Count);
                //                otherSeries.Points.AddXY(@event._D, tpes.First().character.health);
                //                tpes.First().skip = true;
                //            }
                //            else
                //            {
                //                series.Points.AddXY(@event._D, 1);
                //            }
                //        }
                //        //series.Points.Add(@event.character.health);
                //    }
                //}

                RecalcPointLabels();

                foreach (Series chart1Series in chart1.Series)
                {
                    //chart1.DataManipulator.InsertEmptyPoints(1, IntervalType.Milliseconds, chart1Series);
                }
                //chart1.DataManipulator.InsertEmptyPoints(100, IntervalType.Milliseconds, string.Join(", ", squad));
                //chart1.Series.ToList().ForEach(series => series.ChartType = SeriesChartType.StackedArea);
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

        private void ButtonOptions_Click(object sender, EventArgs e)
        {
            OptionsForm optionsForm = new OptionsForm();
            optionsForm.StartPosition = FormStartPosition.CenterParent;
            optionsForm.ShowDialog(this);
        }

        private void OpenInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TelFile tfile = (TelFile)listBoxMatches.SelectedItem;
            Process.Start("explorer.exe", "/select," + tfile.FileInfo.FullName);
        }

        private void ListBoxMatches_MouseDown(object sender, MouseEventArgs e)
        {
            listBoxMatches.SelectedIndex = listBoxMatches.IndexFromPoint(e.Location);
        }

        private void CopyPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TelFile tfile = (TelFile)listBoxMatches.SelectedItem;
            Clipboard.SetText(tfile.FileInfo.FullName);
        }
    }

    public class TelFile
    {
        public FileInfo FileInfo { set; get; }
        public string Title { set; get; }
    }
}
