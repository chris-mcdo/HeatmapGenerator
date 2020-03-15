using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace HeatmapGenerator
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            RunFFmpeg();

            // GenerateLocalHeatmaps();  

            GenerateGlobalHeatmap();
        }

        static void RunFFmpeg()
        {
            // work out how to do this
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
                Console.WriteLine("Select the experiment folder (press enter to show dialog)...");
                Console.ReadLine();
            } while (!fbd.ShowDialog().Equals(DialogResult.OK));

            dataDir = fbd.SelectedPath + @"\";
            writeDir = dataDir;
            Console.WriteLine(dataDir);

            Console.WriteLine("Enter the first frame to read");
            long startFrame = long.Parse(Console.ReadLine());

            Console.WriteLine("Enter the final frame to read");
            long endFrame = long.Parse(Console.ReadLine());

            HeatmapWriter hw = new HeatmapWriter(dataDir, writeDir);

            hw.GenerateLocalHeatmaps(startFrame, endFrame);

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
                Console.WriteLine("Select the experiment folder (press enter to show dialog)...");
                Console.ReadLine();
            } while (!fbd.ShowDialog().Equals(DialogResult.OK));

            dataDir = fbd.SelectedPath + @"\";
            writeDir = dataDir;
            Console.WriteLine(dataDir);

            Console.WriteLine("Enter the image frame");
            long startFrame = long.Parse(Console.ReadLine());

            Console.WriteLine("Enter the final frame to read");
            long endFrame = long.Parse(Console.ReadLine());

            HeatmapWriter hw = new HeatmapWriter(dataDir, writeDir);

            hw.GenerateGlobalHeatmap(startFrame, endFrame);


        }

    }
}
