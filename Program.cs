using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace budoco
{
    public class Program
    {
        private static LogEventLevel ParseLogLevel(dynamic value, LogEventLevel defaultLevel)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return defaultLevel;

            string stringValue = value.ToString();
            if (Enum.TryParse<LogEventLevel>(stringValue, true, out LogEventLevel result))
                return result;

            // Try parsing as integer
            if (int.TryParse(stringValue, out int intValue) && Enum.IsDefined(typeof(LogEventLevel), intValue))
                return (LogEventLevel)intValue;

            return defaultLevel;
        }

        public static int Main(string[] args)
        {
            Console.WriteLine("Main"); // here on purpose

            // Build a temporary configuration to initialize bd_config early
            var tempConfig = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Initialize bd_config with the temporary configuration
            bd_config.set_configuration(tempConfig);

            string log_file_location = bd_config.get(bd_config.LogFileFolder) + "/budoco_log_.txt";

            // Parse log levels with fallback to default values
            LogEventLevel microsoft_level = ParseLogLevel(bd_config.get(bd_config.DebugLogLevelMicrosoft), LogEventLevel.Warning);
            LogEventLevel budoco_lovel = ParseLogLevel(bd_config.get(bd_config.DebugLogLevelBudoco), LogEventLevel.Information);
            LogEventLevel postgres_level = ParseLogLevel(bd_config.get(bd_config.DebugLogLevelPostgres), LogEventLevel.Warning);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()

                .MinimumLevel.Override("Microsoft", microsoft_level)
                .MinimumLevel.Override("bd", budoco_lovel)
                .MinimumLevel.Override("bd_pg", postgres_level)

                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate:
                "{Timestamp:HH:mm:ss.ms} {Level:u3} {SourceContext} {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(log_file_location,
                    rollingInterval: RollingInterval.Day,
                outputTemplate:
                "{Timestamp:HH:mm:ss.ms} {Level:u3} {SourceContext} {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            // We have to do this after the Serilog setup.
            bd_util.init_serilog_context();

            // Write config to log, even though budoco can pick up most changes without
            // being restarted and we don't log the changed values.
            bd_config.log_config();

            try
            {
                Log.Information("Starting host");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
            webBuilder.UseKestrel(options =>
            {
                // Because we will control it with nginx,
                // which gives good specific 413 status rather than vague 400 status
                options.Limits.MaxRequestBodySize = null;

                // This works
                //options.ListenAnyIP(8000);
            });
        });

        // public static IHostBuilder CreateHostBuilder(string[] args) =>
        //     Host.CreateDefaultBuilder(args)
        //         // dotnet install package Serilog.AspNetCore
        //         .UseSerilog()

        //         .ConfigureWebHostDefaults(webBuilder =>
        //         {
        //             webBuilder.UseStartup<Startup>();
        //         });
    }
}
