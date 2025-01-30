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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Image = System.Drawing.Image;
using System.Diagnostics;
using System.Collections.Generic;
using CsDLL;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace EdgeDetection
{
    public partial class Window : Form
    {
        // Importowanie funkcji z biblioteki AsmDLL.dll
        [DllImport(@"C:\Users\artur\source\repos\EdgeDetection\x64\Debug\AsmDLL.dll")]
        static extern int EdgeDetectRGB(byte[] redTab, byte[] greenTab, byte[] blueTab, byte[] testTab);

        [DllImport(@"C:\Users\artur\source\repos\EdgeDetection\x64\Debug\AsmDLL.dll")]
        static extern void EdgeDetect(byte[] input, byte[] output, int width, int height);

        private Bitmap MyBitmap; // Przechowuje załadowany obraz

        public Window()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Metoda wywoływana podczas ładowania okna. Inicjalizuje ComboBox z dostępnymi liczbami wątków.
        /// </summary>
        /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
        /// <param name="e">Argumenty zdarzenia.</param>
        private void Window_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            int maxThreads = 64; // Maksymalna liczba wątków

            int threadCount = 1;
            while (threadCount <= maxThreads)
            {
                comboBox1.Items.Add(threadCount);
                threadCount *= 2;
            }

            if (maxThreads > 0 && (maxThreads & (maxThreads - 1)) != 0)
            {
                comboBox1.Items.Add(maxThreads);
            }

            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Metoda wykrywająca krawędzie w obrazie przy użyciu implementacji C#.
        /// </summary>
        /// <param name="redTab">Tablica wartości składowej czerwonej pikseli.</param>
        /// <param name="greenTab">Tablica wartości składowej zielonej pikseli.</param>
        /// <param name="blueTab">Tablica wartości składowej niebieskiej pikseli.</param>
        /// <param name="resultTab">Tablica wynikowa przechowująca przetworzone wartości pikseli.</param>
        public static void EdgeDetectRGB_CS(byte[] redTab, byte[] greenTab, byte[] blueTab, ref byte[] resultTab)
        {
            EdgeDetectionCS.EdgeDetectRGB_CS(redTab, greenTab, blueTab, resultTab);
        }

        /// <summary>
        /// Metoda wykrywająca krawędzie w obrazie przy użyciu implementacji ASM.
        /// </summary>
        /// <param name="redTab">Tablica wartości składowej czerwonej pikseli.</param>
        /// <param name="greenTab">Tablica wartości składowej zielonej pikseli.</param>
        /// <param name="blueTab">Tablica wartości składowej niebieskiej pikseli.</param>
        /// <param name="resultTab">Tablica wynikowa przechowująca przetworzone wartości pikseli.</param>
        public static void EdgeDetectRGB_ASM(byte[] redTab, byte[] greenTab, byte[] blueTab, ref byte[] resultTab)
        {
            EdgeDetectRGB(redTab, greenTab, blueTab, resultTab);
        }

        /// <summary>
        /// Metoda wykrywająca krawędzie w obrazie przy użyciu implementacji C#.
        /// </summary>
        /// <param name="input">Tablica wejściowa zawierająca dane obrazu.</param>
        /// <param name="output">Tablica wynikowa przechowująca przetworzone dane obrazu.</param>
        /// <param name="width">Szerokość obrazu.</param>
        /// <param name="height">Wysokość obrazu.</param>
        public static void EdgeDetect_CS(byte[] input, byte[] output, int width, int height)
        {
            EdgeDetectionCS.EdgeDetectCS(input, output, width, height);
        }

        /// <summary>
        /// Metoda wykrywająca krawędzie w obrazie przy użyciu implementacji ASM.
        /// </summary>
        /// <param name="input">Tablica wejściowa zawierająca dane obrazu.</param>
        /// <param name="output">Tablica wynikowa przechowująca przetworzone dane obrazu.</param>
        /// <param name="width">Szerokość obrazu.</param>
        /// <param name="height">Wysokość obrazu.</param>
        public static void EdgeDetect_ASM(byte[] input, byte[] output, int width, int height)
        {
            EdgeDetect(input, output, width, height);
        }

        /// <summary>
        /// Główna metoda wykrywająca krawędzie w obrazie. Dzieli obraz na fragmenty i przetwarza je równolegle.
        /// </summary>
        /// <param name="bitmap">Obraz wejściowy.</param>
        /// <param name="maxThreads">Maksymalna liczba wątków do przetwarzania.</param>
        /// <param name="chosenDllLibrary">Wybrana biblioteka (0 - C#, 1 - ASM).</param>
        /// <param name="time">Czas przetwarzania w mikrosekundach.</param>
        /// <returns>Przetworzony obraz.</returns>
        public Bitmap EdgeDetectorMain(Bitmap bitmap, int maxThreads, byte chosenDllLibrary, ref long time)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            int bytesPerPixel = 3; // Format 24bpp (RGB)
            int stride = width * bytesPerPixel;
            int chunkHeight = height / maxThreads; // Wysokość każdego fragmentu

            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb
            );

            byte[] inputImage = new byte[height * stride];
            byte[] outputImage = new byte[height * stride];

            // Kopiowanie danych obrazu do tablicy inputImage
            Marshal.Copy(bitmapData.Scan0, inputImage, 0, inputImage.Length);

            List<Task> tasks = new List<Task>();
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                Parallel.For(0, maxThreads, threadIdx =>
                {
                    int startRow = threadIdx * chunkHeight;
                    int endRow = (threadIdx == maxThreads - 1) ? height : startRow + chunkHeight;
                    int chunkSize = (endRow - startRow) * stride;

                    byte[] inputChunk = new byte[chunkSize];
                    byte[] outputChunk = new byte[chunkSize];

                    // Kopiowanie fragmentu z inputImage do inputChunk
                    Buffer.BlockCopy(inputImage, startRow * stride, inputChunk, 0, chunkSize);

                    if (chosenDllLibrary == 0)
                        EdgeDetect_CS(inputChunk, outputChunk, width, endRow - startRow);
                    else if (chosenDllLibrary == 1)
                        EdgeDetect_ASM(inputChunk, outputChunk, width, endRow - startRow);

                    // Kopiowanie przetworzonego fragmentu do outputImage
                    Buffer.BlockCopy(outputChunk, 0, outputImage, startRow * stride, chunkSize);
                });

                // Kopiowanie outputImage z powrotem do bitmapy
                Marshal.Copy(outputImage, 0, bitmapData.Scan0, outputImage.Length);
            }
            finally
            {
                stopwatch.Stop();
                time = (stopwatch.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000));

                bitmap.UnlockBits(bitmapData);
            }
            return bitmap;
        }

        /// <summary>
        /// Metoda wykrywająca krawędzie w obrazie RGB. Dzieli obraz na grupy pikseli i przetwarza je równolegle.
        /// </summary>
        /// <param name="bitmap">Obraz wejściowy.</param>
        /// <param name="maxThreads">Maksymalna liczba wątków do przetwarzania.</param>
        /// <param name="chosenDllLibrary">Wybrana biblioteka (0 - C#, 1 - ASM).</param>
        /// <param name="time">Czas przetwarzania w mikrosekundach.</param>
        /// <returns>Przetworzony obraz.</returns>
        public Bitmap EdgeDetectorRGBMain(Bitmap bitmap, int maxThreads, byte chosenDllLibrary, ref long time)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            const int groupSize = 8;
            int noOfPixelGroups = (width * height + groupSize - 1) / groupSize;

            byte[][] tabOfRedPixelGroups = new byte[noOfPixelGroups][];
            byte[][] tabOfGreenPixelGroups = new byte[noOfPixelGroups][];
            byte[][] tabOfBluePixelGroups = new byte[noOfPixelGroups][];
            byte[][] tabOfResultPixelGroups = new byte[noOfPixelGroups][];

            for (int i = 0, x = 0, y = 0; i < noOfPixelGroups; i++)
            {
                tabOfRedPixelGroups[i] = new byte[groupSize];
                tabOfGreenPixelGroups[i] = new byte[groupSize];
                tabOfBluePixelGroups[i] = new byte[groupSize];
                tabOfResultPixelGroups[i] = new byte[groupSize];

                for (int j = 0; j < groupSize && x < width && y < height; j++)
                {
                    Color originalPixelColor = bitmap.GetPixel(x, y);
                    tabOfRedPixelGroups[i][j] = originalPixelColor.R;
                    tabOfGreenPixelGroups[i][j] = originalPixelColor.G;
                    tabOfBluePixelGroups[i][j] = originalPixelColor.B;
                    y++;
                    if (y >= height) { y = 0; x++; }
                }
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            Parallel.For(0, noOfPixelGroups, new ParallelOptions { MaxDegreeOfParallelism = maxThreads }, i =>
            {
                if (chosenDllLibrary == 0)
                    EdgeDetectRGB_CS(tabOfRedPixelGroups[i], tabOfGreenPixelGroups[i], tabOfBluePixelGroups[i], ref tabOfResultPixelGroups[i]);
                else
                    EdgeDetectRGB_ASM(tabOfRedPixelGroups[i], tabOfGreenPixelGroups[i], tabOfBluePixelGroups[i], ref tabOfResultPixelGroups[i]);
            });

            stopwatch.Stop();
            time = (stopwatch.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000));

            Bitmap resultBitmap = new Bitmap(width, height);
            for (int i = 0, x = 0, y = 0; i < noOfPixelGroups; i++)
            {
                for (int j = 0; j < tabOfResultPixelGroups[i].Length && x < width && y < height; j++)
                {
                    byte grayScale = tabOfResultPixelGroups[i][j];
                    resultBitmap.SetPixel(x, y, Color.FromArgb(grayScale, grayScale, grayScale));
                    y++;
                    if (y >= height) { y = 0; x++; }
                }
            }

            return resultBitmap;
        }

        /// <summary>
        /// Metoda wywoływana po kliknięciu przycisku Start. Rozpoczyna przetwarzanie obrazu.
        /// </summary>
        /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
        /// <param name="e">Argumenty zdarzenia.</param>
        private void Start_Button_Click(object sender, EventArgs e)
        {
            if (MyBitmap == null)
            {
                MessageBox.Show("No image loaded. Please import an image first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            byte library = (byte)(CSharpLibrary.Checked ? 0 : 1); // Wybór biblioteki
            int maxThreads = comboBox1.SelectedItem is int threads ? threads : Environment.ProcessorCount; // Liczba wątków

            try
            {
                Bitmap tempBitmap = (Bitmap)MyBitmap.Clone(); // Kopia obrazu

                long processingTime = 0;
                Bitmap processedImage = EdgeDetectorMain(tempBitmap, maxThreads, library, ref processingTime);

                label5.Text = processingTime + " µs";
                ConvertedPictureBox.Image = processedImage;
                ConvertedPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during conversion: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Metoda wywoływana po kliknięciu przycisku Import. Ładuje obraz z pliku.
        /// </summary>
        /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
        /// <param name="e">Argumenty zdarzenia.</param>
        private void Import_Button_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = @"C:\";
            openFileDialog1.Filter = "Bitmap Files (*.bmp)|*.bmp";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var fileStream = openFileDialog1.OpenFile();

                    MyBitmap?.Dispose(); // Zwolnienie poprzedniego obrazu

                    MyBitmap = new Bitmap(fileStream);
                    ImportPictureBox.Image = MyBitmap;
                    ImportPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Metoda wywoływana po kliknięciu przycisku Save. Zapisuje przetworzony obraz do pliku.
        /// </summary>
        /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
        /// <param name="e">Argumenty zdarzenia.</param>
        private void Save_Button_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Bitmap Files (*.bmp)|*.bmp",
                Title = "Save Image"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK && ConvertedPictureBox.Image != null)
            {
                try
                {
                    ConvertedPictureBox.Image.Save(saveDialog.FileName, ImageFormat.Bmp);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Metoda wywoływana po kliknięciu przycisku Test. Przeprowadza testy wydajnościowe dla różnych liczby wątków.
        /// </summary>
        /// <param name="sender">Obiekt wywołujący zdarzenie.</param>
        /// <param name="e">Argumenty zdarzenia.</param>
        private void TestProgramButton_Click(object sender, EventArgs e)
        {
            if (MyBitmap == null)
            {
                MessageBox.Show("No image loaded. Please import an image first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var testResults = new List<(int ThreadCount, long TimeForCS, long TimeForASM)>();

            int maxThreads = 64;
            const int iterations = 5;

            ImgConvProgressBar.Minimum = 0;
            ImgConvProgressBar.Maximum = maxThreads * iterations * 2; // Całkowita liczba iteracji
            ImgConvProgressBar.Value = 0;

            for (int threadCount = 1; threadCount <= maxThreads; threadCount++)
            {
                long totalTimeCS = 0;
                long totalTimeASM = 0;

                // Testowanie implementacji C#
                for (int i = 1; i < iterations + 1; i++)
                {
                    long timeCS = 0;
                    EdgeDetectorMain(MyBitmap, threadCount, 0, ref timeCS); // 0 - C#
                    totalTimeCS += timeCS;

                    System.Threading.Thread.Sleep(50);

                    ImgConvProgressBar.Value++;
                    Application.DoEvents();
                }

                // Testowanie implementacji ASM
                for (int i = 1; i < iterations + 1; i++)
                {
                    long timeASM = 0;
                    EdgeDetectorMain(MyBitmap, threadCount, 1, ref timeASM); // 1 - ASM
                    totalTimeASM += timeASM;

                    System.Threading.Thread.Sleep(50);

                    ImgConvProgressBar.Value++;
                    Application.DoEvents();
                }

                long avgTimeCS = totalTimeCS / iterations;
                long avgTimeASM = totalTimeASM / iterations;

                testResults.Add((threadCount, avgTimeCS, avgTimeASM));
            }

            WriteResultsToExcel(testResults); // Zapis wyników do Excela

            MessageBox.Show("Testing completed. Results saved to Excel.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Metoda zapisująca wyniki testów wydajnościowych do pliku Excel.
        /// </summary>
        /// <param name="results">Lista wyników testów zawierająca liczbę wątków, czas wykonania dla C# i czas wykonania dla ASM.</param>
        private void WriteResultsToExcel(List<(int ThreadCount, long TimeForCS, long TimeForASM)> results)
        {
            // Tworzenie nowej aplikacji Excel
            var excelApp = new Microsoft.Office.Interop.Excel.Application();
            excelApp.Visible = false; // Ukrycie aplikacji Excel
            var workbook = excelApp.Workbooks.Add(); // Dodanie nowego skoroszytu
            var worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets[1]; // Pobranie pierwszego arkusza

            // Nagłówki kolumn
            worksheet.Cells[1, 1] = "Thread Count"; // Liczba wątków
            worksheet.Cells[1, 2] = "Time for C# (µs)"; // Czas wykonania dla C# w mikrosekundach
            worksheet.Cells[1, 3] = "Time for Assembly (µs)"; // Czas wykonania dla ASM w mikrosekundach

            // Wypełnianie arkusza danymi
            for (int i = 0; i < results.Count; i++)
            {
                worksheet.Cells[i + 2, 1] = results[i].ThreadCount; // Liczba wątków
                worksheet.Cells[i + 2, 2] = results[i].TimeForCS; // Czas dla C#
                worksheet.Cells[i + 2, 3] = results[i].TimeForASM; // Czas dla ASM
            }

            worksheet.Columns.AutoFit(); // Dostosowanie szerokości kolumn do zawartości

            // Pobranie bieżącej daty i sformatowanie jej jako części nazwy pliku
            string currentDate = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"EdgeDetectTestResults_{currentDate}.xlsx"; // Nazwa pliku

            // Ścieżka do zapisu pliku na pulpicie
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
            workbook.SaveAs(filePath); // Zapisanie skoroszytu

            workbook.Close(); // Zamknięcie skoroszytu
            excelApp.Quit(); // Zamknięcie aplikacji Excel

            // Zwolnienie zasobów COM
            Marshal.ReleaseComObject(worksheet);
            Marshal.ReleaseComObject(workbook);
            Marshal.ReleaseComObject(excelApp);
        }
    }
}