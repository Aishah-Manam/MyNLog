using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore;
using NLog.LayoutRenderers;
using NLog.Web;
using System;
using System.IO;
using NLog;

namespace LoggingExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Use correct nlog when in different environment
            var configuringFileName = "nlog.config";
            var environmentSpecificLogFileName = $"nlog.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.config";
            //var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();


            if (File.Exists(environmentSpecificLogFileName))
            {
                configuringFileName = environmentSpecificLogFileName;
            }

            // NLog: setup the logger first to catch all errors
            LayoutRenderer.Register("log-filename", (logevent) =>
            {
                var localNow = DateTime.UtcNow.AddHours(8);
                var reverseSorting = 99999999 - int.Parse($"{localNow: yyyyMMdd}");
                return $"ExampleLog{reverseSorting}x{localNow:yyyyMMdd}";
            });

            LayoutRenderer.Register("log-rowkey", (logevent) =>
            {
                var reverseSorting = long.MaxValue - DateTime.UtcNow.Ticks;
                return $"{reverseSorting}.{Guid.NewGuid():N}";
            });

            var logger = NLogBuilder.ConfigureNLog(configuringFileName).GetCurrentClassLogger();
            try
            {
                logger.Debug("Application started");
                //CreateHostBuilder(args).UseNLog().Build().Run();
            }
            catch (Exception ex)
            {
                //NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }

        }
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
