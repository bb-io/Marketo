using System;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.Marketo
{
	public class ConnectionDefinition : IConnectionDefinition
	{
        public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups => new List<ConnectionPropertyGroup>()
        {
            new ConnectionPropertyGroup
            {
                Name = "non-oauth",
                AuthenticationType = ConnectionAuthenticationType.Undefined,
                ConnectionUsage = ConnectionUsage.Actions,
                ConnectionProperties = new List<ConnectionProperty>()
                {
                    new ConnectionProperty("munchkin"),
                    new ConnectionProperty("clientId"),
                    new ConnectionProperty("clientSecret")
                }
            }
        };

        public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(Dictionary<string, string> values)
        {
            yield return new AuthenticationCredentialsProvider(
                AuthenticationCredentialsRequestLocation.Header,
                "clientId",
                values["clientId"]
            );

            yield return new AuthenticationCredentialsProvider(
                AuthenticationCredentialsRequestLocation.Header,
                "clientSecret",
                values["clientSecret"]
            );

            yield return new AuthenticationCredentialsProvider(
                AuthenticationCredentialsRequestLocation.QueryString,
                "munchkin",
                 values["munchkin"]
             );
        }
    }
}

