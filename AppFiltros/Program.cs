namespace AppFiltros
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Declare an image object
            Image myImage = new Image(5, 5);
            //initialize explicitely the image
            myImage.Pixels = new byte[,]
            {
                {0,1,1,2,3 },
                {1,1,3,4,4},
                {1,1,2,4,2 },
                {2,3,2,2,2},
                {1,1,1,1,1 }
            };
            //Create a filter
            Filter filter = new Filter(3, new sbyte[,]
        {
                {0,1,0},
                {1,-2,1},
                {0,1,0}
            });
            //Print the matrix
            myImage.Print();
            //Apply the filter
            myImage.ApplyFilter(filter);
            Console.WriteLine("\n");
            Console.WriteLine("\n");
            Console.WriteLine("\n");
            Console.WriteLine("\n");
            myImage.Print();
        }

    }
}