namespace Biometrics.Palm {
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System;

    using OpenCvSharp;

    public static class Dump {
        public static class ROI {
            public static void Create (string path, List<PalmModel> listOfPalms) {
                Console.WriteLine ($"[{DateTime.Now}] Creating dump file with ROI images. ");

                listOfPalms.ForEach (x => {
                    OpenCvSharp.Extensions.BitmapConverter.ToBitmap (x.ROI);
                    using (var fs = new FileStorage ($"{path}{x.FileName}.yaml", FileStorage.Mode.Write | FileStorage.Mode.FormatYaml)) {
                        fs.Write ("Id", x.Id);
                        fs.Write ("Owner", x.Owner);
                        fs.Write ("FileName", x.FileName);
                        fs.Write ("Directory", x.Directory);
                        fs.Write ("SourceImage", x.SourceImage);
                        fs.Write ("ThresholdImage", x.ThresholdImage);
                        fs.Write ("Width", x.Height);
                        fs.Write ("Height", x.Height);
                        fs.Write ("ROI", x.ROI);
                    }
                });

                Console.WriteLine ($"[{DateTime.Now}] Dump created! {path}");
            }

            public static List<PalmModel> Load (string path, List<string> listOfImages) {
                Console.WriteLine ("Loading dump ROI file");
                var listOfPalms = new List<PalmModel> ();

                listOfImages.ForEach (x => {
                    x = x.Remove (0, x.LastIndexOf ('\\') + 1).Replace (".jpg", "");
                    using (var fs = new FileStorage ($"{path}{x}.yaml", FileStorage.Mode.Read)) {
                        listOfPalms.Add (new PalmModel () {
                            Id = fs["Id"].ReadString (),
                                Owner = fs["Owner"].ReadString (),
                                FileName = fs["FileName"].ReadString (),
                                Directory = fs["Directory"].ReadString (),
                                SourceImage = fs["SourceImage"].ReadMat (),
                                ThresholdImage = fs["ThresholdImage"].ReadMat (),
                                Width = fs["Width"].ReadInt (),
                                Height = fs["Height"].ReadInt (),
                                ROI = fs["ROI"].ReadMat ()
                        });
                    }
                });

                Console.WriteLine ("Dump was loaded. ");
                Console.WriteLine ($"Total loaded palms: {listOfPalms.Count}");

                return listOfPalms;
            }
        }
    }
}