using Apps.Marketo.Actions;
using Apps.Marketo.Constants;
using Apps.Marketo.Models.Content.Request;
using Apps.Marketo.Models.Identifiers;
using Apps.Marketo.Models.Identifiers.Optional;
using Blackbird.Applications.Sdk.Common.Files;
using Tests.Marketo.Base;

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

	[TestMethod]
	public async Task DownloadContent_IsSuccess()
	{
		// Arrange
		var actions = new ContentActions(InvocationContext, FileManager);
		var contentType = new ContentTypeIdentifier { ContentType = ContentTypes.Form };
		var input = new DownloadContentRequest
		{
			ContentId = "1004"
        };

		// Act
		var result = await actions.DownloadContent(contentType, input);

		// Assert
		PrintJsonResult(result);
		Assert.IsNotNull(result);
    }

	[TestMethod]
	public async Task UploadContent_IsSuccess()
	{
        // Arrange
        var actions = new ContentActions(InvocationContext, FileManager);
		var input = new UploadContentRequest
		{
			Content = new FileReference { Name = "test.html" }
		};

        // Act
		await actions.UploadContent(input);
    }
}
