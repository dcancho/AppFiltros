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
        /// <summary>
        /// Aplicar este filtro a una imagen sourceImage y devuelve una nueva imagen con el resultado.
        /// </summary>
        /// <param name="sourceImage">Imagen a procesar.</param>
        /// <param name="applyRescaling">Indica si se aplica el reescalado linear. Verdadero por defecto. Caso contrario, se trunca usando el módulo</param>
        /// <returns></returns>
        public Layer ApplyFilter(Layer layer, bool applyRescaling = true, byte maxPixelValue = 255, byte minPixelValue = 0)
        {
            int totalRemaining = (int)(layer.Rows * layer.Columns);
            float[,] resultMatrix = new float[layer.Rows, layer.Columns];
            //In case of rescaling is true, this will record highest and lowest values
            float maxValue = layer[0, 0];
            float minValue = layer[0, 0];

            // Create an array of threads
            Thread[] threads = new Thread[Environment.ProcessorCount];
            int numThreads = threads.GetLength(0);

            // Calculate the number of rows each thread will process
            int rowsPerThread = (int)layer.Rows / numThreads;

            // Create a lock object
            object lockObject = new object();

            // Create and start a thread for each processor
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
                            //Console.WriteLine($"Calculating pixel {i},{j}");
                            float pixelValue;
                            lock (lockObject)
                            {
                                pixelValue = CalculatePixel(layer, i, j);
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

            Layer output = new(layer.Type,layer.Rows, layer.Columns);
            if (applyRescaling && maxValue > 255 || minValue < 0)
            {
                //Console.WriteLine($"Applying linear rescaling with range [{sourceImage.MinValue}:{sourceImage.MaxValue}]");
                output.Pixels = Image.ApplyLinearTransform(resultMatrix, maxPixelValue, minPixelValue);
            }
            else
            {
                Console.WriteLine($"Applying truncation with range [{minPixelValue}:{maxPixelValue}]");
                output.Pixels = Image.TrunkValues(resultMatrix, maxPixelValue);
            }
            return output;
        }
        public Image ApplyFilter(Image image, bool applyRescaling = true, byte maxPixelValue = 255, byte minPixelValue = 0)
        {
            Layer[] layers = new Layer[image.LayerDepth];
            for (int i = 0; i < image.LayerDepth; i++)
            {
                layers[i] = ApplyFilter(image[i], applyRescaling, maxPixelValue, minPixelValue);
            }
            return new Image(layers);
        }
        /// <summary>
        /// Calcula el nuevo valor de un píxel, aplicando los factores de la máscara a los correspondientes en sourceImage
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>Nuevo valor del píxel.</returns>
        private float CalculatePixel(Layer layer, int x, int y)
        {
            float sum = 0;
            int d = (MaskSize - 1) / 2;
            int startX = x - d;
            int startY = y - d;
            int filterX = 0;
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
    }
}
