using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppFiltros
{
    public class Filter
    {
        /// <summary>
        /// Tamaño de la máscara a utilizar
        /// </summary>
        public int MaskSize { get; set; }
        /// <summary>
        /// Matriz que contiene los valores de la máscara
        /// </summary>
        public float[,] Mask { get; set; }
        /// <summary>
        /// Indica si se debe aplicar el factor. Verdadero por defecto
        /// </summary>
        public bool ApplyFactor { get; set; }
        /// <summary>
        /// Factor aplicado. 1 por defecto
        /// </summary>
        public float Factor { get; set; }
        public Filter(int maskSize, float[,] mask, bool applyFactor = true, float factor = 1)
        {
            MaskSize = mask.GetLength(0);
            Mask = mask;
            ApplyFactor = applyFactor;
            Factor = factor;
        }
        public float this[int x, int y]
        {
            get
            {
                return Mask[x, y];
            }
            set
            {
                Mask[x, y] = value;
            }
        }
        
        public Filter(float[] maskArray, bool applyFactor = true, float factor = 1)
        {
            var output = GetMatrix(maskArray);
            Mask = output.Item1;
            MaskSize = output.Item2;
            ApplyFactor = applyFactor;
            Factor = factor;
        }

        #endregion


        /// <summary>
        /// Aplicar este filtro a una imagen sourceImage y devuelve una nueva imagen con el resultado.
        /// </summary>
        /// <param name="sourceImage">Imagen a procesar.</param>
        /// <param name="applyRescaling">Indica si se aplica el reescalado linear. Verdadero por defecto. Caso contrario, se trunca usando el módulo</param>
        /// <returns></returns>
        public Image ApplyFilter(Image sourceImage, bool applyRescaling = true)
        {
            int totalPixels = sourceImage.Rows * sourceImage.Columns;
            int totalRemaining = totalPixels;
            int numThreads = Environment.ProcessorCount;
            float[,] resultMatrix = new float[sourceImage.Rows, sourceImage.Columns];
            //In case of rescaling is true, this will record highest and lowest values
            float maxValue = sourceImage[0, 0];
            float minValue = sourceImage[0, 0];

            // Create an array of threads
            Thread[] threads = new Thread[numThreads];

            // Calculate the number of rows each thread will process
            int rowsPerThread = sourceImage.Rows / numThreads;

            // Create a lock object
            object lockObject = new object();

            // Create and start a thread for each processor
            for (int t = 0; t < numThreads; t++)
            {
                int startRow = t * rowsPerThread;
                int endRow = (t == numThreads - 1) ? sourceImage.Rows : startRow + rowsPerThread;
                threads[t] = new Thread(() =>
                {
                    for (int i = startRow; i < endRow; i++)
                    {
                        for (int j = 0; j < sourceImage.Columns; j++)
                        {
                            //Console.WriteLine($"Calculating pixel {i},{j}");
                            float pixelValue;
                            lock (lockObject)
                            {
                                pixelValue = CalculatePixel(sourceImage, i, j);
                                resultMatrix[i, j] = pixelValue;
                            }
                            //totalRemaining--;
                            //Console.WriteLine($"Calculation: Remaining: {totalRemaining}");

                            if (pixelValue > maxValue)
                            {
                                maxValue = pixelValue;
                            }
                            if (pixelValue < minValue)
                            {
                                minValue = pixelValue;
                            }
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

            Image output = new Image(sourceImage.Rows, sourceImage.Columns, 255, 0);
            if (applyRescaling && maxValue > 255 || minValue < 0)
            {
                //Console.WriteLine($"Applying linear rescaling with range [{sourceImage.MinValue}:{sourceImage.MaxValue}]");
                output.Pixels = Image.ApplyLinearTransform(resultMatrix, sourceImage.MaxValue, sourceImage.MinValue);
            }
            else
            {
                Console.WriteLine($"Applying truncation with range [{sourceImage.MinValue}:{sourceImage.MaxValue}]");
                output.Pixels = Image.TrunkValues(resultMatrix, sourceImage.MaxValue);
            }
            return output;
        }

        /// <summary>
        /// Calcula el nuevo valor de un píxel, aplicando los factores de la máscara a los correspondientes en sourceImage
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>Nuevo valor del píxel.</returns>
        private float CalculatePixel(Image sourceImage, int x, int y)
        {
            float sum = 0;
            int d = (MaskSize - 1) / 2;
            int startX = x - d;
            int startY = y - d;
            int filterX = 0;
            int filterY = 0;
            for (int i = startX; i <= x + d; i++)
            {
                filterX = 0;
                for (int j = startY; j <= y + d; j++)
                {
                    #if DEBUG
                    //Console.WriteLine($"\tCalculando pixel ({i}:{j})");
                    #endif
                    try
                    {
                        sum += sourceImage[i, j] * Factor * this[filterY, filterX];
                        #if DEBUG
                        //Console.WriteLine($"\t\t\t{sum}=({sourceImage[i, j]}*{Factor}*{this[filterY, filterX]})");
                        #endif
                    }
                    catch (IndexOutOfRangeException)
                    {
                        #if DEBUG
                        //Console.WriteLine("\tPosición no válida, pasando a la siguiente iteración...");
                        #endif
                        continue;
                    }
                    filterX++;
                }
                filterY++;
            }
            return sum;
        }

        /// <summary>
        /// Trunca los valores de <paramref name="matrix"/> a un rango de 0 a <paramref name="maxValue"/>. 255 por defecto.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        private static byte[,] TrunkValues(float[,] matrix, int maxValue = 255)
        {
            int rows = matrix.GetLength(0);
            int columns = matrix.GetLength(1);
            byte[,] result = new byte[rows, columns];

            // Determine the number of threads to use
            int numThreads = Environment.ProcessorCount;
            int rowsPerThread = rows / numThreads;

            // Create and start the threads
            Thread[] threads = new Thread[numThreads];
            for (int t = 0; t < numThreads; t++)
            {
                int startRow = t * rowsPerThread;
                int endRow = (t == numThreads - 1) ? rows : startRow + rowsPerThread;
                threads[t] = new Thread(() => TrunkValuesThread(matrix, result, startRow, endRow, columns, maxValue));
                threads[t].Start();
            }

            // Wait for all threads to complete
            for (int t = 0; t < numThreads; t++)
                threads[t].Join();

            return result;
        }


        private static void TrunkValuesThread(float[,] matrix, byte[,] result, int startRow, int endRow, int columns, int maxValue)
        {
            for (int i = startRow; i < endRow; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    result[i, j] = (byte)(Math.Round(matrix[i, j]) % maxValue);
                    //Console.WriteLine($"Truncating: Modulo of {matrix[i, j]} by {maxValue} is {result[i, j]}");
                }
            }
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
        private static byte[,] ApplyLinearTransform(float[,] matrix, int matrixMaxValue, int matrixMinValue, byte MaxValue = 255, byte MinValue = 0)
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

        private static (float[,],int) GetMatrix(float[] array)
        {
            float[,] matrix;
            int size = (int)Math.Sqrt(array.GetLength(0));
            matrix = new float[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    matrix[i, j] = array[i * size + j];
                }
            }
            return (matrix, size);
        }
    }
}
