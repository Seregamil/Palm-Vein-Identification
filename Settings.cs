using System.Security.Cryptography.X509Certificates;

namespace Biometrics.Palm {
    public class Settings {
        public const byte COLOR_WHITE = 255;
        public const byte COLOR_BLACK = 0;
    
        public class Images {
            public const string Source = @"small-db\"; // path to images
            public const string Output = @"small-db\output"; // path to output directory
            public const string Dump = @"small-db\output\bin\";
        }

        public static readonly OpenCvSharp.Size ElementSize = new OpenCvSharp.Size (3, 3);
    }
}