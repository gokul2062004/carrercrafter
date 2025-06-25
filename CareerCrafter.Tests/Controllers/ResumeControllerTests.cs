using CareerCrafter.Controllers;
using CareerCrafter.DTOs;
using CareerCrafter.Models;
using CareerCrafter.Repositories.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CareerCrafter.Tests.Controllers
{
    [TestFixture]
    public class ResumesControllerTests
    {
        private ResumesController _controller = null!;
        private Mock<IResumeRepository> _resumeRepoMock = null!;
        private Mock<IWebHostEnvironment> _envMock = null!;

        [SetUp]
        public void Setup()
        {
            _resumeRepoMock = new Mock<IResumeRepository>();
            _envMock = new Mock<IWebHostEnvironment>();

            _envMock.Setup(e => e.WebRootPath).Returns(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));

            _controller = new ResumesController(_resumeRepoMock.Object, _envMock.Object);

            // Simulate authenticated user with ID = 1
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
        public async Task UploadResume_ValidPdf_ReturnsOk()
        {
            var fileMock = new Mock<IFormFile>();
            var content = "dummy resume content";
            var fileName = "resume.pdf";
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Returns((Stream target, CancellationToken ct) => ms.CopyToAsync(target, ct));

            var dto = new ResumeDto { ResumeFile = fileMock.Object };

            var result = await _controller.UploadResume(dto);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task UploadResume_NonPdf_ReturnsBadRequest()
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("resume.txt");
            fileMock.Setup(f => f.Length).Returns(100);

            var dto = new ResumeDto { ResumeFile = fileMock.Object };

            var result = await _controller.UploadResume(dto);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetMyResume_Exists_ReturnsOk()
        {
            var resume = new Resume
            {
                Id = 1,
                FileName = "resume.pdf",
                FilePath = "/resumes/resume.pdf",
                UploadedDate = DateTime.UtcNow
            };

            _resumeRepoMock.Setup(r => r.GetLatestResumeByUserIdAsync(1)).ReturnsAsync(resume);

            var result = await _controller.GetMyResume();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetMyResume_NotFound_ReturnsNotFound()
        {
            _resumeRepoMock.Setup(r => r.GetLatestResumeByUserIdAsync(1)).ReturnsAsync((Resume?)null);

            var result = await _controller.GetMyResume();

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task DeleteResume_Valid_ReturnsOk()
        {
            var resume = new Resume
            {
                Id = 1,
                FilePath = "/resumes/resume.pdf",
                UserId = 1
            };

            _resumeRepoMock.Setup(r => r.GetResumeByIdAndUserAsync(1, 1)).ReturnsAsync(resume);
            _resumeRepoMock.Setup(r => r.DeleteResumeAsync(resume, It.IsAny<string>())).Returns(Task.CompletedTask);

            var result = await _controller.DeleteResume(1);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task DeleteResume_NotFound_ReturnsNotFound()
        {
            _resumeRepoMock.Setup(r => r.GetResumeByIdAndUserAsync(1, 1)).ReturnsAsync((Resume?)null);

            var result = await _controller.DeleteResume(1);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }
    }
}
