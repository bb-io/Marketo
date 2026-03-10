using Apps.Marketo.Constants;
using Apps.Marketo.Services.Content.Concrete;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

namespace Apps.Marketo.Services.Content;

public class ContentServiceFactory(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
{
    public IContentService GetContentService(string contentType)
    {
        return contentType switch
        {
            ContentTypes.Email => new EmailContentService(invocationContext, fileManagementClient),
            ContentTypes.Snippet => new SnippetContentService(invocationContext, fileManagementClient),
            ContentTypes.LandingPage => new LandingPageContentService(invocationContext, fileManagementClient),
            ContentTypes.Form => new FormContentService(invocationContext, fileManagementClient),
            _ => throw new Exception($"Unsupported content type '{contentType}' was passed in ContentServiceFactory")
        };
    }

    public List<IContentService> GetContentServices(IEnumerable<string> contentTypes)
    {
        var contentServices = new List<IContentService>();

        foreach (var contentType in contentTypes)
            contentServices.Add(GetContentService(contentType));

        return contentServices;
    }
}
