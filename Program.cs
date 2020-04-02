// #define SAVEALLRESULTS

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using MoreLinq;

using OpenCvSharp;

using CsvHelper;

namespace Biometrics.Palm {
    public class Program {
        public static List<UserModel> UsersList = new List<UserModel> ();
        public static List<PalmModel> PalmsList = new List<PalmModel> ();

        static void Main (string[] args) {
            if(!Directory.Exists(Settings.Images.Output))
                Directory.CreateDirectory(Settings.Images.Output);
            
            if(!Directory.Exists(Settings.Images.Dump))
                Directory.CreateDirectory(Settings.Images.Dump);

            var listOfImages = Directory.GetFiles (Settings.Images.Source, "*.jpg").ToList ();

            //!Uncomment next line for select images by freq
            listOfImages = listOfImages.Where(x => x.Contains("940")).ToList();

            //! Comment from this to dump loading for dump load, lol
            //? get list of all filenames without .jpg and generate cntr PalmsList
            listOfImages.ForEach (x => {
                x = x.Remove (0, x.LastIndexOf ('\\') + 1).Replace (".jpg", "");
                var owner = x.Substring (0, x.Length - 3);

                PalmsList.Add (new PalmModel () {
                    Id = x.Substring (x.Length - 2),
                    Owner = owner,
                    FileName = x,
                    Directory = $@"{Settings.Images.Output}{owner}"
                });
            });

            //? get names and create name collection
            //? create dirs for saving data
            var userNames = PalmsList.DistinctBy (x => x.Owner);
            userNames.ForEach (x => {

                UsersList.Add (new UserModel () {
                    Name = x.Owner,
                    Patterns = new List<Mat> (),
                    Directory = x.Directory
                });

                if (Directory.Exists (x.Directory) && Directory.GetFiles (x.Directory).Length > 0) {
                    Directory.Delete (x.Directory, true); // recursive
                }

                Directory.CreateDirectory (x.Directory);
            });

            Console.WriteLine ($"Total users: {UsersList.Count}");
            Console.WriteLine ($"Total palm collection: {PalmsList.Count}");

            var totalROIExtractionTime = new Stopwatch ();
            totalROIExtractionTime.Start ();

            //! ROI extraction
            Console.WriteLine ($"[{DateTime.Now}] ROI extraction");
            var ROITask = Task.Factory.StartNew (() => {
                PalmsList.ForEach (x => {
                    Task.Factory.StartNew (() => {
                        var path = $@"{Settings.Images.Source}\{x.FileName}.jpg";
                        x.SourceImage = Cv2.ImRead (path, ImreadModes.Color);

                        x.Height = x.SourceImage.Size ().Height;
                        x.Width = x.SourceImage.Size ().Width;

                        //! apply threshold
                        Cv2.CvtColor (x.SourceImage, x.SourceImage, ColorConversionCodes.BGR2GRAY);
                        
                        //                                          0, 255
                        x.ThresholdImage = x.SourceImage.Threshold (5, 255, ThresholdTypes.Otsu);

                        // save for debug
#if SAVEALLRESULTS
                        Cv2.ImWrite ($@"{x.Directory}\binary_{x.Id}.jpg", x.ThresholdImage);
#endif

                        //! ROI extraction

                        var i1 = x.Height - 50;
                        var i2 = x.Width - 50;

                        var radius = 50;
                        int pX = 0;
                        int pY = 0;

                        for (int i = 50; i != i1; i++) {
                            for (int j = 50; j != i2; j++) {
                                if (x.ThresholdImage.Get<byte> (i, j) == Settings.COLOR_WHITE) {
                                    int a = 0;
                                    for (a = 1; a < 360; a++) {
                                        var y1 = Convert.ToInt16 (j + radius * Math.Cos (a * Math.PI / 180));
                                        var x1 = Convert.ToInt16 (i - radius * Math.Sin (a * Math.PI / 180));

                                        if (x1 < 1 || x1 > i1 || y1 < 1 || y1 > i2 || x.ThresholdImage.Get<byte> (x1, y1) == Settings.COLOR_BLACK)
                                            break;
                                    }

                                    if (a == 360) {
                                        radius += 10;
                                        pX = i;
                                        pY = j;
                                    }
                                }
                            }
                        }

                        radius -= 10;

                        var x0 = Convert.ToInt16 (pY - Math.Sqrt (2) * radius / 2);
                        var y0 = Convert.ToInt16 (pX - Math.Sqrt (2) * radius / 2);
                        var wsize = Convert.ToInt16 (Math.Sqrt (2) * radius);

                        var rect = new Rect (x0, y0, wsize, wsize);

                        // for visual debug
                        Mat drawROIImage = new Mat ();
                        x.SourceImage.CopyTo (drawROIImage);
                        drawROIImage.Rectangle (rect, Scalar.White);

                        x.ROI = new Mat (x.SourceImage, rect)
                            .Resize (new Size (216, 216));

                        Cv2.Rotate (x.ROI, x.ROI, RotateFlags.Rotate90Counterclockwise);

#if SAVEALLRESULTS
                        Cv2.ImWrite ($@"{x.Directory}\ROIOnSource_{x.Id}.jpg", drawROIImage);
                        Cv2.ImWrite ($@"{x.Directory}\ROI_{x.Id}.jpg", x.ROI);
#endif

                    }, TaskCreationOptions.AttachedToParent | TaskCreationOptions.RunContinuationsAsynchronously);
                });
            });

            ROITask.Wait ();
            totalROIExtractionTime.Stop ();
            Console.WriteLine ($"[{DateTime.Now}] Total ROI extracton time: {totalROIExtractionTime.Elapsed}");

            //! Create dump
            Dump.ROI.Create (Settings.Images.Dump, PalmsList);

            //! Uncomment next block for loading images from dump and comment all lines before this
            // PalmsList = Dump.ROI.Load(Settings.Images.Dump, listOfImages);
            // var countFrom = 0;
            // var countTo = PalmsList.Count;
            // ------------------------------

            //! Apply filters

            Console.WriteLine ($"[{DateTime.Now}] Apply filters to ROI image");
            totalROIExtractionTime.Reset ();
            totalROIExtractionTime.Start ();

            var filtersTask = Task.Factory.StartNew (() => {
                PalmsList.ForEach (x => {
                    Task.Factory.StartNew (() => {
                        //! Reduce noise
                        Cv2.MedianBlur (x.ROI, x.ROI, 5);

#if SAVEALLRESULTS
                        Cv2.ImWrite ($@"{x.Directory}\ROI_Median_{x.Id}.jpg", x.ROI);
#endif

                        // Cv2.CvtColor(x.ROI, x.ROI, ColorConversionCodes.BGR2GRAY);
                        Cv2.FastNlMeansDenoising (x.ROI, x.ROI);
                        Cv2.CvtColor (x.ROI, x.ROI, ColorConversionCodes.GRAY2BGR);

#if SAVEALLRESULTS
                        Cv2.ImWrite ($@"{x.Directory}\reduce_noise_{x.Id}.jpg", x.ROI);
#endif

                        //! Equalize hist
                        var element = Cv2.GetStructuringElement (MorphShapes.Cross, Settings.ElementSize); // new Mat(7, 7, MatType.CV_8U);
                        Cv2.MorphologyEx (x.ROI, x.ROI, MorphTypes.Open, element);
                        Cv2.CvtColor (x.ROI, x.ROI, ColorConversionCodes.BGR2YUV);

                        // Cv2.EqualizeHist(x.ROI, x.ROI);
                        var RGB = Cv2.Split (x.ROI);

                        RGB[0] = RGB[0].EqualizeHist ();
                        RGB[1] = RGB[1].EqualizeHist ();
                        RGB[2] = RGB[2].EqualizeHist ();

                        Cv2.Merge (RGB, x.ROI);
                        Cv2.CvtColor (x.ROI, x.ROI, ColorConversionCodes.YUV2BGR);

#if SAVEALLRESULTS
                        Cv2.ImWrite ($@"{x.Directory}\equalized_hist_{x.Id}.jpg", x.ROI);
#endif

                        //! Invert image
                        Cv2.BitwiseNot (x.ROI, x.ROI);

#if SAVEALLRESULTS
                        Cv2.ImWrite ($@"{x.Directory}\inverted_{x.Id}.jpg", x.ROI);
#endif

                        //! Erode image
                        Cv2.CvtColor (x.ROI, x.ROI, ColorConversionCodes.BGR2GRAY);
                        Cv2.Erode (x.ROI, x.ROI, element);

#if SAVEALLRESULTS
                        Cv2.ImWrite ($@"{x.Directory}\eroded_{x.Id}.jpg", x.ROI);
#endif

                        //! Skeletonize
                        var skel = new Mat (x.ROI.Size (), MatType.CV_8UC1, new Scalar (0));
                        var temp = new Mat ();
                        var eroded = new Mat ();

                        do {
                            Cv2.MorphologyEx (x.ROI, eroded, MorphTypes.Erode, element);
                            Cv2.MorphologyEx (eroded, temp, MorphTypes.Dilate, element);
                            Cv2.Subtract (x.ROI, temp, temp);
                            Cv2.BitwiseOr (skel, temp, skel);
                            eroded.CopyTo (x.ROI);

                        } while (Cv2.CountNonZero (x.ROI) != 0);

                        //! Threshold skeletonized image
                        var thr = skel.Threshold (0, 255, ThresholdTypes.Binary);

                        //! Remove contours 
                        thr.Line(new Point(0, 0), new Point(0, thr.Height), Scalar.Black, 2); // rm left contour
                        thr.Line(new Point(0, 0), new Point(thr.Width, 0), Scalar.Black, 2); // rm top contour

                        thr.Line(new Point(thr.Width, thr.Height), new Point(thr.Width, 0), Scalar.Black, 2); // rm right contour
                        thr.Line(new Point(thr.Width, thr.Height), new Point(0, thr.Height), Scalar.Black, 2); // rm bot contour

                        //! Normalize contours
                        element = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(6, 6));

                        Cv2.Dilate(thr, thr, element);
                        Cv2.Erode(thr, thr, element);

                        Cv2.MorphologyEx(thr, thr, MorphTypes.Gradient, element);

                        Cv2.ImWrite ($@"{x.Directory}\{x.Id}.jpg", thr);

                        var owner = UsersList.Find (u => u.Name == x.Owner);
                        owner.Patterns.Add (thr); // add thresholded image to user patterns
                    }, TaskCreationOptions.AttachedToParent | TaskCreationOptions.RunContinuationsAsynchronously);
                });
            });

            filtersTask.Wait ();
            totalROIExtractionTime.Stop ();
            Console.WriteLine ($"[{DateTime.Now}] Total apply filters time: {totalROIExtractionTime.Elapsed}");

            //! Create dump with users and they patterns
            Dump.Patterns.Create(Settings.Images.Dump, UsersList);

            //! Create CSV file
            Dump.CSV.Create(Settings.Images.CSV, PalmsList);
        }
    }
}