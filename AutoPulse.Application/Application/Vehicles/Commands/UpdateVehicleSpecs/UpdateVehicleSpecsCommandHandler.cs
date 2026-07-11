using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Application.Application.Common.Security;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities.NoSql;
using MediatR;

namespace AutoPulse.Application.Application.Vehicles.Commands.UpdateVehicleSpecs
{
    public class UpdateVehicleSpecsCommandHandler : IRequestHandler<UpdateVehicleSpecsCommand, bool>
    {
        private readonly INoSqlRepository<VehicleSpecificationDocument> _vehicleRepository;
        private readonly ICacheService _cacheService;

        public UpdateVehicleSpecsCommandHandler(
            INoSqlRepository<VehicleSpecificationDocument> vehicleRepository,
            ICacheService cacheService)
        {
            _vehicleRepository = vehicleRepository;
            _cacheService = cacheService;
        }

        public async Task<bool> Handle(UpdateVehicleSpecsCommand request, CancellationToken cancellationToken)
        {
            var doc = await _vehicleRepository.GetByIdAsync(request.AuctionId, cancellationToken);
            if (doc is null) return false;

            var sanitizedValue = request.Value is string stringValue
                ? stringValue.SanitizeInput()
                : request.Value;

            doc.UpdateMetadata(request.Key, sanitizedValue);

            await _vehicleRepository.UpdateAsync(request.AuctionId, doc, cancellationToken);

            await _cacheService.RemoveAsync($"vehicles:specs:{request.AuctionId}", cancellationToken);

            return true;
        }
    }
}
