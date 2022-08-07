using pledgemanager.shared.Models;
using pledgemanager.shared.Utils;
using pledgemanager.shared.Services;

try
{
    var envService = new EnvironmentService();
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

    //var daprClient = new DaprClientBuilder().Build();
    var httpClient = new HttpClient();

    if (command == "simulatedonors")
    {
        var code = arg1; // Read from the command line

        var url = $"{envService.GetBaseUrl("CAMPAIGNS")}/users/donors"; 
        Console.WriteLine($"Simulating users URL: {url}");
        var donors = await Utilities.Get<List<User>>(httpClient, url);

        Console.WriteLine($"Simulating {donors.Count} donors using {code} code....");
        foreach(User donor in donors)
        {
            try
            {
                Console.WriteLine($"Simulating {donor.UserName} ....");
                // Generate a verification request
                url = $"{envService.GetBaseUrl("CAMPAIGNS")}/users/verifications/{donor.UserName}"; 
                Console.WriteLine($"Simulating users URL: {url}");
                await Utilities.PostNoRequestNoResponse(httpClient, url);

                // Respond with code (for now temp)
                url = $"{envService.GetBaseUrl("CAMPAIGNS")}/users/verifications/{donor.UserName}/{code}"; 
                Console.WriteLine($"Simulating users verification URL: {url}");
                await Utilities.PostNoRequestNoResponse(httpClient, url);
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
        var url = $"{envService.GetBaseUrl("CAMPAIGNS")}/users/donors"; 
        Console.WriteLine($"Simulating users URL: {url}");
        var donors = await Utilities.Get<List<User>>(httpClient, url);

        url = $"{envService.GetBaseUrl("CAMPAIGNS")}/entities/campaigns"; 
        Console.WriteLine($"Simulating campaigns URL: {url}");
        var campaigns = await Utilities.Get<List<Campaign>>(httpClient, url);

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

                url = $"{envService.GetBaseUrl("CAMPAIGNS")}/entities/campaigns/{campaign.Identifier}/pledges"; 
                Console.WriteLine($"Simulating pledges URL: {url}");

                var pledge = new Pledge();
                pledge.CampaignIdentifier = campaign.Identifier;
                pledge.Amount = Random.Shared.Next(100, 5000);
                pledge.Currency = "USD";
                pledge.UserName = donors[Random.Shared.Next(donors.Count)].UserName;
                await Utilities.PostNoResponse<Pledge>(httpClient, url, pledge);    
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
} 
catch (Exception e)
{
    Console.WriteLine($"****** Exception {e.Message} - {e.InnerException}");
} 

