using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
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
                                pixels[i, j] = image[i, j];
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
            return pixels;
        }

        static public Image GetImage(string path, bool color)
        {
            Image output;
            using (Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(path))
            {
                Rgba32[,] pixels = _readPixelValues(image);


            }
            return output;
        }
        static public Image GetImage(string path)
        {
            int numThreads = Environment.ProcessorCount;
            Bitmap sourceImage = new(path);
            Console.WriteLine($"Size of imported image is: {sourceImage.Height}{sourceImage.Width}");
            //int totalPixels = sourceImage.Height * sourceImage.Width;
            //int totalRemaining = totalPixels;
            //Get values from the image
            int width = sourceImage.Width;
            int height = sourceImage.Height;
            Image image = new Image(height, width);
            Console.WriteLine($"Image created. Size is: {height}x{width}");

            // Create an array of threads
            Thread[] threads = new Thread[numThreads];

            // Calculate the number of rows each thread will process
            int rowsPerThread = image.Rows / numThreads;

            // Create a lock object
            object lockObject = new object();

            // Create and start a thread for each processor
            for (int t = 0; t < numThreads; t++)
            {
                int startRow = t * rowsPerThread;
                int endRow = (t == numThreads - 1) ? image.Rows : startRow + rowsPerThread;
                threads[t] = new Thread(() =>
                {
                    for (int i = startRow; i < endRow; i++)
                    {
                        for (int j = 0; j < image.Columns; j++)
                        {
                            //Console.WriteLine($"Thread {t}: At position {i},{j} of Image");
                            Color pixelColor;
                            lock (lockObject)
                            {
                                pixelColor = sourceImage.GetPixel(j, i);
                            }
                            //Console.WriteLine($"Color is {pixelColor}");
                            //Get the color components
                            int red = pixelColor.R;
                            int green = pixelColor.G;
                            int blue = pixelColor.B;
                            //Assign equivalence in gray to image[i,j]
                            image[i, j] = Convert.ToByte((red + green + blue) / 3);
                            //Console.WriteLine($"Converted value is {image[i, j]}");
                            //totalRemaining -= 1;
                            //Console.WriteLine($"Loading: Remaining: {totalRemaining}");
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
            //is image null?
            if (image == null)
            {
                Console.WriteLine("Image is null");
            }
            else
            {
                Console.WriteLine("Image is not null");
            }
            return image;
        }
        static public void WriteImage(Image image)
        {
            Bitmap resultImage = new(image.Columns, image.Rows);
            for (int i = 0; i < image.Rows; i++)
            {
                for (int j = 0; j < image.Columns; j++)
                {
                    byte gray = image[i, j];
                    Color pixelColor = Color.FromArgb(gray, gray, gray);
                    resultImage.SetPixel(j, i, pixelColor);
                }
            }
            resultImage.Save("filtered_cyno.png");
        }
    }
}
