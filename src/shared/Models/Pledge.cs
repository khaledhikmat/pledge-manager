namespace pledgemanager.shared.Models;

public class Pledge 
{
    public string? Identifier { get; set; } = Guid.NewGuid().ToString();
    public DateTime PledgeTime { get; set; } = DateTime.Now;
    public DateTime? ApprovedTime { get; set; } = null;
    public DateTime? RejectedTime { get; set; } = null;
    public string CampaignIdentifier { get; set; } = "";
    public bool IsAnonymous { get; set; } = false;
    public string UserName { get; set; } = "";
    public string Name { get; set; } = "";
    public string NickName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Currency { get; set; } = "USD";
    public double ExchangeRate { get; set; } = 1;
    public double Amount { get; set; } = 0;
    public double PercentageOfTotalFund { get; set; } = 0;
    public string Confirmation { get; set; } = Guid.NewGuid().ToString();
    public string Note { get; set; } = "";
    public string Error { get; set; } = "";
}
