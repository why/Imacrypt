using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace Imacrypt
{
    /// <summary>
    /// Imacrypt is a small library that lets you encode byte arrays into images, and the other way around.
    /// </summary>
    public static class Imacrypt
    {
        /// <summary>
        /// Returns <see cref="Bitmap"/> containing your encoded array
        /// </summary>
        /// <param name="arr">The array that will be encoded</param>
        public static Bitmap BmpEncrypt(this byte[] arr)
        {

            //Initial bitmap size calculations
            int size = (arr.Length / 3) + (arr.Length % 3);
            int newSize = (int)Math.Round(Math.Sqrt(size));
            //Add 1 to width/height if newSize was rounded down
            newSize += (newSize * newSize < size) ? 1 : 0;
            //An int to keep track of the amount of bytes we wrote to the image
            int byteCounter = 0;
            //Creating the bitmap we will be writing our bytes to
            Bitmap img = new Bitmap(newSize, newSize);
            unsafe
            {
                //Lock bits so we can write bytes without the bitmap unlocking and re-locking itself after every written pixel
                BitmapData bitmapData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, img.PixelFormat);

                //Find the amount of bytes per pixel
                int bytesPerPixel = Image.GetPixelFormatSize(img.PixelFormat) / 8;

                //A few self-explanatory variables
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* PtrFirstPixel = (byte*)bitmapData.Scan0;

                //Loop through each pixel in the bitmap's height
                for (int y = 0; y < heightInPixels; y++)
                {
                    //Get the current row of pixels
                    byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);

                    //Loop through each pixel in the bitmap's length
                    for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                    {
                        //Check if the amount of written bytes is bigger than, or equals the amount of bytes in our array
                        if (byteCounter >= arr.Length)
                        {
                            //Set the alpha value of the current pixel to 0, so we know this pixel doesn't hold any bytes
                            //that we need to retrieve when decrypting the bitmap
                            currentLine[x + 3] = 0;
                        }
                        //Check if there's only 1 or 2 bytes left, this is a very important check
                        else if (byteCounter == arr.Length - 2 || byteCounter == arr.Length - 1)
                        {
                            //The amount of bytes that still need to be written (always 1 or 2)
                            int byteCount = (byte)(arr.Length - byteCounter);

                            //Set the alpha value of the current pixel to either 1 or 2, so we know this pixel only
                            //holds 1 or 2 bytes, instead of the full 3. This is important, because otherwise we
                            //will read too many bytes during decryption, and fail to return the original array
                            currentLine[x + 3] = (byte)byteCount;

                            //Loop through the amount of bytes that is left, again, always 1 or 2
                            //We're doing an inversed loop here because we write pixels as BGRA
                            for (int i = byteCount - 1; i > -1; i--)
                            {
                                //Set the R or G value of the current pixel to next byte in our array
                                currentLine[x + i] = arr[byteCounter];

                                //Increase the amount of written bytes by 1
                                byteCounter++;
                            }
                        }
                        //If we reach this else statement, that means we can still write 3 more bytes, so R, G and B
                        else
                        {
                            //Another inversed loop to write the next 3 bytes in our array to the current pixel
                            //Again, this is in BGRA format, which is why the loop is inversed
                            for (int i = 3; i > -1; i--)
                            {
                                //First execution of this loop will set the A value of the current pixel to 255
                                //and the other 3 executions will set the R, G and B values to the next byte in our array
                                currentLine[x + i] = (i == 3) ? (byte)255 : arr[byteCounter];

                                //Increase amount of written bytes by 1, unless we're in the first loop execution
                                //We don't want to increase that amount if we wrote to A, because that byte
                                //isn't a part of the array we're encrypting
                                byteCounter += (i == 3) ? 0 : 1;
                            }
                        }
                    }
                }
                //After we're done writing all our bytes to the bitmap, we unlock the bits again
                img.UnlockBits(bitmapData);
            }
            //Finally, we return the bitmap
            return img;
        }

        /// <summary>
        /// Returns <see cref="byte[]"/> containing your decoded <see cref="Bitmap"/>
        /// </summary>
        /// <param name="b">The <see cref="Bitmap"/> that will be decoded</param>
        public static byte[] BmpDecrypt(this Bitmap b)
        {
            //Create list in which we will store all the read bytes
            List<byte> result = new List<byte>((b.Width * b.Height) * 3);
            unsafe
            {
                //Lock bits so we can read bytes without the bitmap unlocking and re-locking itself after every read pixel
                BitmapData bitmapData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly, b.PixelFormat);

                //Find the amount of bytes per pixel
                int bytesPerPixel = Image.GetPixelFormatSize(b.PixelFormat) / 8;

                //A few self-explanatory variables
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* PtrFirstPixel = (byte*)bitmapData.Scan0;

                //Loop through each pixel in the bitmap's height
                for (int y = 0; y < heightInPixels; y++)
                {
                    //Get the current row of pixels
                    byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);

                    //Loop through each pixel in the bitmap's length
                    for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                    {
                        //If we reach this if statement, that means we can still read 3 more bytes, so R, G and B
                        if (currentLine[x + 3] == 255)
                        {
                            //An inversed loop to read the next 3 bytes in our array to the current pixel
                            //This is in BGRA format, which is why the loop is inversed
                            for (int i = 3; i > -1; i--)
                            {
                                //Read the R, G and B values of the current pixel, and skip A because that's not
                                //actually part of the original byte array
                                if (i != 3)
                                    result.Add(currentLine[x + i]);
                            }
                        }
                        //If we reach this else if statement, that means we can't read 3 more bytes. Instead,
                        //we will now read as many bytes as there is left. (This is always 1 or 2)
                        else if (currentLine[x + 3] != 0)
                        {
                            //Loop through the amount of bytes that is left, again, always 1 or 2
                            //We're doing an inversed loop here because we read pixels as BGRA
                            for (int j = currentLine[x + 3]; j > -1; j--)
                            {
                                //Read the R, G and B values of the current pixel and add them to our list of bytes
                                //But again, skip the A value because it's not actually part of the original
                                //byte array
                                if (j != currentLine[x + 3])
                                    result.Add(currentLine[x + j]);
                            }
                        }
                        //If we reach this else statement, that means the alpha value of the current pixel
                        //is set to 0, which means there is no bytes left to read and add to the result array
                        //so we can break out of the loop
                        else break;
                    }
                }
                //After we're done reading all the bytes from the bitmap, we unlock the bits again
                b.UnlockBits(bitmapData);
            }
            //Finally, we convert the List<byte> back to a byte array and return it
            return result.ToArray();
        }
    }
}
