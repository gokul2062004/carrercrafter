using CareerCrafter.Controllers;
using CareerCrafter.DTOs;
using CareerCrafter.Models;
using CareerCrafter.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Tests.Controllers
{
    [TestFixture]
    public class AuthControllerTests
    {
        private Mock<IUserRepository> _userRepoMock = null!;
        private AuthController _controller = null!;
        private IConfiguration _configuration = null!;

        [SetUp]
        public void Setup()
        {
            _userRepoMock = new Mock<IUserRepository>();

            var inMemorySettings = new Dictionary<string, string?>
            {
                { "Jwt:Key", "ThisIsASecretKeyForTestingOnly123!" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            _controller = new AuthController(_userRepoMock.Object, _configuration);
        }

        [Test]
        public async Task Register_NewUser_ReturnsOk()
        {
            var dto = new RegisterDto
            {
                Email = "testuser@mail.com",
                Password = "password123",
                Role = "JobSeeker"
            };

            _userRepoMock.Setup(r => r.UserExistsAsync(dto.Email)).ReturnsAsync(false);
            _userRepoMock.Setup(r => r.RegisterUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var result = await _controller.Register(dto);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Register_ExistingEmail_ReturnsBadRequest()
        {
            var dto = new RegisterDto
            {
                Email = "existing@mail.com",
                Password = "pass",
                Role = "Employer"
            };

            _userRepoMock.Setup(r => r.UserExistsAsync(dto.Email)).ReturnsAsync(true);

            var result = await _controller.Register(dto);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            var dto = new LoginDto
            {
                Email = "valid@mail.com",
                Password = "pass123"
            };

            var passwordHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(dto.Password)));

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = passwordHash,
                Role = "JobSeeker",
                Id = 1
            };

            _userRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync(user);

            var result = await _controller.Login(dto);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            var dto = new LoginDto
            {
                Email = "unknown@mail.com",
                Password = "wrong"
            };

            _userRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync((User?)null);

            var result = await _controller.Login(dto);

            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }
    }
}
