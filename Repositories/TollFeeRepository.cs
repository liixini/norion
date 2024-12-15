using Dapper;
using Microsoft.Data.SqlClient;
using NorionBankProgrammingTest.Interfaces;
using NorionBankProgrammingTest.Models;

namespace NorionBankProgrammingTest.Repositories;

public class TollFeeRepository : ITollFeeRepository
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TollFeeRepository> _logger;

    public TollFeeRepository(IConfiguration configuration, ILogger<TollFeeRepository> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    public async Task<Dictionary<string, TollFreeVehicleModel>> GetTollFreeVehicleTypes()
    {
        var sql = "SELECT * FROM tollcalculator.TollFreeVehicle";

        await using var connection = new SqlConnection(_configuration["ConnectionString"]);
        try
        {
            var tollfreeVehicles = await connection.QueryAsync<TollFreeVehicleModel>(sql);
            return tollfreeVehicles.ToDictionary(x => x.VehicleType);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "An error occurred while fetching toll free vehicles");
        }
        
        return new Dictionary<string, TollFreeVehicleModel>();
    }
}