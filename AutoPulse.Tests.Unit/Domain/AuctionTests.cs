using AutoPulse.Domain.Entities;
using AutoPulse.Domain.Events;
using AutoPulse.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AutoPulse.Tests.Unit.Domain
{
    public class AuctionTests
    {
        private static Vehicle CreateSampleVehicle()
        {
            return Vehicle.Create(
                Guid.NewGuid(),
                "1HGCR2F83HA000000",
                "Honda",
                "Accord",
                2020,
                50000,
                "2020 Honda Accord",
                Money.CreateUSD(15000),
                Money.CreateUSD(250),
                "Sedan",
                "docs/vehicle-1.pdf"
            );
        }

        [Fact]
        public void Create_WithValidParameters_ShouldInitializeCorrectlyAndRaiseDomainEvent()
        {
            // Arrange
            var auctionId = Guid.NewGuid();
            var auctioneerId = Guid.NewGuid();
            var vehicle = CreateSampleVehicle();
            var startingPrice = Money.CreateUSD(15000);
            var endTime = DateTime.UtcNow.AddDays(7);

            // Act
            var auction = Auction.Create(auctionId, auctioneerId, vehicle, startingPrice, endTime);

            // Assert
            auction.Should().NotBeNull();
            auction.Id.Should().Be(auctionId);
            auction.AuctioneerId.Should().Be(auctioneerId);
            auction.Vehicle.Should().Be(vehicle);
            auction.StartingPrice.Should().Be(startingPrice);
            auction.CurrentPrice.Should().Be(startingPrice);
            auction.EndTime.Should().Be(endTime);
            auction.IsActive.Should().BeTrue();
            auction.Bids.Should().BeEmpty();

            auction.DomainEvents.Should().HaveCount(1);
            var domainEvent = auction.DomainEvents.First().Should().BeOfType<AuctionCreatedDomainEvent>().Subject;
            domainEvent.AuctionId.Should().Be(auctionId);
            domainEvent.Title.Should().Be("2020 Honda Accord");
            domainEvent.BasePrice.Should().Be(15000);
        }

        [Fact]
        public void Create_WithEndTimeInPast_ShouldThrowArgumentException()
        {
            // Arrange
            var auctionId = Guid.NewGuid();
            var auctioneerId = Guid.NewGuid();
            var vehicle = CreateSampleVehicle();
            var startingPrice = Money.CreateUSD(15000);
            var pastEndTime = DateTime.UtcNow.AddDays(-1);

            // Act
            Action act = () => Auction.Create(auctionId, auctioneerId, vehicle, startingPrice, pastEndTime);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*future*");
        }

        [Fact]
        public void PlaceBid_WithAmountLowerThanCurrentPrice_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var auction = Auction.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                CreateSampleVehicle(),
                Money.CreateUSD(10000),
                DateTime.UtcNow.AddDays(1)
            );
            var bidderId = Guid.NewGuid();
            var lowBid = Money.CreateUSD(5000);

            // Act
            Action act = () => auction.PlaceBid(bidderId, lowBid);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("The bid amount must be higher than the current price");
        }

        [Fact]
        public void PlaceBid_WithValidAmount_ShouldAddBidAndUpdateCurrentPrice()
        {
            // Arrange
            var auction = Auction.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                CreateSampleVehicle(),
                Money.CreateUSD(10000),
                DateTime.UtcNow.AddDays(1)
            );
            var bidderId = Guid.NewGuid();
            var validBidAmount = Money.CreateUSD(12000);

            // Act
            var bid = auction.PlaceBid(bidderId, validBidAmount);

            // Assert
            bid.Should().NotBeNull();
            bid.BidderId.Should().Be(bidderId);
            bid.Amount.Should().Be(validBidAmount);
            auction.CurrentPrice.Should().Be(validBidAmount);
            auction.Bids.Should().HaveCount(1);
        }

        [Fact]
        public void Close_WhenAuctionIsActive_ShouldDeactivateAndSetWinner()
        {
            // Arrange
            var auction = Auction.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                CreateSampleVehicle(),
                Money.CreateUSD(10000),
                DateTime.UtcNow.AddDays(1)
            );
            var winningBidderId = Guid.NewGuid();
            auction.PlaceBid(winningBidderId, Money.CreateUSD(15000));

            // Act
            auction.Close();

            // Assert
            auction.IsActive.Should().BeFalse();
            auction.WinnerId.Should().Be(winningBidderId);
        }

        [Fact]
        public void Close_WhenAlreadyClosed_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var auction = Auction.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                CreateSampleVehicle(),
                Money.CreateUSD(10000),
                DateTime.UtcNow.AddDays(1)
            );
            auction.Close();

            // Act
            Action act = () => auction.Close();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Auction is already closed");
        }
    }
}
