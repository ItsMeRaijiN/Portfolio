/***************************************************************
 *  Projekt: JaProjIP
 *  Plik: ThreadManager.cs
 *  Semestr/Rok akademicki: 2024/2025
 *  Autor: Igor Potoczny
 *
 *  Krótki opis:
 *      Klasa zarządzająca liczbą wątków używanych podczas
 *      przetwarzania obrazu. Pozwala ustawiać przedział 1-64 wątków.
 *
 ***************************************************************/

using System;

namespace JaCSharpLib
{
    /// <summary>
    /// Klasa do zarządzania liczbą wątków używanych w operacjach przetwarzania.
    /// </summary>
    public class ThreadManager
    {
        /// <summary>
        /// Aktualnie ustawiona liczba wątków.
        /// </summary>
        private int threadCount;

        /// <summary>
        /// Konstruktor klasy ThreadManager.
        /// Ustawia domyślną liczbę wątków na liczbę logicznych procesorów.
        /// </summary>
        public ThreadManager()
        {
            threadCount = Environment.ProcessorCount;
        }

        /// <summary>
        /// Pobiera aktualną liczbę wątków.
        /// </summary>
        /// <returns>Liczba wątków.</returns>
        public int GetThreadCount()
        {
            return threadCount;
        }

        /// <summary>
        /// Ustawia liczbę wątków, zapewniając jej zakres 1–64.
        /// </summary>
        /// <param name="count">Pożądana liczba wątków.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Rzucane, gdy wartość count wychodzi poza przedział [1, 64].
        /// </exception>
        public void SetThreadCount(int count)
        {
            if (count >= 1 && count <= 64)
            {
                threadCount = count;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Thread count must be between 1 and 64.");
            }
        }
    }
}
