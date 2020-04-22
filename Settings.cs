namespace Biometrics.Palm {
    public class Settings {
        public class Images {

            public const string Source = "../images/"; // path to images
            public const string Output = "Model/"; // path to output directory
        }

        public static readonly OpenCvSharp.Size ElementSize = new OpenCvSharp.Size (3, 3);
    }
}