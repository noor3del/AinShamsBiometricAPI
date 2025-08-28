using AinShamsBiometric.Contracts.Responses;

namespace AinShamsBiometric.Application.Interfaces
{
    public interface IFaceService
    {
        /// <summary>
        /// Evaluate ICAO compliance and facial attributes from an image stream.
        /// </summary>
        Task<IcaoResultDto> CheckIcaoAsync(
            Stream imageStream,
            string? fileName = null,
            CancellationToken ct = default);

        /// <summary>
        /// Convenience overload when you only have a file path.
        /// </summary>
        Task<IcaoResultDto> CheckIcaoAsync(
            string imagePath,
            CancellationToken ct = default);
        Task<string> SegmentImageAsync(Stream imageStream, string? fileName = null, CancellationToken ct = default);

    }
}
