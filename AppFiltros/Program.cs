namespace AppFiltros
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Image myImage = new AppFiltros.Image(10,10);
            #region
            Random rand = new Random();
            for (byte i = 0; i < myImage.Width; i++)
            {
                for (byte j = 0; j < myImage.Height; j++)
                {
                    myImage[i, j] = (byte)rand.Next(0, 255);
                }
            }
            #endregion

        }
    }
}