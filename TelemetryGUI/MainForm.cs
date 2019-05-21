using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Equin.ApplicationFramework;
using Newtonsoft.Json;

namespace MyPubgTelemetry.GUI
{
    public partial class MainForm : Form
    {
        private InputBox MatchSearchInputBox { get; }
        private const string ChartTitleDateFormat = "ddd M/d/yy h:mm tt";
        private const string XAxisDateFormat = "h:mm:ss tt";
        private HashSet<string> Squad { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private Regex RegexCsv { get; } = new Regex(@"\s*,\s*", RegexOptions.IgnoreCase);
        private ConcurrentDictionary<string, string> AccountIds { get; } = new ConcurrentDictionary<string, string>();
        private BlockingCollection<PreparedData> PreparedDataQ { get; } = new BlockingCollection<PreparedData>();
        private BlockingCollection<TelemetryFile> MatchMetaDataQ { get; } = new BlockingCollection<TelemetryFile>();
        private readonly TaskFactory _taskFactory;
        private bool ReloadingMetaData { get; set; }
        private CancellationTokenSource CtsMatchSwitch { get; set; }
        private CancellationTokenSource CtsMatchMetaData { get; set; }

        public MainForm()
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            InitChart();
            InitMatchesList();
            MatchSearchInputBox = new InputBox { InputText = Clipboard.GetText(), Text = @"Search match IDs, dates, and player names" };
            toolStripProgressBar1.TextChanged += delegate (object sender, EventArgs args)
            {
                toolStripStatusLabel1.Text = toolStripProgressBar1.Text;
            };
            var qts = new QueuedTaskScheduler(Environment.ProcessorCount - 1, "QTS", false, ThreadPriority.BelowNormal);
            _taskFactory = new TaskFactory(qts);
        }

