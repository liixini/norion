using NorionBankProgrammingTest.DTOs;
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

app.MapPost("/GetTollFee", async (ITollFeeService tollFeeService, PassagesModel passages) =>
{
    app.Logger.LogDebug($"Calculating toll fee for {passages.VehicleType}");
    var amount = await tollFeeService.CalculateTollFee(passages);
    app.Logger.LogDebug($"Calculated toll fee {amount} for {passages.VehicleType}");
    return new TollFeeDTO
    {
        Fee = amount
    };
})
.WithName("GetTollFee")
.WithOpenApi();

app.Run();