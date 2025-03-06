/***************************************************************
*  Projekt: JaProjIP
*  Plik: TimeMeasurement.cs
*  Semestr/Rok akademicki: Zimowy 2024/2025
*  Autor: Igor Potoczny
*  Wersja: 1.0
*  Historia zmian:
*      - v1.0: Utworzenie klasy do pomiaru czasu
*  Opis pliku:
*      Klasa do mierzenia czasu wykonania akcji w milisekundach,
*      z możliwością wielokrotnych pomiarów i uśrednienia wyników,
*      przy czym pierwszy pomiar jest pomijany.
***************************************************************/

using System;
using System.Diagnostics;

namespace JaProjIP
{
    /// <summary>
    /// Klasa do mierzenia czasu wykonania akcji w milisekundach.
    /// </summary>
    public class TimeMeasurement
    {
        /// <summary>
        /// Mierzy czas wykonania przekazanej akcji.
        /// </summary>
        /// <param name="action">Metoda/funkcja do wykonania.</param>
        /// <returns>Czas wykonania w milisekundach.</returns>
        public double MeasureTime(Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.Elapsed.TotalMilliseconds;
        }

        /// <summary>
        /// Oblicza średni czas wykonania akcji z zadanej liczby powtórzeń.
        /// Pomija pierwszy pomiar (tzw. warm-up).
        /// </summary>
        /// <param name="action">Metoda/funkcja do wykonania.</param>
        /// <param name="iterations">
        /// Liczba powtórzeń (domyślnie 5).
        /// minimalnie 2, ponieważ pierwszy pomiar jest pomijany.
        /// </param>
        /// <returns>
        /// Średni czas (w milisekundach) z (iterations-1) wywołań,
        /// z wyłączeniem pierwszego pomiaru.
        /// </returns>
        public double GetAverageTime(Action action, int iterations = 5)
        {
            if (iterations < 2)
                throw new ArgumentException("iterations musi być ≥ 2, aby można było pominąć pierwszy pomiar.");

            // Pomiar rozgrzewkowy
            _ = MeasureTime(action);

            double total = 0;
            for (int i = 1; i < iterations; i++)
            {
                total += MeasureTime(action);
            }

            return total / (iterations - 1);
        }
    }
}
