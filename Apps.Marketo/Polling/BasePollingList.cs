using Apps.Marketo.Invocables;
using Apps.Marketo.Polling.Models.Memories;
using Blackbird.Applications.Sdk.Common.Polling;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Marketo.Polling;

public class BasePollingList(InvocationContext invocationContext) : MarketoInvocable(invocationContext)
{
    protected static async Task<PollingEventResponse<DateMemory, T>> HandlePolling<T>(
        PollingEventRequest<DateMemory> request, 
        Func<DateMemory, Task<T>> func, 
        Func<T, bool> isResultValid)
    {
        if (request.Memory is null)
        {
            return new()
            {
                FlyBird = false,
                Memory = new() { LastInteractionDate = DateTime.UtcNow }
            };
        }

        var result = await func(request.Memory);
        if (!isResultValid(result))
        {
            return new()
            {
                FlyBird = false,
                Memory = new() { LastInteractionDate = DateTime.UtcNow }
            };
        }

        return new()
        {
            FlyBird = true,
            Result = result,
            Memory = new() { LastInteractionDate = DateTime.UtcNow }
        };
    }
}
