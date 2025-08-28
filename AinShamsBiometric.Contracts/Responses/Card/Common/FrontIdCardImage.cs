using System.Text.Json.Serialization;

namespace AinShamsBiometric.Contracts.Responses.Card.Common
{
    public class FrontIdCardImage
    {
        public IdCardImage? Document { get; set; }
        [JsonPropertyName("Portrait-V")]
        public IdCardPortraitFront? PortraitV { get; set; } // "Portrait-V"
    }
    public class IdCardPortraitFront
    {
        public IdCardPosition Position { get; set; } = new();
        public string? Image { get; set; }
    }

}
