using System;
using System.Drawing;

namespace Imacrypt
{
    public static class Imacrypt
    {
        private static Random RgbRandomizer = new Random();

        public static Bitmap BmpEncrypt(this string str)
        {
            int size = (int)Math.Round(Math.Sqrt(str.Length));
            if ((size * size) < str.Length) size++;
            Bitmap img = new Bitmap(size, size);
            int x = 0;
            int y = 0;
            foreach (char c in str)
            {
                bool even = (x + y) % 2 == 0;
                int random = RgbRandomizer.Next(0, 256); //hi
                img.SetPixel(x, y, Color.FromArgb(255,
                    even ? (c / 256) : random,
                    !even ? (c / 256) : random,
                    (c % 256)));
                x++;
                if (x == size)
                {
                    x = 0;
                    y++;
                }
            }
            return img;
        }

        public static string BmpDecrypt(this Bitmap img)
        {
            string result = string.Empty;

            for (int w = 0; w < img.Width; w++)
                for (int h = 0; h < img.Height; h++)
                {
                    Color c = img.GetPixel(h, w);
                    if (c != Color.FromArgb(0, 0, 0, 0))
                        result += (char)(((w + h) % 2 == 0 ? c.R : c.G * 256) + c.B);
                }

            return result;
        }
    }
}