namespace Biometrics.Palm {
    public class Settings {        
        public const byte COLOR_WHITE = 255;
        public const byte COLOR_BLACK = 0;
    
        public class Images {
            public const string Source = @"..\images\"; // path to images
            public const string Output = @"images\output\"; // path to output directory
            public const string Dump = @"images\output\bin\";
            public const string CSV = @"images\data.csv";
        }

        // public static readonly OpenCvSharp.Size ElementSize = new OpenCvSharp.Size (3, 3);
        public static readonly OpenCvSharp.Size ElementSize = new OpenCvSharp.Size (6, 6);
    }
}