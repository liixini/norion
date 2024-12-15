using NorionBankProgrammingTest.Interfaces;

namespace NorionBankProgrammingTest.Models
{
    public class Motorbike : IVehicle
    {
        public string GetVehicleType()
        {
            return "Motorbike";
        }
    }
}
