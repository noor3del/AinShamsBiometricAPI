using AinShamsBiometric.Application.Interfaces;
using AinShamsBiometric.Contracts.Helpers;
using AinShamsBiometric.Contracts.Responses.Card;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AinShamsBiometric.Application.Services
{
    public class CardService : ICardService
    {
        private readonly ILogger<CardService> _log;
        private readonly HttpClient _http;

        public CardService(ILogger<CardService> logger, HttpClient httpClient)
        {
            _log = logger;
            _http = httpClient;
        }
        private static string GetMediaTypeFromExtension(string ext)
        {
            return ext?.ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                ".tif" or ".tiff" => "image/tiff",
                _ => "application/octet-stream"
            };
        }

        public async Task<FrontIdCardResponse> FrontIdExtractor(
            Stream imageStream,
            string? fileName = null,
            CancellationToken ct = default)
        {
            // We’ll buffer the incoming stream so we can compute Content-Length and avoid chunked upload
            if (imageStream.CanSeek) imageStream.Position = 0;

            var safeName = string.IsNullOrWhiteSpace(fileName) ? "front.png" : fileName;
            var contentType = GetMediaTypeFromExtension(Path.GetExtension(safeName));

            // Read the stream WITHOUT disposing the caller's stream
            byte[] fileBytes;
            if (imageStream is MemoryStream ms && ms.TryGetBuffer(out var seg))
            {
                fileBytes = seg.AsSpan(0, (int)ms.Length).ToArray();
            }
            else
            {
                using var buffer = new MemoryStream();
                await imageStream.CopyToAsync(buffer, ct);
                fileBytes = buffer.ToArray();
            }

            // Build multipart body manually to guarantee Content-Length (no Transfer-Encoding: chunked)
            var boundary = "--------------------------" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var sb = new StringBuilder();
            sb.AppendLine($"--{boundary}");
            sb.AppendLine($@"Content-Disposition: form-data; name=""file""; filename=""{safeName}""");
            sb.AppendLine($"Content-Type: {contentType}");
            sb.AppendLine(); // blank line before binary

            var headerBytes = Encoding.UTF8.GetBytes(sb.ToString());
            var trailerBytes = Encoding.UTF8.GetBytes($"\r\n--{boundary}--\r\n");

            var payload = new byte[headerBytes.Length + fileBytes.Length + trailerBytes.Length];
            Buffer.BlockCopy(headerBytes, 0, payload, 0, headerBytes.Length);
            Buffer.BlockCopy(fileBytes, 0, payload, headerBytes.Length, fileBytes.Length);
            Buffer.BlockCopy(trailerBytes, 0, payload, headerBytes.Length + fileBytes.Length, trailerBytes.Length);

            using var body = new ByteArrayContent(payload);
            body.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data")
            {
                Parameters = { new NameValueHeaderValue("boundary", boundary) }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.miniai.live/api/check_id")
            {
                Version = System.Net.HttpVersion.Version11,
                VersionPolicy = HttpVersionPolicy.RequestVersionExact,
                Content = body
            };

            // Keep headers minimal; no browser/CORS headers needed for server-to-server
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            // OPTIONAL but recommended to avoid some proxies' 100-continue behavior
            _http.DefaultRequestHeaders.ExpectContinue = false;

            using var response = await _http.SendAsync(request, ct);
            var responseText = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _log.LogWarning("ID check failed: {Status} {Body}", (int)response.StatusCode, responseText);
                return new FrontIdCardResponse { Status = $"HTTP {(int)response.StatusCode}" };
            }

            _log.LogDebug("Response: {Json}", responseText);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<FrontIdCardResponse>(responseText, options)
                   ?? new FrontIdCardResponse { Status = "Failed to parse JSON" };
        }


        public async Task<MultiIdCardResponse> MultiIdExtractor(
            Stream frontImageStream,
            string? frontFileName = null,
            Stream? backImageStream = null,
            string? backFileName = null,
            CancellationToken ct = default)
        {
            // Ensure readable positions
            if (frontImageStream.CanSeek) frontImageStream.Position = 0;
            if (backImageStream != null && backImageStream.CanSeek) backImageStream.Position = 0;

            var frontName = string.IsNullOrWhiteSpace(frontFileName) ? "front.png" : frontFileName;
            var backName = string.IsNullOrWhiteSpace(backFileName) ? "back.png" : backFileName;

            var frontContentType = GetMediaTypeFromExtension(Path.GetExtension(frontName));
            var backContentType = backImageStream != null ? GetMediaTypeFromExtension(Path.GetExtension(backName)) : null;

            // Buffer streams (don’t dispose caller’s streams)
            byte[] frontBytes;
            if (frontImageStream is MemoryStream fms && fms.TryGetBuffer(out var fseg))
                frontBytes = fseg.AsSpan(0, (int)fms.Length).ToArray();
            else
            {
                using var fb = new MemoryStream();
                await frontImageStream.CopyToAsync(fb, ct);
                frontBytes = fb.ToArray();
            }

            byte[]? backBytes = null;
            if (backImageStream != null)
            {
                if (backImageStream is MemoryStream bms && bms.TryGetBuffer(out var bseg))
                    backBytes = bseg.AsSpan(0, (int)bms.Length).ToArray();
                else
                {
                    using var bb = new MemoryStream();
                    await backImageStream.CopyToAsync(bb, ct);
                    backBytes = bb.ToArray();
                }
            }

            // Build multipart by hand to guarantee Content-Length
            var boundary = "--------------------------" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            static byte[] PartHeader(string boundary, string field, string filename, string contentType)
            {
                var s = new StringBuilder();
                s.AppendLine($"--{boundary}");
                s.AppendLine($@"Content-Disposition: form-data; name=""{field}""; filename=""{filename}""");
                s.AppendLine($"Content-Type: {contentType}");
                s.AppendLine();
                return Encoding.UTF8.GetBytes(s.ToString());
            }

            var frontHeader = PartHeader(boundary, "front", frontName, frontContentType);
            var backHeader = backBytes != null ? PartHeader(boundary, "back", backName, backContentType!) : Array.Empty<byte>();
            var trailerBytes = Encoding.UTF8.GetBytes($"\r\n--{boundary}--\r\n");

            var frontTrailer = Encoding.UTF8.GetBytes("\r\n");
            var backTrailer = backBytes != null ? Encoding.UTF8.GetBytes("\r\n") : Array.Empty<byte>();

            var totalLength =
                frontHeader.Length + frontBytes.Length + frontTrailer.Length +
                (backBytes != null ? backHeader.Length + backBytes.Length + backTrailer.Length : 0) +
                trailerBytes.Length;

            var payload = new byte[totalLength];
            var offset = 0;

            Buffer.BlockCopy(frontHeader, 0, payload, offset, frontHeader.Length);
            offset += frontHeader.Length;

            Buffer.BlockCopy(frontBytes, 0, payload, offset, frontBytes.Length);
            offset += frontBytes.Length;

            Buffer.BlockCopy(frontTrailer, 0, payload, offset, frontTrailer.Length);
            offset += frontTrailer.Length;

            if (backBytes != null)
            {
                Buffer.BlockCopy(backHeader, 0, payload, offset, backHeader.Length);
                offset += backHeader.Length;

                Buffer.BlockCopy(backBytes, 0, payload, offset, backBytes.Length);
                offset += backBytes.Length;

                Buffer.BlockCopy(backTrailer, 0, payload, offset, backTrailer.Length);
                offset += backTrailer.Length;
            }

            Buffer.BlockCopy(trailerBytes, 0, payload, offset, trailerBytes.Length);

            using var body = new ByteArrayContent(payload);
            body.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data")
            {
                Parameters = { new NameValueHeaderValue("boundary", boundary) }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.miniai.live/api/check_id_multi")
            {
                Version = HttpVersion.Version11,
                VersionPolicy = HttpVersionPolicy.RequestVersionExact,
                Content = body
            };

            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            _http.DefaultRequestHeaders.ExpectContinue = false;

            using var response = await _http.SendAsync(request, ct);
            var responseText = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _log.LogWarning("ID multi-check failed: {Status} {Body}", (int)response.StatusCode, responseText);
                return new MultiIdCardResponse { Status = $"HTTP {(int)response.StatusCode}" };
            }

            _log.LogDebug("Multi response: {Json}", responseText);

            // ✅ Deserialize to wire model, then map to your DTOs
            var wire = JsonSerializer.Deserialize<WireRoot>(responseText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new WireRoot { Status = "Failed", Result = new() };

            var dto = MultiIdCardDtoMapper.Map(wire);
            return dto;
        }


    }
}
