namespace Apps.Marketo.Models.Entities.Form;

public class ThankYouItem
{
    public string FollowupType { get; set; }

    public bool Default { get; set; }

    public string? SubjectField { get; set; }

    public string? Operator { get; set; }

    public IEnumerable<string>? Values { get; set; }
}
