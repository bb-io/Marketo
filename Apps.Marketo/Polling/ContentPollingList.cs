using Apps.Marketo.Constants;
using Apps.Marketo.Dtos.Content;
using Apps.Marketo.Helper.Filter;
using Apps.Marketo.Models.Content.Response;
using Apps.Marketo.Models.Entities.Email;
using Apps.Marketo.Models.Entities.LandingPage;
using Apps.Marketo.Models.Entities.Snippet;
using Apps.Marketo.Models.Identifiers.Optional;
using Apps.Marketo.Polling.Models.Memories;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using Blackbird.Applications.SDK.Blueprints;
using RestSharp;

namespace Apps.Marketo.Polling;

[PollingEventList("Content")]
public class ContentPollingList(InvocationContext invocationContext) : BasePollingList(invocationContext)
{
    [PollingEvent("On content approved", "On emails, snippets and landing pages approved")]
    [BlueprintEventDefinition(BlueprintEvent.ContentCreatedOrUpdatedMultiple)]
    public async Task<PollingEventResponse<DateMemory, SearchContentResponse>> OnContentApproved(
        PollingEventRequest<DateMemory> request,
        [PollingEventParameter] OptionalPollingContentTypeIdentifier contentTypesInput)
    {
        return await HandlePolling<SearchContentResponse>(request, async memory =>
        {
            contentTypesInput.ApplyDefaultValues();

            var fetchTasks = new List<Task<IEnumerable<ContentDto>>>();

            if (contentTypesInput.ContentTypes!.Contains(ContentTypes.Email))
            {
                fetchTasks.Add(FetchContent<EmailEntity>(
                    "/rest/asset/v1/emails.json", 
                    memory.LastInteractionDate, 
                    x => x.UpdatedAt,
                    x => new ContentDto(x)));
            }

            if (contentTypesInput.ContentTypes!.Contains(ContentTypes.LandingPage))
            {
                fetchTasks.Add(FetchContent<LandingPageEntity>(
                    "/rest/asset/v1/landingPages.json", 
                    memory.LastInteractionDate,
                    x => x.UpdatedAt,
                    x => new ContentDto(x)));
            }

            if (contentTypesInput.ContentTypes!.Contains(ContentTypes.Snippet))
            {
                fetchTasks.Add(FetchContent<SnippetEntity>(
                    "/rest/asset/v1/snippets.json",
                    memory.LastInteractionDate,
                    x => x.UpdatedAt,
                    x => new ContentDto(x)));
            }

            var results = await Task.WhenAll(fetchTasks);

            var contentList = results.SelectMany(x => x).ToList();
            return new(contentList);
        },
        result => result.Items.Count != 0);
    }

    private async Task<IEnumerable<ContentDto>> FetchContent<TEntity>(
        string endpoint,
        DateTime lastInteractionDate, 
        Func<TEntity, DateTime?> dateSelector,
        Func<TEntity, ContentDto> mapToDto)
    {
        var request = new RestRequest(endpoint, Method.Get).AddQueryParameter("status", "approved");

        var entities = await Client.Paginate<TEntity>(request);
        var filteredEntities = entities.ApplyDateAfterFilter(lastInteractionDate, dateSelector);

        return filteredEntities.Select(mapToDto);
    }
}
