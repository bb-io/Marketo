using RestSharp;
using Apps.Marketo.Dtos;
using Apps.Marketo.Polling.Models.Memories;
using Apps.Marketo.Models.LandingPages.Responses;
using Blackbird.Applications.Sdk.Common.Polling;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Marketo.Polling;

[PollingEventList("Landing pages")]
public class LandingPagePollingList(InvocationContext invocationContext) : BasePollingList(invocationContext)
{
    [PollingEvent("On landing pages created or updated", "On any landing pages are created or updated")]
    public PollingEventResponse<DateMemory, ListLandingPagesResponse> OnLandingPagesCreatedOrUpdated(
        PollingEventRequest<DateMemory> request) => HandlePolling<ListLandingPagesResponse>(request, memory =>
        {
            var endpoint = "/rest/asset/v1/landingPages.json";
            var response = Client.Paginate<LandingPageDto>(new MarketoRequest(endpoint, Method.Get, Credentials));

            var pages = response
                .Where(x => x.CreatedAt >= memory.LastInteractionDate || x.UpdatedAt >= memory.LastInteractionDate)
                .ToList();
            return new(pages);
        }, result => result.LandingPages.Count != 0);
}
