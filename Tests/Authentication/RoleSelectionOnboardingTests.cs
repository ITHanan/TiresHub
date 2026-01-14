using ApplicationLayer.Features.StartAuth.Commands;
using ApplicationLayer.Interfaces;
using DomainLayer.Enums;
using DomainLayer.Users;
using FluentAssertions;
using Moq;
using System.Reflection.Metadata;
using Xunit;

namespace Tests.Authentication
{
    public class RoleSelectionOnboardingTests
    {


        private readonly StartAuthCommandHandler _handler;


        public RoleSelectionOnboardingTests()
        {
            var _codes = new Mock<IVerificationCodeRepository>();
            _handler = new StartAuthCommandHandler(
                _codes.Object
            );
        }

        [Fact]
        public async Task StartAuth_Allows_VehicleOwner()
        {
            // Arrange
            var codes = new Mock<IVerificationCodeRepository>();
            var handler = new StartAuthCommandHandler(codes.Object);

            var command = new StartAuthCommand(
                "test@email.com",
                UserRole.VehicleOwner
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            codes.Verify(c => c.AddAsync(It.IsAny<VerificationCode>()), Times.Once);
            codes.Verify(c => c.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task StartAuth_Blocks_ShopManager()
        {
            // Arrange
            var command = new StartAuthCommand(
                Identifier: "manager@email.com",
                Role: UserRole.ShopManager
            );

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("This role cannot be registered directly.");
        }

        [Fact]
        public async Task StartAuth_Blocks_Employee()
        {
            // Arrange
            var command = new StartAuthCommand(
                Identifier: "employee@email.com",
                Role: UserRole.Employee
            );

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("This role cannot be registered directly.");
        }


    }
}