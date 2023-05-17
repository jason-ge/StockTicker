using MessageObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickerGeneratorInterface
{
    public class TickerEventArgs : EventArgs
    {
        public IEnumerable<TickerMessage> Tickers;
        public TickerEventArgs(IEnumerable<TickerMessage> tickers)
        {
            Tickers = tickers;
        }
    }
}
