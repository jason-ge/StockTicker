using MessageObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TickerServerInterface;
using TickerGeneratorInterface;
using System.Reflection.Emit;
using System.Net.Http;

namespace TickerServer
{
    internal class TcpTickerServer : ITickerServer
    {
        private Settings _settings;
        private object locker = new object();
        private ITickerGenerator _generator;
        private CancellationTokenSource _token;
        private List<TcpClient> _tcpClients;
        public TcpTickerServer(IOptions<Settings> settings, ITickerGenerator generator)
        {
            _settings = settings.Value;
            _generator = generator;
        }

        public void Publish()
        {
            IPAddress ipAddress = GetIPAddressFromHost(_settings.HostName);
            if (ipAddress == null)
            {
                throw new Exception($"Cannot resolve host name {_settings.HostName}");
            }
            _generator.Tick += TickersReceived;
            _generator.Start();
            _token = new CancellationTokenSource();

            _tcpClients = new List<TcpClient>();
            Task.Factory.StartNew(() =>
            {
                var server = new TcpListener(ipAddress, _settings.Port);
                server.Start();
                while (!_token.IsCancellationRequested)
                {
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Accepted connection from {0}", client.Client.RemoteEndPoint);
                    lock (locker)
                    {
                        _tcpClients.Add(client);
                    }
                }
            });
        }

        public void Stop()
        {
            _generator.Stop();
            _token.Cancel();
            foreach (TcpClient tcpClient in _tcpClients) {
                tcpClient.Close();
            }
            _tcpClients.Clear();
        }

        private void TickersReceived(object? sender, TickerEventArgs e)
        {
            lock (locker)
            {
                if (_tcpClients.Count == 0)
                {
                    Console.WriteLine("No connection yet");
                }
                var cloneClients = _tcpClients.ToArray();
                foreach (TcpClient tcpClient in cloneClients)
                {
                    if (tcpClient.Connected)
                    {
                        NetworkStream networkStream = tcpClient.GetStream();
                        StreamWriter streamWriter = new StreamWriter(networkStream);
                        string data = JsonConvert.SerializeObject(e.Tickers);
                        Console.WriteLine("Messege {0} to {1}", data, tcpClient.Client.RemoteEndPoint);
                        streamWriter.WriteLine(data);
                        streamWriter.Flush();
                    }
                    else
                    {
                        Console.WriteLine("Connection from client {0} has terminated.", tcpClient.Client.RemoteEndPoint);
                        _tcpClients.Remove(tcpClient);
                    }
                }
            }
        }

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