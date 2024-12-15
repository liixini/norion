using NorionBankProgrammingTest.Interfaces;
using NorionBankProgrammingTest.Models;
using NorionBankProgrammingTest.Repositories;
using NorionBankProgrammingTest.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();
builder.Services.AddHealthChecks();
builder.Configuration.AddUserSecrets<Program>();

//DI
builder.Services.AddScoped<ITollFeeService, TollFeeService>();
builder.Services.AddScoped<ITollFeeRepository, TollFeeRepository>();

var app = builder.Build();
app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/GetTollFee", (ITollFeeService tollFeeService, string vehicleType, List<DateTime> passages) =>
{
    app.Logger.LogInformation($"Calculating toll fee for {vehicleType}");
    var tollFee = tollFeeService.GetTollFee(vehicleType, passages.ToArray());
    return new { response = tollFee };
})
.WithName("GetTollFee")
.WithOpenApi();

app.Run();