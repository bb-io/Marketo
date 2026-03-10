using Apps.Marketo.Helper.Interfaces;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Marketo.Helper.Validator;

public static class ValidatorHelper
{
    public static void ValidateDates(this IDateRange input)
    {
        List<string> errors = [];

        if (input is ICreatedDateRange c &&
            c.CreatedAfter.HasValue && c.CreatedBefore.HasValue &&
            c.CreatedAfter > c.CreatedBefore)
        {
            errors.Add("'Created after' date cannot be later than 'Created before' date");
        }

        if (input is IUpdatedDateRange u &&
            u.UpdatedAfter.HasValue && u.UpdatedBefore.HasValue &&
            u.UpdatedAfter > u.UpdatedBefore)
        {
            errors.Add("'Updated after' date cannot be later than 'Updated before' date");
        }

        if (errors.Count > 0)
            throw new PluginMisconfigurationException(string.Join(". ", errors));
    }
}
