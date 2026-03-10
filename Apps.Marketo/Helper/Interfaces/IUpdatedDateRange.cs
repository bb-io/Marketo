namespace Apps.Marketo.Helper.Interfaces;

public interface IUpdatedDateRange : IDateRange
{
    public DateTime? UpdatedAfter { get; set; }
    public DateTime? UpdatedBefore { get; set; }
}
