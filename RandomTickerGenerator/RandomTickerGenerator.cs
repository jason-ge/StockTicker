using MessageObjects;
using System.Threading;
using System.Timers;
using TickerGeneratorInterface;

namespace RandomTickerGenerator
{
    public class RandomTickerGenerator : ITickerGenerator
    {
        private System.Timers.Timer timer;
        private Random rnd = new Random();
        public RandomTickerGenerator()
        {
            timer = new System.Timers.Timer(1000);
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = false;
        }

        public event EventHandler<TickerEventArgs> Tick;

        public void Start()
        {
            timer.Enabled = true;
        }

        public void Stop()
        {
            timer.Enabled = false;
        }

        private void OnTimedEvent(object? sender, ElapsedEventArgs e)
        {
            int ticker1 = rnd.Next(24000, 27000);
            int ticker2 = rnd.Next(18900, 21000);
            List<TickerMessage> tickers = new List<TickerMessage>()
            {
                new TickerMessage() { Ticker = "Ticker1", Price = (decimal)(ticker1/100.00), TimeStamp = DateTime.Now },
                new TickerMessage() { Ticker = "Ticker2", Price = (decimal)(ticker2/100.00), TimeStamp = DateTime.Now }
            };
            if (Tick != null)
            {
                Tick(this, new TickerEventArgs(tickers));
            }
        }
    }
}