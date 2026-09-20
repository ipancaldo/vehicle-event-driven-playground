using AutoAuctionPlayground.Domain.Enums;

namespace AutoAuctionPlayground.Domain.ValueObjects
{
    // Every amount in the system carries its currency so it is never implicit. The system currently
    // runs single-currency: entry points default to DefaultCurrency, and comparing two different
    // currencies is a programming error, not a conversion.
    public sealed record Money
    {
        public const Currency DefaultCurrency = Currency.EUR;

        public decimal Amount { get; private set; }
        public Currency Currency { get; private set; } = DefaultCurrency;

        private Money() { }
        private Money(decimal amount, Currency currency)
        {
            Amount = amount;
            Currency = currency;
        }

        public static Money Of(decimal amount, Currency currency = DefaultCurrency)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(amount);
            if (!Enum.IsDefined(currency))
                throw new ArgumentOutOfRangeException(nameof(currency), "Unknown currency.");

            return new Money(amount, currency);
        }

        public static bool operator >(Money left, Money right) => left.Amount > left.SameCurrency(right).Amount;
        public static bool operator <(Money left, Money right) => left.Amount < left.SameCurrency(right).Amount;
        public static bool operator >=(Money left, Money right) => left.Amount >= left.SameCurrency(right).Amount;
        public static bool operator <=(Money left, Money right) => left.Amount <= left.SameCurrency(right).Amount;

        public override string ToString() => $"{Amount:N2} {Currency}";

        private Money SameCurrency(Money other)
        {
            if (other.Currency != Currency)
                throw new InvalidOperationException($"Cannot compare {Currency} with {other.Currency}.");
            return other;
        }
    }
}
