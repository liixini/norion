using NorionBankProgrammingTest.Interfaces;
using NorionBankProgrammingTest.Models;

namespace NorionBankProgrammingTest.Services;

public class TollFeeService : ITollFeeService
{
    private readonly ITollFeeRepository _tollFeeRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TollFeeService> _logger;
    private readonly int _freePassingLengthInMinutes;
    private readonly int _maxDailyFee;

    public TollFeeService(ITollFeeRepository tollFeeRepository, IConfiguration configuration, ILogger<TollFeeService> logger)
    {
        _tollFeeRepository = tollFeeRepository;
        _configuration = configuration;
        _logger = logger;
        _freePassingLengthInMinutes = Convert.ToInt32(_configuration["FreePassingDuration"]);
        _maxDailyFee = Convert.ToInt32(_configuration["MaxDailyFee"]);
    }
    
    public async Task<int> GetTollFee(PassagesModel passages)
    {
        //We could collect all data at once for faster processing, but we save 2 data calls if the vehicle is toll free
        var tollFreeVehicleTypes = await _tollFeeRepository.GetTollFreeVehicleTypes();
        
        if(IsTollFreeVehicle(passages.VehicleType, tollFreeVehicleTypes))
        {
            _logger.LogInformation("Vehicle {passages.VehicleType} is toll free", passages.VehicleType);
            return 0;
        }
        
        //sort all passages chronologically for easier logic later down the line
        passages.Passages.Sort();

        //Get data used for calculations
        var getFreeDatesTask = _tollFeeRepository.GetTollFreeDates();
        var tollFeesTask = _tollFeeRepository.GetTollFees();
        await Task.WhenAll(getFreeDatesTask, tollFeesTask);
        var tollFees = tollFeesTask.Result;
        var freeDates = getFreeDatesTask.Result;
        
        //filter out irrelevant passages
        passages.Passages = RemoveFreeDates(passages.Passages, freeDates);
        var filteredPassages = RemoveFreePassages(passages, tollFees, _freePassingLengthInMinutes);
        
        return CalculateTollFee(filteredPassages, _maxDailyFee);
    }
    
    internal int CalculateTollFee(List<FreePassageModel> filteredPassages, int maxDailyFee)
    {
        var totalFee = 0;

        var passagesByDay = filteredPassages
            .GroupBy(date => new { date.PassageTime.Year, date.PassageTime.DayOfYear })
            .Select(group => group.ToList())
            .ToList();

        foreach (var passageList in passagesByDay)
        {
            var dailyFee = 0;
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
    /// <param name="freePassingMinutes">The duration in minutes of how long we should consolidate free passings from the first passing</param>
    /// <returns></returns>
    internal List<FreePassageModel> RemoveFreePassages(PassagesModel passages, List<TollFeeModel> tollFees, int freePassingMinutes)
    {
        var freePassages = new List<FreePassageModel>();

        //For loop to be able to modify the index as we jump forward as we remove free passages
        for (int i = 0; i < passages.Passages.Count; i++)
        {
            var maxTollAmount = 0;
            var freePassageDate = passages.Passages[i].AddMinutes(freePassingMinutes);
            
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

    internal int GetTollFeePerPassing(DateTime date, List<TollFeeModel> tollFees)
    {

        var tollFee = tollFees.FirstOrDefault(x =>
            IsDateBetweenWithMinutePrecision(
                GetDateWithHourAndMinutePrecision(date),
                GetDateWithHourAndMinutePrecision(x.StartDate),
                GetDateWithHourAndMinutePrecision(x.StopDate)));

        if (tollFee == null)
        {
            _logger.LogCritical("No fee found for {date}", date);
            return 0;
        }

        return tollFee.Fee;
    }
    
    internal bool IsTollFreeVehicle(string vehicleType, Dictionary<String, TollFreeVehicleModel> tollFreeVehicles)
    {
        return tollFreeVehicles.ContainsKey(vehicleType) && tollFreeVehicles[vehicleType].Active;
    }

    internal List<DateTime> RemoveFreeDates(List<DateTime> dates, List<TollFreeDateModel> tollFreeDates)
    {
        //Remove all dates that are toll free
        var tollFreeDatesToRemove = tollFreeDates
            .SelectMany(tollFreeDate => dates
            .Where(date => IsDateBetweenWithMinutePrecision(date, tollFreeDate.StartDate, tollFreeDate.StopDate)))
            .ToList();

        return dates.Where(x => !tollFreeDatesToRemove.Contains(x)).ToList();
    }
    
    //These probably belong to some sort of Date helper class, but for simplicity they're included here
    internal static bool IsDateBetweenWithMinutePrecision(DateTime date, DateTime startDate, DateTime stopDate)
    {
        var minutePrecisionDate = GetDateWithMinutePrecision(date);
        var minutePrecisionStartDate = GetDateWithMinutePrecision(startDate);
        var minutePrecisionStopDate = GetDateWithMinutePrecision(stopDate);

        return minutePrecisionDate >= minutePrecisionStartDate && minutePrecisionDate <= minutePrecisionStopDate;
    }
    
    internal static DateTime GetDateWithMinutePrecision(DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
    }

    internal static DateTime GetDateWithHourAndMinutePrecision(DateTime date)
    {
        return new DateTime(1, 1, 1, date.Hour, date.Minute, 0);
    }
}