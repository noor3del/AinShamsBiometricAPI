using AinShamsBiometric.Application.Interfaces;
using AinShamsBiometric.Contracts.Responses;
using Microsoft.Extensions.Logging;
using Neurotec.Biometrics;
using Neurotec.Biometrics.Client;
using Neurotec.Images;
using Neurotec.IO;
using System.Diagnostics;

namespace AinShamsBiometric.Application.Services;

public sealed class FaceService : IFaceService
{

    private readonly NBiometricClient _client;
    private readonly ILogger<FaceService> _log;
    public FaceService(NBiometricClient client, ILogger<FaceService> log)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _log = log;
        ConfigureClient(_client);
    }
    /// <summary>
    /// Segment + background removal + red-eye removal, then return Base64 JPEG (compressed and resized).
    /// </summary>
    public async Task<string> SegmentImageAsync(Stream imageStream, string? fileName = null, CancellationToken ct = default)
    {
        if (imageStream is null || !imageStream.CanRead)
            throw new ArgumentException("Invalid image stream", nameof(imageStream));

        ct.ThrowIfCancellationRequested();

        using var subject = new NSubject();
        using var nstream = NStream.FromStream(imageStream);
        using var nimg = NImage.FromStream(nstream);
        using var face = new NFace { Image = nimg };
        if (!string.IsNullOrWhiteSpace(fileName)) face.FileName = fileName;


        subject.Faces.Add(face);

        subject.IsMultipleSubjects = true;

        // enable ICAO processing
        _client.FacesIcaoRemoveBackground = true;
        _client.FacesIcaoRemoveRedEye = true;
        _client.FacesTemplateSize = NTemplateSize.Small;
        _client.FacesMinimalInterOcularDistance = 32; //32

        var task = _client.CreateTask(
            NBiometricOperations.CreateTemplate |
            NBiometricOperations.AssessQuality |
            NBiometricOperations.Segment,
            subject);

        var sw = Stopwatch.StartNew();
        await _client.PerformTaskAsync(task);
        sw.Stop();
        var segmentedFace = subject.Faces.LastOrDefault()?.Image;
        if (segmentedFace == null)
            throw new InvalidOperationException("No segmented face found");

        var buffer = segmentedFace.Save(NImageFormat.Jpeg); // returns NBuffer
        var base64 = Convert.ToBase64String(buffer.ToArray());

        NLAttributes? attributes = face.Objects?.FirstOrDefault();
        _log.LogInformation("Segmented {File} in {Elapsed} ms; original={OrigW}x{OrigH}, quality={Quality}, bytes={Bytes}",
         fileName, sw.ElapsedMilliseconds, segmentedFace.Width, segmentedFace.Height, attributes?.Quality, buffer.Size);

        return base64;

    }

    public async Task<IcaoResultDto> CheckIcaoAsync(string imagePath, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(imagePath);
        using var subject = new NSubject();
        using var face = new NFace { FileName = imagePath };
        subject.Faces.Add(face);
        subject.IsMultipleSubjects = true;

        var task = _client.CreateTask(
            NBiometricOperations.DetectSegments | NBiometricOperations.AssessQuality,
            subject);

        await Task.Run(() => _client.PerformTask(task), ct);

        return BuildResult(task, face);
    }

    public async Task<IcaoResultDto> CheckIcaoAsync(Stream imageStream, string? fileName = null, CancellationToken ct = default)
    {
        if (imageStream is null || !imageStream.CanRead) throw new ArgumentException("Invalid image stream", nameof(imageStream));
        using var subject = new NSubject();
        using var nstream = NStream.FromStream(imageStream);
        using var nimg = NImage.FromStream(nstream);
        using var face = new NFace { Image = nimg };
        if (!string.IsNullOrWhiteSpace(fileName)) face.FileName = fileName;

        subject.Faces.Add(face);
        subject.IsMultipleSubjects = true;

        var task = _client.CreateTask(
            NBiometricOperations.DetectSegments | NBiometricOperations.AssessQuality,
            subject);

        await Task.Run(() => _client.PerformTask(task), ct);

        return BuildResult(task, face);
    }

    #region Helpers 

    private static void ConfigureClient(NBiometricClient client)
    {
        client.FacesCheckIcaoCompliance = true;
        client.FacesDetectAllFeaturePoints = true;
        client.FacesDetectProperties = true;
        client.FacesDetermineGender = true;
        client.FacesDetermineAge = true;
    }


    private static bool PassIfNo(NIcaoWarnings warnings, NIcaoWarnings flag)
    => (warnings & flag) != flag;


    private static bool CheckForFlags(NIcaoWarnings warnings, params NIcaoWarnings[] flags)
    {
        return flags.Any(f => (f & warnings) == f) ? false : true;
    }
    private static bool CheckForConfidence(NIcaoWarnings warnings, NIcaoWarnings flag, byte confidence)
    {
        if ((warnings & flag) == flag)
        {
            return confidence <= 100 ? false : true;
        }
        return true;
    }


    private static int? AsScore(NLAttributes attrs, NBiometricAttributeId id)
    {
        try
        {
            var v = attrs.GetAttributeValue(id);
            return v <= 100 ? v : null;
        }
        catch { return null; }
    }
    private static IcaoChecksDto BuildIcaoChecks(NLAttributes attrs)
    {
        var warnings = attrs.IcaoWarnings;

        return new IcaoChecksDto(
            FaceDetected: PassIfNo(warnings, NIcaoWarnings.FaceNotDetected),
            ExpressionOk: CheckForConfidence(warnings, NIcaoWarnings.Expression, attrs.GetAttributeValue(NBiometricAttributeId.Smile)),
            DarkGlassesOk: CheckForConfidence(warnings, NIcaoWarnings.DarkGlasses, attrs.GetAttributeValue(NBiometricAttributeId.DarkGlasses)),
            BlinkOk: CheckForConfidence(warnings, NIcaoWarnings.Blink, attrs.GetAttributeValue(NBiometricAttributeId.EyesOpen)),
            MouthClosed: CheckForConfidence(warnings, NIcaoWarnings.MouthOpen, attrs.GetAttributeValue(NBiometricAttributeId.MouthOpen)),
            LookingStraight: CheckForConfidence(warnings, NIcaoWarnings.LookingAway, attrs.GetAttributeValue(NBiometricAttributeId.LookingAway)),
            RedEyeFree: CheckForConfidence(warnings, NIcaoWarnings.RedEye, attrs.GetAttributeValue(NBiometricAttributeId.RedEye)),
            FaceBrightnessOk: CheckForConfidence(warnings, NIcaoWarnings.FaceDarkness, attrs.GetAttributeValue(NBiometricAttributeId.FaceDarkness)),
            SkinToneNatural: CheckForConfidence(warnings, NIcaoWarnings.UnnaturalSkinTone, attrs.GetAttributeValue(NBiometricAttributeId.SkinTone)),
            ColorsNotWashedOut: CheckForConfidence(warnings, NIcaoWarnings.WashedOut, attrs.GetAttributeValue(NBiometricAttributeId.WashedOut)),
            NoPixelation: CheckForConfidence(warnings, NIcaoWarnings.Pixelation, attrs.GetAttributeValue(NBiometricAttributeId.Pixelation)),
            NoSkinReflection: CheckForConfidence(warnings, NIcaoWarnings.SkinReflection, attrs.GetAttributeValue(NBiometricAttributeId.SkinReflection)),
            NoGlassesReflection: CheckForConfidence(warnings, NIcaoWarnings.GlassesReflection, attrs.GetAttributeValue(NBiometricAttributeId.GlassesReflection)),

            RollOk: CheckForFlags(warnings, NIcaoWarnings.RollLeft, NIcaoWarnings.RollRight),
            YawOk: CheckForFlags(warnings, NIcaoWarnings.YawLeft, NIcaoWarnings.YawRight),
            PitchOk: CheckForFlags(warnings, NIcaoWarnings.PitchDown, NIcaoWarnings.PitchUp),
            DistanceOk: CheckForFlags(warnings, NIcaoWarnings.TooNear, NIcaoWarnings.TooFar),
            PositionOk: CheckForFlags(warnings, NIcaoWarnings.TooNorth, NIcaoWarnings.TooSouth, NIcaoWarnings.TooEast, NIcaoWarnings.TooWest),
            NoHeavyFrame: CheckForFlags(warnings, NIcaoWarnings.HeavyFrame),

            Sharpness: AsScore(attrs, NBiometricAttributeId.Sharpness),
            Saturation: AsScore(attrs, NBiometricAttributeId.Saturation),
            GrayscaleDensity: AsScore(attrs, NBiometricAttributeId.GrayscaleDensity),
            BackgroundUniformity: AsScore(attrs, NBiometricAttributeId.BackgroundUniformity),

            LivenessOk: PassIfNo(warnings, NIcaoWarnings.Liveness),

            Age: NormalizeAge(attrs.GetAttributeValue(NBiometricAttributeId.Age)),
            Gender: NormalizeGender(attrs.Gender),
            GenderConfidence: attrs.GenderConfidence is 255 ? null : attrs.GenderConfidence
        );
    }


    private static IcaoResultDto BuildResult(NBiometricTask task, NFace face)
    {
        if (task.Status != NBiometricStatus.Ok)
        {
            if (task.Error is not null) throw task.Error;
            throw new InvalidOperationException($"Face processing failed. Status = {task.Status}");
        }

        var attrs = face.Objects?.FirstOrDefault();
        if (attrs is null)
        {
            return new IcaoResultDto(false, null, new() { "No face detected" }, null);
        }

        var checks = BuildIcaoChecks(attrs);

        // collect failed checks
        var warnings = new List<string>();
        if (!checks.ExpressionOk) warnings.Add("Expression issue (Smile/Expression)");
        if (!checks.DarkGlassesOk) warnings.Add("Dark glasses detected");
        if (!checks.BlinkOk) warnings.Add("Possible blink");
        if (!checks.MouthClosed) warnings.Add("Mouth open");
        if (!checks.LookingStraight) warnings.Add("Looking away");
        if (!checks.RedEyeFree) warnings.Add("Red eye detected");
        if (!checks.FaceBrightnessOk) warnings.Add("Face brightness issue");
        if (!checks.SkinToneNatural) warnings.Add("Unnatural skin tone");
        if (!checks.ColorsNotWashedOut) warnings.Add("Colors washed out");
        if (!checks.NoPixelation) warnings.Add("Pixelation detected");
        //if (!checks.NoSkinReflection) warnings.Add("Skin reflection issue");
        if (!checks.NoGlassesReflection) warnings.Add("Glasses reflection issue");
        if (!checks.RollOk) warnings.Add("Head roll out of tolerance");
        if (!checks.YawOk) warnings.Add("Head yaw out of tolerance");
        if (!checks.PitchOk) warnings.Add("Head pitch out of tolerance");
        if (!checks.DistanceOk) warnings.Add("Subject too near/far");
        if (!checks.PositionOk) warnings.Add("Subject not centered");
        if (!checks.NoHeavyFrame) warnings.Add("Heavy frame glasses detected");
        if (!checks.LivenessOk) warnings.Add("Liveness issue");

        return new IcaoResultDto(
            IsCompliant: warnings.Count == 0,
            OverallScore: attrs.Quality,
            Warnings: warnings,
            Attributes: checks
        );
    }


    private static byte? NormalizeAge(byte? v) => v is null or 254 ? null : v;

    private static string? NormalizeGender(NGender gender)
    {
        // Depending on SDK, NGender.Unknown might be default
        return gender switch
        {
            NGender.Female => "Female",
            NGender.Male => "Male",
            _ => null
        };
    }

    #endregion
}
