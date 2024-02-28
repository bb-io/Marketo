using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Marketo.Invocables;

public class MarketoInvocable : BaseInvocable
{
    protected readonly IEnumerable<AuthenticationCredentialsProvider> Credentials;
    protected readonly MarketoClient Client;

    protected MarketoInvocable(InvocationContext invocationContext) : base(invocationContext)
    {
        Credentials = invocationContext.AuthenticationCredentialsProviders;
        Client = new(Credentials);
    }
}