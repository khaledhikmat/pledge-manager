namespace pledgemanager.shared.Models;

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