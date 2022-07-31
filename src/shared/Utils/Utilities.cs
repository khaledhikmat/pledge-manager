using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace pledgemanager.shared.Utils;

public class Utilities
{
    public static string FormatAsUSD(object value)
    {
        return ((double)value).ToString("C0", CultureInfo.CreateSpecificCulture("en-US"));
    }

    public static async Task<TResponse> Get<TResponse>(HttpClient httpClient, string url)
    {
        TResponse responseObject = default(TResponse);

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new UriBuilder(url).Uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                responseObject = JsonConvert.DeserializeObject<TResponse>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                throw new Exception("Bad return code: " + response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }

        return responseObject;
    }

    public static async Task<TResponse> Post<TRequest, TResponse>(HttpClient httpClient, string url, TRequest requestObject)
    {
        TResponse responseObject = default(TResponse);

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new UriBuilder(url).Uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(JsonConvert.SerializeObject(requestObject), Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                responseObject = JsonConvert.DeserializeObject<TResponse>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                throw new Exception("Bad return code: " + response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }

        return responseObject;
    }

    public static async Task PostNoResponse<TRequest>(HttpClient httpClient, string url, TRequest requestObject)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new UriBuilder(url).Uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(JsonConvert.SerializeObject(requestObject), Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Bad return code: " + response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public static async Task PostNoRequestNoResponse(HttpClient httpClient, string url)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new UriBuilder(url).Uri);
            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Bad return code: " + response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}