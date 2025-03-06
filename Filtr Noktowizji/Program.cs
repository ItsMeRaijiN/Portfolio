/***************************************************************
*  Projekt: JaProjIP
*  Plik: Program.cs
*  Semestr/Rok akademicki: Zimowy 2024/2025
*  Autor: Igor Potoczny
*  Wersja: 1.0
*  Historia zmian:
*      - v1.0: Stworzenie głównego punktu wejścia aplikacji
*  Opis pliku:
*      Główny punkt startowy aplikacji Windows Forms. Uruchamia formularz Form1.
***************************************************************/

using System;
using System.Windows.Forms;

namespace JaProjIP
{
    /// <summary>
    /// Punkt wejścia dla aplikacji WinForms.
    /// Inicjuje styl graficzny i wyświetla główny formularz.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// Metoda Main - punkt wejścia aplikacji w trybie STA (Single Threaded Apartment).
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Ustawienia stylu graficznego
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Uruchamia główny formularz (Form1)
            Application.Run(new Form1());
        }
    }
}
