using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

// SetProperty in VolatileBindableBase uses synchronization to make sure that writes to ref'd volatiles get flushed,
// so let's ignore the compiler warning about the runtime not respecting volatile on ref'd variables.
#pragma warning disable 420

namespace MyPubgTelemetry.GUI
{
    public class ViewModel : VolatileBindableBase
    {
        public ViewModel(Control mainControl) : base(mainControl)
        {
        }

        public TaskFactory TaskFactory { get; set; }

        public InputBox MatchSearchInputBox { get; set; }

        public BindingList<string> Squad { get; } = new BindingList<string>(new List<string> { "Squad (sum)" }) { AllowNew = true, AllowRemove = true };

        public Regex RegexCsv { get; } = new Regex(@"\s*,\s*", RegexOptions.IgnoreCase);

        public ConcurrentDictionary<string, string> AccountIds { get; } = new ConcurrentDictionary<string, string>();

        public BlockingCollection<PreparedData> PreparedDataQ { get; } = new BlockingCollection<PreparedData>();

        public CancellationTokenSource CtsMatchSwitch { get; set; }

        public CancellationTokenSource CtsMatchMetaData { get; set; }

        public StringBuilder LogBuffer { get; set; } = new StringBuilder();

        private volatile bool _downloadActive;
        private bool _reloadActive;

        public bool DownloadActive
        {
            get => _downloadActive;
            set => SetProperty(ref _downloadActive, value);
        }

        public bool ReloadActive
        {
            get => _reloadActive;
            set => SetProperty(ref _reloadActive, value);
        }

        public Dictionary<string, Color> PlayerColors { get; set; } = new Dictionary<string, Color>(StringComparer.CurrentCultureIgnoreCase);

        private int PlayerColorCycleIndex { get; set; } = -1;

        public List<Color> PlayerColorPalette { get; set; } = new List<Color>();

        public string SquadCsv
        {
            set
            {

                IList<string> newMates = RegexCsv.Split(value).Distinct().Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                List<string> toAdd = new List<string>();
                for (int i = Squad.Count - 1; i >= 1; i--)
                {
                    Squad.RemoveAt(i);
                }
                foreach (string newMate in newMates)
                {
                    Squad.Add(newMate);
                }
                //foreach (var v in values)
                //{
                //    if (Squad.IndexOf(v));
                //}
                //var i = 1;
                //for (; i < values.Count; i++)
                //{
                //    if (i >= Squad.Count)
                //    {
                //        break;
                //    }
                //    Squad[i] = values[i - 1];
                //}
                //i--;
                //while (values.Count >= Squad.Count)
                //{
                //    Squad.Add(values[i++]);
                //}
                //for (int i = Squad.Count - 1; i >= 1; i--)
                //{
                //    Squad.RemoveAt(i);
                //}
                //valueSet.RemoveWhere(x => x.Length == 0);
                //HashSet<string> squadSet = Squad.ToHashSet();
                //for (int i = Squad.Count - 1; i >= 0; i--)
                //{
                //    string item = Squad[i];
                //    if (!valueSet.Contains(item))
                //    {
                //        Squad.RemoveAt(i);
                //        valueSet.Remove(item);
                //    }
                //}
                //squadSet.Union(valueSet);
                //Squad.UnionWith(valueSet);
            }
            get => string.Join(",", Squad.ToList().GetRange(1, Squad.Count));
        }

        public string SelectedPlayer { get; set; }

        public Color NextPlayerColor()
        {
            if (PlayerColorPalette.Count == 0)
            {
                return Color.DeepPink;
            }
            if (PlayerColorCycleIndex >= PlayerColorPalette.Count)
            {
                PlayerColorCycleIndex = -1;
            }
            return PlayerColorPalette[++PlayerColorCycleIndex];
        }

        public Color ColorForPlayer(string name)
        {
            if (PlayerColors.ContainsKey(name))
            {
                return PlayerColors[name];
            }
            return PlayerColors[name] = NextPlayerColor();
        }

        public const string ChartTitleDateFormat = "ddd M/d/yy h:mm tt";
        public const string XAxisDateFormat = "h:mm:ss tt";
    }
}