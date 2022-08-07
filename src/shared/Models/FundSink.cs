using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace pledgemanager.shared.Models;

public class FundSink 
{
    //Cosmos requires an `id`
    public string id { 
        get {
            return Identifier;
        } 
        set {
            Identifier = value;
        } 
    }
    public string Identifier { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedTime { get; set; } = DateTime.Now;
    public DateTime? LastUpdatedTime { get; set; } = null;
    public DateTime? LastRefreshTime { get; set; } = null;
    public FundSinkTypes Type {get; set;} = FundSinkTypes.Unknown;
    public string PartitionKey {get; set;} = "Unknown";
    public string Name { get; set; } = "";
    public string Currency { get; set; } = "USD";
    public double ExchangeRate { get; set; } = 1;
    public double Fund { get; set; } = 0;
    public int ChildrenCount { get; set; } = 0;
    public int FulfilledPledgesCount { get; set; } = 0;
    public int DonorsCount { get; set; } = 0;
    public int PeriodsCount { get; set; } = 10;
    public FundSinkPeriodTypes PeriodType { get; set; } = FundSinkPeriodTypes.Minute;
    public List<FundSinkPeriod> Periods { get; set; } = new();

    public static string GetPartitionKey(FundSink sink) 
    {
        if (sink.Type == FundSinkTypes.Campain) 
        {
            return "Campaign";
        }
        else if (sink.Type == FundSinkTypes.Institution)
        {
            return "Institution";
        }
        else if (sink.Type  == FundSinkTypes.Global ||
                 sink.Type  == FundSinkTypes.Country || 
                 sink.Type  == FundSinkTypes.State ||
                 sink.Type  == FundSinkTypes.City)
        {
            return "FundSink";
        }
        else 
        {
            return "Unknown";
        }
    }
    public static string GetPartitionKeyByType(FundSinkTypes type) 
    {
        if (type == FundSinkTypes.Campain) 
        {
            return "Campaign";
        }
        else if (type == FundSinkTypes.Institution)
        {
            return "Institution";
        }
        else if (type == FundSinkTypes.Global ||
                 type == FundSinkTypes.Country || 
                 type == FundSinkTypes.State ||
                 type == FundSinkTypes.City)
        {
            return "FundSink";
        }
        else 
        {
            return "Unknown";
        }
    }
}

public enum FundSinkTypes
{
    Unknown,
    Campain,
    Institution,
    City,
    State,
    Country,
    Global
}

public enum FundSinkPeriodTypes
{
    Minute,
    Hour
}

public class FundSinkPeriod 
{
    public string Period { get; set; } = "";
    public int Count { get; set; } = 0;
    public double Amount { get; set; } = 0;
}