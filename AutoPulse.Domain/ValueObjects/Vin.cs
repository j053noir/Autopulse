using System.Text.RegularExpressions;

namespace AutoPulse.Domain.ValueObjects
{
    public record Vin
    {
        private static readonly Regex VinRegex = new(@"^[A-HJ-NPR-Z0-9]{17}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string Value { get; }

        private Vin(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("VIN cannot be empty.", nameof(value));

            value = value.Trim().ToUpper();

            if (!VinRegex.IsMatch(value))
                throw new ArgumentException("Invalid VIN format. Must be exactly 17 alfanumeric characters (excluding I, O, Q).", nameof(value));

            Value = value;
        }

        public static Vin Create(string value) => new(value);

        public override string ToString() => Value;

        public static implicit operator string(Vin vin) => vin.Value;
    }
}
