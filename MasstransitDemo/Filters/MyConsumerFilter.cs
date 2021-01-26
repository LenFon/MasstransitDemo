using GreenPipes;
using MassTransit;
using MasstransitDemo.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MasstransitDemo.Filters
{
    public class MyConsumerFilter<T> : IFilter<ConsumeContext<T>> where T : class
    {
        private readonly ILogger<MyConsumerFilter<T>> _logger;
        private readonly ITestService _message;
        public MyConsumerFilter(ILogger<MyConsumerFilter<T>> logger, ITestService message)
        {
            _logger = logger;
            _message = message;
        }

        public void Probe(ProbeContext context)
        {
            //context.CreateFilterScope("scope");
        }

        public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            _logger.LogInformation("index: {@index},test service hash code:{code}", context.Message, _message.GetHashCode());
            await next.Send(context);
        }
    }
}
