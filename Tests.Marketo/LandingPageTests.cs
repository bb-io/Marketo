using Tests.Marketo.Base;
using Apps.Marketo.Actions;
using Apps.Marketo.Models.Identifiers;
using Apps.Marketo.Models.LandingPages.Requests;

namespace Tests.Marketo;

[TestClass]
public class LandingPageTests : TestBase
{
    [TestMethod]
    public async Task GetLandingPages()
    {
        var action = new LandingPageActions(InvocationContext,FileManager);
        var response = await action.GetLandingPageAsHtml(new LandingPageIdentifier { LandingPageId = "1006" },
            new SegmentationIdentifier { SegmentationId= "1002" },
            new SegmentIdentifier { Segment = "Test" },
            new GetLandingPageAsHtmlRequest { });
        Assert.IsNotNull(response);
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
