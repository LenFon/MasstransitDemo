using Autofac.Extensions.DependencyInjection;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;

namespace MasstransitDemo
{
    public class Program
    {
        internal static readonly string AppName = typeof(Program).Assembly.GetName().Name;

        public static int Main(string[] args)
        {
            var outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}] {Message:lj}{NewLine}{Exception}";
            using var logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .Enrich.WithProperty("ApplicationContext", Program.AppName)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(outputTemplate: outputTemplate)
                    .WriteTo.Seq("http://10.4.7.56:5341")
                    .CreateLogger();

            try
            {
                logger.Information("Starting host");

                CreateHostBuilder(args).Build().Run();

                return 0;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Host terminated unexpectedly");

                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webHostBuilder =>
                {
                    webHostBuilder.UseStartup<Startup>();
                })
                //.ConfigureAppConfiguration((host, builder) =>
                //{
                //    //builder.SetBasePath(Directory.GetCurrentDirectory());
                //    //builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                //    //builder.AddJsonFile($"appsettings.{host.HostingEnvironment.EnvironmentName}.json", optional: true);
                //    builder.AddEnvironmentVariables();
                //    builder.AddCommandLine(args);
                //})
                .UseSerilog((host, config) =>
                {
                    //var seqServerUrl = configuration["Serilog:SeqServerUrl"];
                    //var logstashUrl = configuration["Serilog:LogstashgUrl"];
                    var outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}] {Message:lj}{NewLine}{Exception}";
                    config
                        .MinimumLevel.Verbose()
                        .Enrich.WithProperty("ApplicationContext", Program.AppName)
                        .Enrich.FromLogContext()
                        .WriteTo.Console(outputTemplate: outputTemplate)
                        .WriteTo.Seq("http://10.4.7.56:5341")
                        //.WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)
                        //.WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://logstash:8080" : logstashUrl)
                        .ReadFrom.Configuration(host.Configuration)
                        .Destructure.ByTransforming<HeaderValue>(r => new { r.Key, r.Value });
                })
                ;
    }
}
