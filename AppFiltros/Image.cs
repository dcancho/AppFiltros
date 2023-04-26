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
        /// Objetos Layer contenidos en la imagen.
        /// </summary>
        public Layer[] Layers { get; set; }


        public uint Rows { get; set; }


        public uint Columns { get; set; }


        /// <summary>
        /// Valor máximo admitido por pixel
        /// </summary>
        public byte MaxValue { get; set; }


        /// <summary>
        /// Valor mínimo admitido por pixel
        /// </summary>
        public byte MinValue { get; set; }


        public Image(Layer[] layers, byte maxValue = 255, byte minValue = 0)
        {
            Rows = layers[0].Rows;
            Columns = layers[0].Columns;
            LayerDepth = (byte)layers.GetLength(0);
            Layers = layers;
            MaxValue = maxValue;
            MinValue = minValue;
        }

        public Image(string path, bool keepColor)
        {
            GetImage(path, keepColor);
            MaxValue= 255;
            MinValue = 0;
            if (Layers == null)
            {
                throw new Exception("No se pudo cargar la imagen. Posiblemente es un archivo dañado.");
            }
        }


        /// <summary>
        /// x: Rows
        /// y: Columns
        /// z: Layer
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public byte this[int x, int y, int z]
        {
            get
            {
                return Layers[z].Pixels[x, y];
            }
            set
            {
                Layers[z].Pixels[x, y] = value;
            }
        }


        public Layer this[int i]
        {
            set { Layers[i] = value; }
            get { return Layers[i]; }
        }

        /// <summary>
        /// Guarda la imagen con el nombre y formato especificado. Formato por defecto es PNG.
        /// </summary>
        /// <param name="path"></param>
        public void WriteImage(string path)
        {
            Image<Rgba32> resultImage = new((int)Columns, (int)Rows);
            for (uint i = 0; i < Rows; i++)
            {
                for (uint j = 0; j < Columns; j++)
                {
                    resultImage[(int)j, (int)i] = LayerDepth > 1
                        ? new Rgba32(this[0][i, j], this[1][i, j], this[2][i, j], 255)
                        : new Rgba32(this[0][i, j], this[0][i, j], this[0][i, j], 255);

                }
            }
            resultImage.SaveAsPng(path);
        }



        /// <summary>
        /// Leer los valores de los pixeles de una imagen. Devuelve una matrix Rgba32. Multithread.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private static Rgba32[,] ReadPixelValues(Image<Rgba32> image)
        {
            int remainingPixels = image.Height * image.Width;
            int processorsCount = Environment.ProcessorCount;
            int rowsPerThread = image.Height / processorsCount;
            Rgba32[,] pixels = new Rgba32[image.Height, image.Width];
            Thread[] threads = new Thread[processorsCount];
            object lockObject = new();
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
                                pixels[i, j] = image[j, i];
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
            Layer output = new(ValueType.Grayscale, layers[0].Rows, layers[0].Columns);
            Thread[] threads = new Thread[Environment.ProcessorCount];
            uint numThread = (uint)threads.GetLength(0);
            uint rowsPerThread = (uint)layers[0].Rows / numThread;
            object lockObject = new();
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
                                output[(uint)i, (uint)j] = (byte)((layers[0][i, j] + layers[1][i, j] + layers[2][i, j]) / 3);
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

        private static Layer ExtractValuesToLayer(Rgba32[,] image, int type)
        {
            var layerType = type switch
            {
                0 => ValueType.R,
                1 => ValueType.G,
                2 => ValueType.B,
                3 => ValueType.A,
                _ => ValueType.R,
            };
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
                        case ValueType.Grayscale:
                            break;
                        default:
                            bytes[i, j] = image[i, j].R;
                            break;
                    }
                }
            }
            return new Layer(layerType, bytes);
        }
        public void GetImage(string path, bool keepColor)
        {
            Layer[] layers;
            using (Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(path))
            {
                Rgba32[,] pixels = ReadPixelValues(image);
                Layer[] extractedLayers = new Layer[pixels.Length];
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
                    layers = extractedLayers;
                }
            }
            Rows = layers[0].Rows;
            Columns = layers[0].Columns;
            LayerDepth = (byte)layers.Length;
            Layers = layers;
        }
    }
}
