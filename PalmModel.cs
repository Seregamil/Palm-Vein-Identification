using OpenCvSharp;

namespace Biometrics.Palm {
    public class PalmModel {
        public string Id { get; set; }
        public string Owner { get; set; }
        public char Type { get; set; }
        public string FileName { get; set; }
        public string Directory { get; set; }
        public Mat SourceImage { get; set; }
        public Mat ThresholdImage { get; set; } // = binary

        // get image params
        public int Width { get; set; }
        public int Height { get; set; }

        public Mat ROI { get; set; }

        public Mat Model { get; set; } = new Mat ();
    }
}