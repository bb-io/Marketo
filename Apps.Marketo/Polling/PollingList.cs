using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Emails.Responses;
using Apps.Marketo.Models.EmailTemplates.Response;
using Apps.Marketo.Models.Forms.Responses;
using Apps.Marketo.Models.LandingPages.Responses;
using Apps.Marketo.Models.Snippets.Response;
using Apps.Marketo.Polling.Models.Memories;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using RestSharp;

namespace Apps.Marketo.Polling;

[PollingEventList]
public class PollingList : MarketoInvocable
{
    public PollingList(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    [PollingEvent("On emails created or updated", "On any emails are created or updated")]
    public PollingEventResponse<DateMemory, ListEmailsResponse> OnEmailsCreatedOrUpdated(
        PollingEventRequest<DateMemory> request) => HandlePolling<ListEmailsResponse>(request, memory =>
    {
        var endpoint = "/rest/asset/v1/emails.json";
        var response = Client.Paginate<EmailDto>(new MarketoRequest(endpoint, Method.Get, Credentials));

        return new()
        {
            Emails = response
                .Where(x => x.CreatedAt >= memory.LastInteractionDate || x.UpdatedAt >= memory.LastInteractionDate)
                .ToList()
        };
    }, result => result.Emails.Any());

    [PollingEvent("On email templates created or updated", "On any email templates are created or updated")]
    public PollingEventResponse<DateMemory, ListEmailTemplatesResponse> OnEmailTemplatesCreatedOrUpdated(
        PollingEventRequest<DateMemory> request) => HandlePolling<ListEmailTemplatesResponse>(request, memory =>
    {
        var endpoint = "/rest/asset/v1/emailTemplates.json";
        var response = Client.Paginate<EmailTemplateDto>(new MarketoRequest(endpoint, Method.Get, Credentials));

        return new()
        {
            EmailTemplates = response.Where(x =>
                x.CreatedAt >= memory.LastInteractionDate || x.UpdatedAt >= memory.LastInteractionDate).ToList()
        };
    }, result => result.EmailTemplates.Any());

    [PollingEvent("On forms created or updated", "On any forms are created or updated")]
    public PollingEventResponse<DateMemory, ListFormsResponse> OnFormsCreatedOrUpdated(
        PollingEventRequest<DateMemory> request) => HandlePolling<ListFormsResponse>(request, memory =>
    {
        var endpoint = "/rest/asset/v1/forms.json";
        var response = Client.Paginate<FormDto>(new MarketoRequest(endpoint, Method.Get, Credentials));

        return new()
        {
            Forms = response.Where(x =>
                x.CreatedAt >= memory.LastInteractionDate || x.UpdatedAt >= memory.LastInteractionDate).ToList()
        };
    }, result => result.Forms.Any());

    [PollingEvent("On snippets created or updated", "On any snippets are created or updated")]
    public PollingEventResponse<DateMemory, ListSnippetsResponse> OnSnippetsCreatedOrUpdated(
        PollingEventRequest<DateMemory> request) => HandlePolling<ListSnippetsResponse>(request, memory =>
    {
        var endpoint = "/rest/asset/v1/snippets.json";
        var response = Client.Paginate<SnippetDto>(new MarketoRequest(endpoint, Method.Get, Credentials));

        return new()
        {
            Snippets = response.Where(x =>
                x.CreatedAt >= memory.LastInteractionDate || x.UpdatedAt >= memory.LastInteractionDate).ToList()
        };
    }, result => result.Snippets.Any());

    [PollingEvent("On landing pages created or updated", "On any landing pages are created or updated")]
    public PollingEventResponse<DateMemory, ListLandingPagesResponse> OnLandingPagesCreatedOrUpdated(
        PollingEventRequest<DateMemory> request) => HandlePolling<ListLandingPagesResponse>(request, memory =>
    {
        var endpoint = "/rest/asset/v1/landingPages.json";
        var response = Client.Paginate<LandingPageDto>(new MarketoRequest(endpoint, Method.Get, Credentials));

        return new()
        {
            LandingPages = response.Where(x =>
                x.CreatedAt >= memory.LastInteractionDate || x.UpdatedAt >= memory.LastInteractionDate).ToList()
        };
    }, result => result.LandingPages.Any());

    private PollingEventResponse<DateMemory, T> HandlePolling<T>(
        PollingEventRequest<DateMemory> request, Func<DateMemory, T> func, Func<T, bool> isResultValid)
    {
        if (request.Memory is null)
        {
            return new()
            {
                FlyBird = false,
                Memory = new()
                {
                    LastInteractionDate = DateTime.UtcNow
                }
            };
        }

        var result = func(request.Memory);
        if (!isResultValid(result))
        {
            return new()
            {
                FlyBird = false,
                Memory = new()
                {
                    LastInteractionDate = DateTime.UtcNow
                }
            };
        }

        return new()
        {
            FlyBird = true,
            Result = result,
            Memory = new()
            {
                LastInteractionDate = DateTime.UtcNow
            }
        };
    }
}