        private void InitMatchesList()
        {
            dataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = SystemColors.Control;
            dataGridView1.RowHeadersDefaultCellStyle.SelectionBackColor = SystemColors.Control;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.Empty;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.AllowUserToResizeColumns = true;
            dataGridView1.MultiSelect = false;
            dataGridView1.BackgroundColor = SystemColors.ControlLightLight;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.CellFormatting += delegate (object sender, DataGridViewCellFormattingEventArgs e)
            {
                if (e.Value is DateTime value)
                {
                    switch (value.Kind)
                    {
                        case DateTimeKind.Local:
                            break;
                        case DateTimeKind.Unspecified:
                            e.Value = DateTime.SpecifyKind(value, DateTimeKind.Utc).ToLocalTime();
                            break;
                        case DateTimeKind.Utc:
                            e.Value = value.ToLocalTime();
                            break;
                    }
                }
            };
            //dataGridView1.DataSource = _dataTable;
            dataGridView1.KeyDown += delegate (object sender, KeyEventArgs args)
            {
                switch (args.KeyCode)
                {
                    case Keys.Enter:
                        args.Handled = true;
                        break;
                }
            };
            dataGridView1.MouseDown += delegate (object sender, MouseEventArgs args)
            {
                DataGridView.HitTestInfo hti = dataGridView1.HitTest(args.X, args.Y);
                if (hti.RowIndex >= 0)
                {
                    dataGridView1.Rows[hti.RowIndex].Selected = true;
                }
            };

            dataGridView1.ColumnCount = 3;

            dataGridView1.Columns[0].Name = "Name";
            dataGridView1.Columns[0].DataPropertyName = "Title";
            dataGridView1.Columns[0].ValueType = typeof(string);
            dataGridView1.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView1.Columns[0].Frozen = false;

            dataGridView1.Columns[1].Name = "Date";
            dataGridView1.Columns[1].DataPropertyName = "MatchDate";
            dataGridView1.Columns[1].ValueType = typeof(DateTime);
            dataGridView1.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView1.Columns[1].Frozen = false;
            dataGridView1.Columns[1].DefaultCellStyle.Format = ChartTitleDateFormat;

            dataGridView1.Columns[2].Name = "SquadKills";
            dataGridView1.Columns[2].DataPropertyName = "SquadKills";
            dataGridView1.Columns[2].ValueType = typeof(DateTime);
            dataGridView1.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView1.Columns[2].Frozen = false;
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

        private void LoadMatches(bool deep=false)
        {

            var squaddies = RegexCsv.Split(textBoxSquad.Text);
            int sumLen = squaddies.Sum(s => s.Length);
            if (sumLen == 0) return; // nothin' but whitespace and commas.
            Squad.Clear();
            Squad.UnionWith(squaddies);
            var di = new DirectoryInfo(TelemetryApp.App.TelemetryDir);
            List<FileInfo> jsonFiles = di.GetFiles("*.json").ToList();
            jsonFiles.AddRange(di.GetFiles("*.json.gz"));
            if (jsonFiles.Count == 0)
            {
                BeginInvoke((MethodInvoker)delegate ()
                {
                    MessageBox.Show("No telemetry files found.\nUse the separate TelemetryDownloader program to download." +
                                    "\nEventually the GUI will have support for downloading!");
                });
                return;
            }
            List<TelemetryFile> telFiles = jsonFiles.Select(jsonFile => new TelemetryFile { FileInfo = jsonFile, Title = "" }).ToList();
            telFiles.Sort((x, y) => y.FileInfo.CreationTime.CompareTo(x.FileInfo.CreationTime));
            var blv = new BindingListView<TelemetryFile>(telFiles);
            dataGridView1.DataSource = blv;
            toolStripProgressBar1.Maximum = telFiles.Count;
            toolStripProgressBar1.Visible = true;
            for (int col = 0; col < dataGridView1.ColumnCount; col++)
            {
                dataGridView1.Columns[col].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            CtsMatchMetaData?.Cancel();
            CtsMatchMetaData = new CancellationTokenSource();
            Task.Run(() => UpdateMatchListMetaData(telFiles, squaddies, deep, CtsMatchMetaData.Token));
        }

        private void UpdateMatchListMetaData(List<TelemetryFile> telFiles, string[] squaddies, bool deep, CancellationToken cancellationToken)
        {
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < telFiles.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested) break;
                TelemetryFile file = telFiles[i];
                file.Index = i;
                Task task = _taskFactory.StartNew(() => ReadTelemetryMetaData(file, squaddies, deep), cancellationToken);
                task.ContinueWith(continuationAction: (_) =>
                {
                    file.TelemetryMetaDataLoaded = true;
                    BeginInvoke((MethodInvoker)delegate ()
                   {
                       UiUpdateOneFile(file);
                   });
                }, cancellationToken);
                tasks.Add(task);
            }

            _taskFactory.ContinueWhenAny(tasks.ToArray(), (_) =>
            {
                BeginInvoke((MethodInvoker)delegate ()
               {
                   dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
               });
            }, cancellationToken);

            _taskFactory.ContinueWhenAll(tasks.ToArray(), (_) =>
            {
                DebugThreadWriteLine("Done loading metadata (worker).");
                BeginInvoke((MethodInvoker)delegate ()
               {
                   DebugThreadWriteLine("Done loading metadata (UI).");
                   ReloadingMetaData = false;
                    //dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
                    toolStripProgressBar1.Visible = false;
                   for (int col = 0; col < dataGridView1.ColumnCount; col++)
                   {
                       dataGridView1.Columns[col].SortMode = DataGridViewColumnSortMode.Automatic;
                   }
                   dataGridView1.Sort(dataGridView1.Columns[1], ListSortDirection.Descending);
               });
            }, cancellationToken);

            void UiUpdateOneFile(TelemetryFile telemetryFile)
            {
                int loadedCount = telFiles.Count(x => x.TelemetryMetaDataLoaded);
                toolStripProgressBar1.Text = $"Loaded {loadedCount} of {telFiles.Count} matches.";
                int fi = telemetryFile.Index;

                for (int row = 0; row < dataGridView1.RowCount; row++)
                {
                    for (int col = 0; col < dataGridView1.Columns.Count; col++)
                    {
                        dataGridView1.UpdateCellValue(col, row);
                    }
                }

                if (fi % 3 == 0 && fi < dataGridView1.DisplayedRowCount(true))
                {
                    dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
                }

                toolStripProgressBar1.Value = loadedCount;
            }
        }

