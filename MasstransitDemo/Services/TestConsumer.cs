using MassTransit;
using MassTransit.Topology;
using MassTransit.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MasstransitDemo.Services
{
    public class TestConsumer : IConsumer<TestMessage>
    {
        private readonly ILogger<TestConsumer> _logger;
        private readonly ITestService _message;
        public TestConsumer(ILogger<TestConsumer> logger, ITestService message)
        {
            _logger = logger;
            _message = message;
        }

        public Task Consume(ConsumeContext<TestMessage> context)
        {
            if (context.TryGetPayload<IServiceScope>(out var existingServiceScope))
            {
                var ee = existingServiceScope.ServiceProvider.GetService<ITestService>();
            }
            _logger.LogInformation("index: {@index},test service hash code:{code},headers:{@headers}", context.Message, _message.GetHashCode(), context.Headers.ToList());
            return TaskUtil.Completed;
        }
    }

    [EntityName("test-message")]
    public interface TestMessage
    {
        string Content { get; }
    }
}
