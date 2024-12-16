using NorionBankProgrammingTest.Models;

namespace NorionBankProgrammingTest.Interfaces;

public interface ITollFeeService
{
    Task<int> GetTollFee(PassagesModel passages);
}