using System.Collections.ObjectModel;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Application.Application.Vehicles.Commands.CreateVehicleSpecs;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities;
using AutoPulse.Domain.Entities.NoSql;
using AutoPulse.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AutoPulse.Tests.Unit.Application.Commands.Vehicles
{
    public class CreateVehicleSpecsCommandHandlerTests
    {
        private readonly INoSqlRepository<VehicleSpecificationDocument> _noSqlRepository;
        private readonly IRepository<Auction> _auctionRepository;
        private readonly ICacheService _cacheService;
        private readonly CreateVehicleSpecsCommandHandler _handler;

        public CreateVehicleSpecsCommandHandlerTests()
        {
            _noSqlRepository = Substitute.For<INoSqlRepository<VehicleSpecificationDocument>>();
            _auctionRepository = Substitute.For<IRepository<Auction>>();
            _cacheService = Substitute.For<ICacheService>();

            _handler = new CreateVehicleSpecsCommandHandler(_noSqlRepository, _auctionRepository, _cacheService);
        }

        [Fact]
        public async Task Handle_WithInvalidAuctionIdFormat_ShouldThrowArgumentException()
        {
            // Arrange
            var dict = new Dictionary<string, object> { { "Color", "Red" } };
            var command = new CreateVehicleSpecsCommand(
                "not-a-guid",
                new ReadOnlyDictionary<string, object>(dict),
                Guid.NewGuid()
            );

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Invalid AuctionId");
        }

        [Fact]
        public async Task Handle_WhenAuctionNotFound_ShouldThrowArgumentException()
        {
            // Arrange
            var auctionId = Guid.NewGuid().ToString();
            var dict = new Dictionary<string, object> { { "Color", "Red" } };
            var command = new CreateVehicleSpecsCommand(
                auctionId,
                new ReadOnlyDictionary<string, object>(dict),
                Guid.NewGuid()
            );

            _auctionRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((Auction?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"*{auctionId}*was not found*");
        }

        [Fact]
        public async Task Handle_WithValidRequest_ShouldCreateDocumentRemoveCacheAndReturnId()
        {
            // Arrange
            var auctionIdGuid = Guid.NewGuid();
            var auctionId = auctionIdGuid.ToString();
            var dict = new Dictionary<string, object> { { "Engine", "V6" }, { "Color", "Blue" } };
            var command = new CreateVehicleSpecsCommand(
                auctionId,
                new ReadOnlyDictionary<string, object>(dict),
                Guid.NewGuid()
            );

            var vehicle = Vehicle.Create(
                Guid.NewGuid(),
                "1HGCR2F83HA000000",
                "Ford",
                "Mustang",
                2022,
                5000,
                "2022 Ford Mustang",
                Money.CreateUSD(30000),
                Money.CreateUSD(1000),
                "Sports",
                "key.pdf"
            );

            var auction = Auction.Create(auctionIdGuid, Guid.NewGuid(), vehicle, Money.CreateUSD(30000), DateTime.UtcNow.AddDays(5));

            _auctionRepository.GetByIdAsync(auctionIdGuid, Arg.Any<CancellationToken>()).Returns(auction);
            _noSqlRepository.GetByIdAsync(auctionId, Arg.Any<CancellationToken>()).Returns((VehicleSpecificationDocument?)null);
            _noSqlRepository.AddAsync(Arg.Any<VehicleSpecificationDocument>(), Arg.Any<CancellationToken>()).Returns(auctionId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(auctionId);
            await _noSqlRepository.Received(1).AddAsync(Arg.Is<VehicleSpecificationDocument>(doc => doc.Id == auctionId), Arg.Any<CancellationToken>());
            await _cacheService.Received(1).RemoveAsync($"vehicles:specs:{auctionId}", Arg.Any<CancellationToken>());
        }
    }
}
