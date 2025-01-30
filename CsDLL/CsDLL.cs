/*************************************************************************************************************
 *  Temat   :   EdgeDetection - Prosty program do detekcji krawędzi w obrazach,
 *              udało mi się zaimplementować algorytm poszarzania i blurowania w obrazach w języku C# oraz w języku asemblera.
 *  Autor   :   Artur Szaboń
 *  Opis    :   Program pozwala na wczytanie obrazu w formacie .bmp, a następnie przetworzenie go za pomocą algorytmu przekształcenia
 *              na odcienie szarości, poprzez przemnożenie wartości pikseli przez odpowiednie wagi. Następnie obraz jest rozmywany
 *              poprzez wzięcie średniej z 4 sąsiadujących pikseli oraz piksela centralnego. Wynikowy obraz jest wyświetlany w oknie.
 *  Wersja  :   V0.5 - implementacja działającego blura w C# oraz w ASM
 *              V0.4 - próby dodania algorytmu blurowania w ASM, dodanie nowego algorytmu w C#
 *              V0.3 - zamiana algorytmu wektorowego na algorytm obsługujący pojedyńcze piksele aby dodać blur, dodanie zapisu wyniku do pliku Excel
 *              V0.2 - dodanie obsługi wielowątkowości, dodanie obsługi bibliotek DLL, dodanie algorytmu w C#, modyfikacja interfejsu
 *              V0.1 - stworzenie projektu i dodanie bibliotek DLL, implementacja algorytmu wektorowego w ASM, stworzenie prostego interfejsu
 *************************************************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace CsDLL
{
    public class EdgeDetectionCS
    {
        /// <summary>
        /// Metoda dokonująca konwersję do skali szarości.
        /// Przetwarza każdy piksel, stosując wagi dla składowych RGB.
        /// </summary>
        /// <param name="tab_red">Tablica wartości składowej czerwonej obrazu.</param>
        /// <param name="tab_green">Tablica wartości składowej zielonej obrazu.</param>
        /// <param name="tab_blue">Tablica wartości składowej niebieskiej obrazu.</param>
        /// <param name="tab_result">Tablica wynikowa zawierająca obraz w skali szarości.</param>
        public static void EdgeDetectRGB_CS(byte[] tab_red, byte[] tab_green, byte[] tab_blue, byte[] tab_result)
        {
            // Iterowanie przez piksele i przeliczanie ich wartości na skalę szarości
            for (int x = 0; x < tab_red.Length; x++)
            {
                float red = tab_red[x] * 0.3f;
                float green = tab_green[x] * 0.59f;
                float blue = tab_blue[x] * 0.11f;

                byte grayValue = (byte)(red + green + blue);

                tab_result[x] = grayValue;
            }
        }

        /// <summary>
        /// Metoda dokonująca konwersję do skali szarości oraz zastosowanie efektu rozmycia.
        /// </summary>
        /// <param name="input">Tablica bajtów reprezentująca obraz wejściowy w formacie RGB.</param>
        /// <param name="output">Tablica bajtów zawierająca wynik przetwarzania obrazu.</param>
        /// <param name="width">Szerokość obrazu.</param>
        /// <param name="height">Wysokość obrazu.</param>
        public static void EdgeDetectCS(byte[] input, byte[] output, int width, int height)
        {
            // Konwersja obrazu na skalę szarości
            byte[] grayscale = new byte[input.Length / 3];
            for (int i = 0; i < input.Length; i += 3)
            {
                byte r = input[i];
                byte g = input[i + 1];
                byte b = input[i + 2];

                grayscale[i / 3] = (byte)(r * 0.3 + g * 0.59 + b * 0.11); // Obliczanie wartości szarości
                output[i] = output[i + 1] = output[i + 2] = grayscale[i / 3];
            }

            // Zastosowanie efektu rozmycia poprzez uśrednianie wartości piksela i jego sąsiadów
            for (int y = 1; y < height - 1; y++) // Pomijanie krawędzi
            {
                for (int x = 1; x < width - 1; x++) // Pomijanie krawędzi
                {
                    // Pobieranie wartości skali szarości dla centralnego piksela i jego sąsiadów
                    byte center = grayscale[y * width + x];
                    byte top = grayscale[(y - 1) * width + x];
                    byte bottom = grayscale[(y + 1) * width + x];
                    byte left = grayscale[y * width + (x - 1)];
                    byte right = grayscale[y * width + (x + 1)];

                    // Obliczanie średniej wartości
                    byte blurredValue = (byte)((center + top + bottom + left + right) / 5);

                    // Ustawienie rozmytej wartości w tablicy wynikowej (dla wszystkich kanałów RGB)
                    int outputIndex = (y * width + x) * 3;
                    output[outputIndex] = output[outputIndex + 1] = output[outputIndex + 2] = blurredValue;
                }
            }
        }
    }
}
