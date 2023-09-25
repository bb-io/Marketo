using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common.Authentication;
using RestSharp;

namespace Apps.Marketo;

public class MarketoRequest : RestRequest
{
    public MarketoRequest(string endpoint, Method method, IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) : base(endpoint, method)
    {
        this.AddQueryParameter("access_token", GetAccessToken(authenticationCredentialsProviders));
        this.AddHeader("accept", "*/*");
    }

    private string GetAccessToken(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var client = new MarketoClient(authenticationCredentialsProviders: authenticationCredentialsProviders);

        var clientId = authenticationCredentialsProviders.First(v => v.KeyName == "Client ID").Value;
        var clientSecret = authenticationCredentialsProviders.First(v => v.KeyName == "Client secret").Value;

        var authRequest = new RestRequest("/identity/oauth/token", Method.Get);
        authRequest.AddQueryParameter("grant_type", "client_credentials");
        authRequest.AddQueryParameter("client_id", $"{clientId}");
        authRequest.AddQueryParameter("client_secret", $"{clientSecret}");

        var authResponse = client.Execute<AuthDto>(authRequest);
        if (authResponse.Data == null) throw new Exception("Auth response was null");
        return authResponse.Data.AccessToken;
    }
}