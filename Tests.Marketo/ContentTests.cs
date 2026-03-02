using Tests.Marketo.Base;
using Apps.Marketo.Actions;
using Apps.Marketo.Constants;
using Apps.Marketo.Models.Content.Request;
using Apps.Marketo.Models.Identifiers.Optional;

namespace Tests.Marketo;

[TestClass]
public class ContentTests : TestBase
{
    [TestMethod]
    public async Task SearchContent_ReturnsContent()
    {
		// Arrange
		var actions = new ContentActions(InvocationContext, FileManager);
		var input = new SearchContentRequest
		{
			
		};
		var contentTypes = new OptionalContentTypesIdentifier 
		{ 
			ContentTypes = [ContentTypes.Form] 
		};

		// Act
		var result = await actions.SearchContent(contentTypes, input);

		// Assert
		PrintJsonResult(result);
		Assert.IsNotNull(result);
	}
}
