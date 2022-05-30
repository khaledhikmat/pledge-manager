namespace pledgemanager.shared.Models;

public class Institution : FundSink
{
    public string Country { get; set; } = "USA";
    public string State { get; set; } = "TX";
    public string City { get; set; } = "SAT";
    public string Title { get; set; } = "Some Institution";
    public string Description { get; set; } = "Some Institution desc";
    public string ImageUrl { get; set; } = "";
}