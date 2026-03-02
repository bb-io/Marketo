using Tests.Marketo.Base;
using Apps.Marketo.Actions;
using Apps.Marketo.Models.Identifiers;
using Apps.Marketo.Models.LandingPages.Requests;

namespace Tests.Marketo;

[TestClass]
public class LandingPageTests : TestBase
{
    [TestMethod]
    public async Task DownloadLandingPage_IsSuccess()
    {
        // Arrange
        var action = new LandingPageActions(InvocationContext, FileManager);
        var landingId = new LandingPageIdentifier { LandingPageId = "1007" };
        var input = new DownloadLandingPageRequest
        {
            IncludeImages = true,
            SegmentationId = "1002",
            Segment = "Default"
        };

        // Act
        var result = await action.GetLandingPageAsHtml(landingId, input);

        // Assert
        Console.WriteLine(result.Content.Name);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ListLandingPages_ReturnsLandingPages()
    {
        // Arrange
        var action = new LandingPageActions(InvocationContext, FileManager);
        var input = new SearchLandingPagesRequest
        {
            UpdatedAfter = new DateTime(2024, 11, 13, 19, 10, 0, DateTimeKind.Utc),
            UpdatedBefore = new DateTime(2024, 11, 13, 19, 15, 0, DateTimeKind.Utc),
        };

        // Act
        var result = await action.ListLandingPages(input);

        // Assert
        Console.WriteLine($"Count: {result.LandingPages.Count}");
        PrintJsonResult(result);
        Assert.IsNotNull(result);
    }
}
