using AutoPulse.Application.Application.Authentication.Common.Specification;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Application.Application.Common.Security;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Common.Security;
using AutoPulse.Domain.Entities;
using MediatR;

namespace AutoPulse.Application.Application.Authentication.Commands.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAutoPulseDbContext _context;

        public RegisterUserCommandHandler
            (
                IRepository<User> userRepository,
                IPasswordHasher passwordHasher,
                IAutoPulseDbContext context
            )
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _context = context;
        }

        public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // 1. Check if user with given username or email exists
            var spec = new ActiveUserSpecificationByUsernameOrEmail(request.Username, request.Email);
            var entities = await _userRepository.ListAsync(spec, cancellationToken);

            if (entities?.Count > 0) throw new ArgumentException("Given Username or Email have been taken");

            var userId = Guid.NewGuid();

            // 1. Hash the password
            var passwordHash = _passwordHasher.Hash(request.Password);

            // 2. Create user entity using its factory methods
            var permissions = CustomerPermissions();
            var sanitizedUsername = request.Username.SanitizeInput();

            var user = User.Create(userId, sanitizedUsername, request.Email, passwordHash, permissions);

            // 3. Add the user entity to the repository
            _userRepository.Add(user);

            // 4. Save changes to the database
            await _context.SaveChangesAsync(cancellationToken);

            // 5. Return the new resource Id
            return userId;
        }

        private List<string> CustomerPermissions()
        {
            return
            [
                Permissions.Auctions.Create,
                Permissions.Auctions.Bid
            ];
        }
    }
}
