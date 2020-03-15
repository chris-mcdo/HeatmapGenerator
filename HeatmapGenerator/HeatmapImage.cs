using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HeatmapGenerator
{
    /// <summary>
    /// Class to write heatmap onto a bitmap image
    /// To do:
    /// 1) Work out a good structure for this class
    /// 2) Check constructor + methods make sense / work
    /// 3) Check Bitmap reader works
    /// 4) Work out how to blend (see wiki article) alpha compositing
    /// 5) alpha compositing could probably be made more efficient
    /// </summary>
 
    class HeatmapImage
    {
        public int Width; // 1680
        public int Height; // 987

        PixelFormat Pf = PixelFormats.Bgra32; // Pixel format: blue-green-red-alpha

        int Bypp; // bytes per pixel
        int Stride; // bytes per row (Changing pixel format will mess things up)
        int Len; // length of byte array

        public BitmapSource Result { get; set; }

        public HeatmapImage(int width = 1680, int height = 987)
        {
            // Size
            Width = width;
            Height = height;

            // Image properties
            Bypp = Pf.BitsPerPixel / 8;
            Stride = Width * Bypp;
            Len = Width * Height * Bypp;
        }

        // Overlays heatmap onto source image
        public void Overlay(BitmapSource source, HeatmapArray heatmap)
        {

            // Bottom layer (background image) as byte array 
            byte[] bottom = BmpToByteArray(source);

            // Top layer (heatmap) as byte array
            byte[] top = HeatmapToByteArray(heatmap);

            // Blending
            byte[] rawImage = Blend(bottom, top);

            // Storing result
            Result = BitmapSource.Create(Width, Height, 96d, 96d, Pf, null, rawImage, Stride);
        }

        // Blends two layers using alpha compositing
        public byte[] Blend(byte[] bottom, byte[] top)
        {
            byte[] rawImage = new byte[Len];

            double alpha1;
            double alpha2;
            double alpha;

            double c1;
            double c2;
            double c;

            // alpha compositing: blending individual pixels
            for (int i = 0; i < Len / Bypp; i++)
            {
                // Calculating transparency for blended image
                alpha1 = Convert.ToDouble(top[4 * i + 3]) / 255;
                alpha2 = Convert.ToDouble(bottom[4 * i + 3]) / 255;
                alpha = alpha1 + alpha2 * (1 - alpha1);

                // RGB channels
                for (int j = 0; j < Bypp - 1; j++)
                {
                    c1 = Convert.ToDouble(top[4 * i + j]) / 255;
                    c2 = Convert.ToDouble(bottom[4 * i + j]) / 255;
                    if (alpha != 0)
                    {
                        c = (c1 * alpha1 + c2 * alpha2 * (1 - alpha1)) / alpha;
                    } else
                    {
                        c = 0;
                    }

                    rawImage[4 * i + j] = (byte)(255 * c);
                }

                // Alpha channel (0=transparent, 255 =opaque)
                rawImage[4 * i + 3] = (byte)(255 * alpha); 
            }

            return rawImage;
        }

        public byte[] HeatmapToByteArray(HeatmapArray ha)
        {
            // Scaled intensity: on interval (0,1)
            double[,] intensity = ha.GetScaledIntensity();

            // Writing bytes (flattening 2d array)
            byte[] bytes = new byte[Len]; 

            int pixNo; // pixel number
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    pixNo = y * Width + x;
                    // blue, green, red are 0; greyscale
                    // bytes[4 * i] = 255; // R
                    bytes[4 * pixNo + 1] = 255; // G
                    // bytes[4 * i + 2] = 255; // B
                    bytes[4 * pixNo + 3] = (byte)(254 * intensity[x, y]); // alpha (0=transparent, 255 =opaque)
                    
                }
            }

            return bytes;
        }

        // Converting bitmap image to byte array
        public byte[] BmpToByteArray(BitmapSource bmp)
        {
            byte[] bytes = new byte[Len];
            bmp.CopyPixels(bytes, Stride, 0);
            return bytes;
        }

        // Writes PNG bitmap to specified path
        public void Save(String imagePath)
        {
            using (var stream = new FileStream(imagePath + ".png", FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(Result));
                // encoder.QualityLevel = 100;
                encoder.Save(stream);
            }

        }

        // Heatmap converted to bitmap
        public BitmapSource HeatmapToBitmap(HeatmapArray heatmap)
        {
            // Creating bitmap
            return BitmapSource.Create(Width, Height, 96d, 96d, Pf, null, HeatmapToByteArray(heatmap), Stride);
        }
    }
}