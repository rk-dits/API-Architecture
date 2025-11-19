using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Configure to use HTTP only for demo
builder.WebHost.UseUrls("http://localhost:5227");

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

// app.UseHttpsRedirection(); // Commented out for demo purposes

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    using var rng = RandomNumberGenerator.Create();
    var forecast = Enumerable.Range(1, 5).Select(index =>
    {
        var tempBytes = new byte[4];
        var summaryBytes = new byte[4];
        rng.GetBytes(tempBytes);
        rng.GetBytes(summaryBytes);

        var temperature = Math.Abs(BitConverter.ToInt32(tempBytes, 0)) % 75 - 20;
        var summaryIndex = Math.Abs(BitConverter.ToInt32(summaryBytes, 0)) % summaries.Length;

        return new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            temperature,
            summaries[summaryIndex]
        );
    })
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
