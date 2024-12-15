using NorionBankProgrammingTest.Interfaces;
using NorionBankProgrammingTest.Models;
using NorionBankProgrammingTest.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();
builder.Services.AddHealthChecks();

//DI
builder.Services.AddScoped<ITollFeeService, TollFeeService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");

app.MapPost("/GetTollFee", (ITollFeeService tollFeeService, string vehicleType, List<DateTime> passages) =>
{
    app.Logger.LogInformation($"Calculating toll fee for {vehicleType}");
    var tollFee = tollFeeService.GetTollFee(new Car(), passages.ToArray());
    return new { response = tollFee };
})
.WithName("GetTollFee")
.WithOpenApi();

app.Run();