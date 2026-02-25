namespace Apps.Marketo.Helper.Filter.Interfaces;

public interface INamePatternInput
{
    public List<string>? NamePatterns { get; set; }
    public bool? ExcludeMatched { get; set; }
}
