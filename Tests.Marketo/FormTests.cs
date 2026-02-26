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
}
