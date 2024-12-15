using NorionBankProgrammingTest.Interfaces;
using NorionBankProgrammingTest.Models;

namespace NorionBankProgrammingTest.Services;

public class TollFeeService : ITollFeeService
{
    private readonly ITollFeeRepository _tollFeeRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TollFeeService> _logger;

    public TollFeeService(ITollFeeRepository tollFeeRepository, IConfiguration configuration, ILogger<TollFeeService> logger)
    {
        _tollFeeRepository = tollFeeRepository;
        _configuration = configuration;
        _logger = logger;
    }
    
    public async Task<int> CalculateTollFee(PassagesModel passages)
    {
        if (await IsTollFreeVehicle(passages.VehicleType))
        {
            return 0;
        }
        
        passages.Passages.Sort();

        //Get data used for calculations
        var removeFreeDatesTask = RemoveFreeDates(passages.Passages);
        var tollFeesTask = GetTollFees();
        await Task.WhenAll(removeFreeDatesTask, tollFeesTask);
        passages.Passages = removeFreeDatesTask.Result;
        var tollFees = tollFeesTask.Result;
        var passagesWithoutFreePassages = RemoveFreePassages(passages, tollFees);
        
        var totalFee = 0;
        var dailyFee = 0;

        //Not sure if the daily fee should be read from DB or not
        //So decided to actually use the config file for something
        var maxDailyFee = Convert.ToInt32(_configuration["MaxDailyFee"]);

        var passagesByDay = passagesWithoutFreePassages
            .GroupBy(date => new { date.PassageTime.Year, date.PassageTime.DayOfYear })
            .Select(group => group.ToList())
            .ToList();

        foreach (var passageList in passagesByDay)
        {
            dailyFee = 0;
            foreach (var passage in passageList)
            {
                var tollFee = passage.TollFee;
                if (tollFee != 0)
                {
                    dailyFee += tollFee;
                    if (dailyFee > maxDailyFee)
                    {
                        dailyFee = maxDailyFee;
                        break;
                    }
                }
            }
            
            totalFee += dailyFee;
        }
        
        return totalFee;
    }
    
    /// <summary>
    /// Removes free passages from the passage list
    /// and applies the business logic that the highest toll within the free passage duration
    /// should be used for that one passage that isn't free
    /// </summary>
    /// <param name="passages">The list of passages</param>
    /// <param name="tollFees">Toll fees to prevent having to look them up from DB</param>
    /// <returns></returns>
    private List<FreePassageModel> RemoveFreePassages(PassagesModel passages, List<TollFeeModel> tollFees)
    {
        //Get the duration of the free passage
        var FreePassingLength = Convert.ToInt32(_configuration["FreePassingDuration"]);
        var freePassages = new List<FreePassageModel>();

        //For loop to be able to modify the index as we jump forward as we remove free passages
        for (int i = 0; i < passages.Passages.Count; i++)
        {
            var maxTollAmount = 0;
            var freePassageDate = passages.Passages[i].AddMinutes(FreePassingLength);
            
            //Gets the passages that are within the free passage duration including the original passage
            //List is chronologically sorted so we can always assume the next passages relate to the current one
            var passingsWithinFreePassageDate = passages.Passages
                .Where(date => date >= passages.Passages[i] && date <= freePassageDate)
                .ToList();

            //Business rule - use the highest toll fee within the free passage duration
            foreach (var freePassage in passingsWithinFreePassageDate)
            {
                var tollFee = GetTollFeePerPassing(freePassage, tollFees);
                if (tollFee > maxTollAmount)
                {
                    maxTollAmount = tollFee;
                }
            }
            
            freePassages.Add(new FreePassageModel
            {
                PassageTime = passages.Passages[i],
                TollFee = maxTollAmount
            });
            
            if (passingsWithinFreePassageDate.Count == 0)
            {
                break;
            }
            
            //Skip the next dates as they've already been counted as part of the free passage
            //Minus one as we include the current iteration's date
            i += passingsWithinFreePassageDate.Count - 1;
        }

        return freePassages;
    }

    private int GetTollFeePerPassing(DateTime date, List<TollFeeModel> tollFees)
    {

        var tollFee = tollFees.FirstOrDefault(x =>
            IsDateBetweenWithMinutePrecision(
                GetDateWithHourAndMinutePrecision(date),
                GetDateWithHourAndMinutePrecision(x.StartDate),
                GetDateWithHourAndMinutePrecision(x.StopDate)));

        if (tollFee == null)
        {
            _logger.LogWarning("No fee found for {date}", date);
            return 0;
        }

        return tollFee.Fee;
    }

    private async Task<List<TollFeeModel>> GetTollFees()
    {
        return await _tollFeeRepository.GetTollFees();
    }
    
     private async Task<bool> IsTollFreeVehicle(string vehicleType)
    {
        var tollFreeVehicles = await _tollFeeRepository.GetTollFreeVehicleTypes();
        return tollFreeVehicles.ContainsKey(vehicleType) && tollFreeVehicles[vehicleType].Active;
    }

    private async Task<List<DateTime>> RemoveFreeDates(List<DateTime> dates)
    {
        var tollFreeDates = await _tollFeeRepository.GetTollFreeDates();
        
        //Remove all dates that are toll free
        var tollFreeDatesToRemove = tollFreeDates
            .SelectMany(tollFreeDate => dates
            .Where(date => IsDateBetweenWithMinutePrecision(date, tollFreeDate.StartDate, tollFreeDate.StopDate)))
            .ToList();

        return dates.Where(x => !tollFreeDatesToRemove.Contains(x)).ToList();
    }
    
    private static bool IsDateBetweenWithMinutePrecision(DateTime date, DateTime startDate, DateTime stopDate)
    {
        var minutePrecisionDate = GetDateWithMinutePrecision(date);
        var minutePrecisionStartDate = GetDateWithMinutePrecision(startDate);
        var minutePrecisionStopDate = GetDateWithMinutePrecision(stopDate);

        return minutePrecisionDate >= minutePrecisionStartDate && minutePrecisionDate <= minutePrecisionStopDate;
    }
    
    private static DateTime GetDateWithMinutePrecision(DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
    }

    private static DateTime GetDateWithHourAndMinutePrecision(DateTime date)
    {
        return new DateTime(1, 1, 1, date.Hour, date.Minute, 0);
    }
}