using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasstransitDemo
{
    internal static class Extensions
    {
        public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration configuration)
        {
            var hcBuilder = services.AddHealthChecks();

            hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());

            //hcBuilder.AddSqlServer(
            //        configuration["ConnectionString"],
            //        name: "OrderingTaskDB-check",
            //        tags: new string[] { "orderingtaskdb" });

            //if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
            //{
            //    hcBuilder.AddAzureServiceBusTopic(
            //            configuration["EventBusConnection"],
            //            topicName: "eshop_event_bus",
            //            name: "orderingtask-servicebus-check",
            //            tags: new string[] { "servicebus" });
            //}
            //else
            //{
            //    hcBuilder.AddRabbitMQ(
            //            $"amqp://{configuration["EventBusConnection"]}",
            //            name: "orderingtask-rabbitmqbus-check",
            //            tags: new string[] { "rabbitmqbus" });
            //}


            return services;
        }

        public static ILoggingBuilder UseSerilog(this ILoggingBuilder builder, IConfiguration configuration)
        {
            //var seqServerUrl = configuration["Serilog:SeqServerUrl"];
            //var logstashUrl = configuration["Serilog:LogstashgUrl"];
            var outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}] {Message:lj}{NewLine}{Exception}";
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("ApplicationContext", Program.AppName)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: outputTemplate)
                //.WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)
                //.WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://logstash:8080" : logstashUrl)
                .ReadFrom.Configuration(configuration)
                .Destructure.ByTransforming<HeaderValue>(r => new { r.Key, r.Value })
                .CreateLogger();

            return builder;
        }
    }
}
