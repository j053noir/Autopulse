using AutoPulse.Domain.ValueObjects;
using FluentValidation;

namespace AutoPulse.Application.Application.Auctions.Commands.CreateAuction
{
    public class CreateAuctionCommandValidator : AbstractValidator<CreateAuctionCommand>
    {
        public CreateAuctionCommandValidator()
        {
            RuleFor(x => x.Vin)
                .NotEmpty().WithMessage("VIN is required.")
                .Must(vin => {
                    try
                    {
                        Vin.Create(vin);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }).WithMessage("Invalid VIN format. Must be exactly 17 characters.");

            RuleFor(x => x.BasePrice)
                .GreaterThan(0).WithMessage("Base price must be greater than 0.");

            RuleFor(x => x.MinimumBidIncrement)
                .GreaterThan(0).WithMessage("Minimum bid increment must be greater than 0.");

            RuleFor(x => x.EndTime)
                .Must(endTime => endTime > DateTime.UtcNow.AddHours(24))
                .WithMessage("Auction end time must be at least 24 hours in the future.");

            RuleFor(x => x.DocumentStorageKey)
                .NotEmpty().WithMessage("Document storage key is required.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Vehicle title is required.");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Vehicle category is required.");
        }
    }
}
