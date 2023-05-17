namespace TickerServerInterface
{
    public interface ITickerServer
    {
        void Publish();
        void Stop();
    }
}