using Autofac.Extensions.DependencyInjection;
using EventBus.Base.Standard.Configuration;
using EventBus.RabbitMQ.Standard.Configuration;
using EventBus.RabbitMQ.Standard.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RabbitMQ.Examples.Receiver
{
    internal static class Program
    {
        public static IConfiguration Configuration { get; set; }
        
        static void Main(string[] args)
        {
            Configuration = new ConfigurationBuilder().AddJsonFile($"appsettings.json", optional: false).Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            
            var serviceProviderFactory = new AutofacServiceProviderFactory();
            var builder = serviceProviderFactory.CreateBuilder(serviceCollection);
            var serviceProvider = serviceProviderFactory.CreateServiceProvider(builder);
            Configure(serviceProvider);
        }
        private static void ConfigureServices(IServiceCollection services)
        {
            var rabbitMqOptions = Configuration.GetSection("RabbitMq").Get<RabbitMqOptions>();

            services.AddRabbitMqConnection(rabbitMqOptions);
            services.AddRabbitMqRegistration(rabbitMqOptions);
            services.AddEventBusHandling(EventBusExtension.GetHandlers());
        }
        private static void Configure(IServiceProvider serviceProvider)
        {
            serviceProvider.SubscribeToEvents();
        }
    }
}