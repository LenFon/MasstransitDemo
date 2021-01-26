using MassTransit;
using MassTransit.Scheduling;
using MasstransitDemo.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MasstransitDemo
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ITestService _message;
        private readonly IBus _bus;
        public Worker(ILogger<Worker> logger, ITestService message, IBus bus)
        {
            _logger = logger;
            _message = message;
            _bus = bus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //var endpoint = await _bus.GetSendEndpoint(new Uri("queue:input-with-timeout"));

            //await endpoint.Send("hello", ctx => ctx.TimeToLive = TimeSpan.FromMinutes(1));
            //await _bus.ScheduleRecurringSend(new Uri("queue:input-test"), new MySchedule("0/10 * * * * ?", "test"), "hello test");
            //var schedulerEndpoint = await _bus.GetSendEndpoint(new Uri("queue:quartz"));
            //var scheduledRecurringMessage = await schedulerEndpoint.ScheduleRecurringSend(new Uri("queue:input-test"), new PollExternalSystemSchedule(), new PollExternalSystem());
            //var index = 1;
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    await _bus.Send<TestMessage>(new { Content = (index++).ToString() });
            //    //_logger.LogInformation("Worker running at: {time},message hash code:{code}", DateTimeOffset.Now, _message.GetHashCode());
            //    await Task.Delay(1000, stoppingToken);
            //}
        }

        public class PollExternalSystemSchedule : DefaultRecurringSchedule
        {
            public PollExternalSystemSchedule()
            {
                CronExpression = "0/10 * * * * ?"; // this means every minute
            }
        }

        public class PollExternalSystem { }

        private class MySchedule : DefaultRecurringSchedule
        {
            public MySchedule(string cronExpression, string description)
            {
                CronExpression = cronExpression;
                Description = description;
            }
        }
    }
}
