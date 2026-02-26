using Apps.Marketo.Constants;
using Apps.Marketo.Dtos;
using Apps.Marketo.Models.Utility.Error;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Marketo.Api;

public class MarketoClient : BlackBirdRestClient
{
    public MarketoClient(IEnumerable<AuthenticationCredentialsProvider> creds)
        : base(new RestClientOptions { BaseUrl = GetUri(creds) })
    {
        this.AddDefaultHeader("Authorization", $"Bearer {GetAccessToken(creds).GetAwaiter().GetResult()}");
        this.AddDefaultHeader("accept", "*/*");
    }

    private static Uri GetUri(IEnumerable<AuthenticationCredentialsProvider> creds)
    {
        string url = creds.Get(CredsNames.MunchkinAccountId).Value;
        return new($"https://{url}.mktorest.com");
    }

    private async Task<string> GetAccessToken(IEnumerable<AuthenticationCredentialsProvider> creds)
    {
        string clientId = creds.Get(CredsNames.ClientId).Value;
        string clientSecret = creds.Get(CredsNames.ClientSecret).Value;

        var authRequest = new RestRequest("/identity/oauth/token", Method.Get);
        authRequest.AddQueryParameter("grant_type", "client_credentials");
        authRequest.AddQueryParameter("client_id", clientId);
        authRequest.AddQueryParameter("client_secret", clientSecret);

        var authResponse = await this.ExecuteAsync<AuthDto>(authRequest);
        if (authResponse.Data == null) 
            throw new Exception("Auth response was null");
        return authResponse.Data.AccessToken;
    }

    public async Task<IEnumerable<T>> Paginate<T>(RestRequest request)
    {
        var offset = 0;
        var limit = 200;

        var baseUrl = request.Resource.SetQueryParameter("maxReturn", limit.ToString());

        var result = new List<T>();
        IEnumerable<T> response;
        do
        {
            request.Resource = baseUrl.SetQueryParameter("offset", offset.ToString());

            response = await ExecuteWithErrorHandling<T>(request);
            result.AddRange(response);

            offset += limit;
        } while (response.Any());

        return result;
    }

    public async Task<T> ExecuteWithErrorHandlingFirst<T>(RestRequest request)
    {
        var result = await ExecuteWithErrorHandling<T>(request);
        if (result == null || !result.Any())
            throw new PluginApplicationException("The requested data was not found");

        return result.First();
    }

    public new async Task<IEnumerable<T>> ExecuteWithErrorHandling<T>(RestRequest request)
    {
        var response = await ExecuteWithErrorHandling(request);
        var result = JsonConvert.DeserializeObject<BaseResponseDto<T>>(response.Content!);
        return result?.Result ?? [];
    }

    public new async Task<RestResponse> ExecuteWithErrorHandling(RestRequest request)
    {
        var response = await ExecuteAsync(request);
        if (response.Content == null)
            throw new PluginApplicationException("No content received from the server");

        if (!response.IsSuccessStatusCode)
            ConfigureErrorException(response);        

        return response;
    }

    public async Task<BaseResponseDto<T>?> ExecuteNoErrorHandling<T>(RestRequest request)
    {
        var response = await ExecuteAsync(request);
        var result = JsonConvert.DeserializeObject<BaseResponseDto<T>>(response.Content!);
        return result;
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        var statusCode = response.StatusCode;
        var errors = JsonConvert.DeserializeObject<ErrorResponse>(response.Content!) 
            ?? throw new PluginApplicationException($"Status {statusCode}, but no errors received from the server");

        if (errors.Errors.Count != 0)
        {
            var errorsList = errors.Errors.Select(x => x.Message);
            string errorMessage = string.Join("; ", errorsList);
            throw new PluginApplicationException(errorMessage);
        }

        throw new PluginApplicationException($"Status {statusCode}. Unknown error");
    }
}