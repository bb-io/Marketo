using Apps.Marketo.Constants;
using Apps.Marketo.DataSourceHandlers;
using Apps.Marketo.DataSourceHandlers.FolderDataHandlers;
using Apps.Marketo.Models.Identifiers;
using Apps.Marketo.Models.Identifiers.Optional;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Tests.Marketo.Base;

namespace Tests.Marketo;

[TestClass]
public class DataHandlerTests : TestBase
{
    [TestMethod]
    public async Task EmailDataHandler_ReturnsEmails()
    {
        // Arrange
        var handler = new EmailDataHandler(InvocationContext);

        // Act
        var result = await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None);

        // Assert
        PrintDataHandlerResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task FileDataHandlert_IsSuccess()
    {
        var handler = new FileDataHandler(InvocationContext);

        var result = await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None);
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task EmailTemplateDataHandlert_IsSuccess()
    {
        var handler = new EmailTemplateDataHandler(InvocationContext);

        var result = await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None);
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);

        Assert.IsNotNull(result);
    } 
    
    [TestMethod]
    public async Task LandingPageHandler_ReturnsLandingPages()
    {
        // Arrange
        var handler = new LandingPageHandler(InvocationContext);

        // Act
        var result = await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None);

        // Assert
        PrintDataHandlerResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task LandingPageFolderDataHandler_ReturnsFolders()
    {
        // Arrange
        var handler = new LandingPageFolderDataHandler(InvocationContext);

        // Act
        var result = await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None);

        // Assert
        PrintDataHandlerResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task EmailFolderDataHandler_ReturnsEmailFolders()
    {
        // Arrange
        var handler = new EmailFolderDataHandler(InvocationContext);

        // Act
        var result = await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None);

        // Assert
        PrintDataHandlerResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task FormFolderDataHandler_ReturnsFormFolders()
    {
        // Arrange
        var handler = new FormFolderDataHandler(InvocationContext);

        // Act
        var result = await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None);

        // Assert
        PrintDataHandlerResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task SnippetFolderDataHandler_ReturnsSnippetFolders()
    {
        // Arrange
        var handler = new SnippetFolderDataHandler(InvocationContext);

        // Act
        var result = await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None);

        // Assert
        PrintDataHandlerResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task EmailTemplateFolderDataHandler_ReturnsEmailTemplateFolders()
    {
        // Arrange
        var handler = new EmailTemplateFolderDataHandler(InvocationContext);

        // Act
        var result = await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None);

        // Assert
        PrintDataHandlerResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ContentFolderDataHandler_ReturnsFoldersForSpecificContentTypes()
    {
        // Arrange
        var types = new OptionalContentTypesIdentifier { ContentTypes = [ContentTypes.Form] };
        var handler = new ContentFolderDataHandler(InvocationContext, types);

        // Act
        var result = await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None);

        // Assert
        PrintDataHandlerResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ContentDataHandler_ReturnsContent()
    {
        // Arrange
        var input = new ContentTypeIdentifier { ContentType = ContentTypes.Form };
        var handler = new ContentDataHandler(InvocationContext, input);

        // Act
        var result = await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None);

        // Assert
        PrintDataHandlerResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task SegmentationDataHandler_ReturnsSegmentations()
    {
        // Arrange
        var handler = new SegmentationDataHandler(InvocationContext);

        // Act
        var result = await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None);

        // Assert
        PrintDataHandlerResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task SegmentBySegmentationDataHandler_ReturnsSegmentsForSegmentation()
    {
        // Arrange
        var segmentId = new SegmentationIdentifier { SegmentationId = "1002" };
        var handler = new SegmentBySegmentationDataHandler(InvocationContext, segmentId);

        // Act
        var result = await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None);

        // Assert
        PrintDataHandlerResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task FormFieldDataHandler_ReturnsFormFields()
    {
        // Arrange
        var formId = new FormIdentifier { FormId = "1005" };
        var handler = new FormFieldDataHandler(InvocationContext, formId);

        // Act
        var result = await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None);

        // Assert
        PrintDataHandlerResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task SnippetDataHandler_ReturnsSnippets()
    {
        // Arrange
        var handler = new SnippetDataHandler(InvocationContext);

        // Act
        var result = await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None);

        // Assert
        PrintDataHandlerResult(result);
        Assert.IsNotNull(result);
    }
}
