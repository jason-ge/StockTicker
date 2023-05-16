using MessageObjects;
using System.Windows;

namespace TickerWpf
{
    /// <summary>
    /// Interaction logic for TickerHistory.xaml
    /// </summary>
    public partial class TickerHistory : Window
    {
        public TickerMessage[] _history;
        public TickerHistory(TickerMessage[] history)
        {
            _history = history;
            InitializeComponent();
            lstTickerHistory.ItemsSource = _history;
        }
    }
}
