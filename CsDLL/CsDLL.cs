using System;
using System.Runtime.InteropServices;

namespace CsDLL
{
    public class EdgeDetectionCS
    {
        public static void EdgeDetectRGB_CS(byte[] tab_red, byte[] tab_green, byte[] tab_blue, byte[] tab_result)
        {
            //Iteratiing through the pixels and multiplying them by weigths

            for (int x = 0; x < tab_red.Length; x++)
            {
                float red = tab_red[x] * 0.3f;
                float green = tab_green[x] * 0.59f;
                float blue = tab_blue[x] * 0.11f;

                byte grayValue = (byte)(red + green + blue);

                tab_result[x] = grayValue;
            }
        }

        public static void EdgeDetectCS(IntPtr inputPtr, IntPtr outputPtr, int width, int height)
        {
            byte[] buffer = new byte[height * width * 3];
            Marshal.Copy(inputPtr, buffer, 0, buffer.Length);

            for (int i = 0; i < buffer.Length; i += 3)
            {
                byte r = buffer[i + 2];
                byte g = buffer[i + 1];
                byte b = buffer[i];

                byte gray = (byte)(r * 0.3 + g * 0.59 + b * 0.11);
                buffer[i] = buffer[i + 1] = buffer[i + 2] = gray; // Set grayscale value
            }

            Marshal.Copy(buffer, 0, outputPtr, buffer.Length);
        }
    }
}
