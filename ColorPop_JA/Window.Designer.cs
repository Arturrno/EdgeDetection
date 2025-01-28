namespace EdgeDetection
{
    partial class Window
    {
        /// <summary>
        /// Wymagana zmienna projektanta.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Wyczyść wszystkie używane zasoby.
        /// </summary>
        /// <param name="disposing">prawda, jeżeli zarządzane zasoby powinny zostać zlikwidowane; Fałsz w przeciwnym wypadku.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kod generowany przez Projektanta formularzy systemu Windows

        /// <summary>
        /// Metoda wymagana do obsługi projektanta — nie należy modyfikować
        /// jej zawartości w edytorze kodu.
        /// </summary>
        private void InitializeComponent()
        {
            this.ConvertedPictureBox = new System.Windows.Forms.PictureBox();
            this.ASMLibrary = new System.Windows.Forms.RadioButton();
            this.CSharpLibrary = new System.Windows.Forms.RadioButton();
            this.Save_Button = new System.Windows.Forms.Button();
            this.ImportButton = new System.Windows.Forms.Button();
            this.StartButton = new System.Windows.Forms.Button();
            this.ImportPictureBox = new System.Windows.Forms.PictureBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.TestProgramButton = new System.Windows.Forms.Button();
            this.ImgConvProgressBar = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.ConvertedPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImportPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // ConvertedPictureBox
            // 
            this.ConvertedPictureBox.Location = new System.Drawing.Point(457, 222);
            this.ConvertedPictureBox.Name = "ConvertedPictureBox";
            this.ConvertedPictureBox.Size = new System.Drawing.Size(352, 244);
            this.ConvertedPictureBox.TabIndex = 8;
            this.ConvertedPictureBox.TabStop = false;
            // 
            // ASMLibrary
            // 
            this.ASMLibrary.AutoSize = true;
            this.ASMLibrary.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ASMLibrary.Location = new System.Drawing.Point(271, 52);
            this.ASMLibrary.Name = "ASMLibrary";
            this.ASMLibrary.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ASMLibrary.Size = new System.Drawing.Size(65, 24);
            this.ASMLibrary.TabIndex = 9;
            this.ASMLibrary.TabStop = true;
            this.ASMLibrary.Text = "ASM";
            this.ASMLibrary.UseVisualStyleBackColor = true;
            // 
            // CSharpLibrary
            // 
            this.CSharpLibrary.AutoSize = true;
            this.CSharpLibrary.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CSharpLibrary.Location = new System.Drawing.Point(271, 105);
            this.CSharpLibrary.Name = "CSharpLibrary";
            this.CSharpLibrary.Size = new System.Drawing.Size(49, 24);
            this.CSharpLibrary.TabIndex = 10;
            this.CSharpLibrary.TabStop = true;
            this.CSharpLibrary.Text = "C#";
            this.CSharpLibrary.UseVisualStyleBackColor = true;
            // 
            // Save_Button
            // 
            this.Save_Button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Save_Button.Location = new System.Drawing.Point(457, 47);
            this.Save_Button.Name = "Save_Button";
            this.Save_Button.Size = new System.Drawing.Size(183, 35);
            this.Save_Button.TabIndex = 12;
            this.Save_Button.Text = "Save";
            this.Save_Button.UseVisualStyleBackColor = true;
            this.Save_Button.Click += new System.EventHandler(this.Save_Button_Click);
            // 
            // ImportButton
            // 
            this.ImportButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ImportButton.Location = new System.Drawing.Point(46, 47);
            this.ImportButton.Name = "ImportButton";
            this.ImportButton.Size = new System.Drawing.Size(183, 35);
            this.ImportButton.TabIndex = 13;
            this.ImportButton.Text = "Import";
            this.ImportButton.UseVisualStyleBackColor = true;
            this.ImportButton.Click += new System.EventHandler(this.Import_Button_Click);
            // 
            // StartButton
            // 
            this.StartButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StartButton.Location = new System.Drawing.Point(46, 100);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(183, 35);
            this.StartButton.TabIndex = 14;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.Start_Button_Click);
            // 
            // ImportPictureBox
            // 
            this.ImportPictureBox.Location = new System.Drawing.Point(46, 222);
            this.ImportPictureBox.Name = "ImportPictureBox";
            this.ImportPictureBox.Size = new System.Drawing.Size(352, 244);
            this.ImportPictureBox.TabIndex = 29;
            this.ImportPictureBox.TabStop = false;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(731, 54);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 20);
            this.label5.TabIndex = 27;
            this.label5.Text = "0 ms";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(673, 54);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(52, 20);
            this.label6.TabIndex = 28;
            this.label6.Text = "Time:";
            // 
            // comboBox1
            // 
            this.comboBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(271, 160);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(80, 28);
            this.comboBox1.TabIndex = 30;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // TestProgramButton
            // 
            this.TestProgramButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TestProgramButton.Location = new System.Drawing.Point(46, 157);
            this.TestProgramButton.Name = "TestProgramButton";
            this.TestProgramButton.Size = new System.Drawing.Size(183, 35);
            this.TestProgramButton.TabIndex = 31;
            this.TestProgramButton.Text = "Test program";
            this.TestProgramButton.UseVisualStyleBackColor = true;
            this.TestProgramButton.Click += new System.EventHandler(this.TestProgramButton_Click);
            // 
            // ImgConvProgressBar
            // 
            this.ImgConvProgressBar.Location = new System.Drawing.Point(457, 160);
            this.ImgConvProgressBar.Name = "ImgConvProgressBar";
            this.ImgConvProgressBar.Size = new System.Drawing.Size(352, 35);
            this.ImgConvProgressBar.TabIndex = 32;
            // 
            // Window
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Menu;
            this.ClientSize = new System.Drawing.Size(859, 519);
            this.Controls.Add(this.ImgConvProgressBar);
            this.Controls.Add(this.TestProgramButton);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.ImportPictureBox);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.ImportButton);
            this.Controls.Add(this.Save_Button);
            this.Controls.Add(this.CSharpLibrary);
            this.Controls.Add(this.ASMLibrary);
            this.Controls.Add(this.ConvertedPictureBox);
            this.Name = "Window";
            this.Text = "ColorPop";
            this.Load += new System.EventHandler(this.Window_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ConvertedPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImportPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.PictureBox ConvertedPictureBox;
        private System.Windows.Forms.RadioButton ASMLibrary;
        private System.Windows.Forms.RadioButton CSharpLibrary;
        private System.Windows.Forms.Button Save_Button;
        private System.Windows.Forms.Button ImportButton;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.PictureBox ImportPictureBox;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button TestProgramButton;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ProgressBar ImgConvProgressBar;
    }
}