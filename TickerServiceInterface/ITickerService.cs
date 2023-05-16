using MessageObjects;

namespace TickerServiceInterface
{
    /// <summary>
    /// TickerService Inerface
    /// </summary>
    public interface ITickerService
    {
        Task<bool> Subscribe(string ticker, Action<IEnumerable<TickerMessage>> callback);
        bool Unsubscribe(string ticker);
    }
}