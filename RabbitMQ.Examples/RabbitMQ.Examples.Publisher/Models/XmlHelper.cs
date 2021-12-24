using System.Xml;
using System.Xml.Schema;
using Newtonsoft.Json;
using RabbitMQ.Examples.Common;

namespace RabbitMQ.Examples.Publisher.Models;

public static class XmlHelper
{
    public static string XmlToJson(XmlDocument xml)
    {
        return JsonConvert.SerializeXmlNode(xml, Newtonsoft.Json.Formatting.Indented, false);
    }

    public static Tariff ProcessXml(XmlDocument xml)
    {
        var xRoot = xml.DocumentElement!;


        var tariff = new Tariff
        {
            Id = xRoot.SelectSingleNode("@Id")?.Value ?? "",
            Name = xRoot.SelectSingleNode("Name")?.InnerText ?? "",
            OperatorName = xRoot.SelectSingleNode("OperatorName")?.InnerText ?? "",
            Payroll = Convert.ToInt32(xRoot.SelectSingleNode("Payroll")?.InnerText),
            PricePerSms = Convert.ToInt32(xRoot.SelectSingleNode("PricePerSms")?.InnerText)
        };

        var callPriceNodes = xRoot.SelectNodes("//CallPrices/*")!;
        var parameterNodes = xRoot.SelectNodes("//Parameters/*")!;

        tariff.CallPrices = callPriceNodes.Cast<XmlNode>()
            .Select(callPriceNode => new CallPrice
            {
                CallPriceType =
                    Enum.Parse<CallPriceType>(callPriceNode.SelectSingleNode("CallPriceType")?.InnerText ?? "Unknown"),
                PricePerMinute = Convert.ToInt32(callPriceNode.SelectSingleNode("PricePerMinute")?.InnerText)
            }).ToArray();

        tariff.Parameters = parameterNodes.Cast<XmlNode>()
            .Select(parameterNode => new TariffParameter
            {
                HasFavouriteNumber = Convert.ToInt32(parameterNode.SelectSingleNode("HasFavouriteNumber")?.InnerText),
                BillingPlan =
                    Enum.Parse<BillingPlan>(parameterNode.SelectSingleNode("BillingPlan")?.InnerText ?? "Unknown"),
                ConnectionFee = Convert.ToInt32(parameterNode.SelectSingleNode("ConnectionFee")?.InnerText)
            }).ToArray();

        return tariff;
    }

    public static bool ValidateXml(XmlDocument xml, out IEnumerable<string> errors)
    {
        var schema = new XmlSchemaSet();
        schema.Add("", "Tariff.xsd");

        var hasErrors = false;
        var errorsList = new LinkedList<string>();
        xml.Schemas.Add(schema);
        xml.Validate((o, e) =>
        {
            if (e.Severity != XmlSeverityType.Error)
                return;

            hasErrors = true;
            errorsList.AddLast(e.Message);
            Console.WriteLine(e.Message);
        });

        errors = errorsList;
        return !hasErrors;
    }
}