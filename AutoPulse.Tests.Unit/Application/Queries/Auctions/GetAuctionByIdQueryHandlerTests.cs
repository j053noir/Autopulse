using AutoPulse.Application.Application.Auctions.Queries.Common.Dto;
using AutoPulse.Application.Application.Auctions.Queries.GetAuctionById;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Common.Specification;
using AutoPulse.Domain.Entities;
using AutoPulse.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AutoPulse.Tests.Unit.Application.Queries.Auctions
{
    public class GetAuctionByIdQueryHandlerTests
    {
        private readonly IRepository<Auction> _auctionRepository;
        private readonly ICacheService _cacheService;
        private readonly GetAuctionByIdQueryHandler _handler;

        public GetAuctionByIdQueryHandlerTests()
        {
            _auctionRepository = Substitute.For<IRepository<Auction>>();
            _cacheService = Substitute.For<ICacheService>();
            _handler = new GetAuctionByIdQueryHandler(_auctionRepository, _cacheService);
        }

        [Fact]
        public async Task Handle_WhenCached_ShouldReturnCachedAuctionWithoutDatabaseQuery()
        {
            // Arrange
            var auctionId = Guid.NewGuid();
            var query = new GetAuctionByIdQuery(auctionId);
            var cachedDto = new AuctionDto(
                auctionId,
                null,
                null,
                15000,
                "USD",
                15000,
                "USD",
                DateTime.UtcNow.AddDays(1),
                true,
                null
            );

            _cacheService.GetAsync<AuctionDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(cachedDto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(auctionId);
            await _auctionRepository.DidNotReceiveWithAnyArgs().GetBySpecAsync(Arg.Any<ISpecification<Auction>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WhenNotCachedAndEntityExists_ShouldFetchFromDbAndSetCache()
        {
            // Arrange
            var auctionId = Guid.NewGuid();
            var query = new GetAuctionByIdQuery(auctionId);

            var vehicle = Vehicle.Create(
                Guid.NewGuid(),
                "1HGCR2F83HA000000",
                "Honda",
                "Civic",
                2022,
                10000,
                "2022 Honda Civic",
                Money.CreateUSD(12000),
                Money.CreateUSD(300),
                "Sedan",
                "docs/key.pdf"
            );

            var auction = Auction.Create(
                auctionId,
                Guid.NewGuid(),
                vehicle,
                Money.CreateUSD(12000),
                DateTime.UtcNow.AddDays(3)
            );

            _cacheService.GetAsync<AuctionDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns((AuctionDto?)null);

            _auctionRepository.GetBySpecAsync(Arg.Any<ISpecification<Auction>>(), Arg.Any<CancellationToken>())
                .Returns(auction);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(auctionId);
            await _cacheService.Received(1).SetAsync(Arg.Any<string>(), Arg.Is<AuctionDto>(d => d.Id == auctionId), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WhenEntityNotFoundInDb_ShouldCacheNullPlaceholderAndReturnNull()
        {
            // Arrange
            var auctionId = Guid.NewGuid();
            var query = new GetAuctionByIdQuery(auctionId);

            _cacheService.GetAsync<AuctionDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns((AuctionDto?)null);

            _auctionRepository.GetBySpecAsync(Arg.Any<ISpecification<Auction>>(), Arg.Any<CancellationToken>())
                .Returns((Auction?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
            await _cacheService.Received(1).SetAsync(Arg.Any<string>(), Arg.Any<AuctionDto>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
        }
    }
}
