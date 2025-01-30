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

        public static void EdgeDetectCS(byte[] input, byte[] output, int width, int height)
        {
            // Convert the image to grayscale first
            byte[] grayscale = new byte[input.Length / 3];
            for (int i = 0; i < input.Length; i += 3)
            {
                byte r = input[i];
                byte g = input[i + 1];
                byte b = input[i + 2];

                grayscale[i / 3] = (byte)(r * 0.3 + g * 0.59 + b * 0.11); // Grayscale value
                output[i] = output[i + 1] = output[i + 2] = grayscale[i / 3];
            }

            // Apply blur by averaging the center pixel and its 4 neighbors
            for (int y = 1; y < height - 1; y++) // Avoid edges
            {
                for (int x = 1; x < width - 1; x ++) // Avoid edges
                {
                    // Get the grayscale values of the center pixel and its 4 neighbors
                    byte center = grayscale[y * width + x];
                    byte top = grayscale[(y - 1) * width + x];
                    byte bottom = grayscale[(y + 1) * width + x];
                    byte left = grayscale[y * width + (x - 1)];
                    byte right = grayscale[y * width + (x + 1)];

                    // Calculate the average
                    byte blurredValue = (byte)((center + top + bottom + left + right) / 5);

                    // Set the blurred value in the output array (RGB channels)
                    int outputIndex = (y * width + x) * 3;
                    output[outputIndex] = output[outputIndex + 1] = output[outputIndex + 2] = blurredValue;
                }
            }
        }
    }
}
