using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppFiltros
{
    /// <summary>
    /// Clase que contiene una imagen de profundidad máxima de 8 bits por pixel
    /// </summary>
    public class Image
    {
        #region Fields and Constructors
        /// <summary>
        /// Matriz que contiene los pixeles de la imagen
        /// </summary>
        public byte[,] Pixels { get; set; }
        /// <summary>
        /// Dimensión de la imagen
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// Valor máximo admitido por pixel
        /// </summary>
        public byte MaxValue { get; set; }
        /// <summary>
        /// Valor mínimo admitido por pixel
        /// </summary>
        public byte MinValue { get; set; }
        public Image(int size, byte maxValue = 255, byte minValue = 0)
        {
            Size = size;
            Pixels = new byte[size, size];
            MaxValue = maxValue;
            MinValue = minValue;
        }
        public Image(byte[,] matrix, byte maxValue = 255, byte minValue = 0)
        {
            Size = matrix.GetLength(0);
            Pixels = matrix;
            MaxValue = maxValue;
            MinValue = minValue;
        }
        public byte this[int x, int y]
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

        /// <summary>
        /// Trunca los valores en una matriz de flotantes y devuelve una matriz de bytes.
        /// </summary>
        /// <param name="matrix">Matriz de flotantes a truncar.</param>
        /// <param name="maxValue">Valor máximo para truncar.</param>
        /// <returns>Matriz de bytes con valores truncados.</returns>
        public static byte[,] TrunkValues(float[,] matrix, int maxValue = 255)
        {
            int matrixSize = matrix.GetLength(0);
            byte[,] result = new byte[matrixSize, matrixSize];

            for (int i = 0; i < matrixSize; i++)
            {
                for (int j = 0; j < matrixSize; j++)
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
            int size = matrix.GetLength(0);
            byte[,] result = new byte[size, size];

            if (matrixMaxValue == matrixMinValue)
            {
                throw new ArgumentException("Max and min values are the same");
            }
            else
            {
                double scalingFactor = (MaxValue - MinValue) / (double)(matrixMaxValue - matrixMinValue);

                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        float val = matrix[i, j];
                        int transformedVal = (int)(scalingFactor * (val - matrixMinValue));
                        transformedVal = Math.Max(MinValue, Math.Min(MaxValue, transformedVal));
                        result[i, j] = (byte)transformedVal;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Imprime la imagen en consola.
        /// </summary>
        public void Print()
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    Console.Write(Pixels[i, j] + "\t");
                }
                Console.Write("\n");
            }
        }
    }
}
