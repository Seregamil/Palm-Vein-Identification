namespace Biometrics.Palm {
    using System.Collections.Generic;

    using OpenCvSharp;

    public class UserModel {
        public string Name { get; set; }
        public List<Mat> Patterns { get; set; }
        public Mat Model { get; set; }

        public string Directory { get; set; }
    }
}