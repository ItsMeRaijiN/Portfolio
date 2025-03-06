/***************************************************************
*  Projekt: JaProjIP
*  Plik: DataManager.cs
*  Semestr/Rok akademicki: 2024/2025
*  Autor: Igor Potoczny
*  Wersja: 2.0
*  Opis pliku:
*      Klasa zawierająca metody do ładowania i zapisywania obrazów
*      w formatach JPG, PNG, BMP itp.
***************************************************************/

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace JaCSharpLib
{
    /// <summary>
    /// Klasa do ładowania i zapisywania obrazów z/do plików.
    /// Wspiera formaty: JPG, PNG, BMP (wybierane po rozszerzeniu).
    /// </summary>
    public class DataManager
    {
        /// <summary>
        ///     Ładuje obraz z podanej ścieżki do obiektu Bitmap.
        /// Parametry:
        ///     string path - Ścieżka do pliku graficznego (jpg, png, bmp, itp.).
        /// Zwracana wartość:
        ///     Bitmap - wczytany obraz w postaci obiektu klasy Bitmap.
        /// </summary>
        public Bitmap LoadImage(string path)
        {
            return new Bitmap(path);
        }

        /// <summary>
        ///     Zapisuje obiekt Bitmap do pliku na dysku, w formacie wynikającym
        ///     z rozszerzenia (jpg, jpeg, png, bmp).
        /// Parametry:
        ///     Bitmap image - obraz do zapisania.
        ///     string path  - ścieżka docelowa zawierająca rozszerzenie.
        /// </summary>
        public void SaveImage(Bitmap image, string path)
        {
            ImageFormat format = ImageFormat.Png;
            string extension = Path.GetExtension(path).ToLower();

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    format = ImageFormat.Jpeg;
                    break;
                case ".bmp":
                    format = ImageFormat.Bmp;
                    break;
                case ".png":
                default:
                    format = ImageFormat.Png;
                    break;
            }

            image.Save(path, format);
        }
    }
}
