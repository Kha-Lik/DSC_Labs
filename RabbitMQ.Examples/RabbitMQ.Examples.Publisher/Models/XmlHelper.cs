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

    public static Tariff[] ProcessXml(XmlDocument xml)
    {
        var tariffs = new List<Tariff>();

        var xRoot = xml.DocumentElement;

        var childNodes = xRoot!.SelectNodes("*")!;
        foreach (XmlNode n in childNodes)
        {
            var tariff = new Tariff
            {
                Id = n.SelectSingleNode("@Id")?.Value ?? "",
                Name = n.SelectSingleNode("Name")?.InnerText ?? "",
                OperatorName = n.SelectSingleNode("OperatorName")?.InnerText ?? "",
                Payroll = Convert.ToInt32(n.SelectSingleNode("Payroll")?.InnerText),
                PricePerSms = Convert.ToInt32(n.SelectSingleNode("PricePerSms")?.InnerText)
            };

            var callPriceNodes = n.SelectNodes("//CallPrices/*")!;
            var parameterNodes = n.SelectNodes("//Parameters/*")!;

            tariff.CallPrices = callPriceNodes.Cast<XmlNode>()
                .Select(callPriceNode => new CallPrice
                {
                    CallPriceType = Enum.Parse<CallPriceType>(callPriceNode.SelectSingleNode("CallPriceType")?.InnerText ?? "Unknown"),
                    PricePerMinute = Convert.ToInt32(callPriceNode.SelectSingleNode("PricePerMinute")?.InnerText)
                }).ToArray();

            tariff.Parameters = parameterNodes.Cast<XmlNode>()
                .Select(parameterNode => new TariffParameter
                {
                    HasFavouriteNumber = Convert.ToInt32(parameterNode.SelectSingleNode("HasFavouriteNumber")?.InnerText),
                    BillingPlan = Enum.Parse<BillingPlan>(parameterNode.SelectSingleNode("BillingPlan")?.InnerText ?? "Unknown"),
                    ConnectionFee = Convert.ToInt32(parameterNode.SelectSingleNode("ConnectionFee")?.InnerText)
                }).ToArray();

            tariffs.Add(tariff);
        }

        tariffs.Sort(new TariffComparer());

        return tariffs.ToArray();
    }

    public static bool ValidateXml(XmlDocument xml)
    {
        var schema = new XmlSchemaSet();
        schema.Add("", "Tariff.xsd");

        var errors = false;
        xml.Schemas.Add(schema);
        xml.Validate((o, e) =>
        {
            if (e.Severity != XmlSeverityType.Error) 
                return;
            
            errors = true;
            Console.WriteLine(e.Message);
        });

        return !errors;
    }
}