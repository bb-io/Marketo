using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;
namespace Apps.Marketo.DataSourceHandlers.FolderDataHandlers
{
    public class EmailTemplateFolderDataHandler : BaseInvocable, IAsyncDataSourceHandler
    {
        private readonly string MarketoTemplatesFolderPath = "/Design Studio/Default/Emails/Marketo Templates";
        public EmailTemplateFolderDataHandler(InvocationContext invocationContext) : base(invocationContext)
        {
        }
        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
            CancellationToken cancellationToken)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/folders.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            request.AddQueryParameter("maxDepth", 10);
            var response = client.Paginate<FolderInfoDto>(request);
            return response
                .DistinctBy(x => x.Id)
                .Where(x => x.FolderType == "Email Template" && !x.Path.Contains(MarketoTemplatesFolderPath))
                .Where(str => context.SearchString is null || str.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(k => $"{k.Id.ToString()}_{k.FolderId.Type}", v => v.Name);
        }
    }
}
