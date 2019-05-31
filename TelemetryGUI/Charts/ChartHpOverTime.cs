using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static MyPubgTelemetry.TelemetryApp;

namespace MyPubgTelemetry.GUI.Charts
{
    public class ChartHpOverTime
    {
        // ReSharper disable once InconsistentNaming
        private readonly Chart chart1;
        public Chart Chart => chart1;

        public ChartHpOverTime()
        {
            chart1 = new Chart();
            var chartArea1 = new ChartArea();
            var legend1 = new Legend();

            chartArea1.Name = "Default";

            legend1.Alignment = StringAlignment.Center;
            legend1.Docking = Docking.Bottom;
            legend1.LegendStyle = LegendStyle.Row;
            legend1.Name = "Legend1";
            legend1.TableStyle = LegendTableStyle.Wide;

            chart1.ChartAreas.Add(chartArea1);
            chart1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
                                             | AnchorStyles.Left
                                             | AnchorStyles.Right;

            chart1.Legends.Add(legend1);
            chart1.Location = new Point(6, -1);
            chart1.Name = "chart1";
            chart1.Size = new Size(990, 326);
            chart1.TabIndex = 6;
            chart1.Text = "chart1";

            // ============================================================ //

            chart1.Tag = new ChartTag();
            chart1.Titles.Add("");
            chart1.Dock = DockStyle.Fill;
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

            chart1.AxisViewChanged += delegate (object sender, ViewEventArgs args)
            {
                chart1.ChartAreas[0].AxisX.Interval = chart1.ChartAreas[0].AxisX.Interval / 10;
                RecalcPointLabels();
                var scaleView = chart1.ChartAreas[0].AxisX.ScaleView;
                double delta = scaleView.ViewMaximum - scaleView.ViewMinimum;
                bool zoomed = chart1.ChartAreas[0].AxisX.ScaleView.IsZoomed;
                chart1.ChartAreas[0].AxisX.Interval = zoomed ? delta * 100 : 0;
            };
            chart1.MouseWheel += delegate (object sender, MouseEventArgs args)
            {
                Debug.WriteLine("wheel delta=" + args.Delta);
                //ChartTag tag = (ChartTag) chart1.Tag;
                if (Control.ModifierKeys.HasFlag(Keys.Control))
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
                    if (Control.ModifierKeys == Keys.None)
                        scrollType = args.Delta < 0 ? ScrollType.SmallIncrement : ScrollType.SmallDecrement;
                    else if (Control.ModifierKeys == Keys.Shift)
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

        public void RecalcPointLabels()
        {
            double scaleViewSize = chart1.ChartAreas[0].AxisX.ScaleView.Size;
            Debug.WriteLine("scaleViewSize " + scaleViewSize);
            bool zoomedIn = chart1.ChartAreas[0].AxisX.ScaleView.IsZoomed; //scaleViewSize < 0.01;
            //series.SmartLabelStyle.MaxMovingDistance = 0;
            chart1.Series.ToList().ForEach(x => x.IsValueShownAsLabel = zoomedIn);
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = zoomedIn ? "m'm'ss's'" : "m'm'";
        }

        public void ClearChart(string msg = "")
        {
            chart1.Titles[0].Font = new Font(FontFamily.GenericSansSerif, 7, FontStyle.Italic);
            chart1.Titles[0].Text = msg ?? "";
            chart1.Series.ToList().ForEach(series => series.Points.Clear());
            foreach (var chart1Series in chart1.Series)
            {
                chart1Series.Points.Clear();
            }

            chart1.Series.Clear();
        }

        public void DrawData(PreparedData pd, MainForm mainForm)
        {
            ClearChart();
            Chart chart1 = this.Chart;
            DateTime? localTime = pd.File?.MatchDate?.ToLocalTime();
            string sdt = "";
            if (localTime.HasValue)
            {
                sdt = localTime.Value.ToString(ViewModel.ChartTitleDateFormat);
                sdt += " " + localTime.Value.GetTimeZoneAbbreviation();
            }

            chart1.ChartAreas[0].AxisY.Title = "HP: 0 to 100";
            chart1.ChartAreas[0].AxisY.TitleFont = mainForm.DeriveFont(chart1.ChartAreas[0].AxisY.TitleFont, 0);

            chart1.ChartAreas[0].AxisX.Title = "Elapsed time since start of match (in minutes).";
            chart1.ChartAreas[0].AxisX.TitleFont = mainForm.DeriveFont(chart1.ChartAreas[0].AxisX.TitleFont, 0);

            ISet<string> squad = pd.File?.Squad ?? new HashSet<string>();
            NormalizedRoster roster =
                pd.File?.NormalizedMatch?.Rosters?.FirstOrDefault(r => r.Players?.Any(p => squad.Contains(p?.Attributes?.Stats?.Name)) ?? false);
            chart1.Titles[0].Text = $"HP over time for one match -- {sdt} -- Squad Rank: {roster?.Roster.Attributes.Stats.Rank}";
            chart1.Titles[0].Font = new Font(chart1.Titles[0].Font.FontFamily, 12, FontStyle.Bold);

            // Declare series first
            foreach (string playerName in pd.PlayerToEvents.Keys)
            {
                var series = chart1.Series.Add(playerName);
                series.Color = mainForm.ViewModel.ColorForPlayer(playerName);
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

            this.RecalcPointLabels();
        }
    }
}