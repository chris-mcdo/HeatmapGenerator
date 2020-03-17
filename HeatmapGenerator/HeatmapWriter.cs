using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace HeatmapGenerator
{
    class HeatmapWriter
    {
        public const String RelPathLogger = @"..\Logger\EyeLogger.json";

        // Default range of eye-tracking points to draw on heatmap in ms (forward and backwards in time)
        public const long BackRange = 200, ForwardRange = 200;

        List<EyePoint> EyePointsList { get; set; }
        public String DataDir, WriteDir;
        public long[] FrameArray { get; set; } // List of image frames in time order

        public int Width;
        public int Height;

        public HeatmapWriter(String dataDir, String writeDir)
        {
            DataDir = dataDir;
            WriteDir = writeDir;

            ReadEyeData();
            GetFileNames();
            int[] dimensions = GetImageDimension(DataDir + FrameArray[0] + ".bmp");
            Width = dimensions[0]; Height = dimensions[1];
        }

        // Generate heatmap integrated over given time
        public void GenerateGlobalHeatmap(long imageFrame, long endFrame)
        {
            long[] frames = Array.FindAll(
                FrameArray,
                fr => fr >= imageFrame && fr <= endFrame
                );

            if (frames.Length > 0)
            {
                long frame = frames[0];
                // Make a new folder to write images (if it doesn't already exist)
                System.IO.Directory.CreateDirectory(WriteDir);

                // Create a heatmap for the given range
                var watch = System.Diagnostics.Stopwatch.StartNew();

                long range = endFrame - imageFrame;

                CreateHeatmap(frame, backRange: 0, forwardRange: range, fileName: "global_"+imageFrame);
                watch.Stop();
                Console.WriteLine("Generated " + frames.Length + " frame(s) in " + watch.ElapsedMilliseconds / 1000 + " seconds");

            }
        }

        // Generate instantaneous heatmap
        public void GenerateLocalHeatmaps(long startFrame, long endFrame)
        {
            // Get list of image frames (file names) that are in specified range
            long[] frames = Array.FindAll(
                FrameArray,
                fr => fr >= startFrame && fr <= endFrame
                );

            // If there are some images in the range
            if (frames.Length > 0)
            {
                // Make a new folder to write images (if it doesn't already exist)
                System.IO.Directory.CreateDirectory(WriteDir);

                // Create a heatmap for each frame
                var watch = System.Diagnostics.Stopwatch.StartNew();

                for (int i = 0; i < frames.Length; i++)
                {
                    CreateHeatmap(frames[i], fileName: i.ToString());
                    Console.WriteLine(i*100/frames.Length + "% complete, " + watch.ElapsedMilliseconds / 1000 + " seconds elapsed (frame " +frames[i]+")");
                }

                GenerateTimecodes(frames); // time codes for frames in text file

                watch.Stop();
                Console.WriteLine("Generated " +frames.Length + " frames in " + watch.ElapsedMilliseconds/1000 + " seconds");

            }

        }

        // Generate a single heatmap at specified frame
        public void CreateHeatmap(long frame, long backRange = BackRange, long forwardRange = ForwardRange, String fileName = "")
        {
            if (fileName.Equals(""))
            {
                fileName = frame.ToString();
            }
            // Select all eye-tracking points in range
            List<EyePoint> pointList = EyePointsList.FindAll(
                p => (p.Frame > frame - backRange && p.Frame < frame + forwardRange)
                );

            // Create new heatmap array
            HeatmapArray ha = new HeatmapArray(width: Width, height: Height, pointCount: pointList.Count);

            // Adding eye-tracking points to heatmap
            // Console.WriteLine("Frame "+frame+": adding " + pointList.Count + " eye-tracking points to image");
            foreach (EyePoint p in pointList)
            {
                ha.AddPoint(p.X, p.Y);
            }

            // Initialising heatmap image object
            HeatmapImage heatImage = new HeatmapImage(width: Width, height: Height);

            // Loading source image
            String readPath = DataDir + frame + ".bmp";
            BitmapImage source = new BitmapImage(new Uri(readPath));

            // Overlay heatmap and original image
            heatImage.Overlay(source, ha);

            // Write to file
            String writePath = WriteDir + fileName; // don't include extension
            heatImage.Save(writePath);

        }

        // Reading eye-tracking data from specified file
        public void ReadEyeData()
        {
            // Reading eye-tracking data into dictionary
            String jsonString = File.ReadAllText(DataDir + RelPathLogger);
            Dictionary<int, InputDataModel> inputModelDict = JsonConvert.DeserializeObject<Dictionary<int, InputDataModel>>(jsonString);

            // Converting to nice format
            EyePointsList = new List<EyePoint>();
            foreach (KeyValuePair<int, InputDataModel> inputModelEntry in inputModelDict)
            {
                var frame = inputModelEntry.Key;
                var inputModel = inputModelEntry.Value;
                double[] coords = Array.ConvertAll(inputModel.XY.Split(','), double.Parse);
                EyePointsList.Add(new EyePoint
                {
                    Frame = frame,
                    X = coords[0],
                    Y = coords[1]
                });
            }
            Console.WriteLine("Reading eye-tracking data, first frame is at t = " + EyePointsList[0].Frame);
        }

        // List of all image frames, from source folder
        public void GetFileNames()
        {
            FrameArray = Directory.GetFiles(DataDir, "*")
                         .Select(Path.GetFileNameWithoutExtension)
                         .Select(str => long.Parse(str))
                         .OrderBy(f => f)
                         .ToArray();
        }

        public int[] GetImageDimension(String imagePath)
        {
            using (var imageStream = File.OpenRead(imagePath))
            {
                var decoder = BitmapDecoder.Create(imageStream, BitmapCreateOptions.IgnoreColorProfile,
                    BitmapCacheOption.Default);
                return new int[2] { decoder.Frames[0].PixelWidth, decoder.Frames[0].PixelHeight };
            }

        }

        public void GenerateInfoFile(long[] frames)
        {
            String infoFileString = "";

            int i;
            for (i = 0; i < frames.Length - 1; i++)
            {
                infoFileString +=
                    "file '" + frames[i] + ".png'\r\n"
                        + "duration " + (frames[i + 1] - frames[i])/1000 + "\r\n";
            }

            infoFileString += "file '" + frames[i - 1] + ".png'"; // final line

            System.IO.File.WriteAllText(WriteDir + @"info.txt", infoFileString);

        }

        public void GenerateTimecodes(long[] frames)
        {
            String timecodes = "# timecode format v2";
            for (int i = 0; i < frames.Length; i++)
            {
                timecodes += "\r\n" + frames[i];
            }
            System.IO.File.WriteAllText(WriteDir + @"timecodes.txt", timecodes);
        }

        // This doesn't work yet!
        public void GenerateVideo()
        {
            String initialDir = Directory.GetCurrentDirectory();

            Directory.SetCurrentDirectory(WriteDir);

            Console.WriteLine("Attempting to generate video from " + WriteDir);

            // Creating video
            Process.Start("CMD.exe", "/C ffmpeg -i %d.png ffmpeg-unadj.mp4").WaitForExit();
            Process.Start("CMD.exe", "/C mp4fpsmod -o vfr.mp4 -t timecodes.txt ffmpeg-unadj.mp4").WaitForExit();
            Process.Start("CMD.exe", "/C ffmpeg -i vfr.mp4 -qscale 0 final-cfr.avi").WaitForExit();

            // Cleaning up
            Process.Start("CMD.exe", "/C del *.mp4 *.png *.txt");


            Console.WriteLine("Finished generating video");

            Directory.SetCurrentDirectory(initialDir);

        }

    }
}
