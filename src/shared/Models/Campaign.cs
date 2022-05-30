namespace pledgemanager.shared.Models;

public class Campaign : FundSink
{
    public string Institution { get; set; } = "";
    public string InstitutionIdentifier { get; set; } = "";
    public string Title { get; set; } = "No-name Title";
    public string Description { get; set; } = "No-name Desc";
    public string ImageUrl { get; set; } = "No-name Image-URL";
    public bool IsFeatured { get; set; } = false;
    public DateTime? Start { get; set; } = null;
    public DateTime? Stop { get; set; } = null;
    public bool IsActive { get; set; } = false;
    public int LastItemsCount { get; set; } = 10;
    public double Goal {get; set; } = 10000;

    // More recent Items (based on LastItemsCount)
    public List<Pledge> Pledges { get; set; } = new List<Pledge>();
    public List<Donor> Donors { get; set; } = new List<Donor>();
    public List<CampaignCommand> Commands { get; set; } = new List<CampaignCommand>();
    public List<Campaign> Updates { get; set; } = new List<Campaign>();
}