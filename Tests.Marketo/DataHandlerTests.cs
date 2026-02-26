using Apps.Marketo.DataSourceHandlers;
using Apps.Marketo.DataSourceHandlers.FolderDataHandlers;
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
        PrintJsonResult(result);
        Assert.IsNotNull(result);
    }
}
