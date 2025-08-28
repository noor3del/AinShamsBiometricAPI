using AinShamsBiometric.Application.Interfaces;
using AinShamsBiometric.Contracts.Requests.Card;
using AinShamsBiometric.Contracts.Responses.Card;
using Microsoft.AspNetCore.Mvc;

namespace AinShamsBiometric.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardController : ControllerBase
    {
        private readonly ICardService _svc;
        private readonly ILogger<CardController> _log;
        public CardController(ICardService svc, ILogger<CardController> log)
        {
            _svc = svc;
            _log = log;
        }


        #region OCR Front Face of Egyptian National ID

        [HttpPost("check_id")] // <— clean, consistent route
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        [RequestSizeLimit(100 * 1024 * 1024)] // 100 MB
        [RequestFormLimits(
          ValueCountLimit = int.MaxValue,
          MultipartBodyLengthLimit = 100 * 1024 * 1024,
          MultipartHeadersCountLimit = 128,
          MultipartHeadersLengthLimit = int.MaxValue)]
        [ProducesResponseType(typeof(FrontIdCardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status499ClientClosedRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FrontIDExtractor([FromForm] FrontIdCardRequest dto, CancellationToken ct)
        {
            // Basic input checks
            if (dto.image is null || dto.image.Length == 0)
                return ValidationProblem(new ValidationProblemDetails
                {
                    Title = "Invalid file",
                    Detail = "The 'image' file is required and must not be empty."
                });

            // Optional: limit to image types your upstream accepts
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "image/jpeg", "image/png", "image/webp", "image/tiff", "image/bmp" };

            if (!string.IsNullOrWhiteSpace(dto.image.ContentType) && !allowed.Contains(dto.image.ContentType))
                return StatusCode(StatusCodes.Status415UnsupportedMediaType,
                    new ProblemDetails
                    {
                        Title = "Unsupported media type",
                        Detail = $"Content-Type '{dto.image.ContentType}' is not supported."
                    });

            try
            {
                await using var stream = dto.image.OpenReadStream();

                // Calls your existing client method that POSTS form-data with the field name "image"
                var result = await _svc.FrontIdExtractor(stream, dto.image.FileName, ct);

                // Optionally map a non-OK status from upstream to 400 here
                if (!string.IsNullOrWhiteSpace(result.Status) &&
                    !string.Equals(result.Status, "ok", StringComparison.OrdinalIgnoreCase))
                {
                    // Still return the body so the caller sees upstream details
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (OperationCanceledException)
            {
                _log.LogWarning("FrontIDExtractor canceled for {File}", dto.image.FileName);
                return Problem("Request canceled", statusCode: StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to process {File}", dto.image.FileName);
                return Problem("Failed to process image.", statusCode: StatusCodes.Status500InternalServerError);
            }
        }
        #endregion

        #region OCR Front & Back Face of Egyptian National ID

        [HttpPost("check_id_multi")] // use the same naming convention as the upstream
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        [RequestSizeLimit(100 * 1024 * 1024)]
        [RequestFormLimits(
            ValueCountLimit = int.MaxValue,
            MultipartBodyLengthLimit = 100 * 1024 * 1024,
            MultipartHeadersCountLimit = 128,
            MultipartHeadersLengthLimit = int.MaxValue)]
        [ProducesResponseType(typeof(FrontIdCardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status499ClientClosedRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MultiIDExtractor([FromForm] MutliIdCardRequest dto, CancellationToken ct)
        {
            if (dto.Front is null || dto.Back is null || dto.Front.Length == 0 || dto.Back.Length == 0)
            {
                return ValidationProblem(new ValidationProblemDetails
                {
                    Title = "Invalid files",
                    Detail = "Fields 'front' and 'back' are required and must not be empty."
                });
            }

            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    { "image/jpeg", "image/png", "image/webp", "image/tiff", "image/bmp" };

            if (!string.IsNullOrWhiteSpace(dto.Front.ContentType) && !allowed.Contains(dto.Front.ContentType))
            {
                return StatusCode(StatusCodes.Status415UnsupportedMediaType, new ProblemDetails
                {
                    Title = "Unsupported media type (front)",
                    Detail = $"Content-Type '{dto.Front.ContentType}' is not supported."
                });
            }

            if (!string.IsNullOrWhiteSpace(dto.Back.ContentType) && !allowed.Contains(dto.Back.ContentType))
            {
                return StatusCode(StatusCodes.Status415UnsupportedMediaType, new ProblemDetails
                {
                    Title = "Unsupported media type (back)",
                    Detail = $"Content-Type '{dto.Back.ContentType}' is not supported."
                });
            }

            try
            {
                await using var frontStream = dto.Front.OpenReadStream();
                await using var backStream = dto.Back.OpenReadStream();

                var result = await _svc.MultiIdExtractor(
                    frontStream, dto.Front.FileName,
                    backStream, dto.Back.FileName,
                    ct);

                if (!string.IsNullOrWhiteSpace(result.Status) &&
                    !string.Equals(result.Status, "ok", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (OperationCanceledException)
            {
                _log.LogWarning("MultiIDExtractor canceled for {Front} & {Back}", dto.Front.FileName, dto.Back.FileName);
                return Problem("Request canceled", statusCode: StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to process {Front} & {Back}", dto.Front.FileName, dto.Back.FileName);
                return Problem("Failed to process images.", statusCode: StatusCodes.Status500InternalServerError);
            }
        }
        #endregion
    }
}
