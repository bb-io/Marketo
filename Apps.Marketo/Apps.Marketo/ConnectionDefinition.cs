using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.Marketo;

public class ConnectionDefinition : IConnectionDefinition
{
    public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups => new List<ConnectionPropertyGroup>()
    {
        new()
        {
            Name = "OAuth",
            AuthenticationType = ConnectionAuthenticationType.Undefined,
            ConnectionUsage = ConnectionUsage.Actions,
            ConnectionProperties = new List<ConnectionProperty>()
            {
                new("Munchkin Account ID"),
                new("Client ID"),
                new("Client secret")
            }
        }
    };

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(Dictionary<string, string> values)
    {
        yield return new AuthenticationCredentialsProvider(
            AuthenticationCredentialsRequestLocation.Body,
            "Client ID",
            values["Client ID"]
        );

        yield return new AuthenticationCredentialsProvider(
            AuthenticationCredentialsRequestLocation.Body,
            "Client secret",
            values["Client secret"]
        );

        yield return new AuthenticationCredentialsProvider(
            AuthenticationCredentialsRequestLocation.QueryString,
            "Munchkin Account ID",
            values["Munchkin Account ID"]
        );
    }
}