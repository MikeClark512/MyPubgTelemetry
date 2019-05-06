using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyPubgTelemetry.GUI
{
    public partial class MainForm : Form
    {
        public TelemetryApp App { get; set; }

        public Dictionary<string, List<HPEvent>> _hpeDict = new Dictionary<string, List<HPEvent>>();

        public HashSet<string> Squad { get; set; }

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
            List<TelFile> telFiles = di.GetFiles("*.json").Select(x => new TelFile {FileInfo = x, Title = x.Name}).ToList();
            listBox1.DisplayMember = "Title";
            listBox1.DataSource = telFiles;


            chart1.Titles.Add("Hitpoints over time (one match)");
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _hpeDict.Clear();
            TelFile file = (TelFile) listBox1.SelectedItem;
            using (StreamReader sr = new StreamReader(file.FileInfo.FullName))
            {
                JToken jt = JToken.Load(new JsonTextReader(sr));
                foreach (var @event in jt)
                {
                    if (@event["_T"].ToString() == "LogPlayerTakeDamage")
                    {
                        DateTime dt = @event["_D"].Value<DateTime>();
                        string playerName = @event["victim"]["name"].Value<String>();
                        if (!Squad.Contains(playerName))
                            continue;
                        float health = @event["victim"]["health"].Value<float>();
                        float damage = @event["damage"].Value<float>();
                        HPEvent hpe = new HPEvent();
                        hpe.name = playerName;
                        hpe.hp = health - damage;
                        hpe.time = dt;
                        _hpeDict.TryGetValue(hpe.name, out List<HPEvent> hpes);
                        if (hpes == null)
                            _hpeDict[hpe.name] = hpes = new List<HPEvent>();
                        hpes.Add(hpe);
                    }
                    else if (@event["_T"].ToString() == "LogPlayerPosition")
                    {
                        DateTime dt = @event["_D"].Value<DateTime>();
                        string playerName = @event["character"]["name"].Value<String>();
                        if (!Squad.Contains(playerName))
                            continue;
                        float health = @event["character"]["health"].Value<float>();
                        HPEvent hpe = new HPEvent { name = playerName, hp = health, time = dt };
                        _hpeDict.TryGetValue(hpe.name, out List<HPEvent> hpes);
                        if (hpes == null)
                            _hpeDict[hpe.name] = hpes = new List<HPEvent>();
                        hpes.Add(hpe);
                    }
                }
            }

            foreach (Series chart1Series in chart1.Series)
            {
                chart1Series.Points.Clear();
            }
            chart1.Series.Clear();

            // Add series.
            foreach (KeyValuePair<string, List<HPEvent>> kvp in _hpeDict)
            {
                Series series = chart1.Series.Add(kvp.Key);
                //series.ChartType = SeriesChartType.Spline;
                foreach (HPEvent hpe in kvp.Value)
                {
                    series.Points.Add(hpe.hp);
                }
            }
        }

        private void Label1_Click(object sender, EventArgs e)
        {

        }
    }

    public class TelFile
    {
        public FileInfo FileInfo { set; get; }
        public string Title { set; get; }
    }

    public class HPEvent
    {
        public string name;
        public float hp;
        public DateTime time;

        public override string ToString()
        {
            return $"{nameof(name)}: {name}, {nameof(hp)}: {hp}, {nameof(time)}: {time}";
        }
    }
}
