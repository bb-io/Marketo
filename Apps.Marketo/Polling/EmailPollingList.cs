using RestSharp;
using Apps.Marketo.Models.Emails.Responses;
using Apps.Marketo.Polling.Models.Memories;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using Apps.Marketo.Models.Entities.Email;
using Apps.Marketo.Dtos.Email;

namespace Apps.Marketo.Polling;

[PollingEventList("Emails")]
public class EmailPollingList(InvocationContext invocationContext) : BasePollingList(invocationContext)
{
    [PollingEvent("On emails created or updated", "On any emails are created or updated")]
    public async Task<PollingEventResponse<DateMemory, SearchEmailsResponse>> OnEmailsCreatedOrUpdated(
        PollingEventRequest<DateMemory> request)
    {
        return await HandlePolling<SearchEmailsResponse>(request, async memory =>
        {
            var endpoint = "/rest/asset/v1/emails.json";
            var request = new RestRequest(endpoint, Method.Get);
            var response = await Client.Paginate<EmailEntity>(request);

            var emails = response
                .Where(x => x.CreatedAt >= memory.LastInteractionDate || x.UpdatedAt >= memory.LastInteractionDate)
                .ToList();
            return new(emails.Select(x => new EmailDto(x)).ToList());
        }, result => result.Emails.Count != 0);
    }
}
