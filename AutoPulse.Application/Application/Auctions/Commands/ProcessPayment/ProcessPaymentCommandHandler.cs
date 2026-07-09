using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities;
using MediatR;

namespace AutoPulse.Application.Application.Auctions.Commands.ProcessPayment
{
    public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, bool>
    {
        private readonly IPaymentService _paymentService;
        private readonly IRepository<User> _userRepository;

        public ProcessPaymentCommandHandler(IPaymentService paymentService, IRepository<User> userRepository)
        {
            _paymentService = paymentService;
            _userRepository = userRepository;
        }

        public async Task<bool> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.WinnerId, cancellationToken);

            if (user == null) throw new InvalidOperationException($"User {request.WinnerId} not found");

            var paymentMethod = user?.PreferredPaymentMethod ?? "credit_card";

            var paymentRequest = new PaymentRequest(
                TransactionId: request.TransactionId,
                UserId: request.WinnerId,
                Amount: request.TotalAmount,
                Currency: request.Currency,
                PaymentMethod: paymentMethod
            );

            var response = await _paymentService.ProcessPaymentAsync(paymentRequest, cancellationToken);
            return response.IsSuccess;
        }
    }
}
