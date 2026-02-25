using Apps.Marketo.Actions;
using Apps.Marketo.Models.Forms.Requests;
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

		};

		// Act
		var result = await action.ListRecentlyCreatedOrUpdatedForms(input);

        // Assert
        Console.WriteLine($"Count: {result.Forms.Count}");
		PrintJsonResult(result);
		Assert.IsNotNull(result);
	}
}
