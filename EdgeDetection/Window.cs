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
        [DllImport(@"C:\Users\artur\source\repos\EdgeDetection\x64\Debug\AsmDLL.dll")]
        static extern int EdgeDetectRGB(byte[] redTab, byte[] greenTab, byte[] blueTab, byte[] testTab);

        [DllImport(@"C:\Users\artur\source\repos\EdgeDetection\x64\Debug\AsmDLL.dll")]
        static extern void EdgeDetect(byte[] input, byte[] output, int width, int height);

        private Bitmap MyBitmap;

        public Window()
        {
            InitializeComponent();
        }

        private void Window_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            //int maxThreads = Environment.ProcessorCount;
            int maxThreads = 64;

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

        // C# RGB
        public static void EdgeDetectRGB_CS(byte[] redTab, byte[] greenTab, byte[] blueTab, ref byte[] resultTab)
        {
            EdgeDetectionCS.EdgeDetectRGB_CS(redTab, greenTab, blueTab, resultTab);
        }

        // ASM RGB
        public static void EdgeDetectRGB_ASM(byte[] redTab, byte[] greenTab, byte[] blueTab, ref byte[] resultTab)
        {
            EdgeDetectRGB(redTab, greenTab, blueTab, resultTab);
        }

        // C#
        public static void EdgeDetect_CS(byte[] input, byte[] output, int width, int height)
        {
            EdgeDetectionCS.EdgeDetectCS(input, output, width, height);
        }

        // ASM
        public static void EdgeDetect_ASM(byte[] input, byte[] output, int width, int height)
        {
            EdgeDetect(input, output, width, height);
        }

        public Bitmap EdgeDetectorMain(Bitmap bitmap, int maxThreads, byte chosenDllLibrary, ref long time)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            int bytesPerPixel = 3; // Assuming 24bpp (RGB format)
            int stride = width * bytesPerPixel;
            int chunkHeight = height / maxThreads; // Height of each chunk

            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb
            );

            byte[] inputImage = new byte[height * stride];
            byte[] outputImage = new byte[height * stride];

            // Copy the bitmap data to the inputImage array
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

                    // Copy the chunk from the inputImage to inputChunk
                    Buffer.BlockCopy(inputImage, startRow * stride, inputChunk, 0, chunkSize);

                    if (chosenDllLibrary == 0)
                        EdgeDetect_CS(inputChunk, outputChunk, width, endRow - startRow);
                    else if (chosenDllLibrary == 1)
                        EdgeDetect_ASM(inputChunk, outputChunk, width, endRow - startRow);

                    // Copy the processed chunk back to the outputImage
                    Buffer.BlockCopy(outputChunk, 0, outputImage, startRow * stride, chunkSize);
                });

                // Copy the outputImage back to the bitmap
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


        private void Start_Button_Click(object sender, EventArgs e)
        {
            if (MyBitmap == null)
            {
                MessageBox.Show("No image loaded. Please import an image first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            byte library = (byte)(CSharpLibrary.Checked ? 0 : 1); // Select library
            int maxThreads = comboBox1.SelectedItem is int threads ? threads : Environment.ProcessorCount; // Ensure valid thread count

            try
            {
                // Create a deep copy of the bitmap to avoid corrupting the original
                Bitmap tempBitmap = (Bitmap)MyBitmap.Clone();

                long processingTime = 0;
                // Bitmap processedImage = EdgeDetectorRGBMain(tempBitmap, maxThreads, library, ref processingTime);
                Bitmap processedImage = EdgeDetectorMain(tempBitmap, maxThreads, library, ref processingTime);

                // No need to restore MyBitmap since it was never modified
                label5.Text = processingTime + " µs";
                ConvertedPictureBox.Image = processedImage;
                ConvertedPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during conversion: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Import_Button_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = @"C:\";
            openFileDialog1.Filter = "Bitmap Files (*.bmp)|*.bmp";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var fileStream = openFileDialog1.OpenFile();

                    MyBitmap?.Dispose(); // Dispose previous image if exists

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

        private void TestProgramButton_Click(object sender, EventArgs e)
        {
            if (MyBitmap == null)
            {
                MessageBox.Show("No image loaded. Please import an image first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var testResults = new List<(int ThreadCount, long TimeForCS, long TimeForASM)>();

            //int maxThreads = Environment.ProcessorCount;
            int maxThreads = 64;

            const int iterations = 5;

            ImgConvProgressBar.Minimum = 0;
            ImgConvProgressBar.Maximum = maxThreads * iterations * 2; // Total test iterations
            ImgConvProgressBar.Value = 0;

            // Test for each thread count
            for (int threadCount = 1; threadCount <= maxThreads; threadCount++)
            {
                long totalTimeCS = 0;
                long totalTimeASM = 0;

                // Test C# implementation
                for (int i = 1; i < iterations + 1; i++)
                {
                    long timeCS = 0;
                    //EdgeDetectorRGBMain(MyBitmap, threadCount, 0, ref timeCS); // 0 = C#
                    EdgeDetectorMain(MyBitmap, threadCount, 0, ref timeCS); // 0 = C#
                    totalTimeCS += timeCS;

                    System.Threading.Thread.Sleep(50);

                    ImgConvProgressBar.Value++;
                    Application.DoEvents();


                }

                // Test Assembly implementation
                for (int i = 1; i < iterations + 1; i++)
                {
                    long timeASM = 0;
                    //EdgeDetectorRGBMain(MyBitmap, threadCount, 1, ref timeASM); // 1 = ASM
                    EdgeDetectorMain(MyBitmap, threadCount, 1, ref timeASM); // 1 = ASM
                    totalTimeASM += timeASM;

                    System.Threading.Thread.Sleep(50);

                    ImgConvProgressBar.Value++;
                    Application.DoEvents();
                }

                // Calculate average time
                long avgTimeCS = totalTimeCS / iterations;
                long avgTimeASM = totalTimeASM / iterations;

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

            worksheet.Cells[1, 1] = "Thread Count";
            worksheet.Cells[1, 2] = "Time for C# (µs)";
            worksheet.Cells[1, 3] = "Time for Assembly (µs)";

            for (int i = 0; i < results.Count; i++)
            {
                worksheet.Cells[i + 2, 1] = results[i].ThreadCount;
                worksheet.Cells[i + 2, 2] = results[i].TimeForCS;
                worksheet.Cells[i + 2, 3] = results[i].TimeForASM;
            }

            worksheet.Columns.AutoFit();

            // Get the current date and format it as part of the filename
            string currentDate = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"EdgeDetectTestResults_{currentDate}.xlsx";

            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
            workbook.SaveAs(filePath);

            workbook.Close();
            excelApp.Quit();
            Marshal.ReleaseComObject(worksheet);
            Marshal.ReleaseComObject(workbook);
            Marshal.ReleaseComObject(excelApp);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedValue = (int)comboBox1.SelectedItem;
        }
    }
}