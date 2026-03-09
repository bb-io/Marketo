using Tests.Marketo.Base;
using Apps.Marketo.Polling;
using Apps.Marketo.Polling.Models.Memories;
using Apps.Marketo.Models.Identifiers.Optional;
using Blackbird.Applications.Sdk.Common.Polling;

namespace Tests.Marketo;

[TestClass]
public class ContentPollingTests : TestBase
{
    [TestMethod]
    public async Task OnContentApproved_ReturnsApprovedContent()
    {
		// Arrange
		var polling = new ContentPollingList(InvocationContext);
		var memory = new DateMemory { LastInteractionDate = new DateTime(2026, 03, 09, 9, 0, 0, DateTimeKind.Utc) };
		var pollingRequest = new PollingEventRequest<DateMemory> { Memory = memory };
        var contentTypes = new OptionalContentTypesIdentifier { };

        // Act
        var result = await polling.OnContentApproved(pollingRequest, contentTypes);

        // Assert
        Console.WriteLine($"Count: {result.Result?.Items.Count}");
        PrintJsonResult(result);
        Assert.IsNotNull(result);
    }
}
