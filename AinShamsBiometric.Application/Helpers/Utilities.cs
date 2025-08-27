using System.Globalization;

namespace AinShamsBiometric.Application.Helpers
{
    public class Utilities
    {
        private const string ERROR_TITLE = "Error";
        private const string INFORMATION_TITLE = "Information";
        private const string QUESTION_TITLE = "Question";

        public static bool GetTrialModeFlag()
        {
            var filePath = @"./../Licenses/TrialFlag.txt";

            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                if (lines.Length > 0 && lines[0].Trim().ToLower().Equals("true"))
                {
                    return true;
                }
            }
            else
            {
                Console.WriteLine("Failed to locate file: " + Path.GetFullPath(filePath));
            }
            return false;
        }

        public static string MatchingThresholdToString(int value)
        {
            double p = -value / 12.0;
            return string.Format(string.Format("{{0:P{0}}}", Math.Max(0, (int)Math.Ceiling(-p) - 2)), Math.Pow(10, p));
        }

        public static int MatchingThresholdFromString(string value)
        {
            double p = Math.Log10(Math.Max(double.Epsilon, Math.Min(1, double.Parse(value.Replace(CultureInfo.CurrentCulture.NumberFormat.PercentSymbol, "")) / 100)));
            return Math.Max(0, (int)Math.Round(-12 * p));
        }

        public static string GetUserLocalDataDir(string productName)
        {
            string localDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            localDataDir = Path.Combine(localDataDir, "AinShamsBiometric");
            if (!Directory.Exists(localDataDir))
            {
                Directory.CreateDirectory(localDataDir);
            }
            localDataDir = Path.Combine(localDataDir, productName);
            if (!Directory.Exists(localDataDir))
            {
                Directory.CreateDirectory(localDataDir);
            }

            return localDataDir;
        }
    }
}
