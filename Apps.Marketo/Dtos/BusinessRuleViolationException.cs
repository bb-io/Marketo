namespace Apps.Marketo.Dtos
{
    public class BusinessRuleViolationException : ArgumentException
    {
        public int ErrorCode { get; set; }
        public BusinessRuleViolationException(int code, string message) : base(message)
        {
            ErrorCode = code;
        }
    }
}
