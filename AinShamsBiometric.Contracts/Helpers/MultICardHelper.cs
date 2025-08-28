using AinShamsBiometric.Contracts.Responses.Card;
using AinShamsBiometric.Contracts.Responses.Card.Common;
using System.Text.Json.Serialization;

namespace AinShamsBiometric.Contracts.Helpers
{
    public static class MultiIdCardDtoMapper
    {
        public static MultiIdCardResponse Map(WireRoot wire)
        {
            var dto = new MultiIdCardResponse
            {
                Status = wire.Status,
                Result = new IdCardResult
                {
                    Front = new IdCardFrontResult(),
                    Back = new IdCardBackResult()
                }
            };

            if (wire.Result is null || wire.Result.Count == 0)
                return dto;

            var (front, back) = SplitFrontBack(wire.Result);

            if (front != null) MapFront(front, dto.Result.Front);
            if (back != null) MapBack(back, dto.Result.Back);

            return dto;
        }

        private static (WireItem? front, WireItem? back) SplitFrontBack(List<WireItem> items)
        {
            if (items.Count == 1) return (items[0], null);

            WireItem? front = null, back = null;
            foreach (var it in items)
            {
                var name = it.DocumentName ?? string.Empty;
                if (name.Contains("Side B", StringComparison.OrdinalIgnoreCase)) back = it;
                else front ??= it;
            }
            front ??= items[0];
            back ??= items.Count > 1 ? items[1] : null;
            return (front, back);
        }

        private static void MapFront(WireItem s, IdCardFrontResult d)
        {
            d.AddressArabic = s.AddressArabic;
            d.Age = s.Age;
            d.AgeAtIssue = s.AgeAtIssue;
            d.DateOfBirth = s.DateOfBirth;
            d.DateOfBirthArabic = s.DateOfBirthArabic;
            d.DateOfExpiry = s.DateOfExpiry;
            d.DateOfIssue = s.DateOfIssue;
            d.DocumentName = s.DocumentName;
            d.DocumentNumber = s.DocumentNumber;
            d.FullName = s.FullName;
            d.FullNameArabic = s.FullNameArabic;
            d.GivenNames = s.GivenNames;
            d.GivenNamesArabic = s.GivenNamesArabic;
            d.IssuingStateCode = s.IssuingStateCode;
            d.IssuingStateName = s.IssuingStateName;
            d.PersonalNumber = s.PersonalNumber;
            d.PersonalNumberArabic = s.PersonalNumberArabic;
            d.RemainderTerm = s.RemainderTerm;
            d.Sex = s.Sex;
            d.Surname = s.Surname;
            d.SurnameArabic = s.SurnameArabic;
            d.YearsSinceIssue = s.YearsSinceIssue;

            d.Images ??= new FrontIdCardImage();
            d.Images.Document = new IdCardImage { Image = s.Images?.Document?.Image };
            d.Images.PortraitV = s.Images?.PortraitV is null ? null : new IdCardPortraitFront
            {
                Image = s.Images.PortraitV.Image,
                Position = s.Images.PortraitV.Position is null ? new IdCardPosition() : new IdCardPosition
                {
                    X1 = s.Images.PortraitV.Position.x1,
                    X2 = s.Images.PortraitV.Position.x2,
                    Y1 = s.Images.PortraitV.Position.y1,
                    Y2 = s.Images.PortraitV.Position.y2
                }
            };
        }

        private static void MapBack(WireItem s, IdCardBackResult d)
        {
            d.DateOfExpiryArabic = s.DateOfExpiryArabic;
            d.DateOfIssueArabic = s.DateOfIssueArabic;
            d.DocumentName = s.DocumentName;
            d.AddressArabic = s.AddressArabic; // may be present on back
            d.MaritalStatusArabic = s.MaritalStatusArabic;
            d.NationalityArabic = s.NationalityArabic;
            d.Other = s.Other;
            d.PersonalNumberArabic = s.PersonalNumberArabic;
            d.ProfessionArabic = s.ProfessionArabic;
            d.SexArabic = s.SexArabic;

            d.Images ??= new BackIdCardImage();
            d.Images.Document = new IdCardImage { Image = s.Images?.Document?.Image };
            d.Images.Barcode = s.Images?.Barcode is null ? null : new IdCardBarcode
            {
                Image = s.Images.Barcode.Image,
                Position = s.Images.Barcode.Position is null ? new IdCardPosition() : new IdCardPosition
                {
                    X1 = s.Images.Barcode.Position.x1,
                    X2 = s.Images.Barcode.Position.x2,
                    Y1 = s.Images.Barcode.Position.y1,
                    Y2 = s.Images.Barcode.Position.y2
                }
            };
        }
    }

