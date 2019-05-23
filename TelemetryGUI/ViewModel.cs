using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public HashSet<string> Squad { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
            set => SetProperty(storage: ref _downloadActive, value);
        }

        public bool ReloadActive
        {
            get => _reloadActive;
            set => SetProperty(storage: ref _reloadActive, value);
        }
    }
}