using System;
using System.IO;
using JetBrains.Annotations;

namespace MadMaps.Common.Collections
{
    [Serializable]
    public class Serializable2DByteArray : Serializable2DArray<byte>
    {
        public Serializable2DByteArray(int width, int data)
            : base(width, data)
        {
        }

        public Serializable2DByteArray(byte[,] data)
            : base(data)
        {
        }

        [Pure]
        public Serializable2DByteArray Select(int x, int z, int width, int height)
        {
            if (x + width > Width || z + height > Height)
            {
                throw new IndexOutOfRangeException();
            }
            var result = new Serializable2DByteArray(width, height);
            for (var u = x; u < x + width; ++u)
            {
                for (var v = z; v < z + height; ++v)
                {
                    result[u - x, v - z] = this[u, v];
                }
            }
            return result;
        }

        [Pure]
        public int[,] DeserializeToInt()
        {
            var data = new int[Width, Height];
            for (var u = 0; u < Width; u++)
            {
                for (var v = 0; v < Height; v++)
                {
                    var index = v * Width + u;
                    data[u, v] = Data[index];
                }
            }
            return data;
        }

        public void DeserializeToInt(int[,] data)
        {
            if (data.GetLength(0) != Width || data.GetLength(1) != Height)
            {
                throw new Exception("Data array wrong size!");
            }
            for (var u = 0; u < Width; u++)
            {
                for (var v = 0; v < Height; v++)
                {
                    var index = v * Width + u;
                    data[u, v] = Data[index];
                }
            }
        }

        protected override byte ReadFromStream(BinaryReader br)
        {
            return br.ReadByte();
        }
    }
}