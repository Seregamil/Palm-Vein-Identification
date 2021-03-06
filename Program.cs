using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

using OpenCvSharp;

namespace Biometrics.Palm {
    public class Program {
        static void Main (string[] args) {
            //! Create directories for output images
            if (!Directory.Exists (Settings.Images.Output))
                Directory.CreateDirectory (Settings.Images.Output);

            //! create list of images 
            var listOfImages = Directory.GetFiles (Settings.Images.Source, "*.jpg")
                .Where (x => x.Contains ("940"))
                .Where (x => x.Contains("_l_"))
                // .Take (6) // take one palm to test
                .ToList ();

            //! variables for ROI
            double radius, a, xx, yy, predictX, predictY, angle;

            Point centerOfPalm;
            Point2f centerOfImage;
            Rect rect;

            //! variables for extracting file parameters
            PalmModel[] Palms = new PalmModel[listOfImages.Count];

            //! matherials
            Mat sourceHandle, thresholdHandle, roiHandle, skel, rotatedSource, eroded, thr, matrix = new Mat ();
            Mat element = Cv2.GetStructuringElement (MorphShapes.Cross, Settings.ElementSize);
            Mat[] RGB;

            for (int i = 0; i != listOfImages.Count; i++) {
                Palms[i] = new PalmModel();
                Palms[i].Filename = listOfImages[i].Remove(0, listOfImages[i].LastIndexOf('/') + 1).Replace(".jpg", "");
                Palms[i].Owner = Palms[i].Filename.Substring(0, Palms[i].Filename.IndexOf('_'));
                Palms[i].Id = Palms[i].Filename.Substring(Palms[i].Filename.LastIndexOf('_') + 2);
                Palms[i].Type = (Palms[i].Filename.IndexOf('r') == -1 ? 'l' : 'r');
                Palms[i].Directory = $"{Settings.Images.Output}{Palms[i].Type}/{Palms[i].Owner}";
                Palms[i].Path = $"{Settings.Images.Source}/{Palms[i].Filename}.jpg";

                if (Directory.Exists(Palms[i].Directory) && Directory.GetFiles(Palms[i].Directory).Length > 0)
                    Directory.Delete(Palms[i].Directory, true); // recursive

                Directory.CreateDirectory(Palms[i].Directory);
            }

            Console.WriteLine ($"[{DateTime.Now}] Transform started");

            var workerTime = new Stopwatch ();
            workerTime.Start ();

            for (int i = 0; i != listOfImages.Count; i++) {
                if (i % 10 == 0)
                    Console.WriteLine($"{i}.{listOfImages.Count}");

                // Read sample image
                sourceHandle = Cv2.ImRead (Palms[i].Path, ImreadModes.AnyColor);

                // Get center of image
                centerOfImage = new Point2f (sourceHandle.Width / 2, sourceHandle.Height / 2);

                // Start loop by rotations
                for (int j = 0; j != 180; j++) {
                    // change rotation angle
                    angle = j * 2;

                    rotatedSource = new Mat ();
                    // get and apply image rotation matrix by angle
                    matrix = Cv2.GetRotationMatrix2D (centerOfImage, angle, 1.0);
                    Cv2.WarpAffine (sourceHandle, rotatedSource, matrix, new Size (sourceHandle.Width, sourceHandle.Height));

                    // apply threshold
                    thresholdHandle = rotatedSource.Threshold (0, 255, ThresholdTypes.Otsu);

                    /*// apply transform distance and convert to cv_8u
                    thresholdHandle.DistanceTransform (DistanceTypes.L2, DistanceMaskSize.Precise);
                    thresholdHandle.ConvertTo (thresholdHandle, MatType.CV_8U);

                    // get center of palm
                    centerOfPalm = GetHandCenter (thresholdHandle, out radius);*/
					double radius1;
					Cv2.DistanceTransform(thresholdHandle, matrix, DistanceTypes.L2, DistanceMaskSize.Mask5);
					matrix.ImWrite($"{Palms[i].Directory}/002_distance-transform.jpg");

					int[] maxIdx = new int[2];
					int[] minIdx = new int[2];
					Cv2.MinMaxIdx(matrix, out radius1, out radius, minIdx, maxIdx, thresholdHandle);
					centerOfPalm = new OpenCvSharp.Point(maxIdx[1], maxIdx[0]);

                    // calculate ROI 
                    a = (2 * radius) / Math.Sqrt (2);

                    xx = centerOfPalm.X - radius * Math.Cos (45 * Math.PI / 180);
                    yy = centerOfPalm.Y - radius * Math.Sin (45 * Math.PI / 180);

                    if (xx < 0) {
                        a += xx; // 200 + -2 -> 200 - 2 = 198
                        xx = 0;
                    }

                    if (yy < 0) {
                        a += yy; // 120 + -10 -> 120 - 10 = 110
                        yy = 0;
                    }

                    predictX = xx + a;
                    predictY = yy + a;

                    if (predictX > rotatedSource.Width) { // if more
                        xx -= predictX - rotatedSource.Width; // (590 - 580) = 10 
                    }

                    if (predictY > rotatedSource.Height) {
                        yy -= predictY - rotatedSource.Height; // 800 - 640 = 160
                    }

                    /*
                        rect = new Rect(new Point(xx + 20, yy + 20), new Size(a, a));
                        rect = new Rect(new Point(xx - 20, yy - 20), new Size(a, a));
                        rect = new Rect(new Point(xx - 20, yy + 20), new Size(a, a));
                        rect = new Rect(new Point(xx + 20, yy - 20), new Size(a, a)); 
                    */

                    rect = new Rect (new Point (xx, yy), new Size (a, a));
                    
                    roiHandle = new Mat (rotatedSource, rect)
                        .Resize (Const.ResizeValue);

                    //! apply filters
                    Cv2.MedianBlur (roiHandle, roiHandle, 5);

                    // Cv2.CvtColor(roiHandle, roiHandle, ColorConversionCodes.BGR2GRAY);
                    Cv2.FastNlMeansDenoising (roiHandle, roiHandle);
                    Cv2.CvtColor (roiHandle, roiHandle, ColorConversionCodes.GRAY2BGR);

                    //! Equalize hist
                    Cv2.MorphologyEx (roiHandle, roiHandle, MorphTypes.Open, element);
                    Cv2.CvtColor (roiHandle, roiHandle, ColorConversionCodes.BGR2YUV);

                    RGB = Cv2.Split (roiHandle);
                    RGB[0] = RGB[0].EqualizeHist ();
                    RGB[1] = RGB[1].EqualizeHist ();
                    RGB[2] = RGB[2].EqualizeHist ();

                    Cv2.Merge (RGB, roiHandle);
                    Cv2.CvtColor (roiHandle, roiHandle, ColorConversionCodes.YUV2BGR);

                    //! Invert image
                    Cv2.BitwiseNot (roiHandle, roiHandle);

                    //! Erode image
                    Cv2.CvtColor (roiHandle, roiHandle, ColorConversionCodes.BGR2GRAY);
                    Cv2.Erode (roiHandle, roiHandle, element);

                    //! Skeletonize
                    skel = new Mat (roiHandle.Size (), MatType.CV_8UC1, Const.ZeroScalar);
                    rotatedSource = new Mat ();
                    eroded = new Mat ();
                    thr = new Mat ();

                    do {
                        Cv2.MorphologyEx (roiHandle, eroded, MorphTypes.Erode, element);
                        Cv2.MorphologyEx (eroded, rotatedSource, MorphTypes.Dilate, element);
                        Cv2.Subtract (roiHandle, rotatedSource, rotatedSource);
                        Cv2.BitwiseOr (skel, rotatedSource, skel);
                        eroded.CopyTo (roiHandle);

                    } while (Cv2.CountNonZero (roiHandle) != 0);

                    //! Threshold skeletonized image
                    thr = skel.Threshold (0, 255, ThresholdTypes.Binary);

                    //! Remove contours 
                    thr.Line (Const.Zero, new Point (0, thr.Height), Scalar.Black, 2); // rm left contour
                    thr.Line (Const.Zero, new Point (thr.Width, 0), Scalar.Black, 2); // rm top contour
                    thr.Line (new Point (thr.Width, thr.Height), new Point (thr.Width, 0), Scalar.Black, 2); // rm right contour
                    thr.Line (new Point (thr.Width, thr.Height), new Point (0, thr.Height), Scalar.Black, 2); // rm bot contour

                    //! Partion lines
                     if (Const.MODE_TYPE) {
                        Cv2.GaussianBlur (thr, thr, new Size (7, 7), sigmaX : 5);
                        Cv2.MorphologyEx (thr, thr, MorphTypes.Gradient, element);
                        Cv2.BitwiseNot (thr, thr);
                    }
                    thr.ImWrite ($"{Palms[i].Directory}/{Palms[i].Id}-{angle}.jpg");
                }
            }
            workerTime.Stop ();
            Console.WriteLine ($"[{DateTime.Now}] Elapsed time: {workerTime.Elapsed}");
        }

        /*private static OpenCvSharp.Point GetHandCenter (Mat mask, out double radius) {
            // http://blog.naver.com/pckbj123/100203325426
            Mat dst = new Mat ();
            double radius1;
            Cv2.DistanceTransform (mask, dst, DistanceTypes.L2, DistanceMaskSize.Mask5);

            int[] maxIdx = new int[2];
            int[] minIdx = new int[2];
            Cv2.MinMaxIdx (dst, out radius1, out radius, minIdx, maxIdx, mask);
            OpenCvSharp.Point output = new OpenCvSharp.Point (maxIdx[1], maxIdx[0]);
            return output;
        }*/
    }
}