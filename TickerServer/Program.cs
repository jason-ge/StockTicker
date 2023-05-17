
using MessageObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RandomTickerGenerator;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using TickerGeneratorInterface;
using TickerServer;
using TickerServerInterface;

// build config
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddOptions();
        services.AddSingleton<ITickerServer, TcpTickerServer>();
        services.AddSingleton<ITickerGenerator, RandomTickerGenerator.RandomTickerGenerator>();
        services.Configure<Settings>(configuration.GetSection("Settings"));
    })
    .Build();
ITickerServer tickers = host.Services.GetRequiredService<ITickerServer>();
tickers.Publish();

Console.ReadKey();

tickers.Stop();


