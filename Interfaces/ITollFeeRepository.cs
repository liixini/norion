using NorionBankProgrammingTest.Models;

namespace NorionBankProgrammingTest.Interfaces;

public interface ITollFeeRepository
{
    Task<Dictionary<string, TollFreeVehicleModel>> GetTollFreeVehicleTypes();
    Task<List<TollFreeDateModel>> GetTollFreeDates();
    Task<List<TollFeeModel>> GetTollFees();
}