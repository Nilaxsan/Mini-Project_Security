
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UniTutor.Interface;

namespace UniTutor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalytics _analyticsRepository;

        public AnalyticsController(IAnalytics analyticsRepository)
        {
            _analyticsRepository = analyticsRepository;
        }

        private bool IsValidMagicNumber(byte[] header)
        {
            // JPG
            if (header[0] == 0xFF &&
                header[1] == 0xD8 &&
                header[2] == 0xFF)
                return true;

            // PNG
            if (header[0] == 0x89 &&
                header[1] == 0x50 &&
                header[2] == 0x4E &&
                header[3] == 0x47)
                return true;

            // PDF
            if (header[0] == 0x25 &&
                header[1] == 0x50 &&
                header[2] == 0x44 &&
                header[3] == 0x46)
                return true;

            return false;
        }


        [HttpPost("UploadProfileImage")]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file.");

            //  Validate extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("File type not allowed.");

            // Validate file size (example: 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("File too large.");

            //  Validate Magic Numbers
            using (var stream = file.OpenReadStream())
            {
                byte[] headerBytes = new byte[4];
                await stream.ReadAsync(headerBytes, 0, 4);

                if (!IsValidMagicNumber(headerBytes))
                    return BadRequest("Invalid file signature.");
            }

            // Upload to Firebase (after validation)
            // (Use Firebase Admin SDK here)

            return Ok("File uploaded securely.");
        }


        [HttpGet("weekly-joined-tutors")]
        public async Task<IActionResult> GetWeeklyJoinedTutors()
        {
            var data = await _analyticsRepository.GetWeeklyJoinedTutorsAsync();
           // var verifiedTutors = data.Where(tutor => tutor.Verified==true);
            return Ok(data);
        }

        [HttpGet("weekly-joined-students")]
        public async Task<IActionResult> GetWeeklyJoinedStudents()
        {
            var data = await _analyticsRepository.GetWeeklyJoinedStudentsAsync();
            return Ok(data);
        }

        [HttpGet("weekly-tutor-requests")]
        public async Task<IActionResult> GetWeeklyTutorRequests()
        {
            var data = await _analyticsRepository.GetWeeklyTutorRequestsAsync();
            return Ok(data);
        }


        [HttpGet("weekly-comments")]
        public async Task<IActionResult> GetWeeklyComments()
        {
            var data = await _analyticsRepository.GetWeeklyCommentsAsync();
            return Ok(data);
        }
    }
}
