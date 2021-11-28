using TariffXml.Core;

namespace RabbitMQ.Examples.Publisher.Models;

public class CallPriceModel
{
    public CallPriceType CallPriceType { get; set; }

    public int PricePerMinute { get; set; }
}