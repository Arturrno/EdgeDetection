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
        [DllImport(@"C:\Users\artur\source\repos\EdgeDetection\x64\Debug\AsmDLL.dll")]
        static extern int EdgeDetect(byte[] redTab, byte[] greenTab, byte[] blueTab, byte[] testTab);

        private Bitmap MyBitmap;

        public Window()
        {
            InitializeComponent();
        }

        private void Window_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            int maxThreads = Environment.ProcessorCount;

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

        // C#
        public static void EdgeDetectCS(byte[] redTab, byte[] greenTab, byte[] blueTab, ref byte[] resultTab)
        {
            EdgeDetectionCS.EdgeDetectCS(redTab, greenTab, blueTab, resultTab);
        }

        // ASM
        public static void EdgeDetectASM(byte[] redTab, byte[] greenTab, byte[] blueTab, ref byte[] resultTab)
        {
            EdgeDetect(redTab, greenTab, blueTab, resultTab);
        }

        public Bitmap EdgeDetectorMain(Bitmap bitmap, int maxThreads, byte chosenDllLibrary, ref long time)
        {
            Bitmap resultBitmap = new Bitmap(bitmap.Width, bitmap.Height);

            // Groups of size 8 cause only this many can fit into xmm0 register
            const int groupSize = 8;

            int noOfPixelGroups = (bitmap.Width * bitmap.Height + groupSize - 1) / groupSize;

            byte[][] tabOfRedPixelGroups = new byte[noOfPixelGroups][];
            byte[][] tabOfGreenPixelGroups = new byte[noOfPixelGroups][];
            byte[][] tabOfBluePixelGroups = new byte[noOfPixelGroups][];

            byte[][] tabOfResultPixelGroups = new byte[noOfPixelGroups][];

            int currPixelIdx = 0; // index for parsing SINGLE 8 pixel group
            int currGroupIdx = 0; //index for parsing through the multiple pixel groups

            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color originalPixelColor = bitmap.GetPixel(x, y);

                    if (currPixelIdx == 0)
                    {
                        // new tables for every group
                        tabOfRedPixelGroups[currGroupIdx] = new byte[groupSize];
                        tabOfGreenPixelGroups[currGroupIdx] = new byte[groupSize];
                        tabOfBluePixelGroups[currGroupIdx] = new byte[groupSize];

                        tabOfResultPixelGroups[currGroupIdx] = new byte[groupSize];
                    }

                    // Colours from picture to the pixel colour groupss
                    tabOfRedPixelGroups[currGroupIdx][currPixelIdx] = originalPixelColor.R;
                    tabOfGreenPixelGroups[currGroupIdx][currPixelIdx] = originalPixelColor.G;
                    tabOfBluePixelGroups[currGroupIdx][currPixelIdx] = originalPixelColor.B;

                    tabOfResultPixelGroups[currGroupIdx][currPixelIdx] = 0;

                    currPixelIdx++;

                    if (currPixelIdx > groupSize - 1)
                    {
                        currPixelIdx = 0;
                        currGroupIdx++; // next group
                    }

                    // Last pixel
                    if (x == bitmap.Width - 1 && y == bitmap.Height - 1)
                    {
                        if (currPixelIdx > 0)
                        {
                            Array.Resize(ref tabOfRedPixelGroups[currGroupIdx], currPixelIdx);
                            Array.Resize(ref tabOfGreenPixelGroups[currGroupIdx], currPixelIdx);
                            Array.Resize(ref tabOfBluePixelGroups[currGroupIdx], currPixelIdx);
                            Array.Resize(ref tabOfResultPixelGroups[currGroupIdx], currPixelIdx);
                        }
                    }
                }
            }

            var stopwatch = Stopwatch.StartNew();

            if (noOfPixelGroups != 0)
            {
                var task = Task.Run(() =>
                {
                    if (chosenDllLibrary == 0)
                    {
                        // Użycie C#
                        Parallel.For(0, noOfPixelGroups, new ParallelOptions { MaxDegreeOfParallelism = maxThreads }, //równoległe przetwarzanie iteracji
                        x => { EdgeDetectCS(tabOfRedPixelGroups[x], tabOfGreenPixelGroups[x], tabOfBluePixelGroups[x], ref tabOfResultPixelGroups[x]); });
                    }
                    else if (chosenDllLibrary == 1)
                    {
                        // Użycie ASM
                        Parallel.For(0, noOfPixelGroups, new ParallelOptions { MaxDegreeOfParallelism = maxThreads },
                        x => { EdgeDetectASM(tabOfRedPixelGroups[x], tabOfGreenPixelGroups[x], tabOfBluePixelGroups[x], ref tabOfResultPixelGroups[x]); });
                    }
                });
                Task.WaitAll(task);
            }

            stopwatch.Stop(); // Zatrzymanie pomiaru czasu
            time = (stopwatch.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000));

            label5.Text = time + " µs";

            currPixelIdx = 0; // index for parsing SINGLE 8 pixel group
            currGroupIdx = 0; //index for parsing through the multiple pixel groups

            // Making new image based on the pixel groups from beforei
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    if (currPixelIdx > groupSize - 1)
                    {
                        currPixelIdx = 0;
                        currGroupIdx++;
                    }

                    byte grayScale = (byte)tabOfResultPixelGroups[currGroupIdx][currPixelIdx];
                    Color newPixelColor = Color.FromArgb(grayScale, grayScale, grayScale);
                    resultBitmap.SetPixel(x, y, newPixelColor);
                    currPixelIdx++;
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
                long processingTime = 0;
                Bitmap processedImage = EdgeDetectorMain(MyBitmap, maxThreads, library, ref processingTime);

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

            int maxThreads = Environment.ProcessorCount;

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
                for (int i = 0; i < iterations; i++)
                {
                    long timeCS = 0;
                    EdgeDetectorMain(MyBitmap, threadCount, 0, ref timeCS); // 0 = C#
                    totalTimeCS += timeCS;

                    //System.Threading.Thread.Sleep(50);

                    ImgConvProgressBar.Value++;
                    Application.DoEvents();


                }

                // Test Assembly implementation
                for (int i = 0; i < iterations; i++)
                {
                    long timeASM = 0;
                    EdgeDetectorMain(MyBitmap, threadCount, 1, ref timeASM); // 1 = ASM
                    totalTimeASM += timeASM;

                    //System.Threading.Thread.Sleep(50);

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

            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "EdgeDetectionTestResults.xlsx");
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