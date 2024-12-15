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

    public async Task<List<TollFreeDateModel>> GetTollFreeDates()
    {
        //Would be an idea perhaps to filter on only dates that matches the dates we're looking for?
        var sql = "SELECT * FROM tollcalculator.TollFreeDates WHERE Active = 1";

        await using var connection = new SqlConnection(_configuration["ConnectionString"]);
        try
        {
             var tollFreeDates = await connection.QueryAsync<TollFreeDateModel>(sql);
             return tollFreeDates.ToList();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "An error occurred while fetching toll free dates");
        }

        return new List<TollFreeDateModel>();
    }
    
    public async Task<List<TollFeeModel>> GetTollFees()
    {
        var sql = "SELECT * FROM tollcalculator.TollFees";

        await using var connection = new SqlConnection(_configuration["ConnectionString"]);
        try
        {
            var tollFees = await connection.QueryAsync<TollFeeModel>(sql);
            return tollFees.ToList();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "An error occurred while fetching toll fees");
        }

        return new List<TollFeeModel>();
    }
}