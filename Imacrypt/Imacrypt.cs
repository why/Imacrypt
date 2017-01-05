using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace Imacrypt
{
    public static class Imacrypt
    {
        private static Random RgbRandomizer = new Random();

        public static Bitmap BmpEncrypt(this byte[] arr)
        {
            int size = (arr.Length / 3) + (arr.Length % 3);
            int newSize = (int)Math.Round(Math.Sqrt(size));
            newSize += (newSize * newSize <= size) ? 1 : 0;
            int byteCounter = 0;

            Bitmap img = new Bitmap(newSize, newSize);
            unsafe
            {
                BitmapData bitmapData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, img.PixelFormat);

                int bytesPerPixel = Image.GetPixelFormatSize(img.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* PtrFirstPixel = (byte*)bitmapData.Scan0;

                for (int y = 0; y < heightInPixels; y++)
                {
                    byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        if (byteCounter >= arr.Length)
                        {
                            currentLine[x + 3] = 0;
                        }
                        else if (byteCounter == arr.Length - 2 || byteCounter == arr.Length - 1)
                        {
                            int byteCount = (byte)(arr.Length - byteCounter);
                            currentLine[x + 3] = (byte)byteCount;

                            for (int i = byteCount - 1; i > -1; i--)
                            {
                                currentLine[x + i] = arr[byteCounter];
                                byteCounter++;
                            }
                        }
                        else
                        {
                            for (int i = 3; i > -1; i--)
                            {
                                currentLine[x + i] = (i == 3) ? (byte)255 : arr[byteCounter];
                                byteCounter += (i == 3) ? 0 : 1;
                            }
                        }
                    }
                }
                img.UnlockBits(bitmapData);
            }
            return img;
        }

        public static byte[] BmpDecrypt(this Bitmap b)
        {
            List<byte> result = new List<byte>((b.Width * b.Height) * 3);

            unsafe
            {
                BitmapData bitmapData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly, b.PixelFormat);

                int bytesPerPixel = Image.GetPixelFormatSize(b.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* PtrFirstPixel = (byte*)bitmapData.Scan0;

                for (int y = 0; y < heightInPixels; y++)
                {
                    byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                    for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                    {
                        if (currentLine[x + 3] == 255)
                        {
                            for (int i = 3; i > -1; i--)
                            {
                                if (i != 3)
                                    result.Add(currentLine[x + i]);
                            }
                        }
                        else if (currentLine[x + 3] != 0)
                            for (int j = currentLine[x + 3]; j > -1; j--)
                            {
                                if (j != currentLine[x + 3])
                                {
                                    byte xx = currentLine[x + j];
                                    result.Add(currentLine[x + j]);
                                }
                            }
                        else break;
                    }
                }
            }
            return result.ToArray();
        }
    }
}
