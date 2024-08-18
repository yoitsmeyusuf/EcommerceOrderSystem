using MySql.Data.MySqlClient;
using OrderService.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();
channel.QueueDeclare(queue: "orderQueue",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    var order = JsonSerializer.Deserialize<Order>(message);
    Console.WriteLine($"Received Order: {order.ID}, Type: {order.ProductType}, Adress: {order.Address}");
    //Process data as you want here
    //And then you can insert the order into your database Its that easy but rightnow I am not going to do that Its all about RabbitMQ and learning the concept
    // MYSQL INSERT Order
     var connectionString = "server=localhost;user=root;password=yourpassword;database=yourdatabase";
    using var sqlConnection = new MySqlConnection(connectionString);
    sqlConnection.Open();

    var query = "INSERT INTO Orders (ID, ProductType, Address) VALUES (@ID, @ProductType, @Address)";
    using var cmd = new MySqlCommand(query, sqlConnection);
    cmd.Parameters.AddWithValue("@ID", order.ID);
    cmd.Parameters.AddWithValue("@ProductType", order.ProductType);
    cmd.Parameters.AddWithValue("@Address", order.Address);
    cmd.ExecuteNonQuery();

};

channel.BasicConsume(queue: "orderQueue",
                     autoAck: true,
                     consumer: consumer);

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

namespace OrderService.Model{
public class Order
{
    public int ID { get; set; }
    public required string ProductType { get; set; }
    public DateTime Date { get; set; }
    
    public required string Address { get; set; }
}
}