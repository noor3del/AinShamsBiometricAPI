namespace AinShamsBiometric.Contracts.Responses
{
    public record IcaoChecksDto(
      bool FaceDetected,
      bool ExpressionOk,
      bool DarkGlassesOk,
      bool BlinkOk,
      bool MouthClosed,
      bool LookingStraight,
      bool RedEyeFree,
      bool FaceBrightnessOk,
      bool SkinToneNatural,
      bool ColorsNotWashedOut,
      bool NoPixelation,
      bool NoSkinReflection,
      bool NoGlassesReflection,
      bool RollOk,
      bool YawOk,
      bool PitchOk,
      bool DistanceOk,
      bool PositionOk,
      bool NoHeavyFrame,
      int? Sharpness,
      int? Saturation,
      int? GrayscaleDensity,
      int? BackgroundUniformity,
      bool LivenessOk,

      byte? Age,
      string? Gender,
      byte? GenderConfidence
  );





    public record IcaoCheckDto(
    bool Value,
    string Status // "Correct" or "Incorrect"
);

    public record IcaoResultDto(
     bool IsCompliant,
     double? OverallScore,
     List<string>? Warnings,
     IcaoChecksDto? Attributes
 );


    public sealed record FaceDetectionDto(
            RectDto BoundingRect,
            FeaturePointDto? LeftEyeCenter,
            FeaturePointDto? RightEyeCenter,
            FeaturePointDto? MouthCenter,
            FeaturePointDto? NoseTip,
            byte? Age,                 // 254 => not detected; we’ll normalize to null in mapping
            string? Gender,            // “Male”/“Female” or null
            byte? GenderConfidence,    // 0-255; normalize if needed
            IReadOnlyList<FeaturePointDto> AllFeaturePoints
        );
    public sealed record RectDto(int X, int Y, int Width, int Height);

    public sealed record FeaturePointDto(ushort X, ushort Y, byte Confidence);

}
