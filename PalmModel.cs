namespace Biometrics.Palm
{
    public class PalmModel
    {
        public string Owner { get; set; }
        public string Id { get; set; }
        public string Directory { get; set; }
        public string Path { get; set; }
        public string Filename { get; set; }
        public char Type { get; set; }
    }
}