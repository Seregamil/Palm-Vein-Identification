namespace Biometrics.Palm {
    public class Settings {
        public const byte COLOR_WHITE = 255;
        public const byte COLOR_BLACK = 0;

        public const int ScoreForApply = 2;

        public class Images {
            public const string Source = @"C:\Users\Сергей\Desktop\сергей\Palm-Vein-Identification\sources\small-db"; // path to images
            public const string Output = @"C:\Users\Сергей\Desktop\сергей\Palm-Vein-Identification\sources\small-db\output"; // path to output directory
            public const string Dump = @"C:\Users\Сергей\Desktop\сергей\Palm-Vein-Identification\sources\small-db\output\bin\";
        }

        public static readonly OpenCvSharp.Size ElementSize = new OpenCvSharp.Size (3, 3);
    }
}