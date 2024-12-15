namespace NorionBankProgrammingTest.Interfaces;

public interface ITollFeeService
{
    int GetTollFee(string vehicleType, DateTime[] dates);
}