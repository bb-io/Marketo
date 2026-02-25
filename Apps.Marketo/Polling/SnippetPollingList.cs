using RestSharp;
using Apps.Marketo.Dtos;
using Apps.Marketo.Models.Snippets.Response;
using Apps.Marketo.Polling.Models.Memories;
using Blackbird.Applications.Sdk.Common.Polling;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Marketo.Polling;

[PollingEventList("Snippets")]
public class SnippetPollingList(InvocationContext invocationContext) : BasePollingList(invocationContext)
{
    [PollingEvent("On snippets created or updated", "On any snippets are created or updated")]
    public async Task<PollingEventResponse<DateMemory, SearchSnippetsResponse>> OnSnippetsCreatedOrUpdated(
        PollingEventRequest<DateMemory> request)
    {
        return await HandlePolling<SearchSnippetsResponse>(request, async memory =>
        {
            var endpoint = "/rest/asset/v1/snippets.json";
            var request = new RestRequest(endpoint, Method.Get);
            var response = await Client.Paginate<SnippetDto>(request);

            var snippets = response
                .Where(x => x.CreatedAt >= memory.LastInteractionDate || x.UpdatedAt >= memory.LastInteractionDate)
                .ToList();
            return new(snippets);
        }, result => result.Snippets.Count != 0);
    }
}
