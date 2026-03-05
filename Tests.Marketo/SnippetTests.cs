using Tests.Marketo.Base;
using Apps.Marketo.Actions;
using Apps.Marketo.Models.Identifiers;
using Apps.Marketo.Models.Snippets.Request;

namespace Tests.Marketo;

[TestClass]
public class SnippetTests : TestBase
{
    [TestMethod]
    public async Task ListSnippets_ReturnsSnippets()
    {
		// Arrange
		var action = new SnippetActions(InvocationContext, FileManager);
		var input = new SearchSnippetsRequest
		{
			UpdatedAfter = DateTime.UtcNow - TimeSpan.FromHours(2),
		};

		// Act
		var result = await action.ListSnippets(input);

        // Assert
        Console.WriteLine($"Count: {result.Snippets.Count}");
		PrintJsonResult(result);
		Assert.IsNotNull(result);
	}

	[TestMethod]
	public async Task DownloadSnippet_IsSuccess()
	{
		// Arrange
		var action = new SnippetActions(InvocationContext, FileManager);
		var snippetId = new SnippetIdentifier { SnippetId = "1" };
		var input = new DownloadSnippetRequest
		{
			SegmentationId = "1002",
			Segment = "Test"
        };

		// Act
		var result = await action.GetSnippetAsHtml(snippetId, input);

        // Assert
        Console.WriteLine(result.Content.Name);
		Assert.IsNotNull(result);
	}
}
