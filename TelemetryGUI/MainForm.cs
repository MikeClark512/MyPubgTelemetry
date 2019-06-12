using Equin.ApplicationFramework;
using MyPubgTelemetry.ApiMatchModel;
using MyPubgTelemetry.GUI.Charts;
using MyPubgTelemetry.GUI.Properties;
using Newtonsoft.Json;
using System;
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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static MyPubgTelemetry.TelemetryApp;

namespace MyPubgTelemetry.GUI
{
    public partial class MainForm : Form
    {
        public ViewModel ViewModel { get; set; }

        public MainForm()
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            InitViewModel();
            InitializeComponent();
            InitChart();
            InitMatchesDataGridView();
            InitToolStrip();
            InitSplitContainer();
        }

        private void InitSplitContainer()
        {
            void SplitContainerOnCollapse(object sender, EventArgs args)
            {
                hideTableToolStripMenuItem.Checked = splitContainer1.Panel1Collapsed;
                hideChartToolStripMenuItem.Checked = splitContainer1.Panel2Collapsed;
            }

            splitContainer1.Panel1.VisibleChanged += SplitContainerOnCollapse;
            splitContainer1.Panel2.VisibleChanged += SplitContainerOnCollapse;
        }

        private void InitViewModel()
        {
            ViewModel = new ViewModel(this);
            var qts = new QueuedTaskScheduler(Environment.ProcessorCount - 1, "QTS", false, ThreadPriority.Lowest);
            ViewModel.TaskFactory = new TaskFactory(qts);
            ViewModel.MatchSearchInputBox = new InputBox { Text = @"Search match IDs, dates, and player names" };
            ViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "DownloadActive" || args.PropertyName == "ReloadActive")
                {
                    bool loadingActive = ViewModel.DownloadActive || ViewModel.ReloadActive;
                    //dataGridView1.Visible = !loadingActive;
                    foreach (ToolStripItem item in toolStrip1.Items)
                    {
                        item.Enabled = !loadingActive;
                    }
                }
            };

        }

        private void InitToolStrip()
        {
            toolStripProgressBar1.TextChanged += delegate (object sender, EventArgs args)
            {
                toolStripStatusLabel1.Text = toolStripProgressBar1.Text;
            };
            toolStripStatusLabel1.TextChanged += delegate (object sender, EventArgs args)
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
                new object[] { true });

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
                DefaultCellStyle = new DataGridViewCellStyle { Format = ViewModel.ChartTitleDateFormat }
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

        private async void MainForm_Load(object sender, EventArgs e)
        {
            Rectangle screen = Screen.FromControl(this).WorkingArea;
            if (Width > screen.Width)
            {
                Left = 0;
                Width = screen.Width;
            }

            var path = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            //toolStripComboBoxPlayerSelect.SelectedIndex = 0;
            DebugThreadWriteLine("Config path = " + path);
            textBoxSquad.Text = Settings.Default.Squad.Trim();
            Type type = toolStripComboBoxPlayerFocus.Control.GetType();
            type.InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, toolStripComboBoxPlayerFocus.Control, new object[] { true });
            ViewModel.Squad.ListChanged += (o, args) =>
            {
                toolStripComboBoxPlayerFocus.Items.Clear();
                toolStripComboBoxPlayerFocus.Items.AddRange(ViewModel.Squad.Cast<object>().ToArray());
                if (toolStripComboBoxPlayerFocus.Items.Count > 0)
                {
                    toolStripComboBoxPlayerFocus.SelectedIndex = 0;
                }
            };
            ViewModel.SquadCsv = textBoxSquad.Text;
            ViewModel.PlayerColorPalette = ViewModel.ChartHpOverTime.Chart.Palette.GetPaletteColors();
            if (!string.IsNullOrWhiteSpace(Settings.Default.PlayerColors))
            {
                ViewModel.PlayerColors = JsonConvert.DeserializeObject<Dictionary<string, Color>>(Settings.Default.PlayerColors);
            }
            if (EnvBooly("MYPUBGTELEMETRY_NODLONSTART"))
            {
                LoadMatchesFromFilesAsync();
            }
            else
            {
                await DownloadAndRefresh();
            }
        }

        private void AddSquadStatColumn(PropertyInfo pi)
        {
            var ignore = new HashSet<string>
            {
                "DeathType", "KillPointsDelta", "KillPoints", "KillStreaks", "LastKillPoints", "Name", "PlayerId", "MostDamage", "RankPoints", "WinPoints",
                "WinPointsDelta", "TeamId", "LastWinPoints", "WinPlace", "KillPlace", "WeaponsAcquired"
            };

            Type pt = pi.PropertyType;
            string name = pi.Name;
            if (ignore.Contains(name))
            {
                return;
            }

            string displayName = name;
            if (displayName.EndsWith("s"))
            {
                displayName = displayName.Substring(0, name.Length - 1);
            }

            var rename = new Dictionary<string, string>()
            {
                {"DamageDealt", "Dmg"},
                {"RoadKill", "RoadK"},
                {"HeadshotKill", "HdK"},
                {"VehicleDestroy", "VDest"},
                {"TeamKill", "TK"},
                {"Boost", "Bst"},
                {"Revive", "Rev"},
                {"LongestKill", "LK"},
                {"WeaponsAcquired", "Weap"},
                {"XFragKill", "FragK"},
                {"TimeSurvived", "Time"},
                {"Assist", "Asst"},
            };

            displayName = rename.GetValueOrDefault(displayName, displayName);
            displayName = displayName.Replace("Distance", "");
            DebugThreadWriteLine($"name={name}, display={displayName}, type={pt}");

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
            col.ToolTipText = dataName;
            if (dataName.StartsWith("X"))
            {
                col.ToolTipText += "\nNote: This stat must be calculated from telemetry,\nwhich is not implemented yet. Stay tuned.";
            }
            col.Frozen = false;
            if (format != null)
            {
                col.DefaultCellStyle.Format = format;
            }
            dataGridView1.Columns.Add(col);
        }

        private void InitChart()
        {
            ViewModel.ChartHpOverTime = new ChartHpOverTime();
            splitContainer1.Panel2.Controls.Add(ViewModel.ChartHpOverTime.Chart);
        }

        // ReSharper disable once UnusedMember.Local
        private static object GetField(object instance, string fieldName)
        {
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var field = instance.GetType().GetField(fieldName, bindFlags);
            return field?.GetValue(instance);
        }

        private async void LoadMatchesFromFilesAsync(bool deep = false)
        {
            if (ViewModel.ReloadActive)
            {
                return;
            }
            ViewModel.ReloadActive = true;
            if (!ValidateInputSquad())
            {
                ViewModel.ReloadActive = false;
                return;
            }
            var squaddies = SquadTextBoxToHashSet();
            ViewModel.ReloadActive = true;
            var di = new DirectoryInfo(App.TelemetryDir);
            List<FileInfo> jsonFiles = di.GetFiles("*.json").ToList();
            jsonFiles.AddRange(di.GetFiles("*.json.gz"));
            if (jsonFiles.Count == 0)
            {
                BeginInvoke((MethodInvoker)delegate ()
               {
                   MessageBox.Show("No telemetry files found!");
               });
                ViewModel.ReloadActive = false;
                return;
            }

            List<TelemetryFile> telFiles = jsonFiles.Select(jsonFile => new TelemetryFile { FileInfo = jsonFile, Title = "" }).ToList();
            telFiles.Sort((x, y) => y.FileInfo.CreationTime.CompareTo(x.FileInfo.CreationTime));

            var blv = new BindingListView2<TelemetryFile>(telFiles);
            //blv.SourceLists.Add(telFiles);

            dataGridView1.DataSource = blv;
            toolStripProgressBar1.Maximum = telFiles.Count;
            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Visible = true;
            dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.EnableResizing;
            for (int col = 0; col < dataGridView1.ColumnCount; col++)
            {
                dataGridView1.Columns[col].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            ViewModel.CtsMatchMetadata?.Cancel();
            ViewModel.CtsMatchMetadata = new CancellationTokenSource();
            await Task.Run(() => LoadMatchesFromFiles(telFiles, squaddies, deep, ViewModel.CtsMatchMetadata.Token));
        }

        private HashSet<string> SquadTextBoxToHashSet()
        {
            return new HashSet<string>(ViewModel.RegexCsv.Split(textBoxSquad.Text));
        }

        private bool ValidateInputSquad()
        {
            //ViewModel.Squad.Clear();
            BindingList<string> squaddies = ViewModel.Squad;
            if (squaddies.Count > 6)
            {
                return false;
            }
            if (squaddies.Count == 1)
            {
                BeginInvoke((MethodInvoker)delegate
               {
                   Control windowHandle = Control.FromHandle(textBoxSquad.TextBox?.Handle ?? Handle);
                   var msg = "Enter some player names (case sensitive, comma/space separated) and then press Download.";
                   textBoxSquad.Focus();
                   toolTipBalloon.ToolTipTitle = "Player names";
                   toolTipBalloon.Hide(windowHandle);
                   toolTipBalloon.Show(msg, windowHandle, 25, textBoxSquad.Bounds.Top - 70, 7500);
               });
                return false;
            }
            return true;
        }

        private void LoadMatchesFromFiles(List<TelemetryFile> telFiles, IEnumerable<string> squaddies, bool deep, CancellationToken cancellationToken)
        {
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < telFiles.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested) break;
                TelemetryFile file = telFiles[i];
                file.Index = i;
                Task task = ViewModel.TaskFactory.StartNew(() => LoadMetadataAndTelemetryForOneMatch(file, squaddies, deep), cancellationToken);
                task.ContinueWith((_) =>
                {
                    file.TelemetryMetadataLoaded = true;
                    BeginInvoke((MethodInvoker)delegate
                   {
                       int loadedCount = telFiles.Count(x => x.TelemetryMetadataLoaded);
                       toolStripProgressBar1.Text = $"Loaded {loadedCount} of {telFiles.Count} matches.";
                       //int fi = telemetryFile.Index;
                       //if (fi <= dataGridView1.DisplayedRowCount(true))
                       //{
                       //    dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
                       //}
                       toolStripProgressBar1.Value = loadedCount;
                   });
                }, cancellationToken);
                tasks.Add(task);
            }

            ViewModel.TaskFactory.ContinueWhenAny(tasks.ToArray(), (_) =>
            {
                BeginInvoke((MethodInvoker)delegate ()
               {
                   //dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
                   BindingListView2<TelemetryFile> bs = (BindingListView2<TelemetryFile>)dataGridView1.DataSource;

                   bs.Refresh();
               });
            }, cancellationToken);

            ViewModel.TaskFactory.ContinueWhenAll(tasks.ToArray(), (_) =>
            {
                DebugThreadWriteLine("Done loading metadata (worker).");
                BeginInvoke((MethodInvoker)delegate ()
               {
                   DebugThreadWriteLine("Done loading metadata (UI).");
                   dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

                   toolStripProgressBar1.Visible = false;
                   for (int coli = 0; coli < dataGridView1.ColumnCount; coli++)
                   {
                       DataGridViewColumn col = dataGridView1.Columns[coli];
                       col.SortMode = DataGridViewColumnSortMode.Automatic;
                       BeginInvoke((MethodInvoker)delegate
                       {
                           int contentWidth = col.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCellsExceptHeader, true);
                           int colWidth = col.Width;
                           int headerWidth = colWidth - contentWidth;
                           if (headerWidth > 0)
                           {
                               int width = col.Width;
                               while (width > contentWidth && width > colWidth - 3)
                               {
                                   width--;
                               }
                               col.Width = width;
                           }
                       });
                   }

                   dataGridView1.Sort(dataGridView1.Columns[1], ListSortDirection.Descending);
                   ViewModel.ReloadActive = false;
                   DataGridView1_SelectionChanged(null, null);
               });
            }, cancellationToken);
        }

        private void LoadMetadataAndTelemetryForOneMatch(TelemetryFile file, IEnumerable<string> squaddies, bool deep)
        {
            NormalizedMatch normalizedMatch = ReadMatchMetadataFromFile(file);
            file.NormalizedMatch = normalizedMatch;
            file.MatchDate = normalizedMatch.Model.Data.Attributes.CreatedAt;
            NormalizedRoster roster = normalizedMatch.Rosters.FirstOrDefault(aRoster =>
            {
                HashSet<string> rosterPlayers = aRoster.Players.Select(p => p.Attributes.Stats.Name).ToHashSet(StringComparer.CurrentCultureIgnoreCase);
                HashSet<string> intersection = rosterPlayers.ToHashSet(StringComparer.CurrentCultureIgnoreCase);
                intersection.IntersectWith(squaddies);
                return intersection.Count > 0;
            });
            var nameToPlayerIncl = new Dictionary<string, MatchModelIncluded>();
            // Remember account IDs to enable clickable links to stats websites.
            normalizedMatch.Rosters.ForEach(x =>
            {
                x.Players.ForEach(p =>
                {
                    ViewModel.AccountIds[p.Attributes.Stats.Name] = p.Attributes.Stats.PlayerId;
                    nameToPlayerIncl[p.Attributes.Stats.Name] = p;
                });
            });
            if (roster != null)
            {
                file.Squad = roster.Players.Select(p => p.Attributes.Stats.Name).ToHashSet(StringComparer.CurrentCultureIgnoreCase);
            }
            file.NormalizedRoster = roster;

            // Foreach stat that looks numeric, sum it across each player on the roster -- even if that stat doesn't make sense as a sum stat :)
            MatchModelStats sqst = file;
            RecalcStats(file, sqst);

            if (!deep) return;

            using (var sr = file.NewFileReader(FileMode.Open, FileAccess.ReadWrite, FileShare.Read, out FileStream fs))
            using (var jtr = new JsonTextReader(sr))
            {
                var teams = new Dictionary<int, SortedSet<string>>();
                jtr.Read();
                PreparedData pd = new PreparedData() { File = file };
                HashSet<int> squadTeamIds = new HashSet<int>();
                while (jtr.Read())
                {
                    if (jtr.TokenType == JsonToken.EndArray)
                    {
                        break;
                    }

                    var serializer = new JsonSerializer();
                    var @event = serializer.Deserialize<TelemetryEvent>(jtr);
                    switch (@event._T)
                    {
                        case "LogPlayerCreate":
                            {
                                string player = @event.character.name;
                                int teamId = @event.character.teamId;
                                SortedSet<string> team = teams.GetOrAdd(teamId, () => new SortedSet<string>());
                                team.Add(player);
                                if (ViewModel.Squad.Contains(player))
                                {
                                    squadTeamIds.Add(teamId);
                                }
                                break;
                            }
                        case "LogMatchStart":
                            file.MatchDate = @event._D;
                            break;
                        case "LogPlayerTakeDamage":
                            @event.character = @event.victim;
                            @event.victim.health -= @event.damage;
                            pd.NormalizedEvents.Add(@event);
                            break;
                        case "LogHeal":
                        case "LogPlayerPosition":
                            @event.victim = @event.character;
                            pd.NormalizedEvents.Add(@event);
                            break;
                        case "LogPlayerKill" when @event.killer == null || @event.victim == null:
                            continue;
                        case "LogPlayerKill":
                            {
                                if (@event.killer.name != @event.victim.name)
                                {
                                    if (@event.damageCauserName == "ProjGrenade_C")
                                    {
                                        nameToPlayerIncl[@event.killer.name].Attributes.Stats.XFragKills += 1;
                                    }
                                    //This is now being calculated from match metadata
                                    //file.SquadKills++;
                                }
                                break;
                            }
                    }
                }

                foreach (var @event in pd.NormalizedEvents)
                {
                    string name = @event.character.name;
                    // Skip events that are about players that haven't been listed by the user in the UI.
                    // When we start doing enemy interaction reports, we probably can't skip non-squad events
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

                if (squadTeamIds.Count <= 0)
                {
                    file.Title = "[No Squad Members in Match]";
                }
                else if (squadTeamIds.Count > 1)
                {
                    file.Title = "[Disjoint Squad]";
                }
                else // Count == 1
                {
                    SortedSet<string> squadTeam = teams[squadTeamIds.First()];
                    file.Squad = squadTeam;
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

        private void RecalcStats(TelemetryFile file, MatchModelStats sqst)
        {
            NormalizedRoster roster = file.NormalizedRoster;
            Func<MatchModelIncluded, bool> wherePred;
            if (ViewModel.SelectedPlayer != null)
            {
                wherePred = (player) => player?.Attributes?.Stats?.Name?.Equals(ViewModel.SelectedPlayer) ?? false;
            }
            else
            {
                wherePred = player => true;
            }
            foreach (PropertyInfo pi in typeof(MatchModelStats).GetProperties())
            {
                if (roster == null) break;
                if (pi.PropertyType == typeof(long?))
                {
                    long? sum = roster.Players.Where(wherePred).Sum(p => (long?)pi.GetValue(p.Attributes?.Stats));
                    pi.SetValue(sqst, sum);
                }
                else if (pi.PropertyType == typeof(double?))
                {
                    double? sum = roster.Players.Where(wherePred).Sum(p => (double?)pi.GetValue(p.Attributes?.Stats));
                    pi.SetValue(sqst, sum);
                }
                else if (typeof(long).IsAssignableFrom(pi.PropertyType))
                {
                    long sum = roster.Players.Where(wherePred).Sum(p => (long)(pi.GetValue(p.Attributes?.Stats) ?? 0));
                    pi.SetValue(sqst, sum);
                }
                else if (typeof(double).IsAssignableFrom(pi.PropertyType))
                {
                    double sum = roster.Players.Where(wherePred).Sum(p => (double)(pi.GetValue(p.Attributes?.Stats) ?? 0));
                    pi.SetValue(sqst, sum);
                }
            }

            // Don't report rank as a sum stat, just set it directly from the roster.
            // "999" -- no matching roster was found -- fudge a value that won't overlap with real ranks.
            sqst.Rank = roster?.Roster.Attributes.Stats.Rank ?? 999;
        }

        private static NormalizedMatch ReadMatchMetadataFromFile(TelemetryFile file)
        {
            string mid = file.GetMatchId();
            string mmn = $"mm-{mid}.json";
            string mmf = Path.Combine(App.MatchDir, mmn);
            string jsonStr = File.ReadAllText(mmf);
            return TelemetryDownloader.NormalizeMatch(mid, jsonStr);
        }

        private void PubgLookup(TelemetryFile file, string user)
        {
            string fname = file.FileInfo.Name;

            fname = TelemetryFilenameToMatchId(fname);

            ViewModel.AccountIds.TryGetValue(user, out string accountId);
            if (accountId == null) return;
            WebClient wc = new WebClient();
            var values = new NameValueCollection { { "region", "pc-na" }, { "player_name", user } };
            wc.UploadValues("https://pubglookup.com/search", "POST", values);
            Task.Run(() =>
            {
                string matchId = fname;
                string url = $"https://pubglookup.com/players/steam/{user}/matches/{matchId}";
                DebugThreadWriteLine("url=" + url);
                App.OpenUrlInWebBrowser($"https://pubglookup.com/players/steam/{user}/matches/{matchId}");
            });
        }

        private void SwitchMatchWorker(TelemetryFile file, CancellationToken cancellationToken)
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
                BeginInvoke((MethodInvoker)ConsumePreparedDataQ);
            }
        }

        private PreparedData PrepareData(TelemetryFile file, CancellationToken cancellationToken)
        {
            if (file.PreparedData != null)
            {
                return file.PreparedData;
            }

            file.PreparedData = null;

            var pd = new PreparedData() { File = file };
            using (StreamReader sr = file.NewFileReader())
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
                    else if (@event._T == "LogPlayerPosition" || @event._T == "LogHeal")
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
                if (!ViewModel.Squad.Contains(name, StringComparer.CurrentCultureIgnoreCase))
                    continue;
                pd.Squad.Add(name);
                var playerEvents = pd.PlayerToEvents.GetOrAdd(name, () => new List<TelemetryEvent>());
                playerEvents.Add(@event);
                var timePlayerToEvents =
                    pd.TimeToPlayerToEvents.GetOrAdd(@event._D, () => new Dictionary<string, List<TelemetryEvent>>());
                var timePlayerEvents = timePlayerToEvents.GetOrAdd(name, () => new List<TelemetryEvent>());
                timePlayerEvents.Add(@event);
            }

            file.PreparedData = cancellationToken.IsCancellationRequested ? null : pd;

            return pd;
        }

        private void ConsumePreparedDataQ()
        {
            while (ViewModel.PreparedDataQ.Count > 0)
            {
                var preparedData = ViewModel.PreparedDataQ.Take();
                DebugThreadWriteLine("ConsumePreparedDataQ loop");
                TelemetryFile selectedTf = GetSelectedMatch();
                if (preparedData.File == selectedTf)
                {
                    DebugThreadWriteLine("ConsumePreparedDataQ inner");
                    ConsumePreparedData(preparedData);
                }
            }
        }

        public Font DeriveFont(Font orig, int sizeAdj)
        {
            return new Font(orig.FontFamily, orig.Size + sizeAdj);
        }

        private void ConsumePreparedData(PreparedData pd)
        {
            ViewModel.ChartHpOverTime.DrawData(pd, this);
        }

        private async void ButtonRefresh_Click(object sender, EventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                LoadMatchesFromFilesAsync(true);
            }
            else if (ModifierKeys.HasFlag(Keys.Shift))
            {
                LoadMatchesFromFilesAsync();
            }
            else
            {
                ViewModel.DownloadActive = true;
                await DownloadAndRefresh();
            }
        }

        private async Task DownloadAndRefresh()
        {
            try
            {
                if (!PreFlight())
                {
                    return;
                }

                TelemetryDownloader downloader = new TelemetryDownloader();
                string squad = string.Join(",", SquadTextBoxToHashSet());
                downloader.DownloadProgressEvent += (sender2, args) =>
                {
                    BeginInvoke((MethodInvoker)delegate ()
                   {
                       if (!args.Rewrite)
                       {
                           DebugThreadWriteLine("DownloadProgressEvent " + args.Msg);
                       }

                       toolStripProgressBar1.Visible = !args.Complete;
                       toolStripProgressBar1.Maximum = (int)args.Max;
                       toolStripProgressBar1.Value = (int)args.Value;
                       toolStripStatusLabel1.Text = args.Msg;
                   });
                };

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

            LoadMatchesFromFilesAsync();
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
            if (string.IsNullOrWhiteSpace(App.ApiKey))
            {
                BeginInvoke((MethodInvoker)delegate ()
               {
                   var msg = "Your API Key is not set. Click \"Options\" and paste a valid API Key.";
                   toolTipBalloon.ToolTipTitle = "API Key";
                   toolTipBalloon.Show(msg, toolStrip1, new Point(toolStripButtonOptions.Bounds.Location.X + 10, -75), 7500);
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


        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                if (e.KeyCode == Keys.F)
                {
                    toolStripButtonSearch.PerformClick();
                }
                else if (e.KeyCode == Keys.E)
                {
                    textBoxSquad.Focus();
                }
            }
            else if (ModifierKeys == Keys.Alt)
            {
                DebugThreadWriteLine($"{e.KeyCode} {e.KeyData} {e.KeyValue} {e.Modifiers}");
                if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
                {
                    string keyName = Enum.GetName(typeof(Keys), e.KeyCode);
                    if (keyName?.Length == 2) keyName = keyName.Substring(1);
                    if (int.TryParse(keyName, out int num))
                    {
                        num = num - 1;
                        if (num >= 0 && num < toolStripComboBoxPlayerFocus.Items.Count)
                        {
                            toolStripComboBoxPlayerFocus.SelectedIndex = num;
                        }
                    }
                }
            }
            else if (e.KeyCode == Keys.F3)
            {
                MatchSearchNext();
            }
        }

        private void SelectMatch(TelemetryFile path, bool scroll = true)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                ObjectView<TelemetryFile> ovtf = (ObjectView<TelemetryFile>)row.DataBoundItem;
                if (ovtf.Object == path)
                {
                    row.Selected = true;
                    if (scroll)
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
            var dataBoundItem = (ObjectView<TelemetryFile>)dataGridView1.Rows[i].DataBoundItem;
            return dataBoundItem.Object;
        }

        private static T GetPrimarySelectedValue<T>(DataGridView dgv) where T : class
        {
            for (int i = 0, s = dgv.Rows.Count; i < s; i++)
            {
                DataGridViewRow row = dgv.Rows[i];
                if (row.Selected)
                {
                    ObjectView<T> objectView = (ObjectView<T>)row?.DataBoundItem;
                    return objectView.Object;
                }
            }
            return null;
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
            ObjectView<TelemetryFile> ovtf = (ObjectView<TelemetryFile>)row.DataBoundItem;
            return ovtf.Object;
        }

        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            LoadChartForSelectedMatch();
        }

        private int GetMatchListPrimarySelectedIndex()
        {
            return dataGridView1.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault()?.Index ?? -1;
        }

        private void LoadChartForSelectedMatch()
        {
            var file = GetSelectedMatch();
            if (file == null)
            {
                ViewModel.ChartHpOverTime.ClearChart("");
                return;
            }
            if (ViewModel.DownloadActive || ViewModel.ReloadActive)
            {
                DebugThreadWriteLine("DataGridView1_SelectionChanged: early return because Download/Reload is active.");
                return;
            }
            DebugThreadWriteLine("DataGridView1_SelectionChanged: " + file?.MatchDate + " " + file?.FileInfo.Name);
            ViewModel.CtsMatchSwitch?.Cancel();
            ViewModel.CtsMatchSwitch = new CancellationTokenSource();
            ViewModel.ChartHpOverTime.ClearChart("...loading...");
            Task.Run(() => SwitchMatchWorker(file, ViewModel.CtsMatchSwitch.Token));
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

        private void DoExportCsv()
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

        private void TextBoxSquad_TextChanged(object sender, EventArgs e)
        {
            ViewModel.SquadCsv = textBoxSquad.Text;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            DebugThreadWriteLine("Form Size: " + Size);
        }

        private void HideChartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel2Collapsed = !splitContainer1.Panel2Collapsed;
        }

        private void HideTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel1Collapsed = !splitContainer1.Panel1Collapsed;
        }

        private void DoOptions()
        {
            var optionsForm = new OptionsForm { StartPosition = FormStartPosition.CenterParent, LogText = ViewModel.LogBuffer.ToString() };
            DialogResult dialogResult = optionsForm.ShowDialog(this);
            if (dialogResult == DialogResult.OK)
            {
                Task.Run(DownloadAndRefresh);
            }
        }

        private void ToolStripButtonOptions_Click(object sender, EventArgs e)
        {
            DoOptions();
        }

        private void ExportCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoExportCsv();
        }

        private void ToolStripButtonSearch_Click(object sender, EventArgs e)
        {
            var dialogResult = ViewModel.MatchSearchInputBox.ShowDialog(this);
            if (dialogResult != DialogResult.OK) return;
            MatchSearchNext();
        }

        private void ToolStripComboBoxPlayerFocus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ViewModel.DownloadActive || ViewModel.ReloadActive)
                return;
            if (toolStripComboBoxPlayerFocus.SelectedIndex == 0)
            {
                ViewModel.SelectedPlayer = null;
            }
            else
            {
                ViewModel.SelectedPlayer = (string)toolStripComboBoxPlayerFocus.SelectedItem;
            }
            if (dataGridView1.DataSource == null)
                return;

            BindingListView2<TelemetryFile> dataSource = (BindingListView2<TelemetryFile>)dataGridView1.DataSource;
            //dataSource.Sort = "";

            int firstDisplayedScrollingRowIndex = dataGridView1.FirstDisplayedScrollingRowIndex;
            TelemetryFile savedSelectedMatch = GetSelectedMatch();
            dataSource.SuspendAutoFilterAndSort();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                ObjectView<TelemetryFile> ovtf = (ObjectView<TelemetryFile>)row.DataBoundItem;
                //ovtf.BeginEdit();
                TelemetryFile file = ovtf.Object;
                RecalcStats(file, file);
                //ovtf.EndEdit();
            }
            dataSource.Refresh();
            dataSource.ResumeAutoFilterAndSort();
            dataGridView1.FirstDisplayedScrollingRowIndex = firstDisplayedScrollingRowIndex;
            dataGridView1.ClearSelection();
            SelectMatch(savedSelectedMatch, false);
            //DataGridViewCell firstDisplayedCell = dataGridView1.FirstDisplayedCell;
            //dataGridView1.FirstDisplayedCell = firstDisplayedCell;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private void TweakColumnHeaderPadding()
        {
            try
            {
                DataGridViewCellStyle cellStyle = dataGridView1.ColumnHeadersDefaultCellStyle;
                FieldInfo fi_DataGridViewCellStyle_propertyStore =
                    cellStyle.GetType().GetField("propertyStore", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo fi_DataGridViewCellStyle_PropPadding = cellStyle.GetType().GetField("PropPadding", BindingFlags.NonPublic | BindingFlags.Static);
                int propId = (int?)fi_DataGridViewCellStyle_PropPadding?.GetValue(null) ?? -1;
                object propertyStore = fi_DataGridViewCellStyle_propertyStore?.GetValue(cellStyle);
                Type t_PropertyStore = propertyStore?.GetType();
                MethodInfo mi_PropertyStore_SetPadding = t_PropertyStore?.GetMethod("SetPadding");
                Padding padding = new Padding(-3, 0, -2, 0);
                mi_PropertyStore_SetPadding?.Invoke(propertyStore, new object[] { propId, padding });
                Padding gp = cellStyle.Padding;
                Debug.WriteLine($"{gp.Left} {gp.Right} {gp.Top} {gp.Bottom}");
            }
            catch (Exception)
            {
                // unimportant attempt to tighten up the text margins in the DGV header labels
            }
        }

        private void ToolStripMenuItemChartRight_Click(object sender, EventArgs e)
        {
            if (hideChartToolStripMenuItem.Checked)
            {
                hideChartToolStripMenuItem.PerformClick();
            }
            if (hideTableToolStripMenuItem.Checked)
            {
                hideTableToolStripMenuItem.PerformClick();
            }
            splitContainer1.Orientation = Orientation.Vertical;
            splitContainer1.SplitterDistance = (int)(splitContainer1.ClientSize.Width * 0.5);
        }

        private void ToolStripMenuItemChartBottom_Click(object sender, EventArgs e)
        {
            if (hideChartToolStripMenuItem.Checked)
            {
                hideChartToolStripMenuItem.PerformClick();
            }
            if (hideTableToolStripMenuItem.Checked)
            {
                hideTableToolStripMenuItem.PerformClick();
            }
            splitContainer1.Orientation = Orientation.Horizontal;
            splitContainer1.SplitterDistance = (int)(splitContainer1.ClientSize.Height * 0.6);
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
            Debug.WriteLine(">>>>>>BLV2 ApplySort(ListSortDescriptionCollection)!" + sorts.Count);
            List<ListSortDescription> sortDescList = sorts.Cast<ListSortDescription>().ToList();
            sortDescList.Add(new ListSortDescription(GetPropertyDescriptor("MatchDate"), ListSortDirection.Descending));
            ListSortDescriptionCollection newSorts = new ListSortDescriptionCollection(sortDescList.ToArray());
            base.ApplySort(newSorts);
        }

        public new void ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            Debug.WriteLine(">>>>>>BLV2 ApplySort(PropertyDescriptor,ListSortDirection)!");
            var sort = new ListSortDescription(property, direction);
            var dateSort = new ListSortDescription(GetPropertyDescriptor("MatchDate"), ListSortDirection.Descending);
            ListSortDescription[] sorts = { sort, dateSort };
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
        public static List<Color> GetPaletteColors(this ChartColorPalette palette)
        {
            Type type = typeof(Chart).Assembly.GetType("System.Windows.Forms.DataVisualization.Charting.Utilities.ChartPaletteColors");
            BindingFlags bf = BindingFlags.Static | BindingFlags.Public;
            MethodInfo mi = type.GetMethod("GetPaletteColors", bf);
            object ret = mi?.Invoke(null, new object[] { palette });
            if (ret is Color[] colors)
            {
                return colors.ToList();
            }

            return new List<Color>();
        }

        public static void PrivateZoom(this AxisScaleView scaleView, double viewPosition, double viewSize, DateTimeIntervalType viewSizeType,
            bool fireChangeEvents, bool saveState)
        {
            scaleView.PrivateInvoke("Zoom", viewPosition, viewSize, viewSizeType, fireChangeEvents, saveState);
        }

        public static void PrivateZoomReset(this AxisScaleView scaleView, int numberOfViews, bool fireChangeEvents)
        {
            scaleView.PrivateInvoke("ZoomReset", numberOfViews, fireChangeEvents);
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
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