    public sealed class WireRoot
    {
        public string Status { get; set; } = "";
        [JsonPropertyName("result")]
        public List<WireItem> Result { get; set; } = new();
    }

    public sealed class WireItem
    {
        // Common
        [JsonPropertyName("Document Name")] public string? DocumentName { get; set; }

        // ---- Front-like fields ----
        [JsonPropertyName("Address-Arabic (Egypt)")] public string? AddressArabic { get; set; }
        [JsonPropertyName("Age")] public string? Age { get; set; }
        [JsonPropertyName("Age at Issue")] public string? AgeAtIssue { get; set; }
        [JsonPropertyName("Date of Birth")] public string? DateOfBirth { get; set; }
        [JsonPropertyName("Date of Birth-Arabic (Egypt)")] public string? DateOfBirthArabic { get; set; }
        [JsonPropertyName("Date of Expiry")] public string? DateOfExpiry { get; set; }
        [JsonPropertyName("Date of Issue")] public string? DateOfIssue { get; set; }
        [JsonPropertyName("Document Number")] public string? DocumentNumber { get; set; }
        [JsonPropertyName("Full Name")] public string? FullName { get; set; }
        [JsonPropertyName("Full Name-Arabic (Egypt)")] public string? FullNameArabic { get; set; }
        [JsonPropertyName("Given Names")] public string? GivenNames { get; set; }
        [JsonPropertyName("Given Names-Arabic (Egypt)")] public string? GivenNamesArabic { get; set; }
        [JsonPropertyName("Issuing State Code")] public string? IssuingStateCode { get; set; }
        [JsonPropertyName("Issuing State Name")] public string? IssuingStateName { get; set; }
        [JsonPropertyName("Personal Number")] public string? PersonalNumber { get; set; }
        [JsonPropertyName("Personal Number-Arabic (Egypt)")] public string? PersonalNumberArabic { get; set; }
        [JsonPropertyName("RemainderTerm")] public string? RemainderTerm { get; set; }
        [JsonPropertyName("Sex")] public string? Sex { get; set; }
        [JsonPropertyName("Surname")] public string? Surname { get; set; }
        [JsonPropertyName("Surname-Arabic (Egypt)")] public string? SurnameArabic { get; set; }
        [JsonPropertyName("Years Since Issue")] public string? YearsSinceIssue { get; set; }

        // ---- Back-like fields ----
        [JsonPropertyName("Date of Expiry-Arabic (Egypt)")] public string? DateOfExpiryArabic { get; set; }
        [JsonPropertyName("Date of Issue-Arabic (Egypt)")] public string? DateOfIssueArabic { get; set; }
        [JsonPropertyName("Marital Status-Arabic (Egypt)")] public string? MaritalStatusArabic { get; set; }
        [JsonPropertyName("Nationality-Arabic (Egypt)")] public string? NationalityArabic { get; set; }
        [JsonPropertyName("Other")] public string? Other { get; set; }
        [JsonPropertyName("Profession-Arabic (Egypt)")] public string? ProfessionArabic { get; set; }
        [JsonPropertyName("Sex-Arabic (Egypt)")] public string? SexArabic { get; set; }

        // Images
        [JsonPropertyName("Images")] public WireImages? Images { get; set; }
    }
    public sealed class WireImages
    {
        [JsonPropertyName("Document")] public WireImageLeaf? Document { get; set; }
        [JsonPropertyName("Portrait-V")] public WireImageWithPos? PortraitV { get; set; } // front
        [JsonPropertyName("Barcode")] public WireImageWithPos? Barcode { get; set; }    // back
    }

    public class WireImageLeaf
    {
        [JsonPropertyName("image")] public string? Image { get; set; } // base64 or empty
    }

    public sealed class WireImageWithPos : WireImageLeaf
    {
        [JsonPropertyName("Position")] public WirePos? Position { get; set; }
    }

    public sealed class WirePos
    {
        public int x1 { get; set; }
        public int x2 { get; set; }
        public int y1 { get; set; }
        public int y2 { get; set; }
    }
}
