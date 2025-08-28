using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AinShamsBiometric.Contracts.Requests.Card
{
    public class MutliIdCardRequest
    {
        [FromForm(Name = "front")]
        public IFormFile? Front { get; set; }
        [FromForm(Name = "back")]
        public IFormFile? Back { get; set; }
    }
}
