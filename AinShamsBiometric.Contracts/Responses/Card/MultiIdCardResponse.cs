using AinShamsBiometric.Contracts.Responses.Card.Common;

namespace AinShamsBiometric.Contracts.Responses.Card
{
    public class MultiIdCardResponse
    {
        public string Status { get; set; } = string.Empty;

        public IdCardResult Result { get; set; } = new();
    }
    public class IdCardResult
    {
        public IdCardFrontResult Front { get; set; } = new();
        public IdCardBackResult Back { get; set; } = new();
    }
    public class IdCardFrontResult
    {
        public string? AddressArabic { get; set; }
        public string? Age { get; set; }
        public string? AgeAtIssue { get; set; }
        public string? DateOfBirth { get; set; }
        public string? DateOfBirthArabic { get; set; }
        public string? DateOfExpiry { get; set; }
        public string? DateOfIssue { get; set; }

        public string? DocumentName { get; set; }
        public string? DocumentNumber { get; set; }
        public string? FullName { get; set; }
        public string? FullNameArabic { get; set; }
        public string? GivenNames { get; set; }
        public string? GivenNamesArabic { get; set; }
        public string? IssuingStateCode { get; set; }
        public string? IssuingStateName { get; set; }
        public string? PersonalNumber { get; set; }
        public string? PersonalNumberArabic { get; set; }
        public string? RemainderTerm { get; set; }
        public string? Sex { get; set; }
        public string? Surname { get; set; }
        public string? SurnameArabic { get; set; }
        public string? YearsSinceIssue { get; set; }
        public FrontIdCardImage Images { get; set; } = new();
    }
    public class IdCardBackResult
    {
        public string? DateOfExpiryArabic { get; set; }
        public string? DateOfIssueArabic { get; set; }
        public string? DocumentName { get; set; }
        public BackIdCardImage Images { get; set; } = new();
        public string? AddressArabic { get; set; }
        public string? MaritalStatusArabic { get; set; }
        public string? NationalityArabic { get; set; }
        public string? Other { get; set; }
        public string? PersonalNumberArabic { get; set; }
        public string? ProfessionArabic { get; set; }
        public string? SexArabic { get; set; }
    }




}
