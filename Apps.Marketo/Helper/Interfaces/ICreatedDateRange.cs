namespace Apps.Marketo.Helper.Interfaces;

public interface ICreatedDateRange : IDateRange
{
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
}
