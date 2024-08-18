
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using OrderService.Model;
var builder = WebApplication.CreateBuilder(args);
var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();
channel.QueueDeclare(queue: "orderQueue",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    var forecast =  Enumerable.Range(1, 5).Select(index =>
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
app.MapPost("/order", (Order order) =>
{


    var orderJson = JsonSerializer.Serialize(order);
    var body = Encoding.UTF8.GetBytes(orderJson);

    channel.BasicPublish(exchange: "",
                         routingKey: "orderQueue",
                         basicProperties: null,
                         body: body);

    return Results.Ok("Order received and published to RabbitMQ");
})
.WithName("CreateOrder")
.WithOpenApi();
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
