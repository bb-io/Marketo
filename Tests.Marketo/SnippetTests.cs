using Tests.Marketo.Base;
using Apps.Marketo.Actions;
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
			CreatedBefore = new DateTime(2024, 08, 28, 9, 0, 0, DateTimeKind.Utc)
		};

		// Act
		var result = await action.ListSnippets(input);

        // Assert
        Console.WriteLine($"Count: {result.Snippets.Count}");
		PrintJsonResult(result);
		Assert.IsNotNull(result);
	}
}
