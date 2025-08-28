using AinShamsBiometric.Contracts.Responses.Card;

namespace AinShamsBiometric.Application.Interfaces
{
    public interface ICardService
    {
        Task<FrontIdCardResponse> FrontIdExtractor(Stream imageStream,
            string? fileName = null, CancellationToken ct = default);

        Task<MultiIdCardResponse> MultiIdExtractor(
   Stream frontImageStream,
   string? frontFileName = null,
   Stream? backImageStream = null,
   string? backFileName = null,
   CancellationToken ct = default);
    }
}
