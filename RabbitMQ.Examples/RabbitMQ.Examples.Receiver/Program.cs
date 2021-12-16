using System.Text;
using Microsoft.Data.SqlClient;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory()
{
    Endpoint = new AmqpTcpEndpoint("localhost", 5672),
    UserName = "user",
    Password = "bitnami"
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare("tariffs-queue", true, false, false, null);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, e) =>
{
    var body = e.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine(message);
    SendToDb(message);
    Console.WriteLine("Send to db!");
};

channel.BasicConsume("tariffs-queue", true, consumer);
Console.ReadLine();

void SendToDb(string candy)
{
    const string connectionString = @"Server=localhost,51433;Database=TariffDb;User Id=sa;Password=yourStrong(!)Password;Trusted_Connection=false;TrustServerCertificate=True";
    var cmdText = @$"INSERT INTO Tariff VALUES('{Guid.NewGuid()}', '{candy}');";
    using var sqlConnection = new SqlConnection(connectionString);
    var command = new SqlCommand(cmdText, sqlConnection);
    command.Connection.Open();
    command.ExecuteNonQuery();
}