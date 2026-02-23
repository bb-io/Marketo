using RestSharp;
using Apps.Marketo.Dtos;
using Apps.Marketo.Polling.Models.Memories;
using Apps.Marketo.Models.EmailTemplates.Response;
using Blackbird.Applications.Sdk.Common.Polling;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Marketo.Polling;

[PollingEventList("Email templates")]
public class EmailTemplatePollingList(InvocationContext invocationContext) : BasePollingList(invocationContext)
{
    [PollingEvent("On email templates created or updated", "On any email templates are created or updated")]
    public PollingEventResponse<DateMemory, ListEmailTemplatesResponse> OnEmailTemplatesCreatedOrUpdated(
        PollingEventRequest<DateMemory> request) => HandlePolling<ListEmailTemplatesResponse>(request, memory =>
        {
            var endpoint = "/rest/asset/v1/emailTemplates.json";
            var response = Client.Paginate<EmailTemplateDto>(new MarketoRequest(endpoint, Method.Get, Credentials));

            var templates = response
                .Where(x => x.CreatedAt >= memory.LastInteractionDate || x.UpdatedAt >= memory.LastInteractionDate)
                .ToList();
            return new(templates);
        }, result => result.EmailTemplates.Count != 0);
}
