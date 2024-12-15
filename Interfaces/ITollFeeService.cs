using NorionBankProgrammingTest.Models;

namespace NorionBankProgrammingTest.Interfaces;

public interface ITollFeeService
{
    Task<int> CalculateTollFee(PassagesModel passages);
}