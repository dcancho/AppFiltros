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
                {0,4,5,5,5 },
                {4,4,5,7,5 },
                {4,4,5,5,5 },
                {0,4,5,5,5 },
                {4,4,5,7,5 }
            };
            //Create a filter
            Filter filter = new Filter(3, new sbyte[,]
        {
                {1,2,1},
                {2,4,2},
                {1,2,1}
            });
            //Print the matrix
            myImage.Print();
            //Apply the filter
            myImage.ApplyFilter(filter);
            Console.WriteLine("\n\n\n");
            myImage.Print();
        }

    }
}