namespace Biometrics.Palm
{
    using OpenCvSharp;
    using System.Collections.Generic;

    public class UserModel
    {
        public string Name { get; set; }
        public List<Mat> Patterns { get; set; }
        public Mat Model { get; set; }

        public string Directory { get; set; }

    }
}