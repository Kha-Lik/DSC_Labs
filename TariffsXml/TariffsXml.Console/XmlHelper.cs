using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Newtonsoft.Json;
using TariffXml.Core;

namespace TariffsXml.ConsoleApp;

public static class XmlHelper
{
    public static void XmlToJson()
    {
        var xDoc = new XmlDocument();
        xDoc.Load("collection.xml");
        var xRoot = xDoc.DocumentElement;

        var childNodes = xRoot!.SelectNodes("*")!;
        foreach (XmlNode n in childNodes)
            Console.WriteLine(JsonConvert.SerializeXmlNode(n, Newtonsoft.Json.Formatting.Indented, false));
    }

    public static Tariff[] ProcessXml()
    {
        var tariffs = new List<Tariff>();

        var xDoc = new XmlDocument();
        xDoc.Load("collection.xml");
        var xRoot = xDoc.DocumentElement;

        var childNodes = xRoot!.SelectNodes("*")!;
        foreach (XmlNode n in childNodes)
        {
            var tariff = new Tariff
            {
                Id = n.SelectSingleNode("@Id")?.Value ?? "",
                Name = n.SelectSingleNode("Name")?.InnerText ?? "",
                OperatorName = n.SelectSingleNode("OperatorName")?.InnerText ?? "",
                Payroll = Convert.ToInt32(n.SelectSingleNode("Payroll")?.InnerText),
            };

            var callPriceNodes = n.SelectNodes("//CallPrices/*")!;
            var parameterNodes = n.SelectNodes("//Parameters/*")!;

            tariff.CallPrices = (callPriceNodes.Cast<XmlNode>()
                .Select(callPriceNode => new CallPrice
                {
                    CallPriceType = Enum.Parse<CallPriceType>(callPriceNode.SelectSingleNode("CallPriceType")?.InnerText ?? "Unknown"),
                    PricePerMinute = Convert.ToInt32(callPriceNode.SelectSingleNode("PricePerMinute")?.InnerText)
                })).ToArray();

            tariff.Parameters = (parameterNodes.Cast<XmlNode>()
                .Select(parameterNode => new TariffParameter
                {
                    HasFavouriteNumber = Convert.ToInt32(parameterNode.SelectSingleNode("HasFavouriteNumber")?.InnerText),
                    BillingPlan = Enum.Parse<BillingPlan>(parameterNode.SelectSingleNode("BillingPlan")?.InnerText ?? "Unknown"),
                    ConnectionFee = Convert.ToInt32(parameterNode.SelectSingleNode("ConnectionFee")?.InnerText)
                })).ToArray();

            tariffs.Add(tariff);
        }

        tariffs.Sort(new TariffComparer());

        return tariffs.ToArray();
    }

    public static void ValidateXml()
    {
        var schema = new XmlSchemaSet();
        schema.Add("", "Tariff.xsd");

        var xmlDoc = new XmlDocument();
        xmlDoc.Load("invalid.xml");
        xmlDoc.Schemas.Add(schema);
        xmlDoc.Validate(ValidationEventHandler!);
    }

    private static void ValidationEventHandler(object sender, ValidationEventArgs e)
    {
        if (e.Severity == XmlSeverityType.Error)
            Console.WriteLine(e.Message);
    }
}