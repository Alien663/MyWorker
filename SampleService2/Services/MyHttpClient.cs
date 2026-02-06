using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using SampleService2.Interfaces;
using SampleService2.Options;
using System.Net.Http.Headers;
using System.Web;

namespace SampleService2.Services;

public class MyHttpClient : IMyHttpClient
{
    private readonly SampleOption _option;
    private readonly IConfidentialClientApplication _app;
    private readonly string[] _scopes;
    private readonly HttpClient _http;
    private readonly ILogger<MyHttpClient> _logger;

    public MyHttpClient(IOptions<SampleOption> option, HttpClient http, ILogger<MyHttpClient> logger)
    {
        // Injection of dependencies
        _option = option.Value;
        _http = http;
        _logger = logger;

        // Initialize confidential client application
        _scopes = _option.Scope?.Split(";") ?? Array.Empty<string>();
        _app = ConfidentialClientApplicationBuilder
            .Create(_option.ClientId)
            .WithClientSecret(_option.ClientSecret)
            .WithAuthority(_option.Authority)
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
