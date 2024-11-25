using System.Globalization;
using Ui.Pages;

namespace Ui.Helpers;

public static class Helpers
{
    // Extension method to group data based on the selected range
    public static IEnumerable<Home.TrafficData> GroupByDataByRange(this IEnumerable<Home.TrafficData> data, string rangeType)
    {
        switch (rangeType)
        {
            case "minutes":
                return data.GroupBy(d => new { d.Time.Year, d.Time.Month, d.Time.Day, d.Time.Hour, d.Time.Minute })
                    .Select(g => new Home.TrafficData
                    {
                        Time = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, g.Key.Minute, 0),
                        SuccessfulRequests = g.Sum(d => d.SuccessfulRequests),
                        FailedRequests = g.Sum(d => d.FailedRequests),
                        ErrorRate = g.Sum(d => d.FailedRequests),
                        HostLoads = SummarizeHostLoads(g.Select(d => d.HostLoads)),
                        ResponseTime = g.Sum(d => d.ResponseTime)
                    });
            case "hours":
                return data.GroupBy(d => new { d.Time.Year, d.Time.Month, d.Time.Day, d.Time.Hour })
                    .Select(g => new Home.TrafficData
                    {
                        Time = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0),
                        SuccessfulRequests = g.Sum(d => d.SuccessfulRequests),
                        FailedRequests = g.Sum(d => d.FailedRequests),
                        ErrorRate = g.Sum(d => d.FailedRequests),
                        HostLoads = SummarizeHostLoads(g.Select(d => d.HostLoads)),
                        ResponseTime = g.Sum(d => d.ResponseTime)
                    });
            case "days":
                return data.GroupBy(d => d.Time.Date)
                    .Select(g => new Home.TrafficData
                    {
                        Time = g.Key,
                        SuccessfulRequests = g.Sum(d => d.SuccessfulRequests),
                        FailedRequests = g.Sum(d => d.FailedRequests),
                        ErrorRate = g.Sum(d => d.FailedRequests),
                        HostLoads = SummarizeHostLoads(g.Select(d => d.HostLoads)),
                        ResponseTime = g.Sum(d => d.ResponseTime)
                    });
            case "weeks":
                return data.GroupBy(d => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(d.Time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday))
                    .Select(g => new Home.TrafficData
                    {
                        Time = g.First().Time,
                        SuccessfulRequests = g.Sum(d => d.SuccessfulRequests),
                        FailedRequests = g.Sum(d => d.FailedRequests),
                        ErrorRate = g.Sum(d => d.FailedRequests),
                        HostLoads = SummarizeHostLoads(g.Select(d => d.HostLoads)),
                        ResponseTime = g.Sum(d => d.ResponseTime)
                    });
            case "months":
                return data.GroupBy(d => new { d.Time.Year, d.Time.Month })
                    .Select(g => new Home.TrafficData
                    {
                        Time = new DateTime(g.Key.Year, g.Key.Month, 1),
                        SuccessfulRequests = g.Sum(d => d.SuccessfulRequests),
                        FailedRequests = g.Sum(d => d.FailedRequests),
                        ErrorRate = g.Sum(d => d.FailedRequests),
                        HostLoads = SummarizeHostLoads(g.Select(d => d.HostLoads)),
                        ResponseTime = g.Sum(d => d.ResponseTime)
                    });
        }

        return data;
    }
    
    private static Dictionary<string, int> SummarizeHostLoads(IEnumerable<Dictionary<string, int>> hostLoadDictionaries)
    {
        var summarizedLoads = new Dictionary<string, int>();

        foreach (var hostLoads in hostLoadDictionaries)
        {
            foreach (var kvp in hostLoads)
            {
                if (summarizedLoads.ContainsKey(kvp.Key))
                {
                    summarizedLoads[kvp.Key] += kvp.Value; // Sum the load for each host
                }
                else
                {
                    summarizedLoads[kvp.Key] = kvp.Value; // Initialize the host load
                }
            }
        }

        return summarizedLoads;
    }
}