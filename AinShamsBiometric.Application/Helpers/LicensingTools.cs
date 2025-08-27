using Neurotec.Biometrics;
using Neurotec.Licensing;

namespace AinShamsBiometric.Application.Helpers
{
    public static class LicensingTools
    {
        #region Private static fields

        private static Dictionary<string, bool> components = new Dictionary<string, bool>();

        #endregion

        #region Public static methods

        public static bool IsComponentActivated(string component)
        {
            if (components.ContainsKey(component))
                return components[component];

            bool result = NLicense.IsComponentActivated(component);
            components[component] = result;
            return result;
        }

        public static bool CanCreateFaceTemplate(NBiometricOperations localOperations)
        {
            return IsComponentActivated("Biometrics.FaceExtraction") || (localOperations & NBiometricOperations.CreateTemplate) == 0;
        }

        public static bool CanDetectFaceSegments(NBiometricOperations localOperations)
        {
            return IsComponentActivated("Biometrics.FaceSegmentation") || (localOperations & NBiometricOperations.DetectSegments) == 0;
        }

        public static bool CanCreatePalmTemplate(NBiometricOperations localOperations)
        {
            return IsComponentActivated("Biometrics.PalmExtraction") || (localOperations & NBiometricOperations.CreateTemplate) == 0;
        }

        public static bool CanCreateIrisTemplate(NBiometricOperations localOperations)
        {
            return IsComponentActivated("Biometrics.IrisExtraction") || (localOperations & NBiometricOperations.CreateTemplate) == 0;
        }

        public static bool CanSegmentIris(NBiometricOperations localOperations)
        {
            return IsComponentActivated("Biometrics.IrisSegmentation") || (localOperations & NBiometricOperations.Segment) == 0;
        }

        public static bool CanCreateVoiceTemplate(NBiometricOperations localOperations)
        {
            return IsComponentActivated("Biometrics.VoiceExtraction") || (localOperations & NBiometricOperations.CreateTemplate) == 0;
        }

        public static bool CanSegmentVoice(NBiometricOperations localOperations)
        {
            return IsComponentActivated("Biometrics.VoiceSegmentation") || (localOperations & NBiometricOperations.Segment) == 0;
        }

        public static bool CanCreateFingerTemplate(NBiometricOperations localOperations)
        {
            return IsComponentActivated("Biometrics.FingerExtraction") || (localOperations & NBiometricOperations.CreateTemplate) == 0;
        }

        public static bool CanSegmentFinger(NBiometricOperations localOperations)
        {
            return IsComponentActivated("Biometrics.FingerSegmentation") || (localOperations & NBiometricOperations.Segment) == 0;
        }

        public static bool CanDetectFingerSegments(NBiometricOperations localOperations)
        {
            return IsComponentActivated("Biometrics.FingerSegmentsDetection") || (localOperations & NBiometricOperations.DetectSegments) == 0;
        }

        public static bool CanAssessFingerQuality(NBiometricOperations localOperations)
        {
            return IsComponentActivated("Biometrics.FingerQualityAssessmentBase") || (localOperations & NBiometricOperations.AssessQuality) == 0;
        }

        public static bool CanFingerBeMatched(NBiometricOperations remoteOperations)
        {
            return IsComponentActivated("Biometrics.FingerMatching") || (remoteOperations & NBiometricOperations.VerifyOffline) == NBiometricOperations.VerifyOffline;
        }

        public static void CheckTrial()
        {

            NLicenseManager.TrialMode = Utilities.GetTrialModeFlag();

            Console.WriteLine("Trial mode: " + NLicenseManager.TrialMode);

        }

        #endregion
    }
}
