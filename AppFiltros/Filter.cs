using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppFiltros
{
    public class Filter
    {
        public float Factor { get; set; }
        public byte MaskSize { get; set; }
        public sbyte[,] Mask { get; set; }
        public Filter(byte maskSize, sbyte[,] mask)
        {
            MaskSize = maskSize;
            Mask = mask;
            Factor = CalculateFactor(mask);
        }
        static float CalculateFactor(sbyte[,] mask)
        {
            float factor = 0;
            for (byte i = 0; i < mask.GetLength(0); i++)
            {
                for (byte j = 0; j < mask.GetLength(1); j++)
                {
                    factor += Math.Abs(mask[i, j]);
                }
            }
            return 1.0f/factor;
        }   
        public Filter(sbyte[,] mask)
        {
            MaskSize = (byte)mask.GetLength(0);
            Mask = mask;
            Factor= CalculateFactor(mask);
        }
        public sbyte this[byte x, byte y]
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
    }
}
