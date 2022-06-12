namespace pledgemanager.shared.Models;

public class FundSink 
{
    public string Identifier { get; set; } = Guid.NewGuid().ToString();
    public DateTime? CreatedTime { get; set; } = null;
    public DateTime? LastUpdatedTime { get; set; } = null;
    public DateTime? LastRefreshTime { get; set; } = null;
    public FundSinkTypes Type {get; set;} = FundSinkTypes.Unknown;
    public string Name { get; set; } = "";
    public string Currency { get; set; } = "USD";
    public double ExchangeRate { get; set; } = 1;
    public double Fund { get; set; } = 0;
    public int ChildrenCount { get; set; } = 0;
    public int FulfilledPledgesCount { get; set; } = 0;
    public int DonorsCount { get; set; } = 0;
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