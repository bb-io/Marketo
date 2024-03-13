using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers;

public class FormDataHandler : BaseInvocable, IDataSourceHandler
{
    public FormDataHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public Dictionary<string, string> GetData(DataSourceContext context)
    {
        var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
        var request = new MarketoRequest("/rest/asset/v1/forms.json", Method.Get, 
            InvocationContext.AuthenticationCredentialsProviders);
        var response = client.Paginate<FormDto>(request);
        return response.Where(form => context.SearchString == null || 
                                             form.Name.Contains(context.SearchString))
            .ToDictionary(form => form.Id.ToString(), form => form.Name);
    }
}