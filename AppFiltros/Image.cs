using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppFiltros
{
    public class Image
    {
        #region Fields and Constructors
        public byte[,] Pixels { get; set; }
        public byte Width { get; set; }
        public byte Height { get; set; }
        public Image(byte width, byte height)
        {
            Width = width;
            Height = height;
            Pixels = new byte[width, height];
        }
        public Image(byte[,] matrix)
        {
            Width = (byte)matrix.GetLength(0);
            Height = (byte)matrix.GetLength(1);
            Pixels = matrix;
        }
        public byte this[byte x, byte y]
        {
            get
            {
                return Pixels[x, y];
            }
            set
            {
                Pixels[x, y] = value;
            }
        }
        #endregion

        public void ApplyFilter(Filter filter, bool applyRescaling = false)
        {
            int[,] resultMatrix = new int[Width, Height];
            int maxValue = int.MinValue;
            int minValue = int.MaxValue;
            for (byte i = 0; i < Width; i++)
            {
                for (byte j = 0; j < Height; j++)
                {
                    resultMatrix[i, j] = CalculatePixel(filter, i, j);
                    if (resultMatrix[i, j] > maxValue)
                    {
                        maxValue = resultMatrix[i, j];
                    }
                    if (resultMatrix[i, j] < minValue)
                    {
                        minValue = resultMatrix[i, j];
                    }
                }
            }
            if(applyRescaling && maxValue > 255 || minValue < 0)
            {
                Pixels = ApplyLinearTransform(resultMatrix, maxValue, minValue);
            }
            else
            {
                Pixels = TrunkValues(resultMatrix);
            }
        }
        private static byte[,] TrunkValues(int[,] matrix, int maxValue = 255, int minValue=0)
        {
            byte[,] result = new byte[matrix.GetLength(0), matrix.GetLength(1)];
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (matrix[i, j] > maxValue)
                    {
                        result[i, j] = (byte)maxValue;
                    }
                    else if (matrix[i, j] < minValue)
                    {
                        result[i, j] = (byte)minValue;
                    }
                    else
                    {
                        result[i, j] = (byte)matrix[i, j];
                    }
                }
            }
            return result;
        }
        private static byte[,] ApplyLinearTransform(int[,] matrix, int matrixMaxValue, int matrixMinValue, byte MaxValue = 255, byte MinValue = 0)
        {
            int numRows = matrix.GetLength(0);
            int numCols = matrix.GetLength(1);
            byte[,] result = new byte[numRows, numCols];

            if (matrixMaxValue == matrixMinValue)
            {
                throw new ArgumentException("Max and min values are the same");
            }
            else
            {
                double scalingFactor = (MaxValue - MinValue) / (double)(matrixMaxValue - matrixMinValue);

                for (int i = 0; i < numRows; i++)
                {
                    for (int j = 0; j < numCols; j++)
                    {
                        int val = matrix[i, j];
                        int transformedVal = (int)(scalingFactor * (val - matrixMinValue));
                        transformedVal = Math.Max(MinValue, Math.Min(MaxValue, transformedVal));
                        result[i, j] = (byte)transformedVal;
                    }
                }
            }

            return result;
        }
        public void Print()
        {
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    Console.Write(Pixels[i, j]+"\t");
                }
                Console.Write("\n");
            }
        }
        private int CalculatePixel(Filter filter, byte x, byte y)
        {
            byte[,] bytes = ExtractMatrix(filter.MaskSize, x, y);
            int sum = 0;

            for (byte i = 0; i < filter.MaskSize; i++)
            {
                for (byte j = 0; j < filter.MaskSize; j++)
                {
                    sum += bytes[i, j] * filter[i, j];
                }
            }
            sum = (int)Math.Round(sum * filter.Factor);
            return sum;

        }
        private byte[,] ExtractMatrix(byte maskSize, byte x, byte y)
        {
            byte[,] bytes = new byte[maskSize, maskSize];
            int startX = (int)Math.Round(((double)x - maskSize / 2),MidpointRounding.ToPositiveInfinity);
            int startY = (int)Math.Round(((double)y - maskSize / 2), MidpointRounding.ToPositiveInfinity);

            for (int i = 0; i < maskSize; i++)
            {
                for (int j = 0; j < maskSize; j++)
                {
                    int pixelX = startX + i;
                    int pixelY = startY + j;
                    if (pixelX >= 0 && pixelX < Width && pixelY >= 0 && pixelY < Height)
                    {
                        bytes[i, j] = Pixels[pixelX, pixelY];
                    }
                    else
                    {
                        bytes[i, j] = 0;
                    }
                }
            }
            return bytes;
        }
    }
}
