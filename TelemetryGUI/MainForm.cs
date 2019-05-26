﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Equin.ApplicationFramework;
using MyPubgTelemetry.ApiMatchModel;
using MyPubgTelemetry.Downloader;
using MyPubgTelemetry.GUI.Properties;
using Newtonsoft.Json;

namespace MyPubgTelemetry.GUI
{
    public partial class MainForm : Form
    {
        public ViewModel ViewModel { get; set; }

        public MainForm()
        {
            InitializeComponent();
            InitChart();
            InitMatchesDataGridView();
            InitToolStrip();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            ViewModel = new ViewModel(this);
            var qts = new QueuedTaskScheduler(Environment.ProcessorCount - 1, "QTS", false, ThreadPriority.Lowest);
            ViewModel.TaskFactory = new TaskFactory(qts);
            ViewModel.MatchSearchInputBox = new InputBox {InputText = Clipboard.GetText(), Text = @"Search match IDs, dates, and player names"};
            ViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "DownloadActive" || args.PropertyName == "ReloadActive")
                {
                    bool loadingActive = ViewModel.DownloadActive || ViewModel.ReloadActive;
                    buttonRefresh.Enabled = !loadingActive;
                }
            };
        }

        private void InitToolStrip()
        {
            toolStripProgressBar1.TextChanged += delegate(object sender, EventArgs args)
            {
                toolStripStatusLabel1.Text = toolStripProgressBar1.Text;
            };
            toolStripStatusLabel1.TextChanged += delegate(object sender, EventArgs args)
            {
                ViewModel.LogBuffer.Append(toolStripStatusLabel1.Text.Trim());
                ViewModel.LogBuffer.Append(Environment.NewLine);
                if (ViewModel.LogBuffer.Length > 500000) ViewModel.LogBuffer.Remove(0, 100000);
            };
        }

        private void InitMatchesDataGridView()
        {
            // Visually faster repainting
            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null,
                dataGridView1,
                new object[] {true});

            TweakColumnHeaderPadding();

            dataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = SystemColors.Control;
            dataGridView1.RowHeadersDefaultCellStyle.SelectionBackColor = SystemColors.Control;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.Empty;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.AllowUserToResizeColumns = true;
            dataGridView1.AllowUserToOrderColumns = true;
            dataGridView1.MultiSelect = false;
            dataGridView1.BackgroundColor = SystemColors.ControlLightLight;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.CellFormatting += delegate(object sender, DataGridViewCellFormattingEventArgs e)
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

                if (e.Value is float floatValue)
                {
                    e.Value = float.Parse(floatValue.ToString("0.00"));
                }
                else if (e.Value is double doubleValue)
                {
                    e.Value = double.Parse(doubleValue.ToString("0.00"));
                }
            };
            //dataGridView1.DataSource = _dataTable;
            dataGridView1.KeyDown += delegate(object sender, KeyEventArgs args)
            {
                switch (args.KeyCode)
                {
                    case Keys.Enter:
                        args.Handled = true;
                        break;
                }
            };
            dataGridView1.MouseDown += delegate(object sender, MouseEventArgs args)
            {
                DataGridView.HitTestInfo hti = dataGridView1.HitTest(args.X, args.Y);
                if (hti.RowIndex >= 0)
                {
                    dataGridView1.Rows[hti.RowIndex].Selected = true;
                }
            };

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Squad",
                DataPropertyName = "Title",
                ValueType = typeof(string),
                SortMode = DataGridViewColumnSortMode.NotSortable,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Frozen = false,
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Date",
                DataPropertyName = "MatchDate",
                ValueType = typeof(DateTime),
                SortMode = DataGridViewColumnSortMode.NotSortable,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Frozen = false,
                DefaultCellStyle = new DataGridViewCellStyle {Format = ViewModel.ChartTitleDateFormat}
            });

            foreach (PropertyInfo pi in typeof(MatchModelStats).GetProperties())
            {
                if (pi.PropertyType.IsValueType || pi.PropertyType == typeof(string))
                {
                    AddSquadStatColumn(pi);
                }
            }

            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                
                column.HeaderCell.Style.Padding = new Padding(-10);
                column.DividerWidth = 0;
                //column.DataGridView.ColumnHeadersDefaultCellStyle.Padding = new Padding(-10);
            }
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private void TweakColumnHeaderPadding()
        {
            try
            {
                DataGridViewCellStyle cellStyle = dataGridView1.ColumnHeadersDefaultCellStyle;
                FieldInfo fi_DataGridViewCellStyle_propertyStore = cellStyle.GetType().GetField("propertyStore", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo fi_DataGridViewCellStyle_PropPadding = cellStyle.GetType().GetField("PropPadding", BindingFlags.NonPublic | BindingFlags.Static);
                int propId = (int?)fi_DataGridViewCellStyle_PropPadding?.GetValue(null) ?? -1;
                object propertyStore = fi_DataGridViewCellStyle_propertyStore?.GetValue(cellStyle);
                Type t_PropertyStore = propertyStore?.GetType();
                MethodInfo mi_PropertyStore_SetPadding = t_PropertyStore?.GetMethod("SetPadding");
                Padding padding = new Padding(-2, 0, -2, 0);
                mi_PropertyStore_SetPadding?.Invoke(propertyStore, new object[] { propId, padding });
                Padding gp = cellStyle.Padding;
                Debug.WriteLine($"{gp.Left} {gp.Right} {gp.Top} {gp.Bottom}");
            }
            catch (Exception)
            {
                // unimportant attempt to tighten up the text margins in the DGV header labels
            }
        }

        private void AddSquadStatColumn(PropertyInfo pi)
        {
            var ignore = new HashSet<string>
            {
                "DeathType", "KillPointsDelta", "KillPoints", "KillStreaks", "LastKillPoints", "Name", "PlayerId", "MostDamage", "RankPoints", "WinPoints",
                "WinPointsDelta", "TeamId", "LastWinPoints"
            };
            var rename = new Dictionary<string, string>()
            {
                {"DamageDealt", "Damage"},
                {"Revives", "Revive"},
                {"RoadKills", "RoadK"},
                {"HeadshotKills", "HeadsK"},
                {"VehicleDestroys", "V.Destroy"},
                {"LongestKill", "LongestK"},
            };

            Type pt = pi.PropertyType;
            string name = pi.Name;
            if (ignore.Contains(name))
            {
                return;
            }

            string displayName = rename.GetValueOrDefault(name, name);
            if (displayName.EndsWith("s"))
            {
                displayName = displayName.Substring(0, displayName.Length - 1);
            }
            displayName = displayName.Replace("Distance", "");
            DebugThreadWriteLine($"name={name}, type={pt}");

            AddStatColumn(displayName, name, pt, "0");
        }

        private void AddStatColumn(string displayName, string dataName, Type valueType, string format)
        {
            DataGridViewColumn col = new DataGridViewTextBoxColumn();
            col.Name = displayName;
            col.DataPropertyName = dataName;
            col.ValueType = valueType;
            col.SortMode = DataGridViewColumnSortMode.Automatic;
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            col.Frozen = false;
            if (format != null)
            {
                col.DefaultCellStyle.Format = format;
            }
            dataGridView1.Columns.Add(col);
        }

        private void InitChart()
        {
            chart1.Tag = new ChartTag();
            chart1.Titles.Add("");
            //chart1.Palette = ChartColorPalette.Berry;

            chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart1.ChartAreas[0].CursorX.LineColor = Color.Black;
            chart1.ChartAreas[0].CursorX.SelectionColor = Color.CornflowerBlue;
            chart1.ChartAreas[0].CursorX.IntervalType = DateTimeIntervalType.Seconds;
            chart1.ChartAreas[0].CursorX.Interval = 1;

            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = false;
            chart1.ChartAreas[0].AxisX.ScaleView.SmallScrollMinSize = 1;
            chart1.ChartAreas[0].AxisX.ScaleView.SmallScrollMinSizeType = DateTimeIntervalType.Seconds;
            chart1.ChartAreas[0].AxisX.ScaleView.SmallScrollSize = 10;
            chart1.ChartAreas[0].AxisX.ScaleView.SmallScrollSizeType = DateTimeIntervalType.Seconds;
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "mm'm'ss's'";
            chart1.ChartAreas[0].AxisX.IsLabelAutoFit = true;
            chart1.ChartAreas[0].AxisX.LabelAutoFitStyle = LabelAutoFitStyles.LabelsAngleStep30;
            chart1.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Minutes;
            chart1.ChartAreas[0].AxisX.Interval = 1;
            chart1.ChartAreas[0].AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;

            chart1.AxisViewChanged += delegate(object sender, ViewEventArgs args)
            {
                chart1.ChartAreas[0].AxisX.Interval = chart1.ChartAreas[0].AxisX.Interval / 10;
                RecalcPointLabels();
                var scaleView = chart1.ChartAreas[0].AxisX.ScaleView;
                double delta = scaleView.ViewMaximum - scaleView.ViewMinimum;
                bool zoomed = chart1.ChartAreas[0].AxisX.ScaleView.IsZoomed;
                chart1.ChartAreas[0].AxisX.Interval = zoomed ? delta * 100 : 0;
            };
            chart1.MouseWheel += delegate(object sender, MouseEventArgs args)
            {
                DebugThreadWriteLine("wheel delta=" + args.Delta);
                //ChartTag tag = (ChartTag) chart1.Tag;
                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    var xax = chart1.ChartAreas[0].AxisX;
                    //var yax = chart1.ChartAreas[0].AxisY;
                    if (args.Delta > 0)
                    {
                        var xMin = xax.ScaleView.ViewMinimum;
                        var xMax = xax.ScaleView.ViewMaximum;
                        var xLen = xMax - xMin;
                        var xDiv = xLen / 4;
                        var posXStart = xax.PixelPositionToValue(args.Location.X) - xDiv;
                        var posXFinish = xax.PixelPositionToValue(args.Location.X) + xDiv;
                        xax.ScaleView.PrivateZoom(posXStart, posXFinish - posXStart, DateTimeIntervalType.Number, true, true);
                    }
                    else
                    {
                        xax.ScaleView.PrivateZoomReset(1, true);
                    }
                }
                else
                {
                    if (!chart1.ChartAreas[0].AxisX.ScaleView.IsZoomed)
                    {
                        return;
                    }

                    ScrollType? scrollType = null;
                    if (ModifierKeys == Keys.None)
                        scrollType = args.Delta < 0 ? ScrollType.SmallIncrement : ScrollType.SmallDecrement;
                    else if (ModifierKeys == Keys.Shift)
                        scrollType = args.Delta < 0 ? ScrollType.LargeIncrement : ScrollType.LargeDecrement;
                    if (scrollType.HasValue)
                        chart1.ChartAreas[0].AxisX.ScaleView.Scroll(scrollType.Value);
                }
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
            bool zoomedIn = chart1.ChartAreas[0].AxisX.ScaleView.IsZoomed; //scaleViewSize < 0.01;
            //series.SmartLabelStyle.MaxMovingDistance = 0;
            chart1.Series.ToList().ForEach(x => x.IsValueShownAsLabel = zoomedIn);
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = zoomedIn ? "m'm'ss's'" : "m'm'";
        }

        // ReSharper disable once UnusedMember.Local
        private static object GetField(object instance, string fieldName)
        {
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var field = instance.GetType().GetField(fieldName, bindFlags);
            return field?.GetValue(instance);
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            var path = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            DebugThreadWriteLine("Config path = " + path);
            textBoxSquad.Text = Settings.Default.Squad.Trim();
            string envNoDlOnStart = Environment.GetEnvironmentVariable("MYPUBGTELEMETRY_NODLONSTART");
            bool bNoDlOnStart = TelemetryApp.ToBooly(envNoDlOnStart);
            ViewModel.PlayerColorPalette = GetPaletteColors(chart1.Palette);
            if (!string.IsNullOrWhiteSpace(Settings.Default.PlayerColors))
            {
                ViewModel.PlayerColors = JsonConvert.DeserializeObject<Dictionary<string, Color>>(Settings.Default.PlayerColors);
            }

            if (bNoDlOnStart)
            {
                LoadMatches();
            }
            else
            {
                await DownloadAndRefresh();
            }

            comboBoxStatsFocus.SelectedIndex = 0;
        }

        private async void LoadMatches(bool deep = false)
        {
            if (!ValidateInputSquad())
            {
                return;
            }

            var squaddies = new HashSet<string>(ViewModel.RegexCsv.Split(textBoxSquad.Text));
            ViewModel.ReloadActive = true;
            ViewModel.Squad.Clear();
            ViewModel.Squad.UnionWith(squaddies);
            var di = new DirectoryInfo(TelemetryApp.App.TelemetryDir);
            List<FileInfo> jsonFiles = di.GetFiles("*.json").ToList();
            jsonFiles.AddRange(di.GetFiles("*.json.gz"));
            if (jsonFiles.Count == 0)
            {
                BeginInvoke((MethodInvoker) delegate()
                {
                    MessageBox.Show("No telemetry files found!");
                });
                ViewModel.ReloadActive = false;
                return;
            }

            List<TelemetryFile> telFiles = jsonFiles.Select(jsonFile => new TelemetryFile {FileInfo = jsonFile, Title = ""}).ToList();
            telFiles.Sort((x, y) => y.FileInfo.CreationTime.CompareTo(x.FileInfo.CreationTime));
            var blv = new BindingListView2<TelemetryFile>(telFiles);
            dataGridView1.DataSource = blv;
            toolStripProgressBar1.Maximum = telFiles.Count;
            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Visible = true;
            dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.EnableResizing;
            for (int col = 0; col < dataGridView1.ColumnCount; col++)
            {
                dataGridView1.Columns[col].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            ViewModel.CtsMatchMetaData?.Cancel();
            ViewModel.CtsMatchMetaData = new CancellationTokenSource();
            await Task.Run(() => UpdateMatchListMetaData(telFiles, squaddies, deep, ViewModel.CtsMatchMetaData.Token));
        }

        private bool ValidateInputSquad()
        {
            var squaddies = ViewModel.RegexCsv.Split(textBoxSquad.Text);
            if (squaddies.Sum(s => s.Length) == 0)
            {
                BeginInvoke((MethodInvoker) delegate
                {
                    toolTipBalloon.Show("", textBoxSquad, 0);
                    var msg = "Enter some player names (case sensitive, comma separated) and then press refresh.";
                    toolTipBalloon.Show(msg, textBoxSquad, 15, textBoxSquad.Height - 5, 7500);
                });
                return false;
            }

            return true;
        }

        private void UpdateMatchListMetaData(List<TelemetryFile> telFiles, IEnumerable<string> squaddies, bool deep, CancellationToken cancellationToken)
        {
            void UiUpdateOneFile(TelemetryFile telemetryFile)
            {
                int loadedCount = telFiles.Count(x => x.TelemetryMetaDataLoaded);
                toolStripProgressBar1.Text = $"Loaded {loadedCount} of {telFiles.Count} matches.";
                int fi = telemetryFile.Index;
                if (fi <= dataGridView1.DisplayedRowCount(true))
                {
                    dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
                }

                toolStripProgressBar1.Value = loadedCount;
            }

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < telFiles.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested) break;
                TelemetryFile file = telFiles[i];
                file.Index = i;
                Task task = ViewModel.TaskFactory.StartNew(() => ReadTelemetryMetaData(file, squaddies, deep), cancellationToken);
                task.ContinueWith((_) =>
                {
                    file.TelemetryMetaDataLoaded = true;
                    BeginInvoke((MethodInvoker) delegate
                    {
                        UiUpdateOneFile(file);
                    });
                }, cancellationToken);
                tasks.Add(task);
            }

            ViewModel.TaskFactory.ContinueWhenAny(tasks.ToArray(), (_) =>
            {
                BeginInvoke((MethodInvoker) delegate()
                {
                    dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
                });
            }, cancellationToken);

            ViewModel.TaskFactory.ContinueWhenAll(tasks.ToArray(), (_) =>
            {
                DebugThreadWriteLine("Done loading metadata (worker).");
                BeginInvoke((MethodInvoker) delegate()
                {
                    DebugThreadWriteLine("Done loading metadata (UI).");
                    dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                    toolStripProgressBar1.Visible = false;
                    for (int col = 0; col < dataGridView1.ColumnCount; col++)
                    {
                        dataGridView1.Columns[col].SortMode = DataGridViewColumnSortMode.Automatic;
                    }

                    dataGridView1.Sort(dataGridView1.Columns[1], ListSortDirection.Descending);
                    ViewModel.ReloadActive = false;
                });
            }, cancellationToken);
        }

        private void ReadTelemetryMetaData(TelemetryFile file, IEnumerable<string> squaddies, bool deep)
        {
            NormalizedMatch normalizedMatch = ReadMatchMetaData(file);
            file.NormalizedMatch = normalizedMatch;
            NormalizedRoster roster = normalizedMatch.Rosters.FirstOrDefault(r =>
            {
                HashSet<string> rosterPlayers = r.Players.Select(p => p.Attributes.Stats.Name.ToLower()).ToHashSet();
                HashSet<string> lowerSquaddies = squaddies.Select(s => s.ToLower()).ToHashSet();
                rosterPlayers.IntersectWith(lowerSquaddies);
                return rosterPlayers.Count > 0;
            });

            // Foreach stat that looks numeric, sum it across each player on the roster -- even if that stat doesn't make sense as a sum stat :)
            MatchModelStats sqst = file;
            foreach (PropertyInfo pi in typeof(MatchModelStats).GetProperties())
            {
                if (roster == null) break;
                if (pi.PropertyType == typeof(long?))
                {
                    long? sum = roster.Players.Sum(p => (long?) pi.GetValue(p.Attributes?.Stats));
                    pi.SetValue(sqst, sum);
                }
                else if (pi.PropertyType == typeof(double?))
                {
                    double? sum = roster.Players.Sum(p => (double?) pi.GetValue(p.Attributes?.Stats));
                    pi.SetValue(sqst, sum);
                }
                else if (typeof(long).IsAssignableFrom(pi.PropertyType))
                {
                    long sum = roster.Players.Sum(p => (long) (pi.GetValue(p.Attributes?.Stats) ?? 0));
                    pi.SetValue(sqst, sum);
                }
                else if (typeof(double).IsAssignableFrom(pi.PropertyType))
                {
                    double sum = roster.Players.Sum(p => (double) (pi.GetValue(p.Attributes?.Stats) ?? 0));
                    pi.SetValue(sqst, sum);
                }
            }

            // Don't report rank as a sum stat, just set it directly from the roster.
            // "999" -- no matching roster was found -- fudge a value that won't overlap with real ranks.
            sqst.Rank = roster?.Roster.Attributes.Stats.Rank ?? 999;

            using (var sr = file.NewMatchMetaDataReader(FileMode.Open, FileAccess.ReadWrite, FileShare.Read, out FileStream fs))
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
                        if (team == null) teams[teamId] = team = new SortedSet<string>();
                        team.Add(player);
                        if (ViewModel.Squad.Contains(player))
                        {
                            squadTeamId = teamId;
                        }

                        ViewModel.AccountIds[player] = @event.character.accountId;
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
                            if (ViewModel.Squad.Contains(@event.killer.name))
                            {
                                if (@event.killer.name != @event.victim.name)
                                {
                                    //This is now being calculated from match metadata
                                    //file.SquadKills++;
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
                    if (!ViewModel.Squad.Contains(name))
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
                    if (file.FileInfo.CreationTimeUtc.ToFileTimeUtc() != matchTime)
                    {
                        fs.SafeFileHandle.SetFileCreateTime(matchTime);
                    }
                }
            }
        }

        private static NormalizedMatch ReadMatchMetaData(TelemetryFile file)
        {
            string mid = file.GetMatchId();
            string mmn = $"mm-{mid}.json";
            string mmf = Path.Combine(TelemetryApp.App.MatchDir, mmn);
            string jsonStr = File.ReadAllText(mmf);
            return TelemetryDownloader.NormalizeMatch(mid, jsonStr);
        }

        private void PubgLookup(TelemetryFile file, string user)
        {
            string fname = file.FileInfo.Name;

            fname = TelemetryApp.TelemetryFilenameToMatchId(fname);

            ViewModel.AccountIds.TryGetValue(user, out string accountId);
            if (accountId == null) return;
            WebClient wc = new WebClient();
            var values = new NameValueCollection {{"region", "pc-na"}, {"player_name", user}};
            wc.UploadValues("https://pubglookup.com/search", "POST", values);
            Task.Run(() =>
            {
                string matchId = fname;
                string url = $"https://pubglookup.com/players/steam/{user}/matches/{matchId}";
                DebugThreadWriteLine("url=" + url);
                TelemetryApp.App.OpenUrlInWebBrowser($"https://pubglookup.com/players/steam/{user}/matches/{matchId}");
            });
        }

        private void SwitchMatch(TelemetryFile file, CancellationToken cancellationToken)
        {
            try
            {
                PreparedData pd = PrepareData(file, cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                {
                    ViewModel.PreparedDataQ.Add(pd, cancellationToken);
                }
                else
                {
                    DebugThreadWriteLine("Not publishing prepared data due to cancellation.");
                }
            }
            finally
            {
                BeginInvoke((MethodInvoker) delegate()
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
                if (!ViewModel.Squad.Contains(name))
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

        private static void DebugThreadWriteLine(string msg, [CallerMemberName] string caller = "")
        {
            Debug.WriteLine($"T:{Thread.CurrentThread.Name}{Thread.CurrentThread.ManagedThreadId} C:{caller} {msg}");
        }

        private void ConsumePreparedDataQ(CancellationToken cancellationToken)
        {
            while (ViewModel.PreparedDataQ.Count > 0)
            {
                var preparedData = ViewModel.PreparedDataQ.Take();
                DebugThreadWriteLine("ConsumePreparedDataQ loop");
                TelemetryFile selectedTf = GetSelectedMatch();
                if (preparedData.File == selectedTf)
                {
                    DebugThreadWriteLine("ConsumePreparedDataQ inner");
                    ConsumePreparedData(preparedData, cancellationToken);
                }
            }
        }

        private Font DeriveFont(Font orig, int sizeAdj)
        {
            return new Font(orig.FontFamily, orig.Size + sizeAdj);
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
                sdt = localTime.Value.ToString(ViewModel.ChartTitleDateFormat);
                sdt += " " + localTime.Value.GetTimeZoneAbbreviation();
            }

            chart1.ChartAreas[0].AxisY.Title = "HP: 0 to 100";
            chart1.ChartAreas[0].AxisY.TitleFont = DeriveFont(chart1.ChartAreas[0].AxisY.TitleFont, 0);

            chart1.ChartAreas[0].AxisX.Title = "Elapsed time since start of match (in minutes).";
            chart1.ChartAreas[0].AxisX.TitleFont = DeriveFont(chart1.ChartAreas[0].AxisX.TitleFont, 0);

            ISet<string> squad = pd.File?.Squad ?? new HashSet<string>();
            NormalizedRoster roster =
                pd.File?.NormalizedMatch?.Rosters?.FirstOrDefault(r => r.Players?.Any(p => squad.Contains(p?.Attributes?.Stats?.Name)) ?? false);
            chart1.Titles[0].Text = $"HP over time for one match -- {sdt} -- Squad Rank: {roster?.Roster.Attributes.Stats.Rank}";
            chart1.Titles[0].Font = new Font(chart1.Titles[0].Font.FontFamily, 12, FontStyle.Bold);

            // Declare series first
            foreach (string playerName in pd.PlayerToEvents.Keys)
            {
                var series = chart1.Series.Add(playerName);
                series.Color = ViewModel.ColorForPlayer(playerName);
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
            DebugThreadWriteLine("About to render " + pd.File?.FileInfo.Name);
            DateTime t0 = pd.TimeToPlayerToEvents.Keys.FirstOrDefault().ToLocalTime();
            foreach (var kv in pd.TimeToPlayerToEvents)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    DebugThreadWriteLine("Cancelling in foreach (var kv in timeToPlayerToEvents)");
                    return;
                }

                TimeSpan eventTimeOffset = kv.Key.ToLocalTime().Subtract(t0);
                DateTime dt = new DateTime(eventTimeOffset.Ticks);
                var timePlayerToEvents = kv.Value;
                foreach (string squadMember in pd.Squad)
                {
                    timePlayerToEvents.TryGetValue(squadMember, out var squadEvents);
                    if (squadEvents != null)
                    {
                        chart1.Series[squadMember].Points.AddXY(dt, squadEvents.First().character.health);
                        float minHp = squadEvents.Select(x => x.character.health).Min();
                        lastHps[squadMember] = minHp;
                    }
                    else
                    {
                        float lastHp = lastHps.GetValueOrDefault(squadMember, () => 100);
                        chart1.Series[squadMember].Points.AddXY(dt, lastHp);
                    }
                }
            }

            RecalcPointLabels();
        }

        private List<Color> GetPaletteColors(ChartColorPalette palette)
        {
            Type type = typeof(Chart).Assembly.GetType("System.Windows.Forms.DataVisualization.Charting.Utilities.ChartPaletteColors");
            BindingFlags bf = BindingFlags.Static | BindingFlags.Public;
            MethodInfo mi = type.GetMethod("GetPaletteColors", bf);
            object ret = mi?.Invoke(null, new object[] {palette});
            if (ret is Color[] colors)
            {
                return colors.ToList();
            }

            return new List<Color>();
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

        private async void ButtonRefresh_Click(object sender, EventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                LoadMatches(true);
            }
            else if (ModifierKeys.HasFlag(Keys.Shift))
            {
                LoadMatches();
            }
            else
            {
                await DownloadAndRefresh();
            }
        }

        private async Task DownloadAndRefresh()
        {
            if (!PreFlight())
            {
                return;
            }

            TelemetryDownloader downloader = new TelemetryDownloader();
            var squadSet = new HashSet<string>(ViewModel.RegexCsv.Split(textBoxSquad.Text));
            string squad = string.Join(",", squadSet);
            downloader.DownloadProgressEvent += (sender2, args) =>
            {
                BeginInvoke((MethodInvoker) delegate()
                {
                    if (!args.Rewrite)
                    {
                        DebugThreadWriteLine("DownloadProgressEvent " + args.Msg);
                    }

                    toolStripProgressBar1.Visible = !args.Complete;
                    toolStripProgressBar1.Maximum = (int) args.Max;
                    toolStripProgressBar1.Value = (int) args.Value;
                    toolStripStatusLabel1.Text = args.Msg;
                });
            };
            ViewModel.DownloadActive = true;
            try
            {
                List<NormalizedMatch> matches = await downloader.DownloadForPlayersAsync(squad);
                //JsonSerializerSettings settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
                //string jsonStr = JsonConvert.SerializeObject(matches, Formatting.Indented, settings);
                //DebugThreadWriteLine("Normalized matches:\n" + jsonStr);
                DebugThreadWriteLine("# Normalized matches: " + matches.Count);
            }
            catch (Exception hre)
            {
                string msg = hre.InnerException?.Message + "\nCheck that your API key is set correctly.";
                MessageBox.Show(msg, "HTTP Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                ViewModel.DownloadActive = false;
            }

            LoadMatches();
        }

        private bool PreFlight()
        {
            if (!ValidateInputSquad())
                return false;
            if (!ValidateApiKey())
                return false;
            return true;
        }

        private bool ValidateApiKey()
        {
            if (string.IsNullOrWhiteSpace(TelemetryApp.App.ApiKey))
            {
                BeginInvoke((MethodInvoker) delegate()
                {
                    var msg = "Your API Key is not set. Please go to the options dialog and paste a valid API Key.";
                    toolTipBalloon.Show("", buttonOptions, 0);
                    toolTipBalloon.Show(msg, buttonOptions);
                });
                return false;
            }

            return true;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.PlayerColors = JsonConvert.SerializeObject(ViewModel.PlayerColors);
            Settings.Default.Squad = textBoxSquad.Text;
            Settings.Default.Save();
        }

        private void TextBoxSquad_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonRefresh.PerformClick();
            }
        }

        private async void ButtonOptions_Click(object sender, EventArgs e)
        {
            var optionsForm = new OptionsForm {StartPosition = FormStartPosition.CenterParent, LogText = ViewModel.LogBuffer.ToString()};
            DialogResult dialogResult = optionsForm.ShowDialog(this);
            if (dialogResult == DialogResult.OK)
            {
                await DownloadAndRefresh();
            }
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
            var dialogResult = ViewModel.MatchSearchInputBox.ShowDialog(this);
            if (dialogResult != DialogResult.OK) return;
            MatchSearchNext();
        }

        private void SelectMatch(TelemetryFile path)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                ObjectView<TelemetryFile> ovtf = (ObjectView<TelemetryFile>) row.DataBoundItem;
                if (ovtf.Object == path)
                {
                    row.Selected = true;
                    dataGridView1.FirstDisplayedScrollingRowIndex = row.Index;
                    break;
                }
            }
        }

        private void MatchSearchNext()
        {
            string txt = ViewModel.MatchSearchInputBox.InputText;
            if (string.IsNullOrEmpty(txt)) return;

            bool Find(TelemetryFile telemetryFile)
            {
                var searchSquadSet = new HashSet<string>(ViewModel.RegexCsv.Split(txt));
                var titleSquadSet = new HashSet<string>(ViewModel.RegexCsv.Split(telemetryFile.Title));
                bool dateMatch = telemetryFile.MatchDate?.ToLocalTime().ToString(ViewModel.ChartTitleDateFormat).Contains(txt) ?? false;
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
            //int rot = chart1.ChartAreas[0].Area3DStyle.Rotation;
            //rot = (rot + 5) % 180;
            //chart1.ChartAreas[0].Area3DStyle.Rotation = rot;
            if (splitContainer1.Panel2Collapsed)
            {
                buttonToggle.Text = "Hide Chart";
                splitContainer1.Panel2Collapsed = false;
            }
            else
            {
                buttonToggle.Text = "Show Chart";
                splitContainer1.Panel2Collapsed = true;
            }
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

        private void ContextMenuMatches_Opening(object sender, CancelEventArgs e)
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
            var dataBoundItem = (ObjectView<TelemetryFile>) dataGridView1.Rows[i].DataBoundItem;
            return dataBoundItem.Object;
        }

        private static T GetPrimarySelectedValue<T>(DataGridView dgv) where T : class
        {
            DataGridViewRow row = dgv.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();
            ObjectView<T> objectView = (ObjectView<T>) row?.DataBoundItem;
            return objectView?.Object;
        }

        private TelemetryFile GetSelectedMatch()
        {
            TelemetryFile o = GetPrimarySelectedValue<TelemetryFile>(dataGridView1);
            DebugThreadWriteLine("GetSelectedMatch: " + o);
            return o;
        }

        // ReSharper disable once UnusedMember.Local
        private TelemetryFile RowToTelemetryFile(DataGridViewRow row)
        {
            ObjectView<TelemetryFile> ovtf = (ObjectView<TelemetryFile>) row.DataBoundItem;
            return ovtf.Object;
        }

        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            var file = GetSelectedMatch();
            DebugThreadWriteLine("DataGridView1_SelectionChanged: " + file?.MatchDate + " " + file?.FileInfo.Name);
            if (file == null)
            {
                return;
            }

            ViewModel.CtsMatchSwitch?.Cancel();
            ViewModel.CtsMatchSwitch = new CancellationTokenSource();
            ClearChart(chart1);

            if (ViewModel.DownloadActive) return;

            Task.Run(() => SwitchMatch(file, ViewModel.CtsMatchSwitch.Token));
        }

        internal static void SaveDataGridViewToCsv(DataGridView dgv, string filename)
        {
            DataGridViewClipboardCopyMode clipboardCopyModeSaved = dgv.ClipboardCopyMode;
            bool multiSelectSaved = dgv.MultiSelect;
            DataGridViewSelectionMode selectionModeSaved = dgv.SelectionMode;
            try
            {
                dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
                dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
                dgv.MultiSelect = true;
                dgv.SelectAll();
                DataObject dataObject = dgv.GetClipboardContent();
                if (dataObject == null)
                {
                    MessageBox.Show("Export failed.", "Export failed.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                File.WriteAllText(filename, dataObject.GetText(TextDataFormat.CommaSeparatedValue));
            }
            finally
            {
                dgv.MultiSelect = multiSelectSaved;
                dgv.SelectionMode = selectionModeSaved;
                dgv.ClipboardCopyMode = clipboardCopyModeSaved;
            }
        }

        private void ButtonExportCsv_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "CSV File|*.csv|All Files|*.*",
                Title = "Export CSV...",
                OverwritePrompt = true
            };
            dialog.ShowDialog();
            if (dialog.FileName == "") return;
            SaveDataGridViewToCsv(dataGridView1, dialog.FileName);
        }

        private void LabelMatches_Click(object sender, EventArgs e)
        {
        }

        private void TextBoxSquad_TextChanged(object sender, EventArgs e)
        {
            string[] squadMembers = ViewModel.RegexCsv.Split(textBoxSquad.Text);
            ViewModel.Squad.Clear();
            ViewModel.Squad.UnionWith(squadMembers);
            string s = string.Join("|", ViewModel.Squad);
            DebugThreadWriteLine("sq=" + s);
        }
    }

    public class ChartTag
    {
    }

    // Helper extension to BindingListView that always applies a secondary sort of MatchDate DESC to whatever primary sort is requested.
    public class BindingListView2<T> : BindingListView<T>, IBindingListView
    {
        public BindingListView2(IList list) : base(list)
        {
        }

        public BindingListView2(IContainer container) : base(container)
        {
        }

        public new void ApplySort(ListSortDescriptionCollection sorts)
        {
            //Debug.WriteLine(">>>>>>BLV2 ApplySort(ListSortDescriptionCollection)!");
            List<ListSortDescription> sortDescList = sorts.Cast<ListSortDescription>().ToList();
            sortDescList.Add(new ListSortDescription(GetPropertyDescriptor("MatchDate"), ListSortDirection.Descending));
            ListSortDescriptionCollection newSorts = new ListSortDescriptionCollection(sortDescList.ToArray());
            base.ApplySort(newSorts);
        }

        public new void ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            //Debug.WriteLine(">>>>>>BLV2 ApplySort(PropertyDescriptor,ListSortDirection)!");
            var sort = new ListSortDescription(property, direction);
            var dateSort = new ListSortDescription(GetPropertyDescriptor("MatchDate"), ListSortDirection.Descending);
            ListSortDescription[] sorts = {sort, dateSort};
            ListSortDescriptionCollection newSorts = new ListSortDescriptionCollection(sorts);
            ApplySort(newSorts);
        }

        private PropertyDescriptor GetPropertyDescriptor(string propertyName)
        {
            return TypeDescriptor.GetProperties(typeof(T)).Find(propertyName, false);
        }
    }

    public static class TelemetryGuiExtensions
    {
        public static void PrivateZoom(this AxisScaleView scaleView, double viewPosition, double viewSize, DateTimeIntervalType viewSizeType,
            bool fireChangeEvents, bool saveState)
        {
            scaleView.PrivateInvoke("Zoom", viewPosition, viewSize, viewSizeType, fireChangeEvents, saveState);
        }

        public static void PrivateZoomReset(this AxisScaleView scaleView, int numberOfViews, bool fireChangeEvents)
        {
            scaleView.PrivateInvoke("ZoomReset", numberOfViews, fireChangeEvents);
        }

        private static object PrivateInvoke(this object o, string methodName, params object[] args)
        {
            var mi = o.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (mi != null)
            {
                return mi.Invoke(o, args);
            }
            return null;
        }
    }
}