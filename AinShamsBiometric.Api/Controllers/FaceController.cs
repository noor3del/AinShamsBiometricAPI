using AinShamsBiometric.Application.Interfaces;
using AinShamsBiometric.Contracts.Requests;
using AinShamsBiometric.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AinShamsBiometric.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FaceController : ControllerBase
    {
        private readonly IFaceService _svc;
        private readonly ILogger<FaceController> _log;
        public FaceController(IFaceService svc, ILogger<FaceController> log)
        {
            _svc = svc;
            _log = log;
        }


        #region  Check ICAO compliance and return attributes/warnings

        [HttpPost("CheckIcaoCompliance")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(100 * 1024 * 1024)] // 100 MB
        [RequestFormLimits(
          ValueCountLimit = int.MaxValue,
          MultipartBodyLengthLimit = 100 * 1024 * 1024,
          MultipartHeadersCountLimit = 128,
          MultipartHeadersLengthLimit = int.MaxValue)]
        [ProducesResponseType(typeof(IcaoResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<IActionResult> CheckIcao([FromForm] IcaoRequest dto, CancellationToken ct)
        {
            if (dto.image is null || dto.image.Length == 0) return BadRequest("Empty file");
            var sw = Stopwatch.StartNew();
            try
            {
                await using var s = dto.image.OpenReadStream();
                var result = await _svc.CheckIcaoAsync(s, dto.image.FileName, ct);
                sw.Stop();
                _log.LogInformation("ICAO checked for {File} in {Elapsed} ms, compliant={Compliant}, warnings={Warnings}",
                    dto.image.FileName, sw.ElapsedMilliseconds, result.IsCompliant, result.Warnings?.Count ?? 0);
                return Ok(result);
            }
            catch (OperationCanceledException)
            {
                _log.LogWarning("ICAO check canceled for {File}", dto.image.FileName);
                return Problem("Request canceled", statusCode: StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _log.LogError(ex, "ICAO check failed for {File} after {Elapsed} ms", dto.image.FileName, sw.ElapsedMilliseconds);
                return Problem("Failed to process image.", statusCode: StatusCodes.Status500InternalServerError);
            }
        }


        #endregion

        #region  Segments the uploaded face image, removes background & red-eye, returns Base64 JPEG (compressed).

        [HttpPost("SegmentImage")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(100 * 1024 * 1024)]
        [RequestFormLimits(
            ValueCountLimit = int.MaxValue,
            MultipartBodyLengthLimit = 100 * 1024 * 1024,
            MultipartHeadersCountLimit = 128,
            MultipartHeadersLengthLimit = int.MaxValue)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SegmentFace([FromForm] IcaoRequest dto, CancellationToken ct)
        {
            if (dto.image == null || dto.image.Length == 0)
                return BadRequest("No file uploaded");

            var sw = Stopwatch.StartNew();
            try
            {
                await using var stream = dto.image.OpenReadStream();
                var base64 = await _svc.SegmentImageAsync(stream, dto.image.FileName, ct);
                sw.Stop();
                _log.LogInformation("Segmentation done for {File} in {Elapsed} ms, size={SizeKb} KB",
                    dto.image.FileName, sw.ElapsedMilliseconds, (base64.Length * 3 / 4) / 1024);
                return Ok(base64);
            }
            catch (OperationCanceledException)
            {
                _log.LogWarning("Segmentation canceled for {File}", dto.image.FileName);
                return Problem("Request canceled", statusCode: StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _log.LogError(ex, "Segmentation failed for {File} after {Elapsed} ms", dto.image.FileName, sw.ElapsedMilliseconds);
                return Problem("Failed to segment image.", statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        #endregion
    }

}



