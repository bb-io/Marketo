using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Marketo.Actions;

public abstract class BaseActions : BaseInvocable
{
    protected readonly IEnumerable<AuthenticationCredentialsProvider> Credentials;
    protected readonly MarketoClient Client;

    protected BaseActions(InvocationContext invocationContext) : base(invocationContext)
    {
        Credentials = invocationContext.AuthenticationCredentialsProviders;
        Client = new MarketoClient(Credentials);
    }
}