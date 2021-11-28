using System.Data.SqlClient;
using EventBus.Base.Standard;
using Newtonsoft.Json;

namespace RabbitMQ.Examples.Receiver;

public class TariffInsertEventHandler : IIntegrationEventHandler<TariffInsertEvent>
{
    public async Task Handle(TariffInsertEvent @event)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = "localhost",
            UserID = "sa",
            Password = "mySuperSecret!p@ssword",
            InitialCatalog = "labDB"
        };
        await using var connection = new SqlConnection(builder.ConnectionString);
        await using var command = new SqlCommand
        {
            Connection = connection,
            CommandType = System.Data.CommandType.Text,
            CommandText = @"-- noinspection SqlNoDataSourceInspection            
                            INSERT INTO Tariffs (
                                tariff_id, 
                                name, 
                                operator_name, 
                                payroll, 
                                call_prices_json, 
                                price_per_sms,
                                parameters_json)
                            VALUES (
                                @tariff_id, 
                                @name, 
                                @operator_name, 
                                @payroll, 
                                @call_prices_json, 
                                @price_per_sms,
                                @parameters_json)"
        };

        var callPricesJson = JsonConvert.SerializeObject(@event.Tariff.CallPrices);
        var parametersJson = JsonConvert.SerializeObject(@event.Tariff.Parameters);
        command.Parameters.AddWithValue("@tariff_id", @event.Tariff.Id);
        command.Parameters.AddWithValue("@name", @event.Tariff.Name);
        command.Parameters.AddWithValue("@operator_name", @event.Tariff.OperatorName);
        command.Parameters.AddWithValue("@payroll", @event.Tariff.Payroll);
        command.Parameters.AddWithValue("@call_prices_json", callPricesJson);
        command.Parameters.AddWithValue("@price_per_sms", @event.Tariff.PricePerSms);
        command.Parameters.AddWithValue("@parameters_json", parametersJson);
        connection.Open();
        command.ExecuteNonQuery();
        
    }
}