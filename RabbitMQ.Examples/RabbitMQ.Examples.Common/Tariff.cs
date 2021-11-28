namespace TariffXml.Core;

public class Tariff
{
    public string Id { get; set; } = "";

    public string Name { get; set; } = "";

    public string OperatorName { get; set; } = "";

    public int Payroll { get; set; }

    public CallPrice[] CallPrices { get; set; } = Array.Empty<CallPrice>();

    public int PricePerSms { get; set; }

    public TariffParameter[] Parameters { get; set; } = Array.Empty<TariffParameter>();
}