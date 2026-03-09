using RestSharp;
using Apps.Marketo.Api;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Authentication;

namespace Apps.Marketo;

public class ConnectionValidator : IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authProviders, CancellationToken cancellationToken)
    {
        try
        {
            var client = new MarketoClient(authProviders);
            var request = new RestRequest("/rest/asset/v1/tagTypes.json", Method.Get);
            await client.ExecuteWithErrorHandling(request);

            return new() { IsValid = true };
        }
        catch (Exception ex)
        {
            return new() 
            {
                IsValid = false,
                Message = ex.Message
            };
        }
    }
}