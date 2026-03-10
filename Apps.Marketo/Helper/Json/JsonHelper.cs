using Newtonsoft.Json.Linq;

namespace Apps.Marketo.Helper.Json;

public static class JsonHelper
{
    public static bool IsJsonObject(string content)
    {
        bool isObject = false;
        try
        {
            JObject.Parse(content);
            isObject = true;
        }
        catch { }
        return isObject;
    }
}
