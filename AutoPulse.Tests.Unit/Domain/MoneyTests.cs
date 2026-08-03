using AutoPulse.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AutoPulse.Tests.Unit.Domain
{
    public class MoneyTests
    {
        [Theory]
        [InlineData("USD")]
        [InlineData("COP")]
        [InlineData("CAD")]
        public void Create_WithValidCurrency_ShouldCreateInstance(string currencyCode)
        {
            // Arrange & Act
            var money = Money.Create(500, currencyCode);

            // Assert
            money.Should().NotBeNull();
            money.Amount.Should().Be(500);
            money.CurrencyCode.Should().Be(currencyCode);
        }

        [Fact]
        public void Create_WithNegativeAmount_ShouldThrowArgumentException()
        {
            // Act
            Action act = () => Money.CreateUSD(-100);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*negative*");
        }

        [Fact]
        public void Create_WithInvalidCurrency_ShouldThrowArgumentException()
        {
            // Act
            Action act = () => Money.Create(100, "EUR");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Invalid currency code*");
        }

        [Fact]
        public void GreaterThanOperator_WithHigherAmountSameCurrency_ShouldReturnTrue()
        {
            // Arrange
            var m1 = Money.CreateUSD(200);
            var m2 = Money.CreateUSD(100);

            // Act & Assert
            (m1 > m2).Should().BeTrue();
        }

        [Fact]
        public void ComparisonOperator_WithDifferentCurrencies_ShouldThrowArgumentException()
        {
            // Arrange
            var usd = Money.CreateUSD(100);
            var cop = Money.CreateCOP(100);

            // Act
            Action act = () => _ = usd > cop;

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*different currency code*");
        }
    }
}
