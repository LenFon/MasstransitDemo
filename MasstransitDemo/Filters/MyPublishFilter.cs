using GreenPipes;
using MassTransit;
using MasstransitDemo.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasstransitDemo.Filters
{
    public class MyPublishFilter<T> : IFilter<PublishContext<T>> where T : class
    {
        private readonly ILogger<MyPublishFilter<T>> _logger;
        private readonly ITestService _message;
        public MyPublishFilter(ILogger<MyPublishFilter<T>> logger, ITestService message)
        {
            _logger = logger;
            _message = message;
        }

        public void Probe(ProbeContext context)
        {
        }

        public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
        {
            context.Headers.Set("user", "lenfon");
            _logger.LogInformation("index: {@index},test service hash code:{code}", context.Message, _message.GetHashCode());
            await next.Send(context);
        }
    }
}
