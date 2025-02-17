using DevSummit.Commons.Pact.Pact.Broker.Entities;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DevSummit.Blog.Consumer.Tests.Pact.Broker;

internal class PactBrokerClient
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    private const string publishContractResource = "/contracts/publish";

    public PactBrokerClient(IConfiguration configuration)
    {
        _configuration = configuration;
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri(_configuration["PactBrokerUrl"])
        };
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/hal+json, application/json, */*; q=0.01");
        _httpClient.DefaultRequestHeaders.Add("X-Interface", "HAL Browser");

        if (!string.IsNullOrEmpty(configuration["PactBrokerUserName"]))
        {
            var authenticationString = $"{configuration["PactBrokerUserName"]}:{configuration["PactBrokerPassword"]}";
            var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);
        }
    }

    public async Task PublishPactContract(PactBrokerPublishContract contract)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, publishContractResource)
        {
            Content = new StringContent(JsonSerializer.Serialize(contract, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }), Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
