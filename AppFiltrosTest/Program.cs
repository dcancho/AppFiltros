using AppFiltros;
using System.Diagnostics;

namespace AppFiltrosTest
{
    public class Program
    {
        static void Main(string[] args)
        {
            string openFilePath = "test.png";
            string saveFilePath = "result.png";
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Image image = new(openFilePath,false);
            float[] floats = {0,4,0,4,-16,4,0,4,0};
            Filter filter = new(floats,false);
            Image imageOutput = filter.ApplyFilter(image, true);
            imageOutput.Save(saveFilePath);
            stopwatch.Stop();
            Console.WriteLine($"Time elapsed: {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}