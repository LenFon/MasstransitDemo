using Autofac;
using HealthChecks.UI.Client;
using MassTransit;
using MassTransit.RabbitMqTransport;
using MassTransit.Topology;
using MasstransitDemo.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl;
using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace MasstransitDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddCustomHealthCheck(Configuration);
            services.Configure<RabbitMqSettings>(Configuration.GetSection("RabbitMqSettings"))
               .Configure<QuartzConfig>(Configuration.GetSection("quartz"));
            services.AddOptions();

            services
                .AddHealthChecksUI(settings =>
                {
                    settings.AddHealthCheckEndpoint("MasstransitDemo", "/hc");
                })
                .AddInMemoryStorage();

            services.AddSingleton<ISchedulerFactory>(x =>
            {
                var quartzConfig = x.GetRequiredService<IOptions<QuartzConfig>>().Value
                    .UpdateConnectionString(Configuration.GetConnectionString("scheduler-db"))
                    .ToNameValueCollection();
                return new StdSchedulerFactory(quartzConfig);
            });

            var mapMethod = Info.OfMethod(nameof(MassTransit), $"{nameof(MassTransit)}.{nameof(EndpointConvention)}", nameof(EndpointConvention.Map), nameof(Uri));

            var types = new[] { typeof(TestMessage) };
            foreach (var item in types)
            {
                var attr = (EntityNameAttribute)item.GetCustomAttribute(typeof(EntityNameAttribute));
                if (attr != null)
                {
                    mapMethod.MakeGenericMethod(new[] { item }).Invoke(null, new[] { new Uri($"exchange:{attr.EntityName}") });
                }
            }

            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumers(typeof(TestConsumer).Assembly);
                x.UsingRabbitMq(ConfigureBus);
            });

            services.AddMassTransitHostedService(true);
            //services.AddHostedService<Worker>();
        }

        private void ConfigureBus(IBusRegistrationContext context, IRabbitMqBusFactoryConfigurator cfg)
        {
            var settings = context.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

            cfg.Host(settings.Host, settings.VirtualHost, h =>
            {
                h.Username(settings.Username);
                h.Password(settings.Password);

                if (settings.SSLActive)
                {
                    h.UseSsl(ssl =>
                    {
                        ssl.ServerName = Dns.GetHostName();
                        ssl.AllowPolicyErrors(SslPolicyErrors.RemoteCertificateNameMismatch);
                        ssl.Certificate = GetX509Certificate(settings);
                        ssl.Protocol = SslProtocols.Tls12;
                        ssl.CertificateSelectionCallback = CertificateSelectionCallback;
                    });
                }
            });

            cfg.UsePublishFilter(typeof(Filters.MyPublishFilter<>), context);
            cfg.UseSendFilter(typeof(Filters.MySendFilter<>), context);
            cfg.UseConsumeFilter(typeof(Filters.MyConsumerFilter<>), context);
            //cfg.UseHealthCheck(context);

            cfg.UseJsonSerializer(); // Because we are using json within Quartz for serializer type
            cfg.UseInMemoryScheduler(context.GetRequiredService<ISchedulerFactory>());
            cfg.ConfigureEndpoints(context);
            cfg.UseInMemoryOutbox();
            //cfg.ManagementEndpoint(c =>
            //{
            //    c.UseMessageRetry(r =>
            //    {
            //        r.Ignore<ArgumentNullException>();
            //        r.Immediate(5);
            //    });
            //});
            cfg.ReceiveEndpoint("nowrap-test", e =>
            {
                //Hacky hack to allow MassTransit processing of messages not wrapped in envelopes.
                //Tested with SQS but there is nothing SQS specific so should work with any transport. 

                //data：{"Content":"hello"}
                e.UseNoEnvelopeMessageDeserializer(); // <---- add this line
                e.Consumer<TestConsumer>(context);
            });
            X509Certificate CertificateSelectionCallback(object sender, string targethost, X509CertificateCollection localcertificates, X509Certificate remotecertificate, string[] acceptableissuers)
            {
                var serverCertificate = localcertificates.OfType<X509Certificate2>()
                                        .FirstOrDefault(cert => cert.Thumbprint.ToLower() == settings.SSLThumbprint.ToLower());

                return serverCertificate ?? throw new Exception("Wrong certificate");
            }
        }

        private X509Certificate2 GetX509Certificate(RabbitMqSettings settings)
        {
            X509Certificate2 x509Certificate = null;

            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            try
            {
                X509Certificate2Collection certificatesInStore = store.Certificates;

                x509Certificate = certificatesInStore.OfType<X509Certificate2>()
                    .FirstOrDefault(cert => cert.Thumbprint?.ToLower() == settings.SSLThumbprint?.ToLower());
            }
            finally
            {
                store.Close();
            }

            return x509Certificate;
        }

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you by the factory.
        public void ConfigureContainer(ContainerBuilder builder)
        {
            // Register your own things directly with Autofac here. Don't
            // call builder.Populate(), that happens in AutofacServiceProviderFactory
            // for you.
            builder.RegisterModule(new MyApplicationModule());
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
                {
                    Predicate = r => r.Name.Contains("self")
                });

                endpoints.MapHealthChecksUI(options =>
                {
                    options.UIPath = "/hc-ui";
                });
            });
        }
    }
}
