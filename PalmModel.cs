using OpenCvSharp;

namespace Biometrics.Palm {
    public class PalmModel {
        public string Id { get; set; }
        public string Owner { get; set; }
        public string FileName { get; set; }
        public string Directory { get; set; }
        public Mat SourceImage { get; set; }
        public Mat ThresholdImage { get; set; } // = binary

        public Mat ROI { get; set; }
    }
}