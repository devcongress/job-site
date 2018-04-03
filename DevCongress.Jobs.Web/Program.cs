using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.IO;

namespace DevCongress.Jobs.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseDefaultServiceProvider((context, options) =>
                {
                    options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
                })
                .ConfigureServices(services =>
                {
                    services
                    .AddAutofac()
                    .AddTransient<IConfigureOptions<KestrelServerOptions>, KestrelServerOptionsSetup>();
                })
                .UseKestrel(
                ////uncomment to remove upload limits
                //options => options.Limits.MaxRequestBodySize = null
                )
                .UseIISIntegration()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;

                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                      .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false, reloadOnChange: true)
                      .AddEnvironmentVariables();

                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }

                // Extra configuration specified via env. Can be used to specify docker secrets path.
                var extraConfig = Environment.GetEnvironmentVariable("PLUTONIUM_CONFIG_FILE");
                    if (!string.IsNullOrWhiteSpace(extraConfig))
                    {
                        config.AddJsonFile(extraConfig, optional: false);
                    }
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                // Configure serilog pipelines
                Log.Logger = new LoggerConfiguration()
                            .MinimumLevel.Debug()
                            .Enrich.FromLogContext()
                            .ReadFrom.Configuration(hostingContext.Configuration)
                            .CreateLogger();

                    logging
                        .AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                })
                .UseStartup<Startup>()
                //.UseUrls("http://localhost:5000;")
                .Build();
    }
}