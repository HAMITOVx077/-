namespace МаршрутСборки.Helpers
{
    public static class AppSettings
    {
        private static string? _outputDirectory;

        public static string OutputDirectory
        {
            get => string.IsNullOrEmpty(_outputDirectory)
                ? Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                : _outputDirectory;
            set => _outputDirectory = value;
        }

        public static bool IsCustomDirectory => !string.IsNullOrEmpty(_outputDirectory);
    }
}
