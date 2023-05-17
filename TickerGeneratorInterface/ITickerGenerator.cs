namespace TickerGeneratorInterface
{
    public interface ITickerGenerator
    {
        event EventHandler<TickerEventArgs> Tick;
        void Start();
        void Stop();
    }
}