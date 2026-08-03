using AutoPulse.Application.Application.Authentication.Commands.LoginUser;
using AutoPulse.Application.Application.Authentication.Common.Dto;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Common.Specification;
using AutoPulse.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AutoPulse.Tests.Unit.Application.Commands.Authentication
{
    public class LoginUserCommandHandlerTests
    {
        private readonly IRepository<User> _userRepository;
        private readonly IAutoPulseDbContext _dbContext;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtProvider _jwtProvider;
        private readonly IConfiguration _configuration;
        private readonly IPermissionCacheService _permissionCacheService;
        private readonly ILogger<LoginUserCommandHandler> _logger;
        private readonly LoginUserCommandHandler _handler;

        public LoginUserCommandHandlerTests()
        {
            _userRepository = Substitute.For<IRepository<User>>();
            _dbContext = Substitute.For<IAutoPulseDbContext>();
            _passwordHasher = Substitute.For<IPasswordHasher>();
            _jwtProvider = Substitute.For<IJwtProvider>();
            _configuration = Substitute.For<IConfiguration>();
            _permissionCacheService = Substitute.For<IPermissionCacheService>();
            _logger = Substitute.For<ILogger<LoginUserCommandHandler>>();

            _handler = new LoginUserCommandHandler(
                _userRepository,
                _dbContext,
                _passwordHasher,
                _jwtProvider,
                _configuration,
                _permissionCacheService,
                _logger
            );
        }

        [Fact]
        public async Task Handle_WithInvalidEmail_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var command = new LoginUserCommand("nonexistent@example.com", "Password123!", false);
            _userRepository.GetBySpecAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
                .Returns((User?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("User not found");
        }

        [Fact]
        public async Task Handle_WithInvalidPassword_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var command = new LoginUserCommand("user@example.com", "WrongPassword", false);
            var user = User.Create(Guid.NewGuid(), "user@example.com", "user@example.com", "HashedPass", new List<string>());

            _userRepository.GetBySpecAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
                .Returns(user);
            _passwordHasher.Verify("WrongPassword", "HashedPass").Returns(false);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("User not found");
        }

        [Fact]
        public async Task Handle_WithValidCredentials_ShouldReturnAuthDtoAndSaveContext()
        {
            // Arrange
            var command = new LoginUserCommand("user@example.com", "ValidPass123!", false);
            var user = User.Create(Guid.NewGuid(), "user@example.com", "user@example.com", "HashedPass", new List<string>());

            _userRepository.GetBySpecAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
                .Returns(user);
            _passwordHasher.Verify("ValidPass123!", "HashedPass").Returns(true);
            _jwtProvider.GenerateSecureString().Returns("refresh-token-xyz");
            _jwtProvider.HashToken("refresh-token-xyz").Returns("hashed-refresh-token-xyz");
            _jwtProvider.GenerateAccessToken(user, Arg.Any<string>()).Returns(("access-token-abc", 60));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.AccessToken.Should().Be("access-token-abc");
            result.RefreshToken.Should().Be("refresh-token-xyz");
            result.ExpiresIn.Should().Be(60);

            await _dbContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
