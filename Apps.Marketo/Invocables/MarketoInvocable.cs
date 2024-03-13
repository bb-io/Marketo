using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using RestSharp;

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

    protected void AddFolderParameter(MarketoRequest request, string? folderId)
    {
        if (folderId != null)
        {
            if (folderId.Contains("_Folder"))
                request.AddQueryParameter("folder", int.Parse(folderId.Replace("_Folder", "")));
            else if (folderId.Contains("_Program"))
                request.AddQueryParameter("folder", JsonConvert.SerializeObject(new
                {
                    id = int.Parse(folderId.Replace("_Program", "")),
                    type = "Program"
                }));
        }
    }
}