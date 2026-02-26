using Tests.Marketo.Base;
using Apps.Marketo.Actions;
using Apps.Marketo.Models.Identifiers;
using Apps.Marketo.Models.Emails.Requests;

namespace Tests.Marketo;

[TestClass]
public class EmailTests : TestBase
{
    [TestMethod]
    public async Task GetEmailInfo_ReturnsEmailInfo()
    {
        // Arrange
        var action = new EmailActions(InvocationContext, FileManager);
        var emailId = new EmailIdentifier { EmailId = "1008" };

        // Act
        var result = await action.GetEmailInfo(emailId);

        // Assert
        PrintJsonResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ListEmails_ReturnsEmails()
    {
        // Arrange
        var action = new EmailActions(InvocationContext, FileManager);
        var input = new SearchEmailsRequest
        {
            NamePatterns = ["Test3423"]
        };

        // Act
        var result = await action.ListEmails(input);

        // Assert
        Console.WriteLine($"Count: {result.Emails.Count}");
        PrintJsonResult(result);
        Assert.IsNotNull(result);
    }
}
