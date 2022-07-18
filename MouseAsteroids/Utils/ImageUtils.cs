using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace MouseAsteroids.Utils
{
    public static class ImageUtils
    {
        public static BitmapSource BitmapToSource(Bitmap bitmap)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        /// <summary>
        /// https://stackoverflow.com/questions/2225363/c-sharp-rotate-bitmap-90-degrees
        /// </summary>
        public static Bitmap RotateBitmap(Bitmap input, float angle)
        {
            //Open the source image and create the bitmap for the rotatated image
            using Bitmap sourceImage = new(input);
            using Bitmap rotateImage = new(sourceImage.Width, sourceImage.Height);

            //Set the resolution for the rotation image
            rotateImage.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);

            //Create a graphics object
            using (Graphics gdi = Graphics.FromImage(rotateImage))
            {
                //Rotate the image
                gdi.TranslateTransform((float)sourceImage.Width / 2, (float)sourceImage.Height / 2);
                gdi.RotateTransform(angle);
                gdi.TranslateTransform(-(float)sourceImage.Width / 2, -(float)sourceImage.Height / 2);
                gdi.DrawImage(sourceImage, new System.Drawing.Point(0, 0));
            }

            return (Bitmap)rotateImage.Clone();
        }

        /// <summary>
        /// http://www.java2s.com/example/csharp/system.drawing/scales-a-bitmap-by-a-scale-factor-growing-or-shrinking-both-axes-inde.html
        /// </summary>
        /// <param name="inputBmp"></param>
        /// <param name="xScaleFactor"></param>
        /// <param name="yScaleFactor"></param>
        /// <returns></returns>
        public static Bitmap ScaleBitmap(Bitmap inputBmp, double xScaleFactor, double yScaleFactor)
        {
            Bitmap newBmp = new(
                                (int)(inputBmp.Size.Width * xScaleFactor),
                                (int)(inputBmp.Size.Height * yScaleFactor),
                                PixelFormat.Format24bppRgb);//Graphics.FromImage doesn't like Indexed pixel format

            using Graphics newBmpGraphics = Graphics.FromImage(newBmp);

            newBmpGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            newBmpGraphics.ScaleTransform((float)xScaleFactor, (float)yScaleFactor);

            Rectangle drawRect = new(0, 0, inputBmp.Size.Width, inputBmp.Size.Height);
            newBmpGraphics.DrawImage(inputBmp, drawRect, drawRect, GraphicsUnit.Pixel);

            return ConvertBitmap(newBmp, inputBmp.RawFormat);
        }

        public static Bitmap ScaleBitmap(Bitmap inputBmp, double scaleFactor)
        {
            return ScaleBitmap(inputBmp, scaleFactor, scaleFactor);
        }
        
        private static Bitmap ConvertBitmap(Bitmap inputBmp, ImageFormat destFormat)
        {
            if (inputBmp.RawFormat.Equals(destFormat))
                return (Bitmap)inputBmp.Clone();

            Stream imgStream = new MemoryStream();
            inputBmp.Save(imgStream, destFormat);
            Bitmap destBitmap = new(imgStream);

            return destBitmap;
        }
    }
}
