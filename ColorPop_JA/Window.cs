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

namespace EdgeDetection
{
    public partial class Window : Form
    {
        public Window()
        {
            InitializeComponent();
        }

        private void Window_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            int maxValue = Environment.ProcessorCount;

            while (maxValue >= 1)
            {
                comboBox1.Items.Add(maxValue);
                maxValue /= 2; // Divide by 2 to get the next value
            }

            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0; // Select the first item by default
            }
        }

        [DllImport(@"C:\Users\artur\source\repos\EdgeDetection\x64\Debug\AsmDLL.dll")]
        static extern int EdgeDetect(byte[] redTab, byte[] greenTab, byte[] blueTab, byte[] testTab);

        // Metoda konwertująca kolory na odcienie szarości w C#
        public static void EdgeDetectCS(byte[] tab_red, byte[] tab_green, byte[] tab_blue, ref byte[] tab_result)
        {
            EdgeDetectionCS.EdgeDetectCS(tab_red, tab_green, tab_blue, tab_result);
        }

        // Metoda konwertująca kolory na odcienie szarości w ASM
        public static void EdgeDetectASM(byte[] tab_red, byte[] tab_green, byte[] tab_blue, ref byte[] tab_result)
        {
            EdgeDetect(tab_red, tab_green, tab_blue, tab_result); // Wywołanie funkcji z biblioteki ASM
        }

        // Metoda do tworzenia obrazu w odcieniach szarości
        public Bitmap EdgeDetectorMain(Bitmap original, int max_threads, byte library, ref long time)
        {
            Bitmap new_bitmap = new Bitmap(original.Width, original.Height);

            // Groups of size 8 cause only this many can fit into xmm0 register
            const int groupSize = 8;

            // Obliczenie długości tablic
            int noOfPixelGroups = (original.Width * original.Height + groupSize - 1) / groupSize;

            // Inicjalizacja tablic dla pikseli
            byte[][] tabOfRedPixelGroups = new byte[noOfPixelGroups][];
            byte[][] tabOfGreenPixelGroups = new byte[noOfPixelGroups][];
            byte[][] tabOfBluePixelGroups = new byte[noOfPixelGroups][];

            byte[][] tabOfResultPixelGroups = new byte[noOfPixelGroups][];

            int i = 0; // Indeks do tablicy pikseli
            int ii = 0; // Indeks do tablicy wynikowej
            for (int x = 0; x < original.Width; x++)
            {
                for (int y = 0; y < original.Height; y++)
                {
                    Color originalPixelColor = original.GetPixel(x, y); // Pobranie koloru piksela

                    // Jeśli kolor wymaga zmiany
                    if (i == 0)
                    {
                        // Jeśli jest to nowa grupa, inicjalizuj nowe tablice
                        tabOfRedPixelGroups[ii] = new byte[groupSize];
                        tabOfGreenPixelGroups[ii] = new byte[groupSize];
                        tabOfBluePixelGroups[ii] = new byte[groupSize];

                        tabOfResultPixelGroups[ii] = new byte[groupSize];
                    }

                    // Przypisanie wartości kolorów do tablic
                    tabOfRedPixelGroups[ii][i] = originalPixelColor.R;
                    tabOfGreenPixelGroups[ii][i] = originalPixelColor.G;
                    tabOfBluePixelGroups[ii][i] = originalPixelColor.B;

                    tabOfResultPixelGroups[ii][i] = 0;

                    i++;

                    // Jeśli mamy już 8 pikseli, przejdź do następnej grupy
                    if (i > groupSize - 1)
                    {
                        i = 0;
                        ii++; // Zwiększamy indeks grupy
                    }

                    // Ostatni piksel
                    if (x == original.Width - 1 && y == original.Height - 1)
                    {
                        if (i > 0)
                        {
                            // Upewnij się, że ostatnia grupa jest zapisana, nawet jeśli nie pełna
                            Array.Resize(ref tabOfRedPixelGroups[ii], i);
                            Array.Resize(ref tabOfGreenPixelGroups[ii], i);
                            Array.Resize(ref tabOfBluePixelGroups[ii], i);
                            Array.Resize(ref tabOfResultPixelGroups[ii], i);
                        }
                    }
                }
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (noOfPixelGroups != 0)
            {
                var task1 = Task.Run(() =>
                {
                    if (library == 0)
                    {
                        // Użycie C#
                        Parallel.For(0, noOfPixelGroups, new ParallelOptions { MaxDegreeOfParallelism = max_threads }, //równoległe przetwarzanie iteracji
                        x => { EdgeDetectCS(tabOfRedPixelGroups[x], tabOfGreenPixelGroups[x], tabOfBluePixelGroups[x], ref tabOfResultPixelGroups[x]); });
                    }
                    else if (library == 1)
                    {
                        // Użycie ASM
                        Parallel.For(0, noOfPixelGroups, new ParallelOptions { MaxDegreeOfParallelism = max_threads },
                        x => { EdgeDetectASM(tabOfRedPixelGroups[x], tabOfGreenPixelGroups[x], tabOfBluePixelGroups[x], ref tabOfResultPixelGroups[x]); });
                    }
                });
                Task.WaitAll(task1); // Czekanie na zakończenie wszystkich zadań
            }

            stopwatch.Stop(); // Zatrzymanie pomiaru czasu
            time = (stopwatch.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000)); // Obliczenie czasu w ms

            label5.Text = (stopwatch.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000) + " ms").ToString(); // Wyświetlenie czasu
            int j = 0; // Indeks do tablicy wynikowej
            int k = 0; // Indeks do tablicy szarości

            // Tworzenie nowego obrazu na podstawie przetworzonych pikseli
            for (int x = 0; x < original.Width; x++)
            {
                for (int y = 0; y < original.Height; y++)
                {
                    if (j > groupSize - 1)
                    {
                        j = 0; // Resetowanie indeksu
                        k++; // Przechodzenie do następnej tablicy wynikowej
                    }

                    byte grayScale = (byte)tabOfResultPixelGroups[k][j]; // Pobranie wartości szarości
                    Color nc = Color.FromArgb(grayScale, grayScale, grayScale); // Utworzenie koloru szarości
                    new_bitmap.SetPixel(x, y, nc); // Ustawienie piksela w nowym obrazie
                    j++; // Przechodzenie do następnego piksela
                }
            }

            return new_bitmap; // Zwrócenie nowego obrazu
        }

        private Bitmap MyImage; // Zmienna do przechowywania zaimportowanego obrazu

        // Metoda obsługująca przycisk zapisu
        private void Save_Button_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "bmp files (*.bmp)|*.bmp"; // Filtr plików
            saveFileDialog1.Title = "Save an Image File"; // Tytuł okna zapisu

            if (saveFileDialog1.ShowDialog() == DialogResult.OK) // Jeśli użytkownik wybrał plik
            {
                string ext = System.IO.Path.GetExtension(saveFileDialog1.FileName); // Pobranie rozszerzenia pliku

                ConvertedPictureBox.Image.Save(saveFileDialog1.FileName, ImageFormat.Bmp); // Zapisanie obrazu
            }
        }

        // Metoda obsługująca przycisk importu
        private void Import_Button_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty; // Zmienna do przechowywania zawartości pliku
            var filePath = string.Empty; // Zmienna do przechowywania ścieżki pliku

            this.openFileDialog1.InitialDirectory = "c:\\"; // Ustawienie katalogu początkowego
            this.openFileDialog1.Filter = "bmp files (*.bmp)|*.bmp"; // Filtr plików

            if (openFileDialog1.ShowDialog() == DialogResult.OK) // Jeśli użytkownik wybrał plik
            {
                // Pobranie ścieżki wybranego pliku
                filePath = openFileDialog1.FileName;

                // Odczytanie zawartości pliku do strumienia
                var fileStream = openFileDialog1.OpenFile();

                using (StreamReader reader = new StreamReader(fileStream))
                {
                    fileContent = reader.ReadToEnd(); // Odczytanie zawartości pliku

                    if (MyImage != null)
                    {
                        MyImage.Dispose(); // Zwolnienie zasobów obrazu
                    }

                    // Rozciągnięcie obrazu, aby dopasować go do PictureBox
                    ImportPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

                    MyImage = new Bitmap(filePath); // Utworzenie nowego obiektu Bitmap z wybranego pliku
                    ImportPictureBox.Image = (Image)MyImage; // Ustawienie obrazu w PictureBox
                }
            }
        }

        // Metoda obsługująca przycisk startu konwersji
        private void Start_Button_Click(object sender, EventArgs e)
        {
            if (MyImage == null)
            {
                MessageBox.Show("No image loaded. Please import an image first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            byte library = (byte)(CSharpLibrary.Checked ? 0 : 1); // Wybór biblioteki

            try
            {
                long time = 0; // Zmienna do przechowywania czasu konwersji
                int max_threads = (int)this.comboBox1.SelectedItem; // Pobranie zaznaczonej liczby wątków
                Bitmap ConvertImage = EdgeDetectorMain(MyImage, max_threads, library, ref time); // Konwersja obrazu
                ConvertedPictureBox.SizeMode = PictureBoxSizeMode.StretchImage; // Ustawienie trybu wyświetlania
                ConvertedPictureBox.Image = ConvertImage; // Ustawienie przetworzonego obrazu

                MessageBox.Show($"Konwersja zakończona pomyślnie w czasie: {time} ms", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas konwersji: {ex.Message}", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedValue = (int)comboBox1.SelectedItem;
        }

        private void TestProgramButton_Click(object sender, EventArgs e)
        {
            if (MyImage == null)
            {
                MessageBox.Show("No image loaded. Please import an image first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Create a list to store test results
            var testResults = new List<(int ThreadCount, long TimeForCS, long TimeForASM)>();

            // Get the maximum number of threads (logical processors)
            int maxThreads = Environment.ProcessorCount;

            // Number of test iterations for each thread count
            const int iterations = 5;

            // Initialize the progress bar
            ImgConvProgressBar.Minimum = 0;
            ImgConvProgressBar.Maximum = maxThreads * iterations * 2; // Total test iterations
            ImgConvProgressBar.Value = 0;

            // Test for each thread count
            for (int threadCount = 1; threadCount <= maxThreads; threadCount++)
            {
                long totalTimeCS = 0;
                long totalTimeASM = 0;

                // Test C# implementation
                for (int i = 0; i < iterations; i++)
                {
                    long timeCS = 0;
                    EdgeDetectorMain(MyImage, threadCount, 0, ref timeCS); // 0 = C#
                    totalTimeCS += timeCS;

                    System.Threading.Thread.Sleep(50);

                    ImgConvProgressBar.Value++;
                    Application.DoEvents();


                }

                // Test Assembly implementation
                for (int i = 0; i < iterations; i++)
                {
                    long timeASM = 0;
                    EdgeDetectorMain(MyImage, threadCount, 1, ref timeASM); // 1 = ASM
                    totalTimeASM += timeASM;

                    System.Threading.Thread.Sleep(50);

                    ImgConvProgressBar.Value++;
                    Application.DoEvents();
                }

                // Calculate average time
                long avgTimeCS = totalTimeCS / iterations;
                long avgTimeASM = totalTimeASM / iterations;

                // Store the result
                testResults.Add((threadCount, avgTimeCS, avgTimeASM));
            }

            // Write results to an Excel file
            WriteResultsToExcel(testResults);

            MessageBox.Show("Testing completed. Results saved to Excel.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void WriteResultsToExcel(List<(int ThreadCount, long TimeForCS, long TimeForASM)> results)
        {
            // Create a new Excel application
            var excelApp = new Microsoft.Office.Interop.Excel.Application();
            excelApp.Visible = false;
            var workbook = excelApp.Workbooks.Add();
            var worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets[1];

            // Add headers
            worksheet.Cells[1, 1] = "Thread Count";
            worksheet.Cells[1, 2] = "Time for C# (ms)";
            worksheet.Cells[1, 3] = "Time for Assembly (ms)";

            // Add data
            for (int i = 0; i < results.Count; i++)
            {
                worksheet.Cells[i + 2, 1] = results[i].ThreadCount;
                worksheet.Cells[i + 2, 2] = results[i].TimeForCS;
                worksheet.Cells[i + 2, 3] = results[i].TimeForASM;
            }

            // Auto-fit columns for better readability
            worksheet.Columns.AutoFit();

            // Save the file
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "EdgeDetectionTestResults.xlsx");
            workbook.SaveAs(filePath);

            // Clean up
            workbook.Close();
            excelApp.Quit();
            Marshal.ReleaseComObject(worksheet);
            Marshal.ReleaseComObject(workbook);
            Marshal.ReleaseComObject(excelApp);
        }
    }
}