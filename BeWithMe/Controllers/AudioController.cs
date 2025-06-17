using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

        using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.IO;
using System;
using BeWithMe.Models;
namespace BeWithMe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AudioController : ControllerBase
    {


            private readonly IWebHostEnvironment _hostingEnvironment;
            private readonly IHttpContextAccessor _httpContextAccessor;

            public AudioController(IWebHostEnvironment hostingEnvironment, IHttpContextAccessor httpContextAccessor)
            {
                _hostingEnvironment = hostingEnvironment;
                _httpContextAccessor = httpContextAccessor;
            }

            [HttpPost("upload")]
            public IActionResult UploadAudio([FromForm] UploadAudioDto dto)
            {
                if (dto.File == null || dto.File.Length == 0)
                    return BadRequest("No file uploaded.");

                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads","audios");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid() + Path.GetExtension(dto.File.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    dto.File.CopyTo(fileStream);
                }

                // Build absolute URL
                var request = _httpContextAccessor.HttpContext.Request;
                var fileUrl = $"{request.Scheme}://{request.Host}/uploads/audios/{uniqueFileName}";

                return Ok(new { Url = fileUrl });
            }
        }
    }

