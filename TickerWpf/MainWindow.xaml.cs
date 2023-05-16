using MessageObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using TickerServiceInterface;

namespace TickerWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ITickerService _tickerService;
        private const string TICKER_SERVICE_LOCATION = "TickerService.dll";
        private Dictionary<string, List<TickerMessage>> _tickerHistory = new Dictionary<string, List<TickerMessage>>();
        
        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            btnSubscribeTicker1.IsEnabled = true;
            btnUnsubscribeTicker1.IsEnabled = false;
            btnSubscribeTicker2.IsEnabled = true;
            btnUnsubscribeTicker2.IsEnabled = false;
            _tickerService = LoadTickerService();
        }

        /// <summary>
        /// btnSubscribeTicker1 click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnSubscribeTicker1_Click(object sender, RoutedEventArgs e)
        {
            if (await _tickerService.Subscribe("Ticker1", DisplayTickers))
            {
                btnSubscribeTicker1.IsEnabled = false;
                btnUnsubscribeTicker1.IsEnabled = true;
                _tickerHistory.Clear();
            }
        }

        /// <summary>
        /// btnUnsubscribeTicker1 click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUnsubscribeTicker1_Click(object sender, RoutedEventArgs e)
        {
            if (_tickerService.Unsubscribe("Ticker1"))
            {
                btnSubscribeTicker1.IsEnabled = true;
                btnUnsubscribeTicker1.IsEnabled = false;
                this.Dispatcher.Invoke(() =>
                {
                    pnlTickers.Children.Clear();
                });
            }
        }

        /// <summary>
        /// btnSubscribeTicker2 click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnSubscribeTicker2_Click(object sender, RoutedEventArgs e)
        {
            if (await _tickerService.Subscribe("Ticker2", DisplayTickers))
            {
                btnSubscribeTicker2.IsEnabled = false;
                btnUnsubscribeTicker2.IsEnabled = true;
                _tickerHistory.Clear();
            }
        }

        /// <summary>
        /// btnUnsubscribeTicker2 click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUnsubscribeTicker2_Click(object sender, RoutedEventArgs e)
        {
            if (_tickerService.Unsubscribe("Ticker2"))
            {
                btnSubscribeTicker2.IsEnabled = true;
                btnUnsubscribeTicker2.IsEnabled = false;
                this.Dispatcher.Invoke(() =>
                {
                    pnlTickers.Children.Clear();
                });
            }
        }

        /// <summary>
        /// Display tickers in stack panel
        /// </summary>
        /// <param name="tickers"></param>
        private void DisplayTickers(IEnumerable<TickerMessage> tickers)
        {
            this.Dispatcher.Invoke(() =>
            {
                pnlTickers.Children.Clear();
            });

            foreach (TickerMessage ticker in tickers)
            {
                if (!_tickerHistory.ContainsKey(ticker.Ticker))
                {
                    _tickerHistory[ticker.Ticker] = new List<TickerMessage>();
                }
                Color background = CalculateBackgroundColor(ticker);
                _tickerHistory[ticker.Ticker].Add(ticker);
                this.Dispatcher.Invoke(() =>
                {
                    System.Windows.Controls.Label lblTicker = new System.Windows.Controls.Label()
                    {
                        Content = $"{ticker.Ticker} - {ticker.Price.ToString("0.00")}",
                        BorderBrush = new SolidColorBrush(),
                        Margin = new Thickness(5),
                        Background = new SolidColorBrush() { Color = background },
                    };
                    lblTicker.MouseDoubleClick += (o, e) => { OpenTickerHistory(_tickerHistory[ticker.Ticker]); };
                    pnlTickers.Children.Add(lblTicker);
                });
            }
        }

        /// <summary>
        /// Open dialog to show ticker price history
        /// </summary>
        /// <param name="history"></param>
        private void OpenTickerHistory(List<TickerMessage> history)
        {
            TickerHistory dialog = new TickerHistory(history.ToArray());
            dialog.ShowDialog();
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
