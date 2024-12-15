using NorionBankProgrammingTest.Reference;

namespace NorionBankProgrammingTest.Interfaces;

public interface ITollFeeService
{
    int GetTollFee(IVehicle vehicle, DateTime[] dates);
}