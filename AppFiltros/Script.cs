using System.Diagnostics;

namespace AppFiltros
{
    public class Script
    {
        private static (float[,],int) GetMatrix(float[] array)
        {
            float[,] matrix;
            int size = (int)Math.Sqrt(array.GetLength(0));
            matrix = new float[size,size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    matrix[i, j] = array[i * size + j];
                }
            }
            return (matrix, size);
        }

        public static void Main()
        {
            Console.WriteLine("Ingrese la ruta del archivo a filtrar: ");
            string path = ""+Console.ReadLine();
            if(path == "")
            {
                path = "test.png";
            }
            Console.WriteLine("¿Desea mantener el color? (S/N)");
            string color = ""+Console.ReadLine();
            bool keepColor;
            _ = (color.ToUpper() == "S") ? keepColor = true : keepColor = false;
            Stopwatch timer = new Stopwatch();
            timer.Start();
            Image image = new(path, keepColor);
            Console.WriteLine($"Image: image has {image.LayerDepth} layers");
            timer.Stop();
            Console.WriteLine("Ingrese los valores del filtro. Use comas para delimitar: ");
            string[] filterValues = ("" + Console.ReadLine()).Split(',');
            float[] floats = new float[filterValues.Length];
            for (int i = 0; i < filterValues.Length; i++)
            {
                floats[i] = Convert.ToSingle(filterValues[i]);
            }
            Console.WriteLine("Ingrese el valor del factor. Negativo para ignorarlo");
            int factor = Convert.ToInt32(Console.ReadLine());
            Filter filter = new(GetMatrix(floats).Item2, GetMatrix(floats).Item1, Convert.ToBoolean(factor), (float)1.0f/factor);
            timer.Start();
            Image image2 = filter.ApplyFilter(image, true, 255, 0);
            timer.Stop();
            Console.WriteLine("Ingrese el nombre del archivo a guardar: ");
            string newpath = "" + Console.ReadLine();
            timer.Start();
            image2.WriteImage(newpath);
            timer.Stop();

            Console.WriteLine($"Time elapsed: {timer.ElapsedMilliseconds}");
        }
    }
}
