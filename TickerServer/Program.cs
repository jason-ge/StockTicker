
using MessageObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using TickerServer;

using IHost host = Host.CreateDefaultBuilder(args).Build();

IConfiguration config = host.Services.GetRequiredService<IConfiguration>();
Settings settings = config.GetRequiredSection("Settings").Get<Settings>();
IPAddress ipAddress = GetIPAddressFromHost(settings.HostName);
if (ipAddress == null)
{
    return;
}
CancellationTokenSource _token = new CancellationTokenSource();

List<TcpClient> tcpClients = new List<TcpClient>();
object locker = new object();
Random rnd = new Random();

System.Timers.Timer timer = new System.Timers.Timer(1000);
timer.Elapsed += OnTimedEvent;
timer.AutoReset = true;
timer.Enabled = true;

Task.Factory.StartNew(() =>
{
    var server = new TcpListener(ipAddress, settings.Port);
    server.Start();
    while (!_token.IsCancellationRequested)
    {
        TcpClient client = server.AcceptTcpClient();
        Console.WriteLine("Accepted connection from {0}", client.Client.RemoteEndPoint);
        lock(locker) {
            tcpClients.Add(client);
        }
    }
});

Console.ReadKey();
_token.Cancel();
timer.Enabled = false;
void OnTimedEvent(object? sender, ElapsedEventArgs e)
{
    int ticker1 = rnd.Next(24000, 27000);
    int ticker2 = rnd.Next(18900, 21000);
    List<TickerMessage> tickers = new List<TickerMessage>()
    {
        new TickerMessage() { Ticker = "Ticker1", Price = (decimal)(ticker1/100.00), TimeStamp = DateTime.Now },
        new TickerMessage() { Ticker = "Ticker2", Price = (decimal)(ticker2/100.00), TimeStamp = DateTime.Now }
    };
    lock (locker)
    {
        if (tcpClients.Count == 0)
        {
            Console.WriteLine("No connection yet");
        }
        var cloneClients = tcpClients.ToArray();
        foreach (TcpClient tcpClient in cloneClients)
        {
            if (tcpClient.Connected)
            {
                NetworkStream networkStream = tcpClient.GetStream();
                StreamWriter streamWriter = new StreamWriter(networkStream);
                string data = JsonConvert.SerializeObject(tickers);
                Console.WriteLine("Messege {0} to {1}", data, tcpClient.Client.RemoteEndPoint);
                streamWriter.WriteLine(data);
                streamWriter.Flush();
            }
            else
            {
                Console.WriteLine("Connection from client {0} has terminated.", tcpClient.Client.RemoteEndPoint);
                tcpClients.Remove(tcpClient);
            }
        }
    }
}

IPAddress GetIPAddressFromHost(string host)
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

