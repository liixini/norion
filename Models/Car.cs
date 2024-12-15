using NorionBankProgrammingTest.Interfaces;

namespace NorionBankProgrammingTest.Models
{
    public class Car : IVehicle
    {
        public String GetVehicleType()
        {
            return "Car";
        }
    }
}