using System.Diagnostics;
namespace AppFiltros
{
    internal class Program
    {
        static void Main(string[] args)
        {
            #region Test de mediana
            Image myImage = new(5, 7, 0)
            {
                Pixels = new byte[,]
            {
                {2,2,2,3,3},
                {2,6,2,3,0},
                {3,3,3,5,5},
                {2,2,2,4,4},
                {2,7,2,4,0}
            }
            };
            Filter filter = new(3, new float[,]
            {
                {1,1,1},
                {1,1,1},
                {1,1,1}
            }, true, (float)1 / 9);
            myImage.Print();
            Image result = filter.ApplyFilter(myImage, false);
            Console.WriteLine("\n");
            result.Print();
            #endregion
        }

    }
}