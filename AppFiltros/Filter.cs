namespace AppFiltros
{
    public class Filter
    {
        #region Properties and constructors
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
        #endregion


        /// <summary>
        /// Aplica el filtro a una capa y devuelve una nueva capa ya filtrada.
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="applyRescaling"></param>
        /// <param name="maxPixelValue"></param>
        /// <param name="minPixelValue"></param>
        /// <returns></returns>
        private Layer ApplyFilter(Layer layer, bool applyRescaling = true, byte maxPixelValue = 255, byte minPixelValue = 0)
        {
            Console.WriteLine($"Applying filter to {layer.Type} layer");
            int totalRemaining = (int)(layer.Rows * layer.Columns);
            float[,] resultMatrix = new float[layer.Rows, layer.Columns];
            float maxValue = layer[0, 0];
            float minValue = layer[0, 0];
            Thread[] threads = new Thread[Environment.ProcessorCount];
            int numThreads = threads.GetLength(0);
            int rowsPerThread = (int)layer.Rows / numThreads;
            object lockObject = new object();
            for (int t = 0; t < numThreads; t++)
            {
                int startRow = t * rowsPerThread;
                int endRow = (t == numThreads - 1) ? (int)layer.Rows : startRow + rowsPerThread;
                threads[t] = new Thread(() =>
                {
                    for (int i = startRow; i < endRow; i++)
                    {
                        for (int j = 0; j < layer.Columns; j++)
                        {
                            float pixelValue;
                            lock (lockObject)
                            {
                                pixelValue = CalculatePixel(layer, i, j);
                                resultMatrix[i, j] = pixelValue;
                            }
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
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
            Layer output = new(layer.Type, layer.Rows, layer.Columns);
            if (applyRescaling && maxValue > 255 || minValue < 0)
            {
                Console.WriteLine($"Applying linear rescaling with range [{minPixelValue}:{maxPixelValue}] to {layer.Type} layer");
                output.Pixels = ApplyLinearTransform(resultMatrix, maxPixelValue, minPixelValue);
            }
            else
            {
                Console.WriteLine($"Applying truncation to layer {layer.Type} with range [{minPixelValue}:{maxPixelValue}] to {layer.Type} layer");
                output.Pixels = TrunkValues(resultMatrix, maxPixelValue);
            }
            return output;
        }


        /// <summary>
        /// Aplicar este filtro a una imagen sourceImage y devuelve una nueva imagen con el resultado.
        /// </summary>
        /// <param name="sourceImage">Imagen a procesar.</param>
        /// <param name="applyRescaling">Indica si se aplica el reescalado linear. Verdadero por defecto. Caso contrario, se trunca usando el módulo</param>
        /// <returns></returns>
        public Image ApplyFilter(Image image, bool applyRescaling = true, byte maxPixelValue = 255, byte minPixelValue = 0)
        {
            Console.WriteLine($"Applying filter to image. {image.LayerDepth} layers detected.");
            Layer[] layers = new Layer[image.LayerDepth];
            for (int i = 0; i < image.LayerDepth; i++)
            {
                layers[i] = ApplyFilter(image[i], applyRescaling, maxPixelValue, minPixelValue);
            }
            return new Image(layers);
        }


        /// <summary>
        /// Calcular nuevo valor de cada pixel según el filtro aplicado.
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private float CalculatePixel(Layer layer, int x, int y)
        {
            float sum = 0;
            int d = (MaskSize - 1) / 2;
            int startX = x - d;
            int startY = y - d;
            int filterX;
            int filterY = 0;
            for (uint i = (uint)startX; i <= x + d; i++)
            {
                filterX = 0;
                for (uint j = (uint)startY; j <= y + d; j++)
                {
                    //Console.WriteLine($"\tCalculando pixel ({i}:{j})");
                    try
                    {
                        sum += layer[i, j] * Factor * this[filterY, filterX];
                        //Console.WriteLine($"\t\t\t{sum}=({sourceImage[i, j]}*{Factor}*{this[filterY, filterX]})");
                    }
                    catch (IndexOutOfRangeException)
                    {
                        //Console.WriteLine("\tPosición no válida, pasando a la siguiente iteración...");
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
    }
}
