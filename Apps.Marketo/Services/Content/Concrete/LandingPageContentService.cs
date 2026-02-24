using Apps.Marketo.Models.Content.Request;
using Apps.Marketo.Models.Content.Response;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

namespace Apps.Marketo.Services.Content.Concrete;

public class LandingPageContentService(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : IContentService
{
    public Task<SearchContentResponse> SearchContent(SearchContentRequest input)
    {
        throw new NotImplementedException();
    }
}
