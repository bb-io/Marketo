using Apps.Marketo.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Newtonsoft.Json.Linq;

namespace Apps.Marketo.Webhooks;

[WebhookList]
public class WebhookList
{
    #region ProjectWebhooks

    [Webhook("On event triggered", Description = "On event triggered")]
    public async Task<WebhookResponse<EventPayload>> EventTriggered(WebhookRequest webhookRequest,
        [WebhookParameter] FieldsToGet input)
    {
        var payload = webhookRequest.Body.ToString();
        ArgumentException.ThrowIfNullOrEmpty(payload);

        var data = JObject.Parse(payload);
        return new()
        {
            HttpResponseMessage = null,
            Result = new()
            {
                Field1 = input.Field1 != null ? data.SelectToken(input.Field1)?.ToString() : null,
                Field2 = input.Field2 != null ? data.SelectToken(input.Field2)?.ToString() : null,
                Field3 = input.Field3 != null ? data.SelectToken(input.Field3)?.ToString() : null,
                Field4 = input.Field4 != null ? data.SelectToken(input.Field4)?.ToString() : null,
                Field5 = input.Field5 != null ? data.SelectToken(input.Field5)?.ToString() : null,
            }
        };
    }

    #endregion
}