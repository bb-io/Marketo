using Apps.Marketo.Api;
using Apps.Marketo.Models.Utility.Error;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Newtonsoft.Json;
using System.Net;

namespace Apps.Marketo;

public class ConnectionValidator : IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authProviders, CancellationToken cancellationToken)
    {
        try
        {
            var client = new MarketoClient(authProviders);
            var result = await client.ExecuteTokenEndpoint();

            if (!result.IsSuccessStatusCode)
            {
                if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var errorResult = JsonConvert.DeserializeObject<AuthError>(result.Content ?? "");
                    throw new PluginApplicationException($"{errorResult?.ErrorDescription} (error code: {errorResult?.Error})");
                }
                else if (!string.IsNullOrEmpty(result.ErrorMessage) && result.ErrorMessage.Contains("No such host is known"))
                {
                    throw new PluginApplicationException("Munchkin Account ID is invalid");
                }
            }

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