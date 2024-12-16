using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NorionBankProgrammingTest.Interfaces;
using NorionBankProgrammingTest.Models;
using NorionBankProgrammingTest.Services;

namespace NorionBankProgrammingTest.Tests;

public class TollFeeServiceTests
{
    [Fact]
    void GetTollFeePerPassing_True_FeeEquals10()
    {
        var mockTollFeeRepository = new Mock<ITollFeeRepository>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockLogger = new Mock<ILogger<TollFeeService>>();
        var tollFreeService = new TollFeeService(mockTollFeeRepository.Object, mockConfiguration.Object, mockLogger.Object);
        List<TollFeeModel> tollFeeModelList =
        [
            new TollFeeModel
            {
                Fee = 10,
                StartDate = DateTime.Now.AddMinutes(-10),
                StopDate = DateTime.Now.AddMinutes(10),
                Active = true
            }
        ];
        
        var fee = tollFreeService.GetTollFeePerPassing(DateTime.Now, tollFeeModelList);
        
        Assert.Equal(10, fee);
    }
    
    [Fact]
    void GetTollFeePerPassing_True_NoFeesFoundEquals0()
    {
        var mockTollFeeRepository = new Mock<ITollFeeRepository>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockLogger = new Mock<ILogger<TollFeeService>>();
        var tollFreeService = new TollFeeService(mockTollFeeRepository.Object, mockConfiguration.Object, mockLogger.Object);
        List<TollFeeModel> tollFeeModelList = [];
        
        var fee = tollFreeService.GetTollFeePerPassing(DateTime.Now, tollFeeModelList);
        
        Assert.Equal(0, fee);
    }

    [Fact]
    void GetDateWithMinutePrecision_True_SecondIsZero()
    {
        var secondPrecisionDate = DateTime.Now;
        var minutePrecisionDate = TollFeeService.GetDateWithMinutePrecision(secondPrecisionDate);
        
        Assert.Equal(0, minutePrecisionDate.Second);
    }
    
    [Fact]
    void GetDateWithHourAndMinutePrecision_True_DatesSetToInitialValues()
    {
        var secondPrecisionDate = DateTime.Now;
        var minuteAndHourPrecisionDate = TollFeeService.GetDateWithHourAndMinutePrecision(secondPrecisionDate);
        
        Assert.Equal(0, minuteAndHourPrecisionDate.Second);
        Assert.Equal(1, minuteAndHourPrecisionDate.Month);
        Assert.Equal(1, minuteAndHourPrecisionDate.Year);
        Assert.Equal(1, minuteAndHourPrecisionDate.DayOfYear);
    }
    
    [Fact]
    void IsDateBetweenWithMinutePrecision_True_DateIsBetween()
    {
        var date = DateTime.Now;
        var startDate = DateTime.Now.AddMinutes(-10);
        var stopDate = DateTime.Now.AddMinutes(10);
        
        var isBetween = TollFeeService.IsDateBetweenWithMinutePrecision(date, startDate, stopDate);
        
        Assert.True(isBetween);
    }
    
    [Fact]
    void IsDateBetweenWithMinutePrecision_True_DateIsNotBetween()
    {
        var date = DateTime.Now;
        var startDate = DateTime.Now.AddMinutes(10);
        var stopDate = DateTime.Now.AddMinutes(20);
        
        var isBetween = TollFeeService.IsDateBetweenWithMinutePrecision(date, startDate, stopDate);
        
        Assert.False(isBetween);
    }

    [Fact]
    void RemoveFreePassages_True_TwoFreePassagesRemoved()
    {
        var mockTollFeeRepository = new Mock<ITollFeeRepository>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockLogger = new Mock<ILogger<TollFeeService>>();
        var tollFreeService = new TollFeeService(mockTollFeeRepository.Object, mockConfiguration.Object, mockLogger.Object);
        var passage = new PassagesModel
        {
            Passages = new List<DateTime>
            {
                DateTime.Now,
                DateTime.Now.AddMinutes(10),
                DateTime.Now.AddMinutes(20)
            },
            VehicleType = "Car"
        };
        
        var tollFees = new List<TollFeeModel>
        {
            new TollFeeModel
            {
                StartDate = DateTime.Now.AddMinutes(-5),
                StopDate = DateTime.Now.AddMinutes(25),
                Fee = 10
            }
        };
        
        List<DateTime> dates = new List<DateTime>
        {
            DateTime.Now,
            DateTime.Now.AddMinutes(10),
            DateTime.Now.AddMinutes(20)
        };

        var result = tollFreeService.RemoveFreePassages(passage, tollFees, 60);
        
        Assert.Single(result);
    }
    
    [Fact]
    void RemoveFreePassages_True_OneFreePassageRemoved()
    {
        int freePassingMinutes = 60;
        var mockTollFeeRepository = new Mock<ITollFeeRepository>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockLogger = new Mock<ILogger<TollFeeService>>();
        var tollFreeService = new TollFeeService(mockTollFeeRepository.Object, mockConfiguration.Object, mockLogger.Object);
        var passage = new PassagesModel
        {
            Passages = new List<DateTime>
            {
                DateTime.Now,
                DateTime.Now.AddMinutes(10),
                DateTime.Now.AddMinutes(freePassingMinutes + 1)
            },
            VehicleType = "Car"
        };
        
        var tollFees = new List<TollFeeModel>
        {
            new TollFeeModel
            {
                StartDate = DateTime.Now.AddMinutes(-5),
                StopDate = DateTime.Now.AddMinutes(freePassingMinutes + 1),
                Fee = 10
            }
        };
        
        List<DateTime> dates = new List<DateTime>
        {
            DateTime.Now,
            DateTime.Now.AddMinutes(10),
            DateTime.Now.AddMinutes(20)
        };

        var result = tollFreeService.RemoveFreePassages(passage, tollFees, freePassingMinutes);
        
        Assert.Equal(2, result.Count);
    }
    
