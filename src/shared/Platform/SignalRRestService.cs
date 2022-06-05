using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace pledgemanager.shared.Platform;

public class SignalRRestService
{
    private IHttpClientFactory _httpFactory;
    private SignalRAuthService _authService;
    private ILogger<SignalRRestService> _logger;

    public SignalRRestService(IHttpClientFactory httpClientFactory, SignalRAuthService auth, ILogger<SignalRRestService> logger)
    {
        _httpFactory = httpClientFactory;
        _authService = auth;
        _logger = logger;
    }

    public async Task CallViaRest(string hubName, PayloadMessage payload) 
    {
        var url =$"{_authService.Endpoint}/api/v1/hubs/{hubName.ToLower()}";  //connection id is used to send message to specific client
        // you can use url as audience parameter when you call GenerateAccessToken method
 
        var request = new HttpRequestMessage(HttpMethod.Post, new UriBuilder(url).Uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authService.GenerateAccessToken(url));//here you can call GenerateAccessToken method by parsing url as audience
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

        var httpClient = _httpFactory.CreateClient("signalr");
        var response = await httpClient.SendAsync(request);
        if (response.StatusCode != HttpStatusCode.Accepted)
        {
            Console.WriteLine($"Sent error: {response.StatusCode}");
        }
    }
}
