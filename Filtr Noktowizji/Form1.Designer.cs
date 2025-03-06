// Projekt: JaProjIP
// Semestr/Rok akademicki: 2024/2025
// Autor: Visual Studio
// Wersja: 1.0
//
// Krótki opis: 
//   Kod automatycznie generowany przez Visual Studio – opisuje układ kontrolek
//   formularza.
//

namespace JaProjIP
{
    partial class Form1
    {
        /// <summary>
        /// Wymagana przez projektanta zmienna.
        /// Zarządza zasobami, które zostają utworzone w metodzie InitializeComponent().
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Metoda zwalniająca zasoby niezarządzane i zarządzane.
        /// </summary>
        /// <param name="disposing">
        /// true, jeśli zasoby zarządzane mają zostać usunięte; false w przeciwnym razie.
        /// </param>
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
        /// Metoda InitializeComponent – kod automatycznie wygenerowany przez Visual Studio.
        /// Tworzy i rozmieszcza wszystkie kontrolki formularza (przyciski, etykiety, PictureBoxy itp.).
        /// Modyfikacje wprowadzone ręcznie mogą zostać nadpisane przy ponownej edycji w projektancie.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnLoadImage = new System.Windows.Forms.Button();
            this.cmbLibrary = new System.Windows.Forms.ComboBox();
            this.lblLibrary = new System.Windows.Forms.Label();
            this.lblThreads = new System.Windows.Forms.Label();
            this.btnRunTest = new System.Windows.Forms.Button();
            this.btnSaveImage = new System.Windows.Forms.Button();
            this.btnConvertImage = new System.Windows.Forms.Button();
            this.pictureBoxOriginal = new System.Windows.Forms.PictureBox();
            this.pictureBoxProcessed = new System.Windows.Forms.PictureBox();
            this.lblOriginal = new System.Windows.Forms.Label();
            this.lblProcessed = new System.Windows.Forms.Label();
            this.trackBarThreads = new System.Windows.Forms.TrackBar(); // Dodany TrackBar

            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOriginal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProcessed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarThreads)).BeginInit();
            this.SuspendLayout();
            // 
            // btnLoadImage
            // 
            this.btnLoadImage.Location = new System.Drawing.Point(12, 12);
            this.btnLoadImage.Name = "btnLoadImage";
            this.btnLoadImage.Size = new System.Drawing.Size(100, 30);
            this.btnLoadImage.TabIndex = 0;
            this.btnLoadImage.Text = "Load Image";
            this.btnLoadImage.UseVisualStyleBackColor = true;
            this.btnLoadImage.Click += new System.EventHandler(this.btnLoadImage_Click);
            // 
            // cmbLibrary
            // 
            this.cmbLibrary.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLibrary.FormattingEnabled = true;
            this.cmbLibrary.Location = new System.Drawing.Point(130, 18);
            this.cmbLibrary.Name = "cmbLibrary";
            this.cmbLibrary.Size = new System.Drawing.Size(150, 21);
            this.cmbLibrary.TabIndex = 1;
            // 
            // lblLibrary
            // 
            this.lblLibrary.AutoSize = true;
            this.lblLibrary.Location = new System.Drawing.Point(130, 2);
            this.lblLibrary.Name = "lblLibrary";
            this.lblLibrary.Size = new System.Drawing.Size(45, 13);
            this.lblLibrary.TabIndex = 2;
            this.lblLibrary.Text = "Library:";
            // 
            // lblThreads
            // 
            this.lblThreads.AutoSize = true;
            this.lblThreads.Location = new System.Drawing.Point(300, 2);
            this.lblThreads.Name = "lblThreads";
            this.lblThreads.Size = new System.Drawing.Size(47, 13);
            this.lblThreads.TabIndex = 4;
            this.lblThreads.Text = "Threads:";
            // 
            // btnRunTest
            // 
            this.btnRunTest.Location = new System.Drawing.Point(420, 12);
            this.btnRunTest.Name = "btnRunTest";
            this.btnRunTest.Size = new System.Drawing.Size(100, 30);
            this.btnRunTest.TabIndex = 5;
            this.btnRunTest.Text = "Run Test";
            this.btnRunTest.UseVisualStyleBackColor = true;
            this.btnRunTest.Click += new System.EventHandler(this.btnRunTest_Click);
            // 
            // btnSaveImage
            // 
            this.btnSaveImage.Location = new System.Drawing.Point(540, 12);
            this.btnSaveImage.Name = "btnSaveImage";
            this.btnSaveImage.Size = new System.Drawing.Size(100, 30);
            this.btnSaveImage.TabIndex = 6;
            this.btnSaveImage.Text = "Save Image";
            this.btnSaveImage.UseVisualStyleBackColor = true;
            this.btnSaveImage.Click += new System.EventHandler(this.btnSaveImage_Click);
            // 
            // btnConvertImage
            // 
            this.btnConvertImage.Location = new System.Drawing.Point(660, 12);
            this.btnConvertImage.Name = "btnConvertImage";
            this.btnConvertImage.Size = new System.Drawing.Size(120, 30);
            this.btnConvertImage.TabIndex = 7;
            this.btnConvertImage.Text = "Convert Image";
            this.btnConvertImage.UseVisualStyleBackColor = true;
            this.btnConvertImage.Click += new System.EventHandler(this.btnConvertImage_Click);
            // 
            // pictureBoxOriginal
            // 
            this.pictureBoxOriginal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxOriginal.Location = new System.Drawing.Point(12, 80);
            this.pictureBoxOriginal.Name = "pictureBoxOriginal";
            this.pictureBoxOriginal.Size = new System.Drawing.Size(500, 400);
            this.pictureBoxOriginal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxOriginal.TabIndex = 8;
            this.pictureBoxOriginal.TabStop = false;
            // 
            // pictureBoxProcessed
            // 
            this.pictureBoxProcessed.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxProcessed.Location = new System.Drawing.Point(540, 80);
            this.pictureBoxProcessed.Name = "pictureBoxProcessed";
            this.pictureBoxProcessed.Size = new System.Drawing.Size(500, 400);
            this.pictureBoxProcessed.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxProcessed.TabIndex = 9;
            this.pictureBoxProcessed.TabStop = false;
            // 
            // lblOriginal
            // 
            this.lblOriginal.AutoSize = true;
            this.lblOriginal.Location = new System.Drawing.Point(12, 60);
            this.lblOriginal.Name = "lblOriginal";
            this.lblOriginal.Size = new System.Drawing.Size(83, 13);
            this.lblOriginal.TabIndex = 10;
            this.lblOriginal.Text = "Original Image:";
            // 
            // lblProcessed
            // 
            this.lblProcessed.AutoSize = true;
            this.lblProcessed.Location = new System.Drawing.Point(540, 60);
            this.lblProcessed.Name = "lblProcessed";
            this.lblProcessed.Size = new System.Drawing.Size(98, 13);
            this.lblProcessed.TabIndex = 11;
            this.lblProcessed.Text = "Processed Image:";
            // 
            // trackBarThreads
            // 
            this.trackBarThreads.Location = new System.Drawing.Point(300, 18);
            this.trackBarThreads.Maximum = 64;
            this.trackBarThreads.Minimum = 1;
            this.trackBarThreads.Name = "trackBarThreads";
            this.trackBarThreads.Size = new System.Drawing.Size(100, 45);
            this.trackBarThreads.TabIndex = 12;
            this.trackBarThreads.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarThreads.Value = 1;
            this.trackBarThreads.Scroll += new System.EventHandler(this.trackBarThreads_Scroll);
            //
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(1064, 501);
            this.Controls.Add(this.trackBarThreads);
            this.Controls.Add(this.lblProcessed);
            this.Controls.Add(this.lblOriginal);
            this.Controls.Add(this.pictureBoxProcessed);
            this.Controls.Add(this.pictureBoxOriginal);
            this.Controls.Add(this.btnConvertImage);
            this.Controls.Add(this.btnSaveImage);
            this.Controls.Add(this.btnRunTest);
            this.Controls.Add(this.lblThreads);
            this.Controls.Add(this.lblLibrary);
            this.Controls.Add(this.cmbLibrary);
            this.Controls.Add(this.btnLoadImage);
            this.Name = "Form1";
            this.Text = "JaProjIP - Night Vision Filter";

            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOriginal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProcessed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarThreads)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        // Deklaracje kontrolek
        private System.Windows.Forms.Button btnLoadImage;
        private System.Windows.Forms.ComboBox cmbLibrary;
        private System.Windows.Forms.Label lblLibrary;
        private System.Windows.Forms.Label lblThreads;
        private System.Windows.Forms.Button btnRunTest;
        private System.Windows.Forms.Button btnSaveImage;
        private System.Windows.Forms.Button btnConvertImage;
        private System.Windows.Forms.PictureBox pictureBoxOriginal;
        private System.Windows.Forms.PictureBox pictureBoxProcessed;
        private System.Windows.Forms.Label lblOriginal;
        private System.Windows.Forms.Label lblProcessed;
        private System.Windows.Forms.TrackBar trackBarThreads;
    }
}
