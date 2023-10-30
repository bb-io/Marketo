using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using RestSharp;

namespace Apps.Marketo;

public class ConnectionValidator : IConnectionValidator
{
    public ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authProviders, CancellationToken cancellationToken)
    {
        try
        {
            var client = new MarketoClient(authProviders);
            var request = new MarketoRequest("/rest/asset/v1/files.json", Method.Get, authProviders);
            client.ExecuteWithError<FileInfoDto>(request);

            return new ValueTask<ConnectionValidationResponse>(Task.FromResult(new ConnectionValidationResponse()
            {
                IsValid = true,
            }));
        }
        catch (Exception ex)
        {
            return new ValueTask<ConnectionValidationResponse>(Task.FromResult(new ConnectionValidationResponse()
            {
                IsValid = false,
                Message = ex.Message
            }));
        }
    }
}