﻿using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common;
using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers;

public class LandingPageHandler : BaseInvocable, IAsyncDataSourceHandler
{
    public LandingPageHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
        var request = new MarketoRequest($"/rest/asset/v1/landingPages.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
        var response = client.Paginate<LandingPageDto>(request);
        return response.Where(str => context.SearchString is null || str.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase)).ToDictionary(k => k.Id.ToString(), v => v.Name);
    }
}