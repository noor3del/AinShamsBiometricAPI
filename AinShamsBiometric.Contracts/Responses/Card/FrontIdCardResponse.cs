using AinShamsBiometric.Contracts.Responses.Card.Common;
using System.Text.Json.Serialization;

namespace AinShamsBiometric.Contracts.Responses.Card
{
    public class FrontIdCardResponse
    {
        [JsonPropertyName("Address-Arabic (Egypt)")]
        public string? AddressArabic { get; set; }

        public string? Age { get; set; }

        [JsonPropertyName("Date of Birth")]
        public string? DateOfBirth { get; set; }

        [JsonPropertyName("Date of Birth-Arabic (Egypt)")]
        public string? DateOfBirthArabic { get; set; }

        [JsonPropertyName("Document Name")]
        public string? DocumentName { get; set; }

        [JsonPropertyName("Document Number")]
        public string? DocumentNumber { get; set; }

        [JsonPropertyName("Full Name")]
        public string? FullName { get; set; }

        [JsonPropertyName("Full Name-Arabic (Egypt)")]
        public string? FullNameArabic { get; set; }

        [JsonPropertyName("Given Names")]
        public string? GivenNames { get; set; }

        [JsonPropertyName("Given Names-Arabic (Egypt)")]
        public string? GivenNamesArabic { get; set; }

        public FrontIdCardImage Images { get; set; } = new();

        [JsonPropertyName("Issuing State Code")]
        public string? IssuingStateCode { get; set; }

        [JsonPropertyName("Issuing State Name")]
        public string? IssuingStateName { get; set; }

        [JsonPropertyName("Personal Number")]
        public string? PersonalNumber { get; set; }

        [JsonPropertyName("Personal Number-Arabic (Egypt)")]
        public string? PersonalNumberArabic { get; set; }

        public string? Sex { get; set; }
        public string? Status { get; set; }
        public string? Surname { get; set; }

        [JsonPropertyName("Surname-Arabic (Egypt)")]
        public string? SurnameArabic { get; set; }
    }




}
