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
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    
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
        var response = ExecuteWithErrorHandling(request);
        return JsonSerializer.Deserialize<BaseResponseDto<T>>(response.Content, _jsonSerializerOptions)!;
    }

    public RestResponse ExecuteWithError(MarketoRequest request) => ExecuteWithErrorHandling(request);

    private RestResponse ExecuteWithErrorHandling(MarketoRequest request)
    {
        var response = this.Execute(request);
        var errors = JsonSerializer.Deserialize<ErrorResponse>(response.Content, _jsonSerializerOptions);

        if (errors.Errors.Any())
        {
            if (errors.Errors.Any(error => error.Code == "606"))
            {
                Thread.Sleep(TimeSpan.FromSeconds(20));
                var retryResponse = this.Execute(request);
                var retryErrors = JsonSerializer.Deserialize<ErrorResponse>(retryResponse.Content, _jsonSerializerOptions);
                
                if (retryErrors.Errors.Any())
                    throw new ArgumentException(errors.Errors.First().Message);

                return retryResponse;
            }
            
            throw new ArgumentException(errors.Errors.First().Message);
        }

        return response;
    }
}