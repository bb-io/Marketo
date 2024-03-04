using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers;

public class FolderDataHandler : BaseInvocable, IAsyncDataSourceHandler
{
    public FolderDataHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }
    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
        var request = new MarketoRequest($"/rest/asset/v1/folders.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
        request.AddQueryParameter("maxReturn", 200);
        request.AddQueryParameter("maxDepth", 10);
        var response = client.Get<BaseResponseDto<FolderInfoDto>>(request);
        return response.Result.DistinctBy(x => x.Id).Where(str => str.Name.Contains(context.SearchString)).ToDictionary(k => k.Id.ToString(), v => v.Name);
    }
}