    [Fact]
    void RemoveFreePassages_True_BreakOnZeroMatches()
    {
        int freePassingMinutes = 60;
        var mockTollFeeRepository = new Mock<ITollFeeRepository>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockLogger = new Mock<ILogger<TollFeeService>>();
        var tollFreeService = new TollFeeService(mockTollFeeRepository.Object, mockConfiguration.Object, mockLogger.Object);
        var passage = new PassagesModel
        {
            Passages = new List<DateTime>(),
            VehicleType = "Car"
        };
        
        var tollFees = new List<TollFeeModel>
        {
            new TollFeeModel
            {
                StartDate = DateTime.Now.AddMinutes(-5),
                StopDate = DateTime.Now.AddMinutes(freePassingMinutes + 1),
                Fee = 10
            }
        };
        
        List<DateTime> dates = new List<DateTime>
        {
            DateTime.Now,
            DateTime.Now.AddMinutes(10),
            DateTime.Now.AddMinutes(20)
        };

        var result = tollFreeService.RemoveFreePassages(passage, tollFees, freePassingMinutes);
        
        Assert.Empty(result);
    }
    
    [Fact]
    void IsTollFreeVehicle_True_VehicleIsTollFree()
    {
        var mockTollFeeRepository = new Mock<ITollFeeRepository>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockLogger = new Mock<ILogger<TollFeeService>>();
        var tollFreeService = new TollFeeService(mockTollFeeRepository.Object, mockConfiguration.Object, mockLogger.Object);
        var tollFreeVehicles = new Dictionary<string, TollFreeVehicleModel>
        {
            {
                "Car", 
                new TollFreeVehicleModel
                {
                    VehicleType = "Car",
                    Active = true
                }
                
            }
        };
        
        var isTollFree = tollFreeService.IsTollFreeVehicle("Car", tollFreeVehicles);
        
        Assert.True(isTollFree);
    }
    
    [Fact]
    void IsTollFreeVehicle_False_VehicleIsNotActive()
    {
        var mockTollFeeRepository = new Mock<ITollFeeRepository>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockLogger = new Mock<ILogger<TollFeeService>>();
        var tollFreeService = new TollFeeService(mockTollFeeRepository.Object, mockConfiguration.Object, mockLogger.Object);
        var tollFreeVehicles = new Dictionary<string, TollFreeVehicleModel>
        {
            {
                "Car", 
                new TollFreeVehicleModel
                {
                    VehicleType = "Car",
                    Active = false
                }
                
            }
        };
        
        var isTollFree = tollFreeService.IsTollFreeVehicle("Car", tollFreeVehicles);
        
        Assert.False(isTollFree);
    }
    
    [Fact]
    void CalculateTollFee_True_MaxDailyFeeReducesTotal()
    {
        int maxDailyFee = 60;
        var mockTollFeeRepository = new Mock<ITollFeeRepository>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockLogger = new Mock<ILogger<TollFeeService>>();
        var tollFreeService = new TollFeeService(mockTollFeeRepository.Object, mockConfiguration.Object, mockLogger.Object);
        
        var passages = new List<FreePassageModel>
        {
            new FreePassageModel
            {
                PassageTime = DateTime.Now,
                TollFee = 65
            }
        };

        var fee = tollFreeService.CalculateTollFee(passages, maxDailyFee);

        Assert.Equal(maxDailyFee, fee);
    }

    [Fact]
    void CalculateTollFee_True_MaxDailyFeeReducesTotalOverSeveralDays()
    {
        int maxDailyFee = 60;
        int maxTotalFeeForThreeDays = maxDailyFee * 3;
        var mockTollFeeRepository = new Mock<ITollFeeRepository>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockLogger = new Mock<ILogger<TollFeeService>>();
        var tollFreeService = new TollFeeService(mockTollFeeRepository.Object, mockConfiguration.Object, mockLogger.Object);

        var passages = new List<FreePassageModel>
        {
            new FreePassageModel
            {
                PassageTime = DateTime.Now,
                TollFee = 65
            },
            new FreePassageModel
            {
                PassageTime = DateTime.Now.AddDays(10),
                TollFee = 65
            },
            new FreePassageModel
            {
                PassageTime = DateTime.Now.AddDays(20),
                TollFee = 65
            }
        };

        var fee = tollFreeService.CalculateTollFee(passages, maxDailyFee);

        Assert.Equal(maxTotalFeeForThreeDays, fee);
    }

    [Fact]
    void RemoveFreeDates_True_FreeDateRemoved()
    {
        var mockTollFeeRepository = new Mock<ITollFeeRepository>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockLogger = new Mock<ILogger<TollFeeService>>();
        var tollFreeService = new TollFeeService(mockTollFeeRepository.Object, mockConfiguration.Object, mockLogger.Object);

        List<DateTime> dates = new List<DateTime>
        {
            DateTime.Now,
            DateTime.Now.AddMinutes(70)
        };
        
        List<TollFreeDateModel> freeDates = new List<TollFreeDateModel>
        {
            new TollFreeDateModel
            {
                StartDate = DateTime.Now,
                StopDate = DateTime.Now.AddMinutes(60),
                Active = true
            }
        };

         dates = tollFreeService.RemoveFreeDates(dates, freeDates);
         
         Assert.Single(dates);
    }
}