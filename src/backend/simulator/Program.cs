using Dapr.Client;
using pledgemanager.shared.Models;
using pledgemanager.shared.Utils;

if (Environment.GetCommandLineArgs().Length < 3)
{
    throw new Exception($"Program requires at least 2 args - CMD ARG1 and optional ARG2");
}

string command = Environment.GetCommandLineArgs()[1];
string arg1 = Environment.GetCommandLineArgs()[2];
string arg2 = "";
if (Environment.GetCommandLineArgs().Length > 3)
{
    arg2 = Environment.GetCommandLineArgs()[3];
}

if (string.IsNullOrEmpty(command) || string.IsNullOrEmpty(arg1))
{
    throw new Exception($"Program requires at least CMD and ARG1 be provided!");
}

var daprClient = new DaprClientBuilder().Build();

List<Institution> institutions = new List<Institution> {
    new Institution() {
        Identifier = "INST-00001", 
        Type = FundSinkTypes.Institution, 
        Name = "ICSA", 
        Title = "Islamic Center of San Antonio", 
        Description = "Islamic Center of San Antonio serves the SAT community .....", 
        ImageUrl = "https://picsum.photos/200/200",
        Currency = "USD",
        Country = "USA",
        State = "TX",
        City = "SAT"
    },
    new Institution() {
        Identifier = "INST-00002", 
        Type = FundSinkTypes.Institution, 
        Name = "MCCC", 
        Title = "Muslim Children and Civic Center", 
        Description = "Muslim Children and Civic Center serves the SAT community .....", 
        ImageUrl = "https://picsum.photos/200/200",
        Currency = "USD",
        Country = "USA",
        State = "TX",
        City = "SAT"
    }
};

List<User> users = new List<User> {
    new User() {
        Identifier = "USER-00001", 
        Type = UserTypes.Organizer, 
        VerificationMethod = UserVerificationMethods.Sms, 
        InstitutionIdentifier = "INST-00001", 
        UserName = "org1", 
        Name = "org1",
        NickName = "org1",
        Phone = "2105551212",
        Email = "org1@sat.com"
    },
    new User() {
        Identifier = "USER-00002", 
        Type = UserTypes.Organizer, 
        VerificationMethod = UserVerificationMethods.Sms, 
        InstitutionIdentifier = "INST-00002", 
        UserName = "org2", 
        Name = "org2",
        NickName = "org2",
        Phone = "2105551313",
        Email = "org2@sat.com"
    }
};

List<Campaign> campaigns = new List<Campaign> {
    new Campaign() {
        Identifier = "CAMP-00001", 
        Type = FundSinkTypes.Campain, 
        Institution = "ICSA", 
        InstitutionIdentifier = "INST-00001", 
        Name = "Masjed Expenses", 
        Title = "ICSA Masjed Expenses", 
        Description = "ICSA Masjed Expenses are very important to allow us to .....", 
        ImageUrl = "https://picsum.photos/200/200",
        Currency = "USD",
        Start = DateTime.Now.AddDays(-10),
        Stop = DateTime.Now.AddDays(10),
        IsActive = true,
        Goal = 50000,
        Behavior = new CampaignBehavior() {
            CampaignIdentifier = "CAMP-00001",
            PledgeMode = CampaignPledgeModes.AutoApproval,
            RestrictedPledgeAmounts = new List<double> {
            },
            AutoDeactivateWhenGoalReached = true,
            MatchSupported = false
        }
    },
    new Campaign() {
        Identifier = "CAMP-00002", 
        Type = FundSinkTypes.Campain, 
        Institution = "ICSA", 
        InstitutionIdentifier = "INST-00001", 
        Name = "Remodeling", 
        Title = "Bathroom Remodeling", 
        Description = "ICSA Bathroom Remodeling is a must due to the .....", 
        ImageUrl = "https://picsum.photos/200/200",
        Currency = "USD",
        Start = DateTime.Now.AddDays(-10),
        Stop = DateTime.Now.AddDays(10),
        IsActive = true,
        Goal = 20000,
        Behavior = new CampaignBehavior() {
            CampaignIdentifier = "CAMP-00002",
            PledgeMode = CampaignPledgeModes.ManualApproval,
            RestrictedPledgeAmounts = new List<double> {
                100,250,500,1000
            },
            AutoDeactivateWhenGoalReached = false,
            MatchSupported = false
        }
    },
    new Campaign() {
        Identifier = "CAMP-00003", 
        Type = FundSinkTypes.Campain, 
        Institution = "ICSA", 
        InstitutionIdentifier = "INST-00001", 
        Name = "School", 
        Title = "School Remodeling", 
        Description = "ICSA School Remodeling is a must due to the .....", 
        ImageUrl = "https://picsum.photos/200/200",
        Currency = "USD",
        Start = DateTime.Now.AddDays(-10),
        Stop = DateTime.Now.AddDays(10),
        IsActive = true,
        Goal = 30000,
        Behavior = new CampaignBehavior() {
            CampaignIdentifier = "CAMP-00003",
            PledgeMode = CampaignPledgeModes.HybridApproval,
            AutoApprovePledgeIfAmountLE = 500,
            AutoApprovePledgeIfAnonymous = true,
            AutoDeactivateWhenGoalReached = true,
            MatchSupported = true
        }
    },
    new Campaign() {
        Identifier = "CAMP-00004", 
        Type = FundSinkTypes.Campain, 
        Institution = "MCCC", 
        InstitutionIdentifier = "INST-00002", 
        Name = "Gym", 
        Title = "Gym Remodeling", 
        Description = "MCCC Gym is a must due to the .....", 
        ImageUrl = "https://picsum.photos/200/200",
        Currency = "USD",
        Start = DateTime.Now.AddDays(-10),
        Stop = DateTime.Now.AddDays(10),
        IsActive = true,
        Goal = 40000,
        Behavior = new CampaignBehavior() {
            CampaignIdentifier = "CAMP-00004",
            PledgeMode = CampaignPledgeModes.HybridApproval,
            AutoApprovePledgeIfAmountLE = 500,
            AutoApprovePledgeIfAnonymous = true,
            AutoDeactivateWhenGoalReached = true,
            MatchSupported = true
        }
    }
};

