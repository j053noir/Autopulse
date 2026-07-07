using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities;
using AutoPulse.Domain.Entities.NoSql;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoPulse.Application.Application.Vehicles.Commands.CreateVehicleSpecs
{
    public class CreateVehicleSpecsCommandHandler : IRequestHandler<CreateVehicleSpecsCommand, string>
    {
        private readonly INoSqlRepository<VehicleSpecificationDocument> _vehicleRepository;
        private readonly IRepository<Auction> _auctionRepository;
        private readonly ICacheService _cacheService;

        public CreateVehicleSpecsCommandHandler(
            INoSqlRepository<VehicleSpecificationDocument> repository, 
            IRepository<Auction> auctionRepository,
            ICacheService cacheService)
        {
            _vehicleRepository = repository;
            _auctionRepository = auctionRepository;
            _cacheService = cacheService;
        }

        public async Task<string> Handle(CreateVehicleSpecsCommand request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(request.AuctionId, out Guid auctionId)) throw new ArgumentException("Invalid AuctionId");
            var auction = await _auctionRepository.GetByIdAsync(auctionId, cancellationToken);

            if (auction is null) throw new ArgumentException($"Auction with Id {request.AuctionId} was not found");

            var doc = await _vehicleRepository.GetByIdAsync(request.AuctionId, cancellationToken);
            if (doc is not null) throw new InvalidOperationException($"There's already a document created for auction {request.AuctionId}");

            doc = VehicleSpecificationDocument.Create(
                request.AuctionId,
                auction.Vehicle.Marquee,
                auction.Vehicle.Model,
                request.KeyValuePairs.ToDictionary()
                );

            var resultId = await _vehicleRepository.AddAsync(doc, cancellationToken);

            await _cacheService.RemoveAsync($"vehicles:specs:{request.AuctionId}", cancellationToken);

            return resultId;
        }
    }
}
