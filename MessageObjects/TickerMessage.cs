namespace MessageObjects
{
    public class TickerMessage
    {
        public string Ticker { get; set; }
        public decimal Price { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}