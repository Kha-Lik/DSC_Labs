using System;
using UdpBroadcastClient;

Console.Write("Enter the port to listen on: ");
var port = int.Parse(Console.ReadLine() ?? "5555");

var client = new Client(port);
client.Start();