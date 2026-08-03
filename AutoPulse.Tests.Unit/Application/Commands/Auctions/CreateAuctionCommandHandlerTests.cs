using AutoPulse.Application.Application.Auctions.Commands.CreateAuction;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace AutoPulse.Tests.Unit.Application.Commands.Auctions
{
    public class CreateAuctionCommandHandlerTests
    {
        private readonly IRepository<Auction> _auctionRepository;
        private readonly IAutoPulseDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly IMediator _mediator;
        private readonly CreateAuctionCommandHandler _handler;

        public CreateAuctionCommandHandlerTests()
        {
            _auctionRepository = Substitute.For<IRepository<Auction>>();
            _context = Substitute.For<IAutoPulseDbContext>();
            _cacheService = Substitute.For<ICacheService>();
            _mediator = Substitute.For<IMediator>();

            _handler = new CreateAuctionCommandHandler(
                _auctionRepository,
                _context,
                _cacheService,
                _mediator
            );
        }

        [Fact]
        public async Task Handle_WithValidRequest_ShouldCreateAuctionPublishEventsAndInvalidateCache()
        {
            // Arrange
            var command = new CreateAuctionCommand(
                "1HGCR2F83HA000000",
                "Honda",
                "Accord",
                2021,
                15000,
                "2021 Honda Accord EX-L",
                20000,
                500,
                "Sedan",
                "docs/honda.pdf",
                Guid.NewGuid(),
                20000,
                "USD",
                DateTime.UtcNow.AddDays(5),
                Guid.NewGuid()
            );

            // Act
            var auctionId = await _handler.Handle(command, CancellationToken.None);

            // Assert
            auctionId.Should().NotBeEmpty();
            _auctionRepository.Received(1).Add(Arg.Is<Auction>(a => a.Id == auctionId && a.AuctioneerId == command.AuctioneerId));
            await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            await _mediator.Received(1).Publish(Arg.Any<INotification>(), Arg.Any<CancellationToken>());
            await _cacheService.Received(1).RemoveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WithPastEndTime_ShouldThrowArgumentException()
        {
            // Arrange
            var command = new CreateAuctionCommand(
                "1HGCR2F83HA000000",
                "Honda",
                "Accord",
                2021,
                15000,
                "2021 Honda Accord EX-L",
                20000,
                500,
                "Sedan",
                "docs/honda.pdf",
                Guid.NewGuid(),
                20000,
                "USD",
                DateTime.UtcNow.AddDays(-1),
                Guid.NewGuid()
            );

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*future*");
        }
    }
}
