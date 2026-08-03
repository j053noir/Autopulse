using AutoPulse.Application.Application.Auctions.Commands.CreateAuctionBid;
using AutoPulse.Application.Application.Authentication.Common.Dto;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities;
using AutoPulse.Domain.Interfaces;
using AutoPulse.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace AutoPulse.Tests.Unit.Application.Commands.Auctions
{
    public class CreateAuctionBidCommandHandlerTests
    {
        private readonly IRepository<Auction> _auctionRepository;
        private readonly IAutoPulseDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly IRepository<User> _userRepository;
        private readonly IUserProfileService _userProfileService;
        private readonly IAuctionEventDispatcher _dispatcher;
        private readonly CreateAuctionBidCommandHandler _handler;

        public CreateAuctionBidCommandHandlerTests()
        {
            _auctionRepository = Substitute.For<IRepository<Auction>>();
            _context = Substitute.For<IAutoPulseDbContext>();
            _cacheService = Substitute.For<ICacheService>();
            _userRepository = Substitute.For<IRepository<User>>();
            _userProfileService = Substitute.For<IUserProfileService>();
            _dispatcher = Substitute.For<IAuctionEventDispatcher>();

            _handler = new CreateAuctionBidCommandHandler(
                _auctionRepository,
                _context,
                _cacheService,
                _userRepository,
                _userProfileService,
                _dispatcher
            );
        }

        private static Auction CreateSampleActiveAuction(Guid auctionId)
        {
            var vehicle = Vehicle.Create(
                Guid.NewGuid(),
                "1HGCR2F83HA000000",
                "Toyota",
                "Camry",
                2021,
                30000,
                "2021 Toyota Camry",
                Money.CreateUSD(10000),
                Money.CreateUSD(500),
                "Sedan",
                "docs/key.pdf"
            );

            return Auction.Create(
                auctionId,
                Guid.NewGuid(),
                vehicle,
                Money.CreateUSD(10000),
                DateTime.UtcNow.AddDays(2)
            );
        }

        [Fact]
        public async Task Handle_WithValidBid_ShouldPlaceBidSaveContextAndDispatchEvent()
        {
            // Arrange
            var auctionId = Guid.NewGuid();
            var bidderId = Guid.NewGuid();
            var command = new CreateAuctionBidCommand(
                auctionId,
                bidderId,
                15000,
                "USD",
                Guid.NewGuid()
            );

            var auction = CreateSampleActiveAuction(auctionId);
            _auctionRepository.GetByIdAsync(auctionId).Returns(auction);

            var profile = new UserProfileDto(bidderId, "JohnDoe", "john@example.com", true, null);
            _userProfileService.GetProfileAsync(bidderId, Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(profile);

            // Act
            var resultBidId = await _handler.Handle(command, CancellationToken.None);

            // Assert
            resultBidId.Should().Be(Guid.Empty);
            auction.CurrentPrice!.Amount.Should().Be(15000);

            await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            await _cacheService.Received(2).RemoveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
            await _dispatcher.Received(1).PublishBidPlaceAsync(auctionId.ToString(), 15000, "JohnDoe");
        }

        [Fact]
        public async Task Handle_WhenAuctionNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var command = new CreateAuctionBidCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                12000,
                "USD",
                Guid.NewGuid()
            );

            _auctionRepository.GetByIdAsync(command.AuctionId).Returns((Auction?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"*{command.AuctionId}*");
        }

        [Fact]
        public async Task Handle_WhenBidAmountIsLowerThanCurrentPrice_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var auctionId = Guid.NewGuid();
            var command = new CreateAuctionBidCommand(
                auctionId,
                Guid.NewGuid(),
                5000, // Lower than starting price 10000
                "USD",
                Guid.NewGuid()
            );

            var auction = CreateSampleActiveAuction(auctionId);
            _auctionRepository.GetByIdAsync(auctionId).Returns(auction);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("The bid amount must be higher than the current price");
        }

        [Fact]
        public async Task Handle_WhenConcurrencyExceptionOccurs_ShouldThrowCustomException()
        {
            // Arrange
            var auctionId = Guid.NewGuid();
            var command = new CreateAuctionBidCommand(
                auctionId,
                Guid.NewGuid(),
                15000,
                "USD",
                Guid.NewGuid()
            );

            var auction = CreateSampleActiveAuction(auctionId);
            _auctionRepository.GetByIdAsync(auctionId).Returns(auction);

            _context.SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns<int>(_ => throw new DbUpdateConcurrencyException());

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*updated by another user*");
        }
    }
}
