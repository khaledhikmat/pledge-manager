using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace pledgemanager.shared.Models;

public class User 
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
    public UserTypes Type {get; set;} = UserTypes.Donor;
    public UserVerificationMethods VerificationMethod {get; set;} = UserVerificationMethods.Sms;
    public DateTime? SignupTime { get; set; } = null;
    public DateTime? LastVerificationTime { get; set; } = null;
    public string UserName { get; set; } = "";
    public string Name { get; set; } = "";
    public string NickName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public int TotalPledgesCount { get; set; } = 0;
    public double TotalContribution { get; set; } = 0;
    public int LastItemsCount { get; set; } = 50;
 
   // More recent Items (based on LastItemsCount)
    public List<Pledge> Pledges { get; set; } = new List<Pledge>();
 
    public UserPermission Permission {get; set;} = new UserPermission();

    // More recent Items (based on LastItemsCount)
    public List<UserVerificationTransaction> Verifications { get; set; } = new List<UserVerificationTransaction>();
}

public enum UserTypes
{
    Donor,
    Organizer,
    Admin
}

public class UserVerificationTransaction 
{
    public string? Identifier { get; set; } = Guid.NewGuid().ToString();
    public DateTime VerificationRequestTime { get; set; } = DateTime.Now;
    public DateTime? VerificationResponseTime { get; set; } = null;
    public DateTime? VerificationAllowedTime { get; set; } = null;
    public string? UserIdentifier { get; set; } = "";
    public UserVerificationMethods VerificationMethod {get; set;} = UserVerificationMethods.Sms;
    public string? Code { get; set; } = "";
    public bool Verified {get; set; } = false;
}

public enum UserVerificationMethods
{
    Email,
    Sms
}

public class UserPermission 
{
    public Dictionary<string, string> Institutions = new();
        
}

public class UserVerificationResponse 
{
    public UserTypes Type {get; set; } = UserTypes.Donor;
    public Dictionary<string, string> Institutions {get; set; } = new();
    public bool Verified {get; set; } = false;
}
