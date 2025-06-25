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

namespace CareerCrafter.Tests.Controllers
{
    [TestFixture]
    public class JobsControllerTests
    {
        private JobsController _controller = null!;
        private Mock<IJobRepository> _jobRepoMock = null!;

        [SetUp]
        public void Setup()
        {
            _jobRepoMock = new Mock<IJobRepository>();
            _controller = new JobsController(_jobRepoMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("UserId", "1"),
                new Claim(ClaimTypes.Role, "Employer")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task PostJob_Valid_ReturnsOk()
        {
            var jobDto = new JobDto
            {
                Title = "Developer",
                Description = "Full stack dev",
                Location = "Remote",
                CompanyName = "TestCorp",
                Salary = 50000
            };

            _jobRepoMock.Setup(r => r.AddJobAsync(It.IsAny<Job>())).Returns(Task.CompletedTask);

            // ✅ FIXED: Await the method
            var result = await _controller.PostJob(jobDto);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task PostJob_NullInput_ReturnsBadRequest()
        {
            // ✅ FIXED: Await the method
            var result = await _controller.PostJob(null!);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }
    }
}
