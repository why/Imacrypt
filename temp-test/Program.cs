using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Imacrypt;
using System.Diagnostics;
using System.Drawing;

namespace temp_test
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] b = new byte[(10000 * 1000)];

            for (int i = 0; i < (10000 * 1000); i++)
                b[i] = (byte)(i % 255);
            Bitmap bmp = b.BmpEncrypt();
            var dec = bmp.BmpDecrypt();
            Console.WriteLine(dec.SequenceEqual(b));
            Console.ReadKey();
        }
    }
}
