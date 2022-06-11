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

    // Campaign Behavior
    public CampaignBehavior Behavior {get; set;} = new CampaignBehavior();

    // More recent Items (based on LastItemsCount)
    public List<Pledge> Pledges { get; set; } = new List<Pledge>();
    public List<Pledge> PendingApprovalPledges { get; set; } = new List<Pledge>();
    public List<Pledge> RejectedPledges { get; set; } = new List<Pledge>();
    public List<Pledge> ErroredPledges { get; set; } = new List<Pledge>();
    public List<Donor> Donors { get; set; } = new List<Donor>();
    public List<CampaignMatch> Matches { get; set; } = new List<CampaignMatch>();
}

public class CampaignBehavior
{
    public string Identifier { get; set; } = Guid.NewGuid().ToString();
    public string CampaignIdentifier { get; set; } = "";
    public CampaignPledgeModes PledgeMode { get; set; } = CampaignPledgeModes.AutoApproval;
    public double AutoApprovePledgeIfAmountLE { get; set; } = 500; //Only applicable in hybrid mode
    public bool AutoApprovePledgeIfAnonymous { get; set; } = false; //Only applicable in hybrid mode
    public double MinPledgeAmount { get; set; } = 100;
    public double MaxPledgeAmount { get; set; } = 5000;
    public List<double> RestrictedPledgeAmounts { get; set; } = new List<double>() {
        100,250,500,1000,2000,5000
    };
    public bool AutoDeactivateWhenGoalReached { get; set; } = true; 
    public bool MatchSupported { get; set; } = true; 
}

public enum CampaignPledgeModes
{
    AutoApproval,
    ManualApproval,
    HybridApproval
}

public class CampaignCommand
{
    public const string START = "start";
    public const string STOP = "stop";
    public const string EXPORT = "export";
    public const string REMIND = "remind";
    public const string FEATURE = "feature";
    public const string UNFEATURE = "unfeature";
    public const string ARCHIVE = "archive";
    public const string MATCH = "match";
    public const string APPROVE_PLEDGE = "approve-pledge";
    public const string REJECT_PLEDGE = "reject-pledge";

    public string Identifier { get; set; } = Guid.NewGuid().ToString();
    public DateTime? CommandTime { get; set; } = DateTime.Now;
    public string CampaignIdentifier { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Command { get; set; } = "";
    public string Arg1 { get; set; } = "";
    public string Arg2 { get; set; } = "";
    public int Arg3 { get; set; } = 0;
    public int Arg4 { get; set; } = 0;
    public double Arg5 { get; set; } = 0;
    public double Arg6 { get; set; } = 0;
    public string Confirmation { get; set; } = Guid.NewGuid().ToString();
    public string Error { get; set; } = "";
}

public class CampaignMatch
{
    public string Identifier { get; set; } = Guid.NewGuid().ToString();
    public DateTime? RequestTime { get; set; } = DateTime.Now;
    public DateTime? MatchTime { get; set; } = null;
    public string CampaignIdentifier { get; set; } = "";
    public string UserName { get; set; } = "";
    public double Amount { get; set; } = 0;
    public string Confirmation { get; set; } = Guid.NewGuid().ToString();
    public string Note { get; set; } = "";
    public string Error { get; set; } = "";
}

/*
This is required to be able to query state as documented:
https://docs.dapr.io/developing-applications/building-blocks/state-management/howto-state-query-api/
*/
public class CampaignQuery 
{
    public Dictionary<string, Object> Filter { get; set; } =  new Dictionary<string, Object>();
    public List<CampaignQuerySortEntry> Sort { get; set; } =  new List<CampaignQuerySortEntry>();
 }

public class CampaignQuerySortEntry 
{
    public string Key { get; set; } =  "";
    public string Order { get; set; } =  "";
}