        private void ReadTelemetryMetaData(TelemetryFile file, string[] squaddies, bool deep)
        {
            using (var fs = file.FileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            using (var bs = new BufferedStream(fs, 1024 * 10))
            using (var gs = new GZipStream(bs, CompressionMode.Decompress))
            using (var sr = new StreamReader(gs))
            using (var jtr = new JsonTextReader(sr))
            {
                var teams = new Dictionary<int, SortedSet<string>>();
                int squadTeamId = -1;
                jtr.Read();
                PreparedData pd = new PreparedData() {File = file};
                while (jtr.Read())
                {
                    if (jtr.TokenType == JsonToken.EndArray)
                    {
                        break;
                    }

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
                        file.MatchDate = @event._D;
                        if (!deep)
                        {
                            break;
                        }
                    }
                    else if (@event._T == "LogPlayerTakeDamage")
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
                    else if (@event._T == "LogPlayerKill")
                    {
                        if (@event.killer != null)
                        {
                            if (Squad.Contains(@event.killer.name))
                            {
                                if (@event.killer.name != @event.victim.name)
                                {
                                    file.SquadKills++;
                                }
                            }
                        }
                    }
                }

                foreach (var @event in pd.NormalizedEvents)
                {
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

                file.PreparedData = deep ? pd : null;


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
                if (file.MatchDate.HasValue)
                {
                    long matchTime = file.MatchDate.Value.ToFileTime();
                    DebugThreadWriteLine("Bout to call SetFileCreateTime");
                    if (file.FileInfo.CreationTimeUtc.ToFileTimeUtc() != matchTime)
                    {
                        fs.SafeFileHandle.SetFileCreateTime(matchTime);
                    }
                }
            }
        }

        private void PubgLookup(TelemetryFile file, string user)
        {
            string fname = file.FileInfo.Name;

            fname = TelemetryApp.TelemetryFilenameToMatchId(fname);

            AccountIds.TryGetValue(user, out string accountId);
            if (accountId == null) return;
            WebClient wc = new WebClient();
            var values = new NameValueCollection { { "region", "pc-na" }, { "player_name", user } };
            wc.UploadValues("https://pubglookup.com/search", "POST", values);
            Task.Run(() =>
            {
                string matchId = fname;
                //
                //TelemetryApp.App.OpenUrlInWebBrowser($"https://pubglookup.com/players/find/{accountId}/{matchId}");
                //                                     https://pubglookup.com/players/steam/celaven/matches/b1065fd9-eaaf-4bf9-aedc-3fec6947f7a8
                string url = $"https://pubglookup.com/players/steam/{user}/matches/{matchId}";
                DebugThreadWriteLine("url=" + url);
                TelemetryApp.App.OpenUrlInWebBrowser($"https://pubglookup.com/players/steam/{user}/matches/{matchId}");
            });
        }

        private void SwitchMatch(TelemetryFile file, CancellationToken cancellationToken)
        {
            try
            {
                //Thread.Sleep(5000);

                PreparedData pd = PrepareData(file, cancellationToken);

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

        private PreparedData PrepareData(TelemetryFile file, CancellationToken cancellationToken)
        {
            if (file.PreparedData != null)
            {
                return file.PreparedData;
            }
            file.PreparedData = null;

            var pd = new PreparedData() {File = file};
            using (StreamReader sr = file.NewTelemetryReader())
            {
                var events = JsonConvert.DeserializeObject<List<TelemetryEvent>>(sr.ReadToEnd());
                foreach (TelemetryEvent @event in events)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        DebugThreadWriteLine("Cancelling in foreach (TelemetryEvent @event in events)");
                        break;
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

            foreach (TelemetryEvent @event in pd.NormalizedEvents)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    DebugThreadWriteLine("Cancelling in foreach (var @event in normalizedEvents)");
                    break;
                }

                string name = @event.character.name;
                // Skip events that are about players that aren't in our squad.
                // When we start doing enemy interaction reports, might want to skip non-squad events
                if (!Squad.Contains(name))
                    continue;
                pd.Squad.Add(name);
                var playerEvents = pd.PlayerToEvents.GetOrAdd(name, () => new List<TelemetryEvent>());
                playerEvents.Add(@event);
                var timePlayerToEvents =
                    pd.TimeToPlayerToEvents.GetOrAdd(@event._D, () => new Dictionary<string, List<TelemetryEvent>>());
                var timePlayerEvents = timePlayerToEvents.GetOrAdd(name, () => new List<TelemetryEvent>());
                timePlayerEvents.Add(@event);
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                file.PreparedData = pd;
            }

            return pd;
        }

        private static void DebugThreadWriteLine(string msg)
        {
            Debug.WriteLine($"T:{Thread.CurrentThread} {msg}");
        }

        private void ConsumePreparedDataQ(CancellationToken cancellationToken)
        {
            while (PreparedDataQ.Count > 0)
            {
                var preparedData = PreparedDataQ.Take();
                DebugThreadWriteLine("ConsumePreparedDataQ loop");
                TelemetryFile selectedTf = GetSelectedMatch();
                if (preparedData.File == selectedTf)
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

            DateTime? localTime = pd.File?.MatchDate?.ToLocalTime();
            string sdt = "";
            if (localTime.HasValue)
            {
                sdt = localTime.Value.ToString(ChartTitleDateFormat) ?? "";
                sdt += " " + localTime.Value.GetTimeZoneAbbreviation();
            }
            chart1.Titles[0].Text = $"{sdt} \u2014 HP over time";
            chart1.Titles[0].Font = new Font(chart1.Titles[0].Font.FontFamily, 12, FontStyle.Bold);

            // Declare series first
            foreach (string playerName in pd.PlayerToEvents.Keys)
            {
                var series = chart1.Series.Add(playerName);
                series.BorderWidth = 3;
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
            chart.Titles[0].Text = "";
            chart.Series.ToList().ForEach(series => series.Points.Clear());
            foreach (var chart1Series in chart1.Series)
            {
                chart1Series.Points.Clear();
            }

            chart.Series.Clear();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                LoadMatches(true);
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

        private void SelectMatch(TelemetryFile path)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                ObjectView<TelemetryFile> ovtf = (ObjectView<TelemetryFile>)row.DataBoundItem;
                if (ovtf.Object == path)
                {
                    row.Selected = true;
                    dataGridView1.FirstDisplayedScrollingRowIndex = row.Index;
                    break;
                }
            }
        }

        // TODO
        private void MatchSearchNext()
        {
            string txt = MatchSearchInputBox.InputText;
            if (string.IsNullOrEmpty(txt)) return;

            bool Find(TelemetryFile telemetryFile)
            {
                var searchSquadSet = new HashSet<string>(RegexCsv.Split(txt));
                var titleSquadSet = new HashSet<string>(RegexCsv.Split(telemetryFile.Title));
                bool dateMatch = telemetryFile.MatchDate?.ToLocalTime().ToString(ChartTitleDateFormat).Contains(txt) ?? false;
                bool matchIdMatch = telemetryFile.FileInfo.FullName.Contains(txt);
                bool squadSetMatch = searchSquadSet.IsSubsetOf(titleSquadSet);
                bool titleSubstringMatch = telemetryFile.Title.IndexOf(txt, StringComparison.OrdinalIgnoreCase) != -1;
                if (dateMatch || matchIdMatch || squadSetMatch || titleSubstringMatch)
                {
                    SelectMatch(telemetryFile);
                    return true;
                }

                return false;
            }

            int selIdx = MatchListGetSelectedIndex();
            int rowCount = dataGridView1.RowCount;
            for (int i = selIdx + 1; i < rowCount; i++)
            {
                TelemetryFile tf = MatchListGetValueAtIndex(i);
                if (Find(tf)) return;
            }

            for (int i = 0; i < selIdx && i < rowCount; i++)
            {
                TelemetryFile tf = MatchListGetValueAtIndex(i);
                if (Find(tf)) return;
            }
        }

        private void ButtonNext_Click(object sender, EventArgs e)
        {
            int rot = chart1.ChartAreas[0].Area3DStyle.Rotation;
            rot = (rot + 5) % 180;
            chart1.ChartAreas[0].Area3DStyle.Rotation = rot;
        }

        private void TsmiCopyPath_Click(object sender, EventArgs e)
        {
            TelemetryFile file = GetSelectedMatch();
            string filename = file.FileInfo.Name;
            var regex = new Regex(@"^(..-)?(?<id>[^.]+)\.json(\.gz)?$", RegexOptions.Multiline);
            Match match = regex.Match(filename);
            if (match.Success)
            {
                Clipboard.SetText(match.Groups["id"].Value);
            }
        }

        private void TsmiOpenInFileExplorer_Click(object sender, EventArgs e)
        {
            var file = GetSelectedMatch();
            Process.Start("explorer.exe", "/select," + file.FileInfo.FullName);
        }

        private void ContextMenuMatches_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var file = GetSelectedMatch();
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

        private int MatchListGetSelectedIndex()
        {
            DataGridViewRow row = dataGridView1.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();
            return row?.Index ?? 0;
        }

        private TelemetryFile MatchListGetValueAtIndex(int i)
        {
            var dataBoundItem = (ObjectView<TelemetryFile>)dataGridView1.Rows[i].DataBoundItem;
            return dataBoundItem.Object;
        }

        private static T GetPrimarySelectedValue<T>(DataGridView dgv) where T : class
        {
            DataGridViewRow row = dgv.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();
            ObjectView<T> objectView = (ObjectView<T>)row?.DataBoundItem;
            return objectView?.Object;
        }

        private TelemetryFile GetSelectedMatch()
        {
            TelemetryFile o = GetPrimarySelectedValue<TelemetryFile>(dataGridView1);
            DebugThreadWriteLine("GetSelectedMatch: " + o);
            return o;
        }

        private TelemetryFile RowToTelemetryFile(DataGridViewRow row)
        {
            ObjectView<TelemetryFile> ovtf = (ObjectView<TelemetryFile>)row.DataBoundItem;
            return ovtf.Object;
        }

        private bool AllMatchesMetaDataLoaded()
        {
            return dataGridView1.Rows.Cast<DataGridViewRow>().All(x => RowToTelemetryFile(x).TelemetryMetaDataLoaded);
        }

        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            //if (ReloadingMetaData)
            //{
            //    DebugThreadWriteLine("DataGridView1_SelectionChanged - ignoring; still reloading metadata");
            //    return;
            //}
            var file = GetSelectedMatch();
            DebugThreadWriteLine("DataGridView1_SelectionChanged: " + file?.MatchDate + " " + file?.FileInfo.Name);
            if (file == null)
            {
                return;
            }
            CtsMatchSwitch?.Cancel();
            CtsMatchSwitch = new CancellationTokenSource();
            ClearChart(chart1);
            Task.Run(() => SwitchMatch(file, CtsMatchSwitch.Token));
        }
    }
}