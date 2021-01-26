using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

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
    }
}
