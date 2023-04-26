using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
    }
}
