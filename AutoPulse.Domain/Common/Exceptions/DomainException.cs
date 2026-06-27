namespace AutoPulse.Domain.Common.Exceptions
{
    public abstract class DomainException : Exception
    {
        public string Title { get; }
        public int StatusCode { get; }

        protected DomainException(string title, string message, int statuCode): base(message)
        {
            Title = title;
            StatusCode = statuCode;
        }
    }
}
