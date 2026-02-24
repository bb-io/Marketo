using Apps.Marketo.Actions;
using Apps.Marketo.Models.Identifiers;
using Tests.Marketo.Base;

namespace Tests.Marketo
{
    [TestClass]
    public class EmailTests : TestBase
    {
        [TestMethod]
        public async Task GetEmailInfo_IsSuccess()
        {
            var action = new EmailActions(InvocationContext, FileManager);
            var result = await action.GetEmailInfo(new EmailIdentifier { EmailId = "1057" });
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
            Console.WriteLine(json);
            Assert.IsNotNull(result);
        }
    }
}
