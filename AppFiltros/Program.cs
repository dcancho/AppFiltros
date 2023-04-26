using System.Diagnostics;

namespace AppFiltros
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //start counting time elapsed
            var watch = Stopwatch.StartNew();
            Console.WriteLine("Path of input file: ");
            string path = "" + Console.ReadLine();
            Image myImage = FileIO.GetImage(path,true);
            Filter filter = new(5, new float[,]
            {
                {1,1,1,1,1},
                {1,1,1,1,1},
                {1,1,1,1,1},
                {1,1,1,1,1},
                {1,1,1,1,1}
            },true, (float)(1.0/25.0));
            Image result = filter.ApplyFilter(myImage, false);
            Console.WriteLine("\n");
            Console.WriteLine("Path of output file: ");
            string outputPath = "" + Console.ReadLine();
            FileIO.WriteImage(result,outputPath);
            //Print resources used
            Console.WriteLine($"Memory used: {Process.GetCurrentProcess().WorkingSet64 / 1024} KB");
            //Print time elapsed
            watch.Stop();
            Console.WriteLine($"Time elapsed: {watch.ElapsedMilliseconds} ms");
            Console.ReadKey();

        }

    }
}