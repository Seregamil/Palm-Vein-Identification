namespace Biometrics.Palm {
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System;
    using System.Globalization;
    using CsvHelper;

    using OpenCvSharp;

    public static class Dump {
        public static class CSV {
            public static void Create(string path, List<PalmModel> listOfPalms)
            {
                if(File.Exists(path))
                    File.Delete(path);

                var CVSModelList = new List<CSVModel>();
                listOfPalms.ForEach(x =>
                {
                    var model = new CSVModel()
                    {
                        Name = x.Owner,
                        Path = $@"{x.Directory}\thr_{x.Id}.jpg"
                    };

                    CVSModelList.Add(model);
                });

                using (var writer = new StreamWriter(path))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(CVSModelList);
                }
            }
        }

        public static class Patterns {
            public static void Create(string path, List<UserModel> listOfUsers) {
                if(!Directory.Exists($@"{path}\Users"))
                    Directory.CreateDirectory($@"{path}\Users");

                Console.WriteLine($"[{DateTime.Now}] Creating dump with patterns");

                listOfUsers.ForEach(x => {
                    var totalPatterns = x.Patterns.Count;

                    using (var fs = new FileStorage($@"{path}\Users\{x.Name}.yaml", FileStorage.Mode.Write | FileStorage.Mode.FormatYaml))
                    {
                        fs.Write("Name", x.Name);
                        fs.Write("Directory", x.Directory);
                        fs.Write("PatternsCount", totalPatterns);

                        var counter = -1;
                        x.Patterns.ForEach(pattern => {
                            fs.Write($"Pattern_{++counter}", pattern);
                        });
                    }
                });

                Console.WriteLine($"[{DateTime.Now}] Dump with patterns created! {path}");
            }

            public static List<UserModel> Load(string path) {
                var listOfUsers = new List<UserModel>();
                var listOfFiles = Directory.GetFiles($@"{path}\Users\", "*.yaml");

                foreach(var file in listOfFiles) {
                    using (var fs = new FileStorage($@"{path}\Users\{file}", FileStorage.Mode.Read))
                    {
                        var userModel = new UserModel() 
                        { 
                            Name = fs["Name"].ReadString(),
                            Directory = fs["Directory"].ReadString()
                        };

                        var patternsCount = fs["patternsCount"].ReadInt();
                        var patternsList = new List<Mat>();

                        for(var i = 0; i != patternsCount; i++) {
                            var pattern = fs[$"Pattern_{i}"].ReadMat();
                            patternsList.Add(pattern);
                        }
                        
                        userModel.Patterns = patternsList;
                        listOfUsers.Add(userModel);
                    }
                }

                return listOfUsers;
            }
        }

        public static class ROI {
            public static void Create (string path, List<PalmModel> listOfPalms) {
                if (!Directory.Exists($@"{path}\ROI"))
                    Directory.CreateDirectory($@"{path}\ROI");

                Console.WriteLine ($"[{DateTime.Now}] Creating dump file with ROI images. ");

                listOfPalms.ForEach (x => {
                    // OpenCvSharp.Extensions.BitmapConverter.ToBitmap (x.ROI);
                    using (var fs = new FileStorage ($@"{path}\ROI\{x.FileName}.yaml", FileStorage.Mode.Write | FileStorage.Mode.FormatYaml)) {
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
                    using (var fs = new FileStorage ($@"{path}\ROI\{x}.yaml", FileStorage.Mode.Read)) {
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