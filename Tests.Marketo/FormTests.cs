using Apps.Marketo.Actions;
using Apps.Marketo.Models.Forms.Requests;
using Apps.Marketo.Models.Identifiers;
using Apps.Marketo.Models.Identifiers.Optional;
using Blackbird.Applications.Sdk.Common.Files;
using Tests.Marketo.Base;

namespace Tests.Marketo;

[TestClass]
public class FormTests : TestBase
{
    [TestMethod]
    public async Task ListRecentlyCreatedOrUpdatedForms_ReturnsForms()
    {
		// Arrange
		var action = new FormActions(InvocationContext, FileManager);
		var input = new SearchFormsRequest
		{
			CreatedAfter = new DateTime(2024, 08, 26, 15, 20, 0, DateTimeKind.Utc),
            CreatedBefore = new DateTime(2024, 08, 26, 15, 25, 0, DateTimeKind.Utc),
        };

		// Act
		var result = await action.ListRecentlyCreatedOrUpdatedForms(input);

        // Assert
        Console.WriteLine($"Count: {result.Forms.Count}");
		PrintJsonResult(result);
		Assert.IsNotNull(result);
	}

	[TestMethod]
	public async Task DownloadForm_IsSuccess()
	{
		// Arrange
		var action = new FormActions(InvocationContext, FileManager);
		var formId = new FormIdentifier { FormId = "1005" };
		var input = new DownloadFormRequest
		{
			//IgnoreVisibilityRules = true
		};

		// Act
		var result = await action.GetFormAsHtml(formId, input);

        // Assert
        Console.WriteLine(result.Content.Name);
		Assert.IsNotNull(result);
	}

	[TestMethod]
	public async Task UploadFormContent_IsSuccess()
	{
        // Arrange
        var action = new FormActions(InvocationContext, FileManager);
        var formId = new OptionalFormIdenfitier { /*FormId = "1005"*/ };
		var input = new UploadFormRequest
		{
			File = new FileReference { Name = "test.html" }
		};

		// Act
		await action.UploadFormContent(formId, input);
    }
}
