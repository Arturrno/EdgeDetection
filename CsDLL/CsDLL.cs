namespace CsDLL
{
    public class EdgeDetectionCS
    {
        public static void EdgeDetectCS(byte[] tab_red, byte[] tab_green, byte[] tab_blue, byte[] tab_result)
        {
            // Iteracja po pikselach - 8 pikseli
            // Przechodzimy przez każdy piksel, zakładając, że tablice mają ten sam rozmiar
            for (int x = 0; x < tab_red.Length; x++)
            {
                float red = tab_red[x] * 0.3f;
                float green = tab_green[x] * 0.59f;
                float blue = tab_blue[x] * 0.11f;

                byte grayValue = (byte)(red + green + blue);

                tab_result[x] = grayValue;
                tab_result[x] = grayValue;
                tab_result[x] = grayValue;
            }
        }
    }
}
