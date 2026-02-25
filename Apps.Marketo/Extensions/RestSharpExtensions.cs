using RestSharp;
using System.Globalization;

namespace Apps.Marketo.Extensions;

public static class RestSharpExtensions
{
    public static RestRequest AddQueryParameterIfNotNull(this RestRequest request, string paramName, string? paramValue)
    {
        if (!string.IsNullOrWhiteSpace(paramValue))
            request.AddQueryParameter(paramName, paramValue);
        return request;
    }

    public static RestRequest AddQueryParameterIfNotNull(this RestRequest request, string paramName, DateTime? paramValue)
    {
        if (paramValue != null)
        {
            string dateTime = paramValue.Value.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            request.AddQueryParameter(paramName, dateTime);
        }
        return request;
    }

    public static RestRequest AddParameterIfNotNull(this RestRequest request, string paramName, string? paramValue)
    {
        if (!string.IsNullOrWhiteSpace(paramValue))
            request.AddParameter(paramName, paramValue);
        return request;
    }
}
