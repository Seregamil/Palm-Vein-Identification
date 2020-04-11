using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using MoreLinq;
using OpenCvSharp;

namespace Biometrics.Palm
{
    public class Program
    {
        // public static List<UserModel> UsersList = new List<UserModel>();
        public static List<PalmModel> PalmsList = new List<PalmModel>();

        static void Main(string[] args)
        {
            if (!Directory.Exists(Settings.Images.Output))
                Directory.CreateDirectory(Settings.Images.Output);

            var listOfImages = Directory.GetFiles(Settings.Images.Source, "*.jpg")
                                                    .Where(x => x.Contains("940"))
                                                    .ToList();

            //? get list of all filenames without .jpg and generate cntr PalmsList
            listOfImages.ForEach(x =>
            {
                x = x.Remove(0, x.LastIndexOf('\\') + 1).Replace(".jpg", "");

                var owner = x.Substring(0, x.IndexOf('_'));
                var id = x.Substring(x.LastIndexOf('_') + 1);
                var type = (x.IndexOf('r') == -1 ? 'l' : 'r');
                var directory = $@"{Settings.Images.Output}{type}\{owner}";

                PalmsList.Add(new PalmModel()
                {
                    Id = id,
                    Owner = owner,
                    FileName = x,
                    Directory = directory
                });

                if (Directory.Exists(directory) && Directory.GetFiles(directory).Length > 0)
                    Directory.Delete(directory, true); // recursive

                Directory.CreateDirectory(directory);
            });

            //? get names and create name collection
            //? create dirs for saving data
            var userNames = PalmsList.DistinctBy(x => x.Owner);

            Console.WriteLine($"Total users: {userNames.Count()}");
            Console.WriteLine($"Total palm collection: {PalmsList.Count}");

            double radius, a, xx, yy;
            Point centerOfPalm;

            Mat skel, temp, eroded, thr = new Mat();
            Mat element = Cv2.GetStructuringElement(MorphShapes.Cross, Settings.ElementSize);

            //! ROI extraction
            Console.WriteLine($"[{DateTime.Now}] Transform started");

            var workerTime = new Stopwatch();
            workerTime.Start();

            PalmsList.ForEach(x =>
            {
                var path = $@"{Settings.Images.Source}\{x.FileName}.jpg";
                x.SourceImage = Cv2.ImRead(path, ImreadModes.AnyColor);
                x.ThresholdImage = x.SourceImage.Threshold(0, 255, ThresholdTypes.Otsu);

                x.ThresholdImage.DistanceTransform(DistanceTypes.L2, DistanceMaskSize.Precise);
                x.ThresholdImage.ConvertTo(x.ThresholdImage, MatType.CV_8U);

                centerOfPalm = GetHandCenter(x.ThresholdImage, out radius);

                a = (2 * radius) / Math.Sqrt(2);

                xx = centerOfPalm.X - radius * Math.Cos(45 * Math.PI / 180);
                yy = centerOfPalm.Y - radius * Math.Sin(45 * Math.PI / 180);

                var rect = new Rect(new Point(xx, yy), new Size(a, a));
                x.ROI = new Mat(x.SourceImage, rect)
                                    .Resize(new Size(150, 150));

                Cv2.Rotate(x.ROI, x.ROI, RotateFlags.Rotate90Counterclockwise);
                
                //! apply filters
                Cv2.MedianBlur(x.ROI, x.ROI, 5);

                // Cv2.CvtColor(x.ROI, x.ROI, ColorConversionCodes.BGR2GRAY);
                Cv2.FastNlMeansDenoising(x.ROI, x.ROI);
                Cv2.CvtColor(x.ROI, x.ROI, ColorConversionCodes.GRAY2BGR);

                //! Equalize hist
                Cv2.MorphologyEx(x.ROI, x.ROI, MorphTypes.Open, element);
                Cv2.CvtColor(x.ROI, x.ROI, ColorConversionCodes.BGR2YUV);

                var RGB = Cv2.Split(x.ROI);
                RGB[0] = RGB[0].EqualizeHist();
                RGB[1] = RGB[1].EqualizeHist();
                RGB[2] = RGB[2].EqualizeHist();

                Cv2.Merge(RGB, x.ROI);
                Cv2.CvtColor(x.ROI, x.ROI, ColorConversionCodes.YUV2BGR);

                //! Invert image
                Cv2.BitwiseNot(x.ROI, x.ROI);

                //! Erode image
                Cv2.CvtColor(x.ROI, x.ROI, ColorConversionCodes.BGR2GRAY);
                Cv2.Erode(x.ROI, x.ROI, element);

                //! Skeletonize
                skel = new Mat(x.ROI.Size(), MatType.CV_8UC1, new Scalar(0));
                temp = new Mat();
                eroded = new Mat();

                do
                {
                    Cv2.MorphologyEx(x.ROI, eroded, MorphTypes.Erode, element);
                    Cv2.MorphologyEx(eroded, temp, MorphTypes.Dilate, element);
                    Cv2.Subtract(x.ROI, temp, temp);
                    Cv2.BitwiseOr(skel, temp, skel);
                    eroded.CopyTo(x.ROI);

                } while (Cv2.CountNonZero(x.ROI) != 0);

                //! Threshold skeletonized image
                thr = skel.Threshold(0, 255, ThresholdTypes.Binary);

                /*//! Remove contours 
                thr.Line(new Point(0, 0), new Point(0, thr.Height), Scalar.Black, 2); // rm left contour
                thr.Line(new Point(0, 0), new Point(thr.Width, 0), Scalar.Black, 2); // rm top contour

                thr.Line(new Point(thr.Width, thr.Height), new Point(thr.Width, 0), Scalar.Black, 2); // rm right contour
                thr.Line(new Point(thr.Width, thr.Height), new Point(0, thr.Height), Scalar.Black, 2); // rm bot contour*/

                Cv2.ImWrite($@"{x.Directory}\{x.Id}.jpg", thr);
            });

            workerTime.Stop();
            Console.WriteLine($"[{DateTime.Now}] Elapsed time: {workerTime.Elapsed}");
        }

        private static OpenCvSharp.Point GetHandCenter(Mat mask, out double radius)
        {
            /*http://blog.naver.com/pckbj123/100203325426*/
            Mat dst = new Mat();
            double radius1;
            Cv2.DistanceTransform(mask, dst, DistanceTypes.L2, DistanceMaskSize.Mask5);

            int[] maxIdx = new int[2];
            int[] minIdx = new int[2];
            Cv2.MinMaxIdx(dst, out radius1, out radius, minIdx, maxIdx, mask);
            OpenCvSharp.Point output = new OpenCvSharp.Point(maxIdx[1], maxIdx[0]);
            return output;
        }
    }
}