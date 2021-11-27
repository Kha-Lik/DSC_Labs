// See https://aka.ms/new-console-template for more information

using TariffsXml.ConsoleApp;
using TariffXml.Core;

var tariffs = XmlHelper.ProcessXml();
foreach (var tariff in tariffs)
{
    Console.WriteLine(tariff.Id);
}

Console.WriteLine("\nXML validation result:");
XmlHelper.ValidateXml();

Console.WriteLine("\nXML -> JSON:");
XmlHelper.XmlToJson();