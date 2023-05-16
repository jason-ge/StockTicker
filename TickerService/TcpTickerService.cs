using MessageObjects;
using Newtonsoft.Json;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using TickerServiceInterface;

namespace TickerService
{
    /// <summary>
    /// Ticker service using TcpClient
    /// </summary>
    public class TcpTickerService : ITickerService
    {
        private TcpClient _tcpClient;
        private string _hostName;
        private List<string> _tickers;
        private int _port;
        private object _locker = new object();
        private Action<IEnumerable<TickerMessage>> _tickersReceived;
        private CancellationTokenSource _token;
        private Configuration _config;

        /// <summary>
        /// Constructor
        /// </summary>
        public TcpTickerService()
        {
            string assemblyPath = new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;
            _config = ConfigurationManager.OpenExeConfiguration(assemblyPath);
            _tickers = new List<string>();
        }

        /// <summary>
        /// Subscribe the a ticker.
        /// </summary>
        /// <param name="ticker">Stock ticker name to subscribe</param>
        /// <param name="onTickersReceived">Call back function when ticker data are received.</param>
        /// <returns>Boolean value to indiate the subscribe is successful or not.</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Subscribe(string ticker, Action<IEnumerable<TickerMessage>> onTickersReceived)
        {
            if (_config == null)
            {
                throw new Exception("Missing configuraion file for the service");
            }
            if (!_tickers.Contains(ticker))
            {
                _tickers.Add(ticker);
            }
            _hostName = _config.AppSettings.Settings["HostName"].Value;
            _port = int.Parse(_config.AppSettings.Settings["Port"].Value);
            _tickersReceived = onTickersReceived;
            try
            {
                if (_tcpClient == null)
                {
                    _tcpClient = new TcpClient();
                    await _tcpClient.ConnectAsync(GetIPAddressFromHost(_hostName), _port);
                }
                _token = new CancellationTokenSource();
                Task.Factory.StartNew(async () =>
                {
                    while (!_token.IsCancellationRequested)
                    {
                        List<TickerMessage> tickers;
                        lock (_locker)
                        {
                            tickers = ReceiveTickers();
                        }
                        if (tickers != null && _tickersReceived != null)
                        {
                            _tickersReceived(tickers.Where(t => _tickers.Contains(t.Ticker)));
                        }
                        else
                        {
                            await Task.Delay(1000);
                        }
                    }
                });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR - {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Unsubscribe a ticker.
        /// </summary>
        /// <param name="ticker">Stock ticker name to unsubscribe</param>
        /// <returns>Boolean value to indiate the Unsubscribe is successful or not.</returns>
        public bool Unsubscribe(string ticker)
        {
            _tickers.Remove(ticker);
            if (_tickers.Count == 0)
            {
                lock (_locker)
                {
                    _token.Cancel();
                    _tcpClient.Close();
                    _tcpClient.Dispose();
                    _tcpClient = null;
                }
            }
            return true;
        }

        /// <summary>
        /// Function to read from ticker sever of the ticker data
        /// </summary>
        /// <returns></returns>
        private List<TickerMessage> ReceiveTickers()
        {
            if (_tcpClient == null || _tcpClient.Connected == false)
            {
                return null;
            }
            NetworkStream networkStream = _tcpClient.GetStream();
            if (networkStream.DataAvailable)
            {
                StreamReader streamReader = new StreamReader(networkStream);
                string tikcerData = streamReader.ReadLine();
                if (tikcerData == null)
                {
                    return null;
                }
                else
                {
                    return JsonConvert.DeserializeObject<List<TickerMessage>>(tikcerData);
                }
            }
            return null;
        }

        /// <summary>
        /// Conver host name to IP address.
        /// </summary>
        /// <param name="host">Host name</param>
        /// <returns>IP Address</returns>
        private IPAddress GetIPAddressFromHost(string host)
        {
            var entry = Dns.GetHostEntry(host);
            if (entry == null || entry.AddressList == null || entry.AddressList.Length == 0)
            {
                Console.WriteLine($"Cannot resolve host name {host}");
                return null;
            }

            IPAddress ipAddress = entry.AddressList.First(addr => addr.AddressFamily == AddressFamily.InterNetwork);
            if (ipAddress == null)
            {
                Console.WriteLine($"Cannot resolve host name {host} to IP address");
                return null;
            }
            return ipAddress;
        }
    }
}