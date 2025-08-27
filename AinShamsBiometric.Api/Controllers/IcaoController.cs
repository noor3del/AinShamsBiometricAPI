using AinShamsBiometric.Application.Interfaces;
using AinShamsBiometric.Contracts.Requests;
using AinShamsBiometric.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AinShamsBiometric.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IcaoController : ControllerBase
    {
        private readonly IICAOService _svc;
        public IcaoController(IICAOService svc) => _svc = svc;

        [HttpPost("check")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(100 * 1024 * 1024)]                 // 100 MB
        [RequestFormLimits(
            ValueCountLimit = int.MaxValue,                   // allow many form keys
            MultipartBodyLengthLimit = 100 * 1024 * 1024,     // 100 MB
            MultipartHeadersCountLimit = 128,
            MultipartHeadersLengthLimit = int.MaxValue)]
        [ProducesResponseType(typeof(IcaoResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckIcao([FromForm] IcaoRequest dto, CancellationToken ct)
        {
            if (dto.image is null || dto.image.Length == 0) return BadRequest("Empty file");
            await using var s = dto.image.OpenReadStream();
            var result = await _svc.CheckIcaoAsync(s, dto.image.FileName, ct);
            return Ok(result);
        }


        /// <summary>
        /// Segments the uploaded face image, removes background and red-eye, and returns the result as Base64.
        /// </summary>
        /// <param name="file">Uploaded image file</param>
        [HttpPost("segment")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(100 * 1024 * 1024)]
        [RequestFormLimits(
            ValueCountLimit = int.MaxValue,                   // allow many form keys
            MultipartBodyLengthLimit = 100 * 1024 * 1024,     // 100 MB
            MultipartHeadersCountLimit = 128,
            MultipartHeadersLengthLimit = int.MaxValue)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SegmentFace([FromForm] IcaoRequest dto, CancellationToken ct)
        {
            if (dto.image == null || dto.image.Length == 0)
                return BadRequest("No file uploaded");

            await using var stream = dto.image.OpenReadStream();
            var base64 = await _svc.SegmentImageAsync(stream, dto.image.FileName, ct);

            // Option 1: Return Base64 directly (JSON string)
            return Ok(base64);

            // Option 2: Return as JSON object with filename
            // return Ok(new { FileName = file.FileName, Base64 = base64 });
        }

    }


}
