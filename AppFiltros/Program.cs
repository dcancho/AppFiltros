using System.Diagnostics;

namespace AppFiltros
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //new log file
            //start counting time elapsed
            var watch = Stopwatch.StartNew();
            #region Test de mediana
            string path = "cyno.png";
            //string path = @"C:\Users\Diego\Pictures\Screenshots\002.png";
            Image myImage = FileIO.GetImage(path);
            Filter filter = new(3, new float[,]
            {
                {1,1,1},
                {1,-8,1},
                {1,1,1}
            },false, 1);
            //myImage.Print();
            Image result = filter.ApplyFilter(myImage, false);
            Console.WriteLine("\n");
            //result.Print();
            FileIO.WriteImage(result);
            #endregion

            //Print resources used
            Console.WriteLine($"Memory used: {Process.GetCurrentProcess().WorkingSet64 / 1024} KB");
            //Print time elapsed
            watch.Stop();
            Console.WriteLine($"Time elapsed: {watch.ElapsedMilliseconds} ms");
            Console.ReadKey();

        }

    }
}