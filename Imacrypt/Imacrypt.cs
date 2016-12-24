using System;
using System.Collections.Generic;
using System.Drawing;

namespace Imacrypt
{
    public static class Imacrypt
    {
        private static Random RgbRandomizer = new Random();

        public static Bitmap BmpEncrypt(this byte[] arr)
        {
            int size = (arr.Length / 3) + (arr.Length % 3);
            int newSize = (int)Math.Round(Math.Sqrt(size));
            newSize += (newSize * newSize < size) ? 1 : 0;

            Bitmap img = new Bitmap(newSize, newSize);
            int x = 0, y = 0;
            List<byte> collection = new List<byte>();
            int count = 0;
            foreach (byte b in arr)
            {
                count++;
                collection.Add(b);
                if (count == 3)
                {
                    img.SetPixel(x, y, Color.FromArgb(255, collection[0], collection[1], collection[2]));
                    collection.Clear();
                    x++;
                    count = 0;
                    if (x == newSize)
                    {
                        x = 0;
                        y++;
                    }
                }
            }

            if (collection.Count > 0)
                img.SetPixel(x, y, Color.FromArgb(collection.Count, collection[0], collection.Count > 1 ? collection[1] : 0, 0));
            return img;
        }

        public static byte[] BmpDecrypt(this Bitmap img)
        {
            List<byte> result = new List<byte>();
            for (int w = 0; w < img.Width; w++)
                for (int h = 0; h < img.Height; h++)
                {
                    Color c = img.GetPixel(h, w);
                    switch (c.A)
                    {
                        case 255: result.AddRange(new byte[] { c.R, c.G, c.B }); break;
                        case 2: result.AddRange(new byte[] { c.R, c.G }); break;
                        case 1: result.Add(c.R); break;
                    }
                }
            return result.ToArray();
        }
    }
}

