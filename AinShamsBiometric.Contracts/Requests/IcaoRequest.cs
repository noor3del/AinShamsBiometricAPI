using Microsoft.AspNetCore.Http;

namespace AinShamsBiometric.Contracts.Requests
{
    public class IcaoRequest
    {
        public IFormFile? image { get; set; }
    }
}
