using Apps.Marketo.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Newtonsoft.Json.Linq;

namespace Apps.Marketo.Webhooks
{
    [WebhookList]
    public class WebhookList 
    {
        #region ProjectWebhooks

        [Webhook("On event triggered", Description = "On event triggered")]
        public async Task<WebhookResponse<EventPayload>> EventTriggered(WebhookRequest webhookRequest, [WebhookParameter] FieldsToGet input)
        {
            var data = JObject.Parse(webhookRequest.Body.ToString());
            if(data is null)
            {
                throw new InvalidCastException(nameof(webhookRequest.Body));
            }
            return new WebhookResponse<EventPayload>
            {
                HttpResponseMessage = null,
                Result = new EventPayload()
                {
                    Field1 = input.Field1 != null ? data.SelectToken(input.Field1).ToString() : null,
                    Field2 = input.Field2 != null ? data.SelectToken(input.Field2).ToString() : null,
                    Field3 = input.Field3 != null ? data.SelectToken(input.Field3).ToString() : null,
                    Field4 = input.Field4 != null ? data.SelectToken(input.Field4).ToString() : null,
                    Field5 = input.Field5 != null ? data.SelectToken(input.Field5).ToString() : null,
                }
            };
        }

        #endregion
    }
}
