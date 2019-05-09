using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyPubgTelemetry.GUI
{
    public partial class MainForm : Form
    {
        public InputBox MatchSearchInputBox { get; }
        private const string ChartTitleDateFormat = "M/d/yy h:mm tt";
        private const string XAxisDateFormat = "h:mm:ss tt";
        public TelemetryApp App { get; set; }
        public HashSet<string> Squad { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public Regex RegexCsv { get; } = new Regex(@"\s*,\s*", RegexOptions.IgnoreCase);
        public ConcurrentDictionary<string, string> AccountIds = new ConcurrentDictionary<string, string>();
        public BlockingCollection<PreparedData> PreparedDataQ { get;  } = new BlockingCollection<PreparedData>();
        public CancellationTokenSource CancellationTokenSourceMatchSwitch { get; set; }

        public MainForm()
        {
            InitializeComponent();
            App = new TelemetryApp();
            InitChart();
            MatchSearchInputBox = new InputBox { InputText = Clipboard.GetText(), Text = @"Search match IDs, dates, and player names"};
        }

        private void InitChart()
        {
            chart1.Titles.Add("Hitpoints over time (one match)");
            chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart1.ChartAreas[0].CursorX.LineColor = Color.Black;
            chart1.ChartAreas[0].CursorX.SelectionColor = Color.CornflowerBlue;
            chart1.ChartAreas[0].CursorX.IntervalType = DateTimeIntervalType.Seconds;
            chart1.ChartAreas[0].CursorX.Interval = 1;
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = XAxisDateFormat;
            chart1.ChartAreas[0].AxisX.IsLabelAutoFit = true;
            chart1.ChartAreas[0].AxisX.LabelAutoFitStyle = LabelAutoFitStyles.LabelsAngleStep30;
            chart1.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Minutes;
            chart1.ChartAreas[0].AxisX.Interval = 2;
            chart1.ChartAreas[0].AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            chart1.AxisViewChanged += delegate (object sender, ViewEventArgs args)
            {
                chart1.ChartAreas[0].AxisX.Interval = chart1.ChartAreas[0].AxisX.Interval / 10;
                RecalcPointLabels();
                var scaleView = chart1.ChartAreas[0].AxisX.ScaleView;
                double delta = scaleView.ViewMaximum - scaleView.ViewMinimum;
                bool zoomed = chart1.ChartAreas[0].AxisX.ScaleView.IsZoomed;
                chart1.ChartAreas[0].AxisX.Interval = zoomed ? delta * 100 : 0;
            };
            //chart1.ChartAreas[0].Area3DStyle.Enable3D = true;
            //chart1.ChartAreas[0].Area3DStyle.IsRightAngleAxes = false;
            //chart1.ChartAreas["Default"].Area3DStyle.Inclination = 40;
            //chart1.ChartAreas["Default"].Area3DStyle.Rotation = 15;
            //chart1.ChartAreas["Default"].Area3DStyle.LightStyle = LightStyle.Realistic;
            //chart1.ChartAreas["Default"].Area3DStyle.Perspective = 5;
            //chart1.ChartAreas["Default"].Area3DStyle.PointGapDepth = 1000;
            //chart1.ChartAreas["Default"].Area3DStyle.PointDepth = 1000;
        }

        private void RecalcPointLabels()
        {
            double scaleViewSize = chart1.ChartAreas[0].AxisX.ScaleView.Size;
            DebugThreadWriteLine("scaleViewSize " + scaleViewSize);
            bool zoomedIn = scaleViewSize < 0.01;
            //series.SmartLabelStyle.MaxMovingDistance = 0;
            chart1.Series.ToList().ForEach(x => x.IsValueShownAsLabel = zoomedIn);
        }

        private static object GetField(object instance, string fieldName)
        {
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var field = instance.GetType().GetField(fieldName, bindFlags);
            return field?.GetValue(instance);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var path = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            DebugThreadWriteLine("config path = " + path);
            textBoxSquad.Text = Properties.Settings.Default.Squad.Trim();
            LoadMatches();
        }

        private void LoadMatches()
        {
            var squaddies = RegexCsv.Split(textBoxSquad.Text);
            int sumLen = squaddies.Sum(s => s.Length);
            if (sumLen == 0) return; // nothin' but whitespace and commas.
            Squad.Clear();
            Squad.UnionWith(squaddies);
            var di = new DirectoryInfo(App.TelemetryDir);
            var telFiles = di.GetFiles("*.json").Select(x => new TelemetryFile { FileInfo = x, Title = x.Name, Date = GetMatchDateTime(x) }).ToList();
            telFiles.Sort((x, y) => y.Date.CompareTo(x.Date));
            var binding = new BindingSource { DataSource = telFiles };

            listBoxMatches.DisplayMember = "Title";
            listBoxMatches.DataSource = binding;

            foreach (var file in telFiles)
            {
                Task.Run(() => UpdateMatchListTitle(file, squaddies));
            }
        }

        private DateTime GetMatchDateTime(FileInfo fileInfo)
        {
            using (var jtr = new JsonTextReader(new StreamReader(fileInfo.FullName)))
            {
                var serializer = new JsonSerializer();
                // TODO: parse error handling
                jtr.Read(); // this reads the array "header" ([)
                jtr.Read(); // this this reads the start of the first object in the array ({)
                // at this point the JsonTextReader is in the correct position to grab array[0] as a whole object
                var @event = serializer.Deserialize<TelemetryEvent>(jtr);
                return @event._D;
            }
        }

        private void UpdateMatchListTitle(TelemetryFile file, string[] squaddies)
        {
            using (var sr = new StreamReader(file.FileInfo.FullName))
            {
                var teams = new Dictionary<int, SortedSet<string>>();
                int squadTeamId = -1;
                var jtr = new JsonTextReader(sr);
                jtr.Read();
                DateTime? matchDate = null;
                while (jtr.Read())
                {
                    var serializer = new JsonSerializer();
                    var @event = serializer.Deserialize<TelemetryEvent>(jtr);
                    if (@event._T == "LogPlayerCreate")
                    {
                        string player = @event.character.name;
                        int teamId = @event.character.teamId;
                        teams.TryGetValue(teamId, out var team);
                        ArrayList als = new ArrayList();
                        if (team == null) teams[teamId] = team = new SortedSet<string>();
                        team.Add(player);
                        if (Squad.Contains(player))
                        {
                            squadTeamId = teamId;
                        }
                        AccountIds[player] = @event.character.accountId;
                    }
                    else if (@event._T == "LogMatchStart")
                    {
                        matchDate = @event._D;
                        break;
                    }
                }
                BeginInvoke((MethodInvoker)delegate ()
                {
                    if (squadTeamId != -1)
                    {
                        SortedSet<string> squadTeam = teams[squadTeamId];
                        file.Title = string.Join(", ", squadTeam);
                        file.Squad = squadTeam;
                    }
                    else
                    {
                        file.Title = "[no squad members in match]";
                    }
                    if (matchDate.HasValue)
                    {
                        file.Title += " - " + matchDate.Value.ToLocalTime().ToString(ChartTitleDateFormat);
                    }
                    listBoxMatches.BeginUpdate();
                    int topIdx = listBoxMatches.TopIndex;
                    int selIdx = listBoxMatches.SelectedIndex;
                    ((BindingSource)listBoxMatches.DataSource).ResetBindings(false);
                    listBoxMatches.TopIndex = topIdx;
                    listBoxMatches.SelectedIndex = selIdx;
                    listBoxMatches.EndUpdate();
                });
            }
        }

        private void PubgLookup(TelemetryFile file, string user)
        {
            string fname = file.FileInfo.Name;
            fname = Path.GetFileNameWithoutExtension(fname);
            const string pfx = "mt-";
            if (fname.StartsWith(pfx))
                fname = fname.Substring(pfx.Length);
            AccountIds.TryGetValue(user, out string accountId);
            if (accountId == null)
                return;
            Task.Run(() =>
            {
                string matchId = fname;
                Process.Start($"https://pubglookup.com/players/find/{accountId}/{matchId}");
            });
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var file = (TelemetryFile)listBoxMatches.SelectedItem;
            string sdt = file.Date.ToLocalTime().ToString(ChartTitleDateFormat);
            chart1.Titles[0].Text = $"{sdt} - HP over time";
            chart1.Titles[0].Font = new Font(chart1.Titles[0].Font.FontFamily, 12, FontStyle.Bold);
            CancellationTokenSourceMatchSwitch?.Cancel();
            CancellationTokenSourceMatchSwitch = new CancellationTokenSource();
            ClearChart(chart1);
            Task.Run(() => SwitchMatch(file, CancellationTokenSourceMatchSwitch.Token));
        }

        private void SwitchMatch(TelemetryFile file, CancellationToken cancellationToken)
        {
            try
            {
                PreparedData pd = new PreparedData() { File = file};
                using (var sr = new StreamReader(file.FileInfo.FullName))
                {
                    var events = JsonConvert.DeserializeObject<List<TelemetryEvent>>(sr.ReadToEnd());
                    foreach (var @event in events)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            DebugThreadWriteLine("Cancelling in foreach (TelemetryEvent @event in events)");
                            return;
                        }
                        if (@event._T == "LogPlayerTakeDamage")
                        {
                            @event.character = @event.victim;
                            @event.victim.health -= @event.damage;
                            pd.NormalizedEvents.Add(@event);
                        }
                        else if (@event._T == "LogPlayerPosition")
                        {
                            @event.victim = @event.character;
                            pd.NormalizedEvents.Add(@event);
                        }
                    }
                }

                foreach (var @event in pd.NormalizedEvents)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        DebugThreadWriteLine("Cancelling in foreach (var @event in normalizedEvents)");
                        return;
                    }
                    string name = @event.character.name;
                    // Skip events that are about players that aren't in our squad.
                    // When we start doing enemy interaction reports, might want to skip non-squad events
                    if (!Squad.Contains(name))
                        continue;
                    pd.Squad.Add(name);
                    var playerEvents = pd.PlayerToEvents.GetOrAdd(name, () => new List<TelemetryEvent>());
                    playerEvents.Add(@event);
                    var timePlayerToEvents = pd.TimeToPlayerToEvents.GetOrAdd(@event._D, () => new Dictionary<string, List<TelemetryEvent>>());
                    var timePlayerEvents = timePlayerToEvents.GetOrAdd(name, () => new List<TelemetryEvent>());
                    timePlayerEvents.Add(@event);
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    PreparedDataQ.Add(pd, cancellationToken);
                }
                else
                {
                    DebugThreadWriteLine("Not publishing prepared data due to cancellation.");
                }
            }
            finally
            {
                BeginInvoke((MethodInvoker)delegate ()
                {
                    ConsumePreparedDataQ(cancellationToken);
                });
            }
        }

        private static void DebugThreadWriteLine(string msg)
        {
            var t = Thread.CurrentThread;
            Debug.WriteLine($"T:{t.ManagedThreadId} {msg}");
        }

        private void ConsumePreparedDataQ(CancellationToken cancellationToken)
        {
            while (PreparedDataQ.Count > 0)
            {
                var preparedData = PreparedDataQ.Take();
                DebugThreadWriteLine("ConsumePreparedDataQ loop");
                if (preparedData.File == listBoxMatches.SelectedItem)
                {
                    DebugThreadWriteLine("ConsumePreparedDataQ inner");
                    ConsumePreparedData(preparedData, cancellationToken);
                }
            }
        }

        private void ConsumePreparedData(PreparedData pd, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                DebugThreadWriteLine("Cancelling in ConsumePreparedData");
                return;
            }
            ClearChart(chart1);
            // Declare series first
            foreach (string playerName in pd.PlayerToEvents.Keys)
            {
                var series = chart1.Series.Add(playerName);
                series.BorderWidth = 4;
                series.ChartType = SeriesChartType.Line;
                chart1.ChartAreas[0].AxisX.IsMarginVisible = false;
                series.SmartLabelStyle.Enabled = true;
                series.SmartLabelStyle.MovingDirection = LabelAlignmentStyles.Center;
                series.LabelFormat = "#";
                series.SmartLabelStyle.IsOverlappedHidden = true;
                series.XValueType = ChartValueType.DateTime;
            }
            // Track player's last known HP so we can fill in a reasonable value at missing time intervals
            var lastHps = new Dictionary<string, float>();
            // Then add data
            DebugThreadWriteLine("About to render " + pd.File.FileInfo.Name);
            foreach (var kv in pd.TimeToPlayerToEvents)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    DebugThreadWriteLine("Cancelling in foreach (var kv in timeToPlayerToEvents)");
                    return;
                }
                var eventGroupTime = kv.Key.ToLocalTime();
                var timePlayerToEvents = kv.Value;
                foreach (string squadMember in pd.Squad)
                {
                    timePlayerToEvents.TryGetValue(squadMember, out var squadEvents);
                    if (squadEvents != null)
                    {
                        chart1.Series[squadMember].Points.AddXY(eventGroupTime, squadEvents.First().character.health);
                        float minHp = squadEvents.Select(x => x.character.health).Min();
                        lastHps[squadMember] = minHp;
                    }
                    else
                    {
                        float lastHp = lastHps.GetValueOrDefault(squadMember, () => 100);
                        chart1.Series[squadMember].Points.AddXY(eventGroupTime, lastHp);
                    }
                }
            }
            RecalcPointLabels();
        }

        private void ClearChart(Chart chart)
        {
            chart.Series.ToList().ForEach(series => series.Points.Clear());
            foreach (var chart1Series in chart1.Series)
            {
                chart1Series.Points.Clear();
            }

            chart.Series.Clear();
        }

        public class PreparedData
        {
            public Dictionary<string, List<TelemetryEvent>> PlayerToEvents { get; } = 
                new Dictionary<string, List<TelemetryEvent>>();
            public Dictionary<DateTime, Dictionary<string, List<TelemetryEvent>>> TimeToPlayerToEvents { get; } =
                new Dictionary<DateTime, Dictionary<string, List<TelemetryEvent>>>();
            public List<TelemetryEvent> NormalizedEvents { get; } = new List<TelemetryEvent>();
            public HashSet<string> Squad { get; } = new HashSet<string>();
            public TelemetryFile File { get; set; }
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

        private void TextBoxSquad_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonRefresh.PerformClick();
            }
        }

        private void ButtonOptions_Click(object sender, EventArgs e)
        {
            var optionsForm = new OptionsForm { StartPosition = FormStartPosition.CenterParent };
            optionsForm.ShowDialog(this);
        }

        private void ListBoxMatches_MouseDown(object sender, MouseEventArgs e)
        {
            listBoxMatches.SelectedIndex = listBoxMatches.IndexFromPoint(e.Location);
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                if (e.KeyCode == Keys.F)
                {
                    buttonSearch.PerformClick();
                }
            }
            else if (e.KeyCode == Keys.F3)
            {
                MatchSearchNext();
            }
        }

        private void ButtonSearch_Click(object sender, EventArgs e)
        {
            var dialogResult = MatchSearchInputBox.ShowDialog(this);
            if (dialogResult != DialogResult.OK) return;
            MatchSearchNext();
        }

        private void MatchSearchNext()
        {
            string txt = MatchSearchInputBox.InputText;
            if (string.IsNullOrEmpty(txt)) return;

            bool Find(TelemetryFile telemetryFile)
            {
                var searchSquadSet = new HashSet<string>(RegexCsv.Split(txt));
                var titleSquadSet = new HashSet<string>(RegexCsv.Split(telemetryFile.Title));
                bool dateMatch = telemetryFile.Date.ToLocalTime().ToString(ChartTitleDateFormat).Contains(txt);
                bool matchIdMatch = telemetryFile.FileInfo.FullName.Contains(txt);
                bool squadSetMatch = searchSquadSet.IsSubsetOf(titleSquadSet);
                bool titleSubstringMatch = telemetryFile.Title.IndexOf(txt, StringComparison.OrdinalIgnoreCase) != -1;
                if (dateMatch || matchIdMatch || squadSetMatch || titleSubstringMatch)
                {
                    listBoxMatches.SelectedItem = telemetryFile;
                    return true;
                }

                return false;
            }

            for (int i = listBoxMatches.SelectedIndex + 1; i < listBoxMatches.Items.Count; i++)
            {
                var tf = (TelemetryFile) listBoxMatches.Items[i];
                if (Find(tf)) return;
            }

            for (int i = 0; i < listBoxMatches.SelectedIndex && i < listBoxMatches.Items.Count; i++)
            {
                var tf = (TelemetryFile) listBoxMatches.Items[i];
                if (Find(tf)) return;
            }
        }

        private void ButtonNext_Click(object sender, EventArgs e)
        {
            int rot = chart1.ChartAreas["Default"].Area3DStyle.Rotation;
            rot = (rot + 5) % 180;
            chart1.ChartAreas["Default"].Area3DStyle.Rotation = rot;
        }

        private void TsmiCopyPath_Click(object sender, EventArgs e)
        {
            var file = (TelemetryFile)listBoxMatches.SelectedItem;
            Clipboard.SetText(file.FileInfo.FullName);
        }

        private void TsmiOpenInFileExplorer_Click(object sender, EventArgs e)
        {
            var file = (TelemetryFile)listBoxMatches.SelectedItem;
            Process.Start("explorer.exe", "/select," + file.FileInfo.FullName);
        }

        private void ContextMenuMatches_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var file = (TelemetryFile)listBoxMatches.SelectedItem;
            if (file.Squad == null || file.Squad.Count == 0) return;
            tsmiPubgLookup.DropDownItems.Clear();
            foreach (string squadTeamMember in file.Squad)
            {
                ToolStripMenuItem tsmi = new ToolStripMenuItem(squadTeamMember, null, (_, __) =>
                {
                    PubgLookup(file, squadTeamMember);
                });
                tsmiPubgLookup.DropDownItems.Add(tsmi);
            }
        }
    }

}
