using RestSharp;
using Apps.Marketo.Models.Forms.Responses;
using Apps.Marketo.Polling.Models.Memories;
using Blackbird.Applications.Sdk.Common.Polling;
using Blackbird.Applications.Sdk.Common.Invocation;
using Apps.Marketo.Dtos.Form;

namespace Apps.Marketo.Polling;

[PollingEventList("Forms")]
public class FormPollingEvent(InvocationContext invocationContext) : BasePollingList(invocationContext)
{
    [PollingEvent("On forms created or updated", "On any forms are created or updated")]
    public async Task<PollingEventResponse<DateMemory, SearchFormsResponse>> OnFormsCreatedOrUpdated(
        PollingEventRequest<DateMemory> request)
    {
        return await HandlePolling<SearchFormsResponse>(request, async memory =>
        {
            var endpoint = "/rest/asset/v1/forms.json";
            var request = new RestRequest(endpoint, Method.Get);
            var response = await Client.Paginate<FormDto>(request);

            var forms = response
                .Where(x => x.CreatedAt >= memory.LastInteractionDate || x.UpdatedAt >= memory.LastInteractionDate)
                .ToList();
            return new(forms);
        }, result => result.Forms.Count != 0);
    }
}
