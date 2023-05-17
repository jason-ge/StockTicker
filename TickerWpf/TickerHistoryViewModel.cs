using MessageObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickerWpf
{
    internal class TickerHistoryViewModel
    {
        private ObservableCollection<TickerMessage> _tickerHistory;
        public TickerHistoryViewModel(List<TickerMessage> tickerHistory)
        {
            _tickerHistory = new ObservableCollection<TickerMessage>(tickerHistory);
        }

        public void AddTickerItem(TickerMessage ticker)
        {
            _tickerHistory.Add(ticker);
        }

        public ObservableCollection<TickerMessage> TickerHistory { get { return _tickerHistory; } }
    }
}
