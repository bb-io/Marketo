using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;
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


}
