namespace pledgemanager.shared.Models;

public class CampaignCommand
{
    public string? Identifier { get; set; } = Guid.NewGuid().ToString();
    public DateTime? CommandTime { get; set; } = DateTime.Now;
    public string CampaignIdentifier { get; set; } = "";
    public string Command { get; set; } = "";
    public string? Confirmation { get; set; } = Guid.NewGuid().ToString();
}