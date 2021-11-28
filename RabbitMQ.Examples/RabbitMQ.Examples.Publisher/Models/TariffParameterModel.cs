using TariffXml.Core;

namespace RabbitMQ.Examples.Publisher.Models;

public class TariffParameterModel
{
    
    public int HasFavouriteNumber { get; set; }

    public BillingPlan BillingPlan { get; set; }

    public int ConnectionFee { get; set; }
}