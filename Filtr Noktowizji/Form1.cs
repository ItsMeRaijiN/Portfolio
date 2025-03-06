// Projekt: JaProjIP
// Semestr/Rok akademicki: 2024/2025
// Autor: Igor Potoczny
//
// Krótki opis:
//   Główny formularz aplikacji WinForms. Pozwala na:
//   - Wczytanie obrazu, zapis
//   - Wybór biblioteki (C# lub ASM)
//   - Ustawienie liczby wątków suwakiem (1–64)
//   - Uruchomienie filtra noktowizji z pomiarem czasu
//   - Uruchomienie filtra zaawansowanego (histogram + night vision + glow)
//   - Prezentację wyników w PictureBox
//

using System;
using System.Drawing;
using System.Windows.Forms;
using JaCSharpLib;

namespace JaProjIP
{
    /// <summary>
    /// Klasa reprezentująca główne okno formularza.
    /// Zawiera logikę zdarzeń związanych z obsługą kontrolek oraz
    /// komunikację z menedżerami (wczytywanie/zapisywanie obrazów,
    /// stosowanie filtrów, pomiar czasu).
    /// </summary>
    public partial class Form1 : Form
    {
        // ****************************************************************************
        // Pola/Obiekty do zarządzania danymi i przetwarzaniem:
        // ****************************************************************************

        /// <summary>
        /// Klasa zarządzająca wczytywaniem/zapisywaniem obrazów.
        /// </summary>
        private DataManager dataManager;

        /// <summary>
        /// Klasa odpowiedzialna za logikę przetwarzania obrazów (C# / ASM).
        /// </summary>
        private ImageProcessor imageProcessor;

        /// <summary>
        /// Klasa odpowiedzialna za zarządzanie liczbą wątków
        /// podczas operacji przetwarzania obrazu.
        /// </summary>
        private ThreadManager threadManager;

        /// <summary>
        /// Klasa do pomiaru czasu wykonywania operacji.
        /// </summary>
        private TimeMeasurement timeMeasurement;

        /// <summary>
        /// Oryginalny obraz wczytany z pliku.
        /// </summary>
        private Bitmap originalImage;

        /// <summary>
        /// Przetworzony obraz.
        /// </summary>
        private Bitmap processedImage;

        /// <summary>
        /// Konstruktor formularza.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        /// <summary>
        /// Metoda inicjująca niestandardowe składniki aplikacji
        /// (m.in. menedżery, konfigurację trackBar itd.).
        /// </summary>
        private void InitializeCustomComponents()
        {
            // Inicjalizacja menedżerów
            dataManager = new DataManager();
            threadManager = new ThreadManager(); // Domyślnie ustawia liczbę wątków = Environment.ProcessorCount
            imageProcessor = new ImageProcessor(threadManager);
            timeMeasurement = new TimeMeasurement();

            // Ustawiamy domyślną liczbę wątków na suwaku:
            int defaultThreads = threadManager.GetThreadCount();
            if (defaultThreads < trackBarThreads.Minimum)
                defaultThreads = trackBarThreads.Minimum;
            if (defaultThreads > trackBarThreads.Maximum)
                defaultThreads = trackBarThreads.Maximum;

            trackBarThreads.Value = defaultThreads;
            lblThreads.Text = $"Threads: {trackBarThreads.Value}";

            // Inicjalizacja ComboBox wyboru biblioteki (C# / ASM)
            cmbLibrary.Items.Clear();
            cmbLibrary.Items.Add("C# Library");
            cmbLibrary.Items.Add("Asm Library");
            cmbLibrary.SelectedIndex = 0; // Domyślny wybór: C#

            pictureBoxOriginal.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxProcessed.SizeMode = PictureBoxSizeMode.Zoom;
        }

        /// <summary>
        /// Obsługa kliknięcia w przycisk "Load Image" – wczytuje obraz z pliku.
        /// </summary>
        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select an Image";
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Wczytanie obrazu za pomocą DataManager
                        originalImage = dataManager.LoadImage(ofd.FileName);
                        pictureBoxOriginal.Image = originalImage;

                        // Czyszczenie poprzedniego obrazu, jeśli istnieje
                        pictureBoxProcessed.Image = null;
                        processedImage?.Dispose();
                        processedImage = null;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to load image: {ex.Message}",
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Obsługa suwaka do wyboru liczby wątków (1–64).
        /// Ustawia wartość w ThreadManager i aktualizuje etykietę lblThreads.
        /// </summary>
        private void trackBarThreads_Scroll(object sender, EventArgs e)
        {
            int selectedThreadCount = trackBarThreads.Value;
            try
            {
                threadManager.SetThreadCount(selectedThreadCount);
                lblThreads.Text = $"Threads: {selectedThreadCount}";
            }
            catch (ArgumentOutOfRangeException ex)
            {
                MessageBox.Show(ex.Message, "Invalid Thread Count",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Obsługa kliknięcia w przycisk "Run Test" – uruchamia filtr noktowizji z pomiarem czasu.
        /// Porównuje efektywność biblioteki C# i/lub ASM w zależności od wyboru.
        /// </summary>
        private void btnRunTest_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
            {
                MessageBox.Show("Please load an image first.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string selectedLibrary = cmbLibrary.SelectedItem.ToString();

            double averageTime = timeMeasurement.GetAverageTime(() =>
            {
                if (selectedLibrary == "C# Library")
                {
                    imageProcessor.SetLibrary("C#");
                    processedImage = imageProcessor.ApplyNightVisionFilter(originalImage);
                }
                else
                {
                    imageProcessor.SetLibrary("Asm");
                    processedImage = imageProcessor.ApplyNightVisionFilter(originalImage);
                }
            }, 5);

            MessageBox.Show($"Average Processing Time: {averageTime} ms",
                            "Test Result", MessageBoxButtons.OK, MessageBoxIcon.Information);

            pictureBoxProcessed.Image = processedImage;
        }

        /// <summary>
        /// Obsługa kliknięcia w przycisk "Save Image" – zapisuje aktualnie przetworzony obraz
        /// do wybranego pliku w formacie JPG/PNG/BMP (wg rozszerzenia).
        /// </summary>
        private void btnSaveImage_Click(object sender, EventArgs e)
        {
            if (processedImage == null)
            {
                MessageBox.Show("No processed image to save.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Save Image";
                sfd.Filter = "JPEG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        dataManager.SaveImage(processedImage, sfd.FileName);
                        MessageBox.Show("Image saved successfully.",
                                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to save image: {ex.Message}",
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Obsługa kliknięcia w przycisk "Convert Image" – stosuje zaawansowany filtr
        /// (histogram + night vision + glow) w zależności od dostępnych opcji biblioteki.
        /// W ASM tylko podstawowy night vision.
        /// </summary>
        private void btnConvertImage_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
            {
                MessageBox.Show("Please load an image first.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string selectedLibrary = cmbLibrary.SelectedItem.ToString();

            try
            {
                if (selectedLibrary == "C# Library")
                {
                    imageProcessor.SetLibrary("C#");
                    processedImage = imageProcessor.ApplyAdvancedNightVisionFilter(originalImage);
                }
                else
                {
                    imageProcessor.SetLibrary("Asm");
                    processedImage = imageProcessor.ApplyNightVisionFilter(originalImage);
                }

                pictureBoxProcessed.Image = processedImage;
                MessageBox.Show("Image conversion completed.",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to convert image: {ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
