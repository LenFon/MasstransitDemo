using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;

namespace MasstransitDemo
{
    public class Program
    {
        internal static readonly string AppName = typeof(Program).Assembly.GetName().Name;

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
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
                .ConfigureLogging((host, builder) => builder.ClearProviders().UseSerilog(host.Configuration).AddSerilog())
                ;
    }
}
