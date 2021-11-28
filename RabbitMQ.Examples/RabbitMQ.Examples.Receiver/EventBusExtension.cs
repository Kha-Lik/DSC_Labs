using EventBus.Base.Standard;
using Microsoft.Extensions.DependencyInjection;

namespace RabbitMQ.Examples.Receiver
{
    public static class EventBusExtension
    {
        public static IEnumerable<IIntegrationEventHandler> GetHandlers()
        {
            return new List<IIntegrationEventHandler>
            {
                new TariffInsertEventHandler()
            };
        }

        public static void SubscribeToEvents(this IServiceProvider serviceProvider)
        {
            var eventBus = serviceProvider.GetRequiredService<IEventBus>();
            eventBus.Subscribe<TariffInsertEvent, TariffInsertEventHandler>();
        }
    }
}