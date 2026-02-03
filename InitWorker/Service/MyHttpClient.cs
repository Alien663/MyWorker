using InitWorker.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using System.Web;

namespace InitWorker.Service;

public class MyHttpClient : IMyHttpClient
{
    private readonly IConfiguration _config;
    private readonly IConfidentialClientApplication _app;
    private readonly string[] _scopes;
    private readonly HttpClient _http;
    private readonly ILogger<MyHttpClient> _logger;

    public MyHttpClient(IConfiguration config, HttpClient http, ILogger<MyHttpClient> logger)
    {
        // Injection of dependencies
        _config = config;
        _http = http;
        _logger = logger;

        // Initialize confidential client application
        _scopes = _config["MyApi:Scopes"]?.Split(';') ?? Array.Empty<string>();
        _app = ConfidentialClientApplicationBuilder
            .Create(_config["AzureAd:ClientId"])
            .WithClientSecret(_config["AzureAd:ClientSecret"])
            .WithAuthority(_config["AzureAd:Authority"])
            .Build();
    }

    private async Task<AuthenticationHeaderValue> GetAuthenticationHeaderAsync()
    {
        AuthenticationResult result = await _app
            .AcquireTokenForClient(_scopes)
            .ExecuteAsync();
        return new AuthenticationHeaderValue("Bearer", result.AccessToken);
    }

    public async Task GetSample(Dictionary<string, string> queryString)
    {
        string qs = "?" + string.Join("&", queryString.Select(
            kvp => $"{HttpUtility.UrlEncode(kvp.Key)}={HttpUtility.UrlEncode(kvp.Value)}"));
        string relativeURL = "sample" + qs;

        using(var request = new HttpRequestMessage(HttpMethod.Get, relativeURL))
        {
            request.Headers.Authorization = await GetAuthenticationHeaderAsync();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            using (var response = await _http.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Response: {responseBody}", responseBody);
            }
        }
    }

    public async Task PostSample()
    {
        string relativeURL = "sample";
        using (var request = new HttpRequestMessage(HttpMethod.Post, relativeURL))
        {
            request.Headers.Authorization = await GetAuthenticationHeaderAsync();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(
                content: """{ "sampleKey": "sampleValue" }""",
                encoding: System.Text.Encoding.UTF8, 
                mediaType: "application/json"
            );
            using (var response = await _http.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Response: {responseBody}", responseBody);
            }
        }
    }
}
