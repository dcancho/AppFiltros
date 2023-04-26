using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Data;
using System.Threading.Tasks;

namespace AppFiltros
{
    public enum ImageType
    {
        BMP,
        JPG,
        PNG
    }

    public static class FileIO
    {
        /// <summary>
        /// Leer los valores de los pixeles de una imagen. Devuelve una matrix Rgba32. Multithread.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private static Rgba32[,] _readPixelValues(Image<Rgba32> image)
        {
            int remainingPixels = image.Height * image.Width;
            int processorsCount = Environment.ProcessorCount;
            int rowsPerThread = image.Height / processorsCount;
            Rgba32[,] pixels = new Rgba32[image.Height, image.Width];
            Thread[] threads = new Thread[processorsCount];
            object lockObject = new object();
            for (int t = 0; t < processorsCount; t++)
            {
                int startRow = t * rowsPerThread;
                int endRow = (t == processorsCount - 1) ? image.Height : startRow + rowsPerThread;
                threads[t] = new Thread(() =>
                {
                    for (int i = startRow; i < endRow; i++)
                    {
                        for (int j = 0; j < image.Width; j++)
                        {
                            lock (lockObject)
                            {
                                pixels[i, j] = image[j,i];
                            }
                            //remainingPixels--;
                            //Console.WriteLine($"Loading: {remainingPixels} remaining");
                        }
                    }
                });
                threads[t].Start();
            }
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
            return pixels;
        }

        private static Layer ToBlackWhite(Layer[] layers)
        {
            Layer output = new Layer(ValueType.Grayscale, layers[0].Rows, layers[0].Columns);
            Thread[] threads= new Thread[Environment.ProcessorCount];
            uint numThread = (uint)threads.GetLength(0);
            uint rowsPerThread = (uint)layers[0].Rows / numThread;
            object lockObject = new object();
            for (int t = 0; t < numThread; t++)
            {
                uint startRow = (uint)t * rowsPerThread;
                uint endRow = (t == numThread - 1) ? (uint)layers[0].Rows : startRow + rowsPerThread;
                threads[t] = new Thread(() =>
                {
                    for (uint i = startRow; i < endRow; i++)
                    {
                        for (uint j = 0; j < layers[0].Columns; j++)
                        {
                            lock (lockObject)
                            {
                                output[(uint)i, (uint)j] = (byte)((layers[0][i, j] + layers[1][i, j] + layers[2][i, j])/3);
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

            return output;
        }

        private static Layer _extractValuesToLayer(Rgba32[,] image, int type)
        {
            ValueType layerType;
            switch (type)
            {
                case 0:
                    layerType = ValueType.R;
                    break;
                case 1:
                    layerType = ValueType.G;
                    break;
                case 2:
                    layerType = ValueType.B;
                    break;
                case 3:
                    layerType = ValueType.A;
                    break;
                default:
                    layerType = ValueType.R;
                    break;
            }
            byte[,] bytes = new byte[image.GetLength(0), image.GetLength(1)];
            for (int i = 0; i < image.GetLength(0); i++)
            {
                for (int j = 0; j < image.GetLength(1); j++)
                {
                    switch (layerType)
                    {
                        case ValueType.R:
                            bytes[i, j] = image[i, j].R;
                            break;
                        case ValueType.G:
                            bytes[i, j] = image[i, j].G;
                            break;
                        case ValueType.B:
                            bytes[i, j] = image[i, j].B;
                            break;
                        case ValueType.A:
                            bytes[i, j] = image[i, j].A;
                            break;
                        default:
                            bytes[i, j] = image[i, j].R;
                            break;
                    }
                }
            }
            return new Layer(layerType, bytes);
        }
        static public Image GetImage(string path, bool color)
        {
            Layer[] layers;
            using (Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(path))
            {
                Rgba32[,] pixels = _readPixelValues(image);
                Layer[] extractedLayers = new Layer[pixels.Length];
                for (int i = 0; i < 4; i++)
                {
                    extractedLayers[i] = _extractValuesToLayer(pixels,i);
                }
                if(!color)
                {
                    layers = new Layer[1];
                    layers[0] = ToBlackWhite(extractedLayers);
                }
                else
                {
                    layers = extractedLayers;
                }
            }
            return new Image(layers);
        }
        static public void WriteImage(Image image, string path)
        {
            Image<Rgba32> resultImage = new((int)image.Columns, (int)image.Rows);
            for (uint i = 0; i < image.Rows; i++)
            {
                for (uint j = 0; j < image.Columns; j++)
                {
                    resultImage[(int)i,(int) j] = new Rgba32(image[0][i, j], image[1][i, j], image[2][i, j], image[3][i, j]);
                }
            }
            resultImage.Save(path);
        }
    }
}
