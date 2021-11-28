using EventBus.Base.Standard;
using TariffXml.Core;

namespace RabbitMQ.Examples.Receiver;

public class TariffInsertEvent : IntegrationEvent
{
    public Tariff Tariff { get; set; } = new();

    public TariffInsertEvent(Tariff tariff)
    {
        Tariff = tariff;
    }
}