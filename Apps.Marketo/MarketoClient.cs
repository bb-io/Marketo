using System.Text.Json;
using System.Text.Json.Serialization;
using Apps.Marketo.Dtos;
using Apps.Marketo.Models;
using Blackbird.Applications.Sdk.Common.Authentication;
using RestSharp;
using RestSharp.Serializers.Json;

namespace Apps.Marketo;

public class MarketoClient : RestClient
{
    public MarketoClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
        : base(
            new RestClientOptions { ThrowOnAnyError = true, BaseUrl = GetUri(authenticationCredentialsProviders) },
            configureSerialization: s => s.UseSystemTextJson(new JsonSerializerOptions(JsonSerializerDefaults.Web)
                { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull })
        ) { }

    private static Uri GetUri(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProvider)
    {
        var url = authenticationCredentialsProvider.First(v => v.KeyName == "Munchkin Account ID").Value;
        return new Uri($"https://{url}.mktorest.com");
    }

    public BaseResponseDto<T> ExecuteWithError<T>(MarketoRequest request)
    {
        var res = this.Execute(request).Content;
        var deserialized = JsonSerializer.Deserialize<BaseResponseDto<T>>(res, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true});
        if (deserialized.Errors.Any())
        {
            throw new ArgumentException(deserialized.Errors.First().Message);
        }
        return deserialized;
    }
    public RestResponse ExecuteWithError(MarketoRequest request)
    {
        var res = this.Execute(request);

        var errors = JsonSerializer.Deserialize<ErrorResponse>(res.Content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true});
     
        if (errors.Errors.Any())
            throw new ArgumentException(errors.Errors.First().Message);

        return res;
    }
}