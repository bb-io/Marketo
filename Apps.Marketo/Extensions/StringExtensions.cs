namespace Apps.Marketo.Extensions;

public static class StringExtensions
{
    public static string ToHtmlFileName(this string entityName)
    {
        string fileName = $"{entityName}.html";
        return fileName.Replace(" ", "_");
    }
}
