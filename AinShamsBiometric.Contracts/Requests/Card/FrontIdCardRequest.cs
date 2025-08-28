using Microsoft.AspNetCore.Http;

namespace AinShamsBiometric.Contracts.Requests.Card
{
    public class FrontIdCardRequest
    {
        public IFormFile? image { get; set; }
    }
}
