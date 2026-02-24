using RestSharp;
using Apps.Marketo.Dtos;
using Apps.Marketo.Models.Emails.Responses;
using Apps.Marketo.Polling.Models.Memories;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;

namespace Apps.Marketo.Polling;

[PollingEventList("Emails")]
public class EmailPollingList(InvocationContext invocationContext) : BasePollingList(invocationContext)
{
    [PollingEvent("On emails created or updated", "On any emails are created or updated")]
    public async Task<PollingEventResponse<DateMemory, ListEmailsResponse>> OnEmailsCreatedOrUpdated(
        PollingEventRequest<DateMemory> request)
    {
        return await HandlePolling<ListEmailsResponse>(request, async memory =>
        {
            var endpoint = "/rest/asset/v1/emails.json";
            var request = new RestRequest(endpoint, Method.Get);
            var response = await Client.Paginate<EmailDto>(request);

            var emails = response
                .Where(x => x.CreatedAt >= memory.LastInteractionDate || x.UpdatedAt >= memory.LastInteractionDate)
                .ToList();
            return new(emails);
        }, result => result.Emails.Count != 0);
    }
}
