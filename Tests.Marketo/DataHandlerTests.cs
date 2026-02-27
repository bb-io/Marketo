using Apps.Marketo.Constants;
using Apps.Marketo.DataSourceHandlers;
using Apps.Marketo.DataSourceHandlers.FolderDataHandlers;
using Apps.Marketo.Models.Identifiers.Optional;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Newtonsoft.Json;
using Tests.Marketo.Base;

namespace Tests.Marketo;

[TestClass]
public class DataHandlerTests : TestBase
{
    [TestMethod]
    public async Task EmailDataHandlert_IsSuccess()
    {
        var handler = new EmailDataHandler(InvocationContext);

        var result = await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None);
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);

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
    public async Task LandingPageHandler_IsSuccess()
    {
        var handler = new LandingPageHandler(InvocationContext);

        var result = await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None);
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);

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
        var types = new OptionalContentTypesIdentifier { ContentTypes = [ContentTypes.Form, ContentTypes.Email] };
        var handler = new ContentFolderDataHandler(InvocationContext, types);

        // Act
        var result = await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None);

        // Assert
        PrintDataHandlerResult(result);
        Assert.IsNotNull(result);
    }
}
