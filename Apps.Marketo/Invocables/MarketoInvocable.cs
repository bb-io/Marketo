using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.Extensions.FileSystemGlobbing;
using Newtonsoft.Json;
using RestSharp;
using System.Text.RegularExpressions;

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

    protected bool IsFilePathMatchingPattern(List<string> patterns, string filePath, bool exclude)
    {
        var matcher = new Matcher();
        foreach(var pattern in patterns)
        {
            if(exclude)
                matcher.AddExclude(pattern);
            else
                matcher.AddInclude(pattern);
        }
        return matcher.Match(filePath).HasMatches;
    }
}