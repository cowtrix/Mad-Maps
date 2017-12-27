using System;
using UnityEngine;

namespace Dingo.Common.Collections
{
    public class Serializable2DArray<T>
    {
        public T[] Data;
        public int Height;
        public int Width;

        public Serializable2DArray(int width, int data)
        {
            Width = width;
            Height = data;
            Data = new T[Width*Height];
        }

        public Serializable2DArray(T[,] data)
        {
            if (data == null)
            {
                Width = 0;
                Height = 0;
                Data = null;
                return;
            }
            Width = data.GetLength(0);
            Height = data.GetLength(1);
            Data = new T[Width * Height];
            for (var u = 0; u < Width; u++)
            {
                for (var v = 0; v < Height; v++)
                {
                    var height = data[u, v];
                    var index = v * Width + u;

                    if (index < 0 || index > Data.Length - 1)
                    {
                        Debug.LogError(string.Format("Invalid index: {0} (width was {1}, height was {2}, x was {3}, y was {4}",
                            index, Width, Height, u, v));
                    }

                    Data[index] = height;
                }
            }
        }
        
        public T[,] Deserialize()
        {
            var heights = new T[Width, Height];
            for (var u = 0; u < Width; u++)
            {
                for (var v = 0; v < Height; v++)
                {
                    var index = v * Width + u;
                    heights[u, v] = Data[index];
                }
            }
            return heights;
        }

        public T this[int u, int v]
        {
            get
            {
                var index = v * Width + u;
                if (index >= Data.Length || index < 0)
                {
                    throw new IndexOutOfRangeException("Index: " + index);
                }
                return Data[index];
            }
            set
            {
                var index = v * Width + u;
                if (index >= Data.Length || index < 0)
                {
                    throw new IndexOutOfRangeException("Index: " + index);
                }
                Data[index] = value;
            }
        }

        public void Clear()
        {
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = default(T);
            }
        }

        public void Fill(T val)
        {
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = val;
            }
        }

        public bool HasData()
        {
            return Data != null && Data.Length > 0;
        }

        /*protected Serializable2DArray<T> Select(int x, int z, int width, int height)
        {
            if (x + width > Width || z + height > Height)
            {
                throw new IndexOutOfRangeException();
            }
            var result = new Serializable2DArray<T>(width, height);
            for (var u = x; u < x + width; ++u)
            {
                for (var v = z; v < z + height; ++v)
                {
                    result[u - x, v - z] = this[u, v];
                }
            }
            return result;
        }*/
    }
}