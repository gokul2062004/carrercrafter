using CareerCrafter.Controllers;
using CareerCrafter.DTOs;
using CareerCrafter.Models;
using CareerCrafter.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CareerCrafter.Tests.Controllers
{
    [TestFixture]
    public class ApplicationsControllerTests
    {
        private ApplicationsController _controller = null!;
        private Mock<IApplicationRepository> _appRepoMock = null!;
        private Mock<IJobRepository> _jobRepoMock = null!;

        [SetUp]
        public void Setup()
        {
            _appRepoMock = new Mock<IApplicationRepository>();
            _jobRepoMock = new Mock<IJobRepository>();

            _controller = new ApplicationsController(_appRepoMock.Object, _jobRepoMock.Object);

            // Mock user claims (JobSeeker with ID = 1)
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("UserId", "1"),
                new Claim(ClaimTypes.Role, "JobSeeker")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task ApplyForJob_ValidApplication_ReturnsOk()
        {
            var dto = new ApplicationDto { JobId = 101 };

            // Job exists
            _jobRepoMock.Setup(r => r.GetJobByIdAsync(dto.JobId)).ReturnsAsync(new Job());

            // Not applied yet
            _appRepoMock.Setup(r => r.HasAlreadyAppliedAsync(1, dto.JobId)).ReturnsAsync(false);

            _appRepoMock.Setup(r => r.AddApplicationAsync(It.IsAny<Application>())).Returns(Task.CompletedTask);

            var result = await _controller.ApplyForJob(dto);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task ApplyForJob_AlreadyApplied_ReturnsBadRequest()
        {
            var dto = new ApplicationDto { JobId = 101 };

            _jobRepoMock.Setup(r => r.GetJobByIdAsync(dto.JobId)).ReturnsAsync(new Job());

            // Already applied
            _appRepoMock.Setup(r => r.HasAlreadyAppliedAsync(1, dto.JobId)).ReturnsAsync(true);

            var result = await _controller.ApplyForJob(dto);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }
    }
}
