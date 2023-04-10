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
		public Image ApplyFilter(Image sourceImage, bool applyRescaling = true)
		{
			float[,] resultMatrix = new float[sourceImage.Size, sourceImage.Size];
			//In case of rescaling is true, this will record highest and lowest values
			float maxValue = sourceImage[0, 0];
			float minValue = sourceImage[0, 0];
			for (byte i = 0; i < sourceImage.Size; i++)
			{
				for (byte j = 0; j < sourceImage.Size; j++)
				{
					Console.WriteLine($"Calculating pixel {i},{j}");
					resultMatrix[i, j] = CalculatePixel(sourceImage, i, j);
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
			Image output = new Image(sourceImage.Size,255,0);
			if (applyRescaling && maxValue > 255 || minValue < 0)
			{
				Console.WriteLine($"Applying linear rescaling with range [{sourceImage.MinValue}:{sourceImage.MaxValue}]");
				output.Pixels = Image.ApplyLinearTransform(resultMatrix, sourceImage.MaxValue, sourceImage.MinValue);
			}
			else
			{
				Console.WriteLine($"Applying truncation with range [{sourceImage.MinValue}:{sourceImage.MaxValue}]");
                output.Pixels = Image.TrunkValues(resultMatrix, sourceImage.MaxValue);
			}
			return output;
		}

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
                    Console.WriteLine($"\tCalculando pixel ({i}:{j})");
                    try
                    {
                        sum += sourceImage[i, j] * Factor * this[filterY, filterX];
                        Console.WriteLine($"\t\t\t{sum}=({sourceImage[i, j]}*{Factor}*{this[filterY, filterX]})");
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Console.WriteLine("\tPosición no válida, pasando a la siguiente iteración...");
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
