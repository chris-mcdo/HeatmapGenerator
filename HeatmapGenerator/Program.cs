using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HeatmapGenerator
{
    class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            int opt = 0;

            while (opt != 3)
            {
                Console.WriteLine("Select: 1 - generate instantaneous heatmaps (and video); 2 - generate global heatmaps, 3 - exit");
                opt = int.Parse(Console.ReadLine());

                if (opt == 1)
                {
                    GenerateLocalHeatmaps();
                }
                else if (opt == 2)
                {
                    GenerateGlobalHeatmap();
                }
                else if (opt == 3)
                {
                    Console.WriteLine("Exiting...");
                }
            }

        }

        // Series of instantaneous heatmaps
        static void GenerateLocalHeatmaps()
        {
            // See readme for required folder organisation
            String dataDir;
            String writeDir;

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            do
            {
                Console.WriteLine("Navigate to the data folder (press enter to show dialog)...");
                Console.ReadLine();
            } while (!fbd.ShowDialog().Equals(DialogResult.OK));

            dataDir = fbd.SelectedPath + @"\";
            writeDir = Path.GetFullPath(Path.Combine(dataDir, @"..\..\Heatmaps\Local Heatmaps\")); ;
            Console.WriteLine(writeDir);

            Console.WriteLine("Enter the first frame to read");
            long startFrame = long.Parse(Console.ReadLine());

            Console.WriteLine("Enter the final frame to read");
            long endFrame = long.Parse(Console.ReadLine());

            HeatmapWriter hw = new HeatmapWriter(dataDir, writeDir);

            hw.GenerateLocalHeatmaps(startFrame, endFrame);

            Console.WriteLine("Write to video? y/n");
            if (Console.ReadLine() == "y")
            {
                hw.GenerateVideo();
            }
        }

        // Single heatmap; integrated over time period
        static void GenerateGlobalHeatmap()
        {
            // See readme for required folder organisation
            String dataDir;
            String writeDir;

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            do
            {
                Console.WriteLine("Navigate to the data folder (press enter to show dialog) ...");
                Console.ReadLine();
            } while (!fbd.ShowDialog().Equals(DialogResult.OK));

            dataDir = fbd.SelectedPath + @"\";
            writeDir = Path.GetFullPath(Path.Combine(dataDir, @"..\..\Heatmaps\Global Heatmaps\")); ;
            Console.WriteLine(writeDir);

            Console.WriteLine("Enter the image frame");
            long startFrame = long.Parse(Console.ReadLine());

            Console.WriteLine("Enter the final frame to read");
            long endFrame = long.Parse(Console.ReadLine());

            HeatmapWriter hw = new HeatmapWriter(dataDir, writeDir);

            hw.GenerateGlobalHeatmap(startFrame, endFrame);


        }

        static void RunTest()
        {
            String str = "\"C:\\Users\\cmcdo\\Documents\\BA Project documents\\6\\\"";

            Process.Start("CMD.exe", "/C dir /a "+str);

            /*
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.WriteLine("ffmpeg -version");
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();
            Console.WriteLine(cmd.StandardOutput.ReadToEnd());
            */
        }

    }
}
