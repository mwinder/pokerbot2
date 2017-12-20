using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Targets;
using LogLevel = NLog.LogLevel;

namespace PokerBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var logging = new LoggingConfiguration();
            //var file = new FileTarget("log.txt");
            //var rule = new LoggingRule("*", LogLevel.Debug, file);

            //logging.AddTarget(file);
            //logging.LoggingRules.Add(rule);

            //LogManager.Configuration = logging;

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseUrls("http://*:2300;")
                .UseStartup<Startup>()
                .Build();
    }
}
