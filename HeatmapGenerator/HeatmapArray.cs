using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HeatmapGenerator
{
    class HeatmapArray
    {
        public double[,] Intensity;
        public double PointRadius;
        public int PointCount;

        public int Width; // 1680
        public int Height; // 987

        public double MaxVal = 1; // Keeping track of max values saves time

        public HeatmapArray(int width = 1680, int height = 987, double pointRadius = 100, int pointCount = 25)
        {
            SetSize(width, height);
            PointRadius = pointRadius;
            PointCount = pointCount;
        }

        public void SetSize(int width, int height)
        {
            Width = width;
            Height = height;
            Intensity = new double[Width, Height];
        }

        public void AddPoint(double xC, double yC)
        {
            AddPoint(xC, yC, PointRadius);
        }

        // Option to specify radius
        public void AddPoint(double xC, double yC, double r)
        {
            // Increment using Gaussian kernel

            // Only increment within 3 standard deviations; sd is 1/3 radius
            double sdSqd = (r / 3) * (r / 3); 

            double rSqd = r * r; double rXYSqd;

            // Increment within circle radius r
            for (int y = (int)Math.Max(yC - r, 0); y <= Math.Min(yC + r, Height - 1); y++)
            {
                for (int x = (int)Math.Max(xC - r, 0); x <= Math.Min(xC + r, Width - 1); x++)
                {
                    rXYSqd = ((x-xC) * (x-xC)) + ((y-yC) * (y-yC));
                    if (rXYSqd <= (rSqd))
                    {
                        Intensity[x, y] += Math.Exp( - (rXYSqd) / (2 * sdSqd) );

                        if (rXYSqd < rSqd / 3 && Intensity[x, y] > MaxVal)
                        {
                            MaxVal = Intensity[x, y];
                        }
                    }
                }
            }
        }

        public double[,] GetScaledIntensity()
        {
            double[,] scaledI = new double[Width, Height];

            // First normalise intensity to interval (0, 1)
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    scaledI[x, y] = Intensity[x,y]/MaxVal;
                }
            }

            // Then perform a scaling: how do you want to do this?
            
            return scaledI;
        }

    }

}
