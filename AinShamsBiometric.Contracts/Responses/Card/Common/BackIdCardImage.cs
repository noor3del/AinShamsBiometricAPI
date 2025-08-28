namespace AinShamsBiometric.Contracts.Responses.Card.Common
{
    public class BackIdCardImage
    {
        public IdCardBarcode? Barcode { get; set; }
        public IdCardImage? Document { get; set; }
    }

    public class IdCardBarcode
    {
        public IdCardPosition Position { get; set; } = new();
        public string? Image { get; set; }
    }

}
