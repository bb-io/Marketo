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
            NamePatterns = ["*sky*"]
        };

        // Act
        var result = await action.ListEmails(input);

        // Assert
        Console.WriteLine($"Count: {result.Emails.Count}");
        PrintJsonResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task DownloadEmail_IsSuccess()
    {
        // Arrange
        var actions = new EmailActions(InvocationContext, FileManager);
        var emailId = new EmailIdentifier { EmailId = "1063" };
        var input = new DownloadEmailRequest
        {
            IncludeImages = true,
            SegmentationId = "1003",
            Segment = "Dutch",
        };

        // Act
        var result = await actions.GetEmailAsHtml(emailId, input);

        // Assert
        Console.WriteLine(result.Content.Name);
        Assert.IsNotNull(result);
    }
}
