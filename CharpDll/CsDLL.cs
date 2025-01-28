namespace CsharpDll
{
    public class EdgeDetectionCS
    {
        // Metoda do konwersji obrazów do odcieni szarości
        public static void EdgeDetectCS(byte[] tab_red, byte[] tab_green, byte[] tab_blue, byte[] tab_result)
        {
            // Iteracja po pikselach - 8 pikseli
            // Przechodzimy przez każdy piksel, zakładając, że tablice mają ten sam rozmiar
            for (int x = 0; x < tab_red.Length; x++)
            {
                // Obliczenie odcienia szarości na podstawie współczynników dla R, G, B
                // Dla każdego piksela obliczamy wartość szarości
                float red = tab_red[x] * 0.3f;
                float green = tab_green[x] * 0.59f;
                float blue = tab_blue[x] * 0.11f;

                // Zsumowanie składowych i zapisanie do wszystkich kanałów
                // Suma waży kanały RGB i tworzy nową wartość szarości /  jako byte 0-255
                byte grayValue = (byte)(red + green + blue);

                // Ustawiamy wartość odcienia szarości dla każdego kanału
                tab_result[x] = grayValue;
                tab_result[x] = grayValue;
                tab_result[x] = grayValue;
            }
        }
    }
}
