using OpenCvSharp;

namespace Biometrics.Palm
{
    public class Const
    {
        /// <summary>
        /// Used for resizing image after selecting ROI
        /// </summary>
        public const int RESIZE_VALUE = 227;

        /// <summary>
        /// Used for selecting output mode type
        /// True - white
        /// False - false
        /// </summary>
        public const bool MODE_TYPE = false; // true is white, false is black

        /// <summary>
        /// Angles for rotating images for creating dataset
        /// </summary>
        /// <value></value>
        public static readonly int[] RotationAngles = {
            0,
            5,
            10,
            15,
            20,
            25,
            30,
            35,
            40,
            45,
            50,
            55,
            60,
            //
            120,
            125,
            130,
            135,
            140,
            145,
            150,
            330
        };

        public static readonly Point Zero = new Point(0, 0);
        public static Size ResizeValue = new Size(RESIZE_VALUE, RESIZE_VALUE);
        public static Scalar ZeroScalar = new Scalar(0);
    }
}