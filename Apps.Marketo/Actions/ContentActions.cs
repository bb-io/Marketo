using Apps.Marketo.Invocables;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Marketo.Actions;

[ActionList("Content")]
public class ContentActions(InvocationContext invocationContext) : MarketoInvocable(invocationContext)
{
}
