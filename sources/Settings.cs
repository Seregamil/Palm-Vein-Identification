namespace Biometrics.Palm
{
    public class Settings
    {
        public const byte COLOR_WHITE = 255;
        public const byte COLOR_BLACK = 0;


        public const int scoreForApply = 2;
        public const string imagesFolderPath = @"F:/Palm-vein-ID/small-db"; // path to images
        public const string imagesOutputDirectory = @"F:/Palm-vein-ID/small-db/output"; // path to output directory
        public const string binaryROIDirectory = "F:/Palm-vein-ID/small-db/output/bin/";
        public static OpenCvSharp.Size elementSize = new OpenCvSharp.Size(3, 3);
    }
}