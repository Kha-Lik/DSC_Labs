using System.ComponentModel.DataAnnotations;

namespace RabbitMQ.Examples.Publisher.Models;

public class TariffModel
{
    public string Id { get; set; } = string.Empty;
    [Required]
    public string Name { get; set; } = "";

    [Required]
    public string OperatorName { get; set; } = "";

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Payroll must be positive")]
    public int Payroll { get; set; }

    public CallPriceModel[] CallPrices { get; set; } = Array.Empty<CallPriceModel>();

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Payroll must be positive")]
    public int PricePerSms { get; set; }

    public TariffParameterModel[] Parameters { get; set; } = Array.Empty<TariffParameterModel>();
}