using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using MasstransitDemo.Services;

namespace MasstransitDemo
{
    internal class MyApplicationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TestService>().As<ITestService>().InstancePerLifetimeScope();
        }
    }
}