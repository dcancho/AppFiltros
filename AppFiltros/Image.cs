using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppFiltros
{
    /// <summary>
    /// Tipos de valor admisibles para <see cref="Layer"/>.
    /// </summary>
    public enum ValueType
    {
        R,          //Red
        G,          //Green
        B,          //Blue
        A,          //Alpha
        Grayscale   //Escala de grises

    }

    /// <summary>
    /// Estructura que almacena una matriz de valores de rango [0:255] y admite varios tipos de valor. Ver <see cref="ValueType"/>
    /// </summary>
    public struct Layer
    {
        /// <summary>
        /// Tipo de valores que esta instancia contiene.
        /// </summary>
        public ValueType Type { get; set; }
        /// <summary>
        /// Número de filas de la matriz de valores.
        /// </summary>
        public uint Rows { get; set; }
        /// <summary>
        /// Número de columnas de la matriz de valores.
        /// </summary>
        public uint Columns { get; set; }
        /// <summary>
        /// Matriz de valores.
        /// </summary>
        public byte[,] Pixels { get; set; }
        public Layer(ValueType type, uint rows, uint columns)
        {
            Type = type;
            Rows = rows;
            Columns = columns;
            Pixels = new byte[Rows, Columns];
        }
        public Layer(ValueType type, byte[,] pixelValues)
        {
            Type = type;
            Pixels = pixelValues;
            Rows = (uint)pixelValues.GetLength(0);
            Columns = (uint)pixelValues.GetLength(1);
        }
        public byte this[uint x, uint y]
        {
            get { return Pixels[x, y]; }
            set { Pixels[x, y] = value; }
        }
    }

    /// <summary>
    /// Clase que contiene una imagen de profundidad máxima de 8 bits por pixel
    /// </summary>
    public class Image
    {
        /// <summary>
        /// Numero de capas que componen a este objeto.
        /// </summary>
        public byte LayerDepth { get; set; }
        /// <summary>
        /// Objetos Layer contenidos
        /// </summary>
        public Layer[] Layers { get; set; }
        /// <summary>
        /// Matriz que contiene los pixeles de la imagen
        /// </summary>
        public byte[,] Pixels { get; set; }
        /// <summary>
        /// Dimensión de la imagen
        /// </summary>
        public int Rows { get; set; }
        public int Columns { get; set; }
        /// <summary>
        /// Valor máximo admitido por pixel
        /// </summary>
        public byte MaxValue { get; set; }
        /// <summary>
        /// Valor mínimo admitido por pixel
        /// </summary>
        public byte MinValue { get; set; }
        public Image(int height, int width, byte maxValue = 255, byte minValue = 0)
        {
            Rows = height;
            Columns = width;
            Pixels = new byte[Rows, Columns];
            MaxValue = maxValue;
            MinValue = minValue;
        }
        public Image(byte[,] matrix, byte maxValue = 255, byte minValue = 0)
        {
            Rows = matrix.GetLength(0);
            Columns = matrix.GetLength(1);
            Pixels = matrix;
            MaxValue = maxValue;
            MinValue = minValue;
        }
        /// <summary>
        /// x: Rows
        /// y: Columns
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public byte this[int x, int y]
        {
            get
            {
                return Pixels[x, y];
            }
            set
            {
                Pixels[x, y] = (byte)value;
            }
        }

        /// <summary>
        /// Trunca los valores en una matriz de flotantes y devuelve una matriz de bytes.
        /// </summary>
        /// <param name="path"></param>
        public void Save(string path)
        {
            int rows = matrix.GetLength(0);
            int columns = matrix.GetLength(1);
            byte[,] result = new byte[rows, columns];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    result[i, j] = (byte)(Math.Round(matrix[i, j]) % maxValue);
#if DEBUG
					Console.WriteLine($"Truncating: Modulo of {matrix[i, j]} by {maxValue} is {result[i, j]}");
#endif
                }
            }
            return result;
        }

        /// <summary>
        /// Aplica una transformación lineal a los valores en una matriz de flotantes para ajustarlos a un rango especificado.
        /// </summary>
        /// <param name="matrix">Matriz de flotantes a transformar.</param>
        /// <param name="matrixMaxValue">Valor máximo en la matriz de entrada.</param>
        /// <param name="matrixMinValue">Valor mínimo en la matriz de entrada.</param>
        /// <param name="MaxValue">Valor máximo del rango de salida.</param>
        /// <param name="MinValue">Valor mínimo del rango de salida.</param>
        /// <returns>Matriz de bytes con valores transformados.</returns>
        public static byte[,] ApplyLinearTransform(float[,] matrix, int matrixMaxValue, int matrixMinValue, byte MaxValue = 255, byte MinValue = 0)
        {
            int numThreads = Environment.ProcessorCount;
            int rows = matrix.GetLength(0);
            int columns = matrix.GetLength(1);
            int totalRemaining = rows * columns;
            byte[,] result = new byte[rows, columns];

            if (matrixMaxValue == matrixMinValue)
            {
                throw new ArgumentException("Max and min values are the same");
            }
            else
            {
                double scalingFactor = (MaxValue - MinValue) / (double)(matrixMaxValue - matrixMinValue);

                // Create an array of threads
                Thread[] threads = new Thread[numThreads];

                // Calculate the number of rows each thread will process
                int rowsPerThread = rows / numThreads;

                // Create and start a thread for each processor
                for (int t = 0; t < numThreads; t++)
                {
                    int startRow = t * rowsPerThread;
                    int endRow = (t == numThreads - 1) ? rows : startRow + rowsPerThread;
                    threads[t] = new Thread(() =>
                    {
                        for (int i = startRow; i < endRow; i++)
                        {
                            for (int j = 0; j < columns; j++)
                            {
                                float val = matrix[i, j];
                                int transformedVal = (int)(scalingFactor * (val - matrixMinValue));
                                transformedVal = Math.Max(MinValue, Math.Min(MaxValue, transformedVal));
                                result[i, j] = (byte)transformedVal;
                                //totalRemaining--;
                                //Console.WriteLine($"Rescaling: Remaining: {totalRemaining}");
                            }
                        }
                    });
                    threads[t].Start();
                }

                // Wait for all threads to complete
                foreach (Thread thread in threads)
                {
                    thread.Join();
                }
            }
            return result;
        }
        public void GetImage(string path, bool keepColor)
        {
            Layer[] layers;
            using (Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(path))
            {
                Rgba32[,] pixels = ReadPixelValues(image);
                Layer[] extractedLayers = new Layer[4];
                for (int i = 0; i < 4; i++)
                {
                    extractedLayers[i] = ExtractValuesToLayer(pixels, i);
                }
                if (!keepColor)
                {
                    layers = new Layer[1];
                    layers[0] = ToBlackWhite(extractedLayers);
                }
                else
                {
                    Console.Write(Pixels[i, j] + "\t");
                }
                Console.Write("\n");
            }
        }
    }
}