List<Donor> donors = new List<Donor> {
    new Donor() {
        UserName = "2105551200", 
        Name = "Abou Ya3goob"
    },
    new Donor() {
        UserName = "2105551201", 
        Name = "Abou Mazen"
    },
    new Donor() {
        UserName = "2105551202", 
        Name = "Sarraj Hasan"
    },
    new Donor() {
        UserName = "2105551203", 
        Name = "Ahmad Dakkaq"
    },
    new Donor() {
        UserName = "2105551204", 
        Name = "Soliman Mohammad"
    },
    new Donor() {
        UserName = "2105551205", 
        Name = "Sameer Fattal"
    },
    new Donor() {
        UserName = "2105551206", 
        Name = "Jamal Kurk"
    },
    new Donor() {
        UserName = "2105551207", 
        Name = "Sameh Jabal"
    },
    new Donor() {
        UserName = "2105551208", 
        Name = "Mo3een Lotfi"
    },
    new Donor() {
        UserName = "2105551209", 
        Name = "Adam Aboudan"
    },
    new Donor() {
        UserName = "2105551210", 
        Name = "Ryadh Idlbi"
    },
    new Donor() {
        UserName = "2105551211", 
        Name = "Ameer Barakat"
    },
    new Donor() {
        UserName = "2105551212", 
        Name = "Jumaa Rahhal"
    },
    new Donor() {
        UserName = "2105551213", 
        Name = "Shoaib Kabeer"
    },
    new Donor() {
        UserName = "2105551214", 
        Name = "Ruth McCormick"
    },
    new Donor() {
        UserName = "2105551214", 
        Name = "Abou Jandal"
    },
    new Donor() {
        UserName = "2105551215", 
        Name = "John McPherson"
    },
    new Donor() {
        UserName = "2105551216", 
        Name = "Brandon Perkins"
    }
};

if (command == "create")
{
    Console.WriteLine("Creating institutions....");
    foreach (Institution institution in institutions)
    {
        await daprClient.InvokeMethodAsync<Institution>("pledgemanager-campaigns", $"entities/institutions", institution);    
    }

    Console.WriteLine("Creating users....");
    foreach (User user in users)
    {
        await daprClient.InvokeMethodAsync<User>("pledgemanager-users", $"users", user);    
    }

    Console.WriteLine("Creating campaigns....");
    foreach (Campaign campaign in campaigns)
    {
        await daprClient.InvokeMethodAsync<Campaign>("pledgemanager-campaigns", $"entities/campaigns", campaign);    
    }
}
else if (command == "simulatedonors")
{
    var code = arg1; // Read from the command line
    Console.WriteLine($"Simulating {donors.Count} donors using {code} code....");
    foreach(Donor donor in donors)
    {
        try
        {
            Console.WriteLine($"Simulating {donor.UserName} ....");
            // Generate a verification request
            await daprClient.InvokeMethodAsync("pledgemanager-users", $"users/verifications/{donor.UserName}");    
            // Respond with code (for now temp)
            await daprClient.InvokeMethodAsync("pledgemanager-users", $"users/verifications/{donor.UserName}/{code}");    
        }
        catch (Exception e)
        {
            /* Ignore */
            Console.WriteLine($"Donor verification error: {e.Message}");
        }
    }
}
else if (command == "simulatepledges")
{
    var pledges = int.Parse(arg1);
    Console.WriteLine($"Simulating {pledges} pledges....");
    string simulatedCampaignId = arg2;

    while (pledges > 0)
    {
        Campaign campaign = new Campaign();

        if (string.IsNullOrEmpty(simulatedCampaignId))
        {
            campaign = campaigns[Random.Shared.Next(campaigns.Count)];
        }
        else 
        {
            campaign = campaigns.Where(c => c.Identifier == simulatedCampaignId).FirstOrDefault();
        }

        try 
        {
            if (campaign == null)
            {
                throw new Exception("Campaign is not valid!!");
            }

            Console.WriteLine($"Submitting a pledge # {pledges} for campaign: {campaign.Identifier}");

            var pledge = new Pledge();
            pledge.CampaignIdentifier = campaign.Identifier;
            pledge.Amount = Random.Shared.Next(100, 5000);
            pledge.Currency = "USD";
            pledge.UserName = donors[Random.Shared.Next(donors.Count)].UserName;
            await daprClient.InvokeMethodAsync<Pledge>("pledgemanager-campaigns", $"entities/campaigns/{campaign.Identifier}/pledges", pledge);    
        }
        catch (Exception e) 
        {
            Console.WriteLine($"Invocation error: {e.Message}");
        }
        finally
        {
            int sleep = Random.Shared.Next(2000, 6000);
            Console.WriteLine($"Sleeping for {sleep} msecs....");
            await Task.Delay(sleep);
            pledges--;
        }
    }
}