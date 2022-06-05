using Dapr.Client;
using pledgemanager.shared.Models;

var daprClient = new DaprClientBuilder().Build();

int pledges = 100;
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
        Goal = 10000
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
        Goal = 20000
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
        Goal = 20000
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
        Goal = 20000
    }
};

List<string> users = new List<string> {
    "Abou Ya3goob",
    "Abou Mazen",
    "Sarraj Hasan",
    "Ahmad Dakkaq",
    "Soliman Mohammad",
    "Sameer Fattal",
    "Jamal Kurk",
    "Sameh Jabal",
    "Mo3een Lotfi",
    "Adam Aboudan",
    "Ryadh Idlbi",
    "Ameer Barakat",
    "Jumaa Rahhal",
    "Shoaib Kabeer",
    "Ruth McCormick",
    "Abou Jandal",
    "John McPherson",
    "Brandon Perkins"
};

string createCommand = Environment.GetCommandLineArgs()[1];
if (createCommand == "create")
{
    Console.WriteLine("Creating institutions....");
    foreach (Institution institution in institutions)
    {
        await daprClient.InvokeMethodAsync<Institution>("pledgemanagerapi", $"entities/institutions", institution);    
    }

    Console.WriteLine("Creating campaigns....");
    foreach (Campaign campaign in campaigns)
    {
        await daprClient.InvokeMethodAsync<Campaign>("pledgemanagerapi", $"entities/campaigns", campaign);    
    }
}

string simulateCommand = Environment.GetCommandLineArgs()[2];
if (simulateCommand == "simulate")
{
    Console.WriteLine($"Simulating {pledges} pledges....");
    string simulatedCampaignId = "";
    if (Environment.GetCommandLineArgs().Length > 3)
    {
        simulatedCampaignId = Environment.GetCommandLineArgs()[3];
    }

    while (pledges > 0)
    {
        Campaign campaign = new Campaign();

        if (string.IsNullOrEmpty(simulateCommand))
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
            pledge.Amount = Random.Shared.Next(50, 5000);
            pledge.Currency = "USD";
            pledge.UserName = users[Random.Shared.Next(users.Count)];
            await daprClient.InvokeMethodAsync<Pledge>("pledgemanagerapi", $"entities/campaigns/{campaign.Identifier}/pledges", pledge);    
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