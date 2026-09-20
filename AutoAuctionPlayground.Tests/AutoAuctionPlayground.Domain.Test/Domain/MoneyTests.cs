using AutoAuctionPlayground.Domain.Enums;
using AutoAuctionPlayground.Domain.ValueObjects;

namespace AutoAuctionPlayground.Tests.Domain
{
    public class MoneyTests
    {
        [Fact]
        public void Of_DefaultsToEur()
        {
            Assert.Equal(Currency.EUR, Money.Of(10).Currency);
        }

        [Fact]
        public void Of_WithSameAmountAndCurrency_AreEqual()
        {
            Assert.Equal(Money.Of(10), Money.Of(10, Currency.EUR));
            Assert.NotEqual(Money.Of(10), Money.Of(10, Currency.USD));
        }

        [Fact]
        public void Of_WithNegativeAmount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Money.Of(-1));
        }

        [Fact]
        public void Of_WithUndefinedCurrency_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Money.Of(10, (Currency)999));
        }

        [Fact]
        public void Comparison_SameCurrency_ComparesAmounts()
        {
            Assert.True(Money.Of(11) > Money.Of(10));
            Assert.True(Money.Of(10) <= Money.Of(10));
        }

        [Fact]
        public void Comparison_DifferentCurrency_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => Money.Of(11) > Money.Of(10, Currency.USD));
        }
    }
}
