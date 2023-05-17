using MessageObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using TickerServiceInterface;

namespace TickerWpf
{
    internal class TickerViewModel : INotifyPropertyChanged
    {
        private string _tickerName;
        private decimal _price;
        private DateTime _timestamp;
        private Color _background;
        private List<TickerMessage> _history;
        private ICommand _showHistoryCommand;
        public TickerViewModel(string tickerName, decimal price, DateTime timestamp, List<TickerMessage> history)
        {
            _tickerName = tickerName;
            _price = price;
            _timestamp = timestamp;
            _history = history;
            _showHistoryCommand = new RelayCommand((o) =>
            {
                TickerHistory dialog = new TickerHistory();
                TickerHistoryViewModel viewModel = new TickerHistoryViewModel(_history);
                dialog.DataContext = viewModel;
                dialog.ShowDialog();
            });
        }

        public Color Background
        {
            get { return _background; }
            set { _background = value; }
        }

        public string Title { get { return _tickerName + " - " + Price.ToString(); } }
        public string TickerName
        {
            get { return _tickerName; }
        }
        public decimal Price
        {
            get { return _price; }
        }
        public DateTime TimeStamp
        {
            get { return _timestamp; }
        }

        public ICommand ShowHistoryCommand
        {
            get
            {
                return _showHistoryCommand;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    internal class MainViewModel : INotifyPropertyChanged
    {
        private ITickerService _tickerService;
        private const string TICKER_SERVICE_LOCATION = "TickerService.dll";
        private Dictionary<string, List<TickerMessage>> _tickerHistory = new Dictionary<string, List<TickerMessage>>();
        private ObservableCollection<TickerViewModel> _activeTickers = new ObservableCollection<TickerViewModel>();
        private ICommand _btnSubscribeTickerClickCommand;
        private ICommand _btnUnsubscribeTickerClickCommand;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            _tickerService = LoadTickerService();
            // Following line is just for testing purpose since I am not able to make the view to display the tickers even though the data are there
            _activeTickers.Add(new TickerViewModel("Ticker1", (decimal)100.00, DateTime.Now, new List<TickerMessage>()));
            _btnSubscribeTickerClickCommand = new RelayCommand(Subscribe, CanExecute);
            _btnUnsubscribeTickerClickCommand = new RelayCommand(Unsubscribe, CanExecute);
        }

        public ICommand btnSubscribeTicker_Click
        {
            get
            {
                return _btnSubscribeTickerClickCommand;
            }
        }

        public ICommand btnUnsubscribeTicker_Click
        {
            get
            {
                return _btnUnsubscribeTickerClickCommand;
            }
        }

        private async void Subscribe(object parameter)
        {
            await _tickerService.Subscribe(parameter.ToString(), DisplayTickers);
            //  Add logic to enable/disable other buttons
        }
        private void Unsubscribe(object parameter)
        {
            _tickerService.Unsubscribe(parameter.ToString());
            //  Add logic to enable/disable other buttons
        }

        private bool CanExecute(object parameter)
        {
            // Add logic to determin if the button should be enabled/disabled.
            return true;
        }

        public ObservableCollection<TickerViewModel> ActiveTickers
        {
            get
            {
                return _activeTickers;
            }
        }
        /// <summary>
        /// Display tickers in stack panel
        /// </summary>
        /// <param name="tickers"></param>
        private void DisplayTickers(IEnumerable<TickerMessage> tickers)
        {
            ActiveTickers.Clear();
            foreach (TickerMessage ticker in tickers)
            {
                if (!_tickerHistory.ContainsKey(ticker.Ticker))
                {
                    _tickerHistory[ticker.Ticker] = new List<TickerMessage>();
                }
                Color background = CalculateBackgroundColor(ticker);
                _tickerHistory[ticker.Ticker].Add(ticker);
                var viewModel = new TickerViewModel(ticker.Ticker, ticker.Price, ticker.TimeStamp, _tickerHistory[ticker.Ticker]);
                viewModel.Background = background;
                ActiveTickers.Add(viewModel);
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ActiveTickers)));
            }
        }

        /// <summary>
        /// Calculate the background color of the tick based on the price compared to the previous one.
        /// </summary>
        /// <param name="ticker">Stock ticker name</param>
        /// <returns>Background color</returns>
        private Color CalculateBackgroundColor(TickerMessage ticker)
        {
            if (_tickerHistory[ticker.Ticker].Count == 0)
            {
                return Colors.White;
            }
            else
            {
                decimal lastPrice = _tickerHistory[ticker.Ticker].Last<TickerMessage>().Price;
                if (ticker.Price > lastPrice)
                {
                    return Colors.Green;
                }
                else if (ticker.Price < lastPrice)
                {
                    return Colors.Red;
                }
                else { return Colors.White; }
            }
        }

        /// <summary>
        /// Load ticker service based on the configured assembly location
        /// </summary>
        /// <returns>An instance of ITickerServiceI</returns>
        /// <exception cref="ArgumentException"></exception>
        private ITickerService LoadTickerService()
        {
            Assembly assembly = Assembly.LoadFrom(TICKER_SERVICE_LOCATION);
            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(ITickerService).IsAssignableFrom(type))
                {
                    ITickerService? result = Activator.CreateInstance(type) as ITickerService;
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            throw new ArgumentException("Cannot find ITickerService implementation in assembly {0}", TICKER_SERVICE_LOCATION);
        }
    }
}
