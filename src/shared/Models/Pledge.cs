using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace pledgemanager.shared.Models;

public class Pledge 
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
    public DateTime PledgeTime { get; set; } = DateTime.Now;
    public DateTime? ApprovedTime { get; set; } = null;
    public DateTime? RejectedTime { get; set; } = null;
    public DateTime? FulfilledTime { get; set; } = null;
    public string CampaignIdentifier { get; set; } = "";
    public string Campaign { get; set; } = "";
    public bool IsAnonymous { get; set; } = false;
    public bool IsMatch { get; set; } = false;
    public bool IsDeferred { get; set; } = false;
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

    public bool CanShowApproval() 
    {
        return this.ApprovedTime == null && this.RejectedTime == null && this.FulfilledTime == null;
    }

    public bool CanShowRejection() 
    {
        return this.CanShowApproval();
    }

    public bool CanShowEmphasize() 
    {
        //TODO: 
        return true;
    }
}
