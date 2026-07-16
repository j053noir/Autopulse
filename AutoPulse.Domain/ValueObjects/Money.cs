namespace AutoPulse.Domain.ValueObjects
{
    public record Money
    {
        public decimal Amount { get; }
        public string CurrencyCode { get; }

        private Money(decimal amount, string currencyCode)
        {
            if (amount < 0) throw new ArgumentException("Amount cannot be negative.", nameof(amount));

            Amount = amount;
            CurrencyCode = currencyCode;
        }

        public static Money Create(decimal amount, string currencyCode)
        {
            switch (currencyCode)
            {
                case "USD":
                    return CreateUSD(amount);
                case "COP":
                    return CreateCOP(amount);
                case "CAD":
                    return CreateCAD(amount);
                default:
                    throw new ArgumentException("Invalid currency code", nameof(currencyCode));
            }
        }

        public static Money CreateUSD(decimal amount) => new(amount, "USD");
        public static Money CreateCOP(decimal amount) => new(amount, "COP");
        public static Money CreateCAD(decimal amount) => new(amount, "CAD");

        public static bool operator >(Money a, Money b)
        {
            if (a.CurrencyCode != b.CurrencyCode) throw new ArgumentException("Cannot compare Money with different currency code");

            return a.Amount > b.Amount;
        }

        public static bool operator <(Money a, Money b)
        {

            if (a.CurrencyCode != b.CurrencyCode) throw new ArgumentException("Cannot compare Money with different currency code");

            return a.Amount < b.Amount;
        }

        public static bool operator >=(Money a, Money b)
        {
            if (a.CurrencyCode != b.CurrencyCode) throw new ArgumentException("Cannot compare Money with different currency code");

            return a.Amount >= b.Amount;
        }

        public static bool operator <=(Money a, Money b)
        {

            if (a.CurrencyCode != b.CurrencyCode) throw new ArgumentException("Cannot compare Money with different currency code");

            return a.Amount <= b.Amount;
        }
    }
}
