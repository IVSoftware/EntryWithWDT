using IVSoftware.Portable;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace EntryWithWDT
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        new MainPageBindingContext BindingContext => (MainPageBindingContext)base.BindingContext;
    }
    class MainPageBindingContext : INotifyPropertyChanged
    {
        public MainPageBindingContext()
        {
            IncrementCountCommand = new Command(onIncrementCount);
            OverlayTappedCommand = new Command((o)=>_wdtOverlay.Cancel());
        }
        public ICommand SetCountCommand { get; private set; }
        public ICommand IncrementCountCommand { get; private set; }
        private void onIncrementCount(object o)
        {
            Count++;
            _wdtCount.StartOrRestart(initialAction: () => _stopwatch = Stopwatch.StartNew(),
            completeAction: () =>
            {
                _stopwatch.Stop();
                var message = $"User stopped clicking after {_stopwatch.Elapsed.ToString(@"ss\:fff")}";
                OverlayText = message;

                // Show overlay briefly
                _wdtOverlay.StartOrRestart(
                    initialAction: () => OverlayVisible = true, 
                    completeAction: () =>OverlayVisible = false);
            });
        }
        public ICommand OverlayTappedCommand { get; private set; }

        WatchdogTimer _wdtCount = new WatchdogTimer { Interval = TimeSpan.FromSeconds(0.5) };
        Stopwatch _stopwatch;
        public string ButtonText
        {
            get => _buttonText;
            set
            {
                if (!Equals(_buttonText, value))
                {
                    _buttonText = value;
                    OnPropertyChanged();
                }
            }
        }
        string _buttonText = "Click me";
        public int Count
        {
            get => _count;
            set
            {
                if (!Equals(_count, value))
                {
                    _count = value;
                    if (Count == 0)
                    {
                        ButtonText = "Click me";
                    }
                    else
                    {
                        ButtonText = $"Clicked {Count} times";
                    }
                    OnPropertyChanged();
                }
            }
        }
        int _count = default;

        public bool OverlayVisible
        {
            get => _overlayVisible;
            set
            {
                if (!Equals(_overlayVisible, value))
                {
                    _overlayVisible = value;
                    OnPropertyChanged();
                }
            }
        }
        bool _overlayVisible = default;

        public string EntryText
        {
            get => _entryText;
            set
            {
                if (!Equals(_entryText, value))
                {
                    _entryText = value;
                    OnPropertyChanged();
                    if (string.IsNullOrWhiteSpace(EntryText))
                    {
                        _wdtEntry.Cancel();
                    }
                    else
                    {
                        _wdtEntry.StartOrRestart(action: () =>
                        {
                            OverlayText = GenerateFilterCommand();
                            // Show overlay briefly
                            _wdtOverlay.StartOrRestart(
                                initialAction: () =>
                                {
                                    IsEntryEnabled = false;
                                    OverlayVisible = true;
                                },
                                completeAction: () =>
                                {
                                    OverlayVisible = false;
                                    IsEntryEnabled = true;
                                }
                             );
                        });
                    }
                }
            }
        }
        string _entryText = string.Empty;

        // <PackageReference Include="IVSoftware.Portable.WatchdogTimer" Version="1.2.1" />
        WatchdogTimer _wdtEntry = new WatchdogTimer { Interval = TimeSpan.FromSeconds(5) };
        WatchdogTimer _wdtOverlay = new WatchdogTimer { Interval = TimeSpan.FromSeconds(10) };
        private string GenerateFilterCommand()
        {
            var where = string.Join(
                "\nAND\n", 
                EntryText.Split(' ')
                .Select(_=>_.Trim())
                .Where(_=>!string.IsNullOrWhiteSpace(_))
                .Select(_ => $"desc LIKE'%{_}%'"));
            var sql = $"SELECT * FROM items \nWHERE {where}";
            return sql;
        }

        public string OverlayText
        {
            get => _overlayText;
            set
            {
                if (!Equals(_overlayText, value))
                {
                    _overlayText = value;
                    OnPropertyChanged();
                }
            }
        }
        string _overlayText = string.Empty;

        public bool IsEntryEnabled
        {
            get => _isEntryEnabled;
            set
            {
                if (!Equals(_isEntryEnabled, value))
                {
                    _isEntryEnabled = value;
                    OnPropertyChanged();
                }
            }
        }
        bool _isEntryEnabled = true;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
