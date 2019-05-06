using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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

        public HashSet<string> Squad { get; set; }

        public JToken TeamId { get; set; }

        public MainForm()
        {
            InitializeComponent();
            App = new TelemetryApp();
            Squad = new HashSet<string>();
            Squad.Add("Celaven");
            Squad.Add("wckd");
            Squad.Add("Giles333");
            Squad.Add("Solunth");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Random r = new Random();
            DirectoryInfo di = new DirectoryInfo(App.TelemetryDir);
            List<TelFile> telFiles = di.GetFiles("*.json").Select(x => new TelFile { FileInfo = x, Title = x.Name }).ToList();
            BindingSource binding = new BindingSource();
            binding.DataSource = telFiles;

            listBox1.DisplayMember = "Title";
            listBox1.DataSource = binding;

            UpdateTitlesAsync();

            chart1.Titles.Add("Hitpoints over time (one match)");

            chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
        }

        private async void UpdateTitlesAsync()
        {
            List<TelFile> telFiles = listBox1.Items.Cast<TelFile>().ToList();
            int i = 0;
            foreach (TelFile file in telFiles)
            {
                i++;
                using (StreamReader sr = new StreamReader(file.FileInfo.FullName))
                {
                    Debug.WriteLine("hi2");
                    SortedSet<string> squadTeam = new SortedSet<string>();
                    JsonTextReader jtr = new JsonTextReader(sr);
                    await jtr.ReadAsync();
                    while (await jtr.ReadAsync())
                    {
                        JObject jti = await JObject.LoadAsync(jtr);
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

                    file.Title = string.Join(", ", squadTeam);
                    listBox1.BeginUpdate();
                    int topIdx = listBox1.TopIndex;
                    int selIdx = listBox1.SelectedIndex;
                    ((BindingSource)listBox1.DataSource).ResetBindings(false);
                    listBox1.TopIndex = topIdx;
                    listBox1.SelectedIndex = selIdx;
                    listBox1.EndUpdate();
                }
            }
            ((BindingSource)listBox1.DataSource).ResetBindings(false);
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TelFile file = (TelFile)listBox1.SelectedItem;
            Task.Run(() => SwitchMatch(file));
        }

        private void SwitchMatch(TelFile file)
        {
            var dict = new Dictionary<string, List<TelEvent>>();
            using (StreamReader sr = new StreamReader(file.FileInfo.FullName))
            {
                List<TelEvent> events = JsonConvert.DeserializeObject<List<TelEvent>>(sr.ReadToEnd());
                foreach (TelEvent @event in events)
                {
                    if (@event._T == "LogPlayerTakeDamage")
                    {
                        DateTime dt = @event._D;
                        string playerName = @event.victim.name;
                        @event.character = @event.victim;
                        if (!Squad.Contains(playerName))
                            continue;
                        @event.victim.health = @event.victim.health - @event.damage;
                        dict.TryGetValue(playerName, out List<TelEvent> playerEvents);
                        if (playerEvents == null)
                            dict[playerName] = playerEvents = new List<TelEvent>();
                        playerEvents.Add(@event);
                    }
                    else if (@event._T == "LogPlayerPosition")
                    {
                        DateTime dt = @event._D;
                        string playerName = @event.character.name;
                        if (!Squad.Contains(playerName))
                            continue;
                        dict.TryGetValue(playerName, out List<TelEvent> playerEvents);
                        if (playerEvents == null)
                            dict[playerName] = playerEvents = new List<TelEvent>();
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
                foreach (KeyValuePair<string, List<TelEvent>> kvp in dict)
                {
                    Series series = chart1.Series.Add(kvp.Key);
                    series.ChartType = SeriesChartType.FastLine;
                    foreach (TelEvent @event in kvp.Value)
                    {
                        series.Points.Add(@event.character.health);

                    }
                }

            });

        }

        public class TelEvent
        {
            public DateTime _D;
            public string _T;
            public float damage;
            public TelPlayer victim;
            public TelPlayer character;
        }

        public class TelPlayer
        {
            public string name;
            public float health;
        }
    }

    public class TelFile
    {
        public FileInfo FileInfo { set; get; }
        public string Title { set; get; }
    }
}
