/***************************************************************
 *  Projekt: JaProjIP
 *  Plik: ImageProcessor.cs
 *  Semestr/Rok akademicki: 2024/2025
 *  Autor: Igor Potoczny
 *  Wersja: 21.37
 *
 *  Krótki opis:
 *      Klasa umożliwiająca przetwarzanie obrazów przy pomocy
 *      filtra "Night Vision" zarówno w C#, jak i przez wywołanie
 *      biblioteki ASM. C# zawiera dodatkowe efekty (Histogram Equalization, Glow).
 ***************************************************************/

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Numerics;

namespace JaCSharpLib
{
    public class ImageProcessor
    {
        private readonly ThreadManager threadManager;
        private string selectedLibrary = "C#";

        public ImageProcessor(ThreadManager tm)
        {
            threadManager = tm;
        }

        public void SetLibrary(string library)
        {
            selectedLibrary = library;
        }

        public byte Clamp(float value, float min, float max)
        {
            if (value < min) return (byte)min;
            if (value > max) return (byte)max;
            return (byte)value;
        }

        private static Bitmap Ensure24bppRgb(Bitmap input)
        {
            if (input.PixelFormat != PixelFormat.Format24bppRgb)
            {
                Bitmap newImage = new Bitmap(input.Width, input.Height, PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(newImage))
                {
                    g.DrawImage(input, new Rectangle(0, 0, newImage.Width, newImage.Height));
                }
                return newImage;
            }
            return input;
        }

        public Bitmap ApplyNightVisionFilter(Bitmap input)
        {
            input = Ensure24bppRgb(input);
            int threadCount = threadManager.GetThreadCount();

            Bitmap output = new Bitmap(input.Width, input.Height, PixelFormat.Format24bppRgb);

            if (selectedLibrary == "C#")
            {
                #region Filtr C# (wektorowy, wielowątkowy)

                Rectangle rect = new Rectangle(0, 0, input.Width, input.Height);
                BitmapData inputData = input.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                BitmapData outputData = output.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                int bytesPerPixel = 3;
                int inputStride = inputData.Stride;
                int outputStride = outputData.Stride;
                int heightImg = input.Height;
                int widthImg = input.Width;
                int inputByteCount = inputStride * heightImg;
                int outputByteCount = outputStride * heightImg;

                byte[] inputPixels = new byte[inputByteCount];
                byte[] outputPixels = new byte[outputByteCount];

                Marshal.Copy(inputData.Scan0, inputPixels, 0, inputByteCount);
                input.UnlockBits(inputData);

                float brightnessFactor = 1.2f;
                float contrastFactor = 1.2f;
                float greenSaturationFactor = 1.2f;
                float noiseIntensity = 5f;

                float contrastAdjust = (259 * (contrastFactor + 255)) / (255 * (259 - contrastFactor));

                ThreadLocal<Random> threadLocalRandom = new ThreadLocal<Random>(() => new Random());

                Vector<float> vBrightness = new Vector<float>(brightnessFactor);
                Vector<float> vGreenSat = new Vector<float>(greenSaturationFactor);
                Vector<float> vContrA = new Vector<float>(contrastAdjust);
                Vector<float> v128 = new Vector<float>(128f);
                Vector<float> vNoiseMax = new Vector<float>(noiseIntensity / 2f);
                Vector<float> vBRfactor = new Vector<float>(0.4f);  // R i B = 0.4 * luminance
                Vector<float> vZero = Vector<float>.Zero;
                Vector<float> vMax255 = new Vector<float>(255f);

                // Luminancja - współczynniki: R=0.2126, G=0.7152, B=0.0722
                Vector<float> vLumR = new Vector<float>(0.2126f);
                Vector<float> vLumG = new Vector<float>(0.7152f);
                Vector<float> vLumB = new Vector<float>(0.0722f);

                Parallel.For(0, heightImg,
                             new ParallelOptions { MaxDegreeOfParallelism = threadCount },
                             y =>
                             {
                                 Random rand = threadLocalRandom.Value;
                                 int rowInput = y * inputStride;
                                 int rowOutput = y * outputStride;

                                 int x = 0;
                                 int fullWidth = widthImg - (widthImg % Vector<float>.Count);

                                 while (x < fullWidth)
                                 {
                                     float[] bufR = new float[Vector<float>.Count];
                                     float[] bufG = new float[Vector<float>.Count];
                                     float[] bufB = new float[Vector<float>.Count];

                                     for (int i = 0; i < Vector<float>.Count; i++)
                                     {
                                         int px = x + i;
                                         int indexIn = rowInput + px * bytesPerPixel;
                                         byte b = inputPixels[indexIn];
                                         byte g = inputPixels[indexIn + 1];
                                         byte r = inputPixels[indexIn + 2];

                                         bufR[i] = r;
                                         bufG[i] = g;
                                         bufB[i] = b;
                                     }

                                     Vector<float> vR = new Vector<float>(bufR);
                                     Vector<float> vG = new Vector<float>(bufG);
                                     Vector<float> vB = new Vector<float>(bufB);

                                     // luminance = R*0.2126 + G*0.7152 + B*0.0722
                                     Vector<float> vLum = vR * vLumR + vG * vLumG + vB * vLumB;

                                     // vLum *= brightnessFactor
                                     vLum *= vBrightness;

                                     // R,B = lum*0.4, G = lum*greenSaturationFactor
                                     Vector<float> vRout = vLum * vBRfactor;
                                     Vector<float> vGout = vLum * vGreenSat;
                                     Vector<float> vBout = vLum * vBRfactor;

                                     // mapped = contrA * (mapped - 128) + 128
                                     // osobno dla R, G, B
                                     vRout = vContrA * (vRout - v128) + v128;
                                     vGout = vContrA * (vGout - v128) + v128;
                                     vBout = vContrA * (vBout - v128) + v128;

                                     float[] noiseArr = new float[Vector<float>.Count];
                                     for (int i = 0; i < Vector<float>.Count; i++)
                                     {
                                         float val = (float)(rand.NextDouble() * noiseIntensity - noiseIntensity / 2);
                                         noiseArr[i] = val;
                                     }
                                     Vector<float> vNoise = new Vector<float>(noiseArr);

                                     //szum do R, G, B
                                     vRout += vNoise;
                                     vGout += vNoise;
                                     vBout += vNoise;

                                     // Ograniczenie [0..255]
                                     vRout = Vector.Min(Vector.Max(vRout, vZero), vMax255);
                                     vGout = Vector.Min(Vector.Max(vGout, vZero), vMax255);
                                     vBout = Vector.Min(Vector.Max(vBout, vZero), vMax255);

                                     float[] outR = new float[Vector<float>.Count];
                                     float[] outG = new float[Vector<float>.Count];
                                     float[] outB = new float[Vector<float>.Count];

                                     vRout.CopyTo(outR);
                                     vGout.CopyTo(outG);
                                     vBout.CopyTo(outB);

                                     for (int i = 0; i < Vector<float>.Count; i++)
                                     {
                                         int px = x + i;
                                         int indexOut = rowOutput + px * bytesPerPixel;

                                         byte bVal = (byte)outB[i];
                                         byte gVal = (byte)outG[i];
                                         byte rVal = (byte)outR[i];

                                         outputPixels[indexOut] = bVal; // Blue
                                         outputPixels[indexOut + 1] = gVal; // Green
                                         outputPixels[indexOut + 2] = rVal; // Red
                                     }

                                     x += Vector<float>.Count;
                                 }

                                 // Obsługa pozostałych pikseli, jeśli (widthImg % 4 != 0)
                                 for (; x < widthImg; x++)
                                 {
                                     int idxIn = rowInput + x * bytesPerPixel;
                                     int idxOut = rowOutput + x * bytesPerPixel;

                                     byte b = inputPixels[idxIn];
                                     byte g = inputPixels[idxIn + 1];
                                     byte r = inputPixels[idxIn + 2];

                                     float luminance = 0.2126f * r + 0.7152f * g + 0.0722f * b;
                                     luminance *= brightnessFactor;

                                     float mappedRed = luminance * 0.4f;
                                     float mappedGreen = luminance * greenSaturationFactor;
                                     float mappedBlue = luminance * 0.4f;

                                     mappedRed = contrastAdjust * (mappedRed - 128) + 128;
                                     mappedGreen = contrastAdjust * (mappedGreen - 128) + 128;
                                     mappedBlue = contrastAdjust * (mappedBlue - 128) + 128;

                                     float noiseVal = (float)(rand.NextDouble() * noiseIntensity - noiseIntensity / 2);
                                     mappedRed += noiseVal;
                                     mappedGreen += noiseVal;
                                     mappedBlue += noiseVal;

                                     if (mappedRed < 0f) mappedRed = 0f; else if (mappedRed > 255f) mappedRed = 255f;
                                     if (mappedGreen < 0f) mappedGreen = 0f; else if (mappedGreen > 255f) mappedGreen = 255f;
                                     if (mappedBlue < 0f) mappedBlue = 0f; else if (mappedBlue > 255f) mappedBlue = 255f;

                                     outputPixels[idxOut] = (byte)mappedBlue;
                                     outputPixels[idxOut + 1] = (byte)mappedGreen;
                                     outputPixels[idxOut + 2] = (byte)mappedRed;
                                 }
                             });

                Marshal.Copy(outputPixels, 0, outputData.Scan0, outputByteCount);
                output.UnlockBits(outputData);

                #endregion
            }
            else if (selectedLibrary == "Asm")
            {
                #region Filtr ASM (z podziałem wątków po stronie C#)
                try
                {
                    Rectangle rect = new Rectangle(0, 0, input.Width, input.Height);

                    BitmapData inputData = input.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                    BitmapData outputData = output.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                    try
                    {
                        int bytes = Math.Abs(inputData.Stride) * input.Height;
                        byte[] processingBuffer = new byte[bytes];

                        // Kopiujemy dane pikseli z inputa do bufora
                        Marshal.Copy(inputData.Scan0, processingBuffer, 0, bytes);

                        // Alokujemy pamięć unmanaged
                        IntPtr processingPtr = Marshal.AllocHGlobal(bytes);
                        try
                        {
                            Marshal.Copy(processingBuffer, 0, processingPtr, bytes);

                            // Dzielimy obraz na fragmenty w pionie (podział na wątki).
                            int totalRows = input.Height;
                            int stride = inputData.Stride;
                            int w = input.Width;

                            // Równoległa pętla – każdy wątek obrabia inny zakres wierszy
                            Parallel.For(0, threadCount, i =>
                            {
                                int rowsPerThread = totalRows / threadCount;
                                int startY = i * rowsPerThread;
                                int endY = (i == threadCount - 1) ? totalRows : startY + rowsPerThread;
                                int localHeight = endY - startY;

                                // Wskaźnik do początku "swoich" wierszy
                                IntPtr localPtr = processingPtr + startY * stride;

                                // Wywołanie asemblera TYLKO dla własnego zakresu wierszy
                                AsmLibrary.ApplyFilter(
                                    localPtr,
                                    w,
                                    localHeight,
                                    stride
                                );
                            });

                            // Po skończonej pracy wszystkich wątków, kopiujemy efekt do output
                            Marshal.Copy(processingPtr, processingBuffer, 0, bytes);
                            Marshal.Copy(processingBuffer, 0, outputData.Scan0, bytes);
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(processingPtr);
                        }
                    }
                    finally
                    {
                        input.UnlockBits(inputData);
                        output.UnlockBits(outputData);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Assembler library failed: {ex.Message}", ex);
                }
                #endregion
            }

            return output;
        }

        public Bitmap HistogramEqualization(Bitmap input)
        {
            input = Ensure24bppRgb(input);

            Bitmap output = new Bitmap(input.Width, input.Height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, input.Width, input.Height);
            BitmapData inputData = input.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData outputData = output.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytesPerPixel = 3;
            int inputStride = inputData.Stride;
            int outputStride = outputData.Stride;
            int heightImg = input.Height;
            int widthImg = input.Width;
            int inputByteCount = inputStride * heightImg;
            int outputByteCount = outputStride * heightImg;

            byte[] inputPixels = new byte[inputByteCount];
            byte[] outputPixels = new byte[outputByteCount];

            Marshal.Copy(inputData.Scan0, inputPixels, 0, inputByteCount);
            input.UnlockBits(inputData);

            int[] histogram = new int[256];
            for (int i = 0; i < inputPixels.Length; i += bytesPerPixel)
            {
                float lum = 0.2126f * inputPixels[i + 2]
                          + 0.7152f * inputPixels[i + 1]
                          + 0.0722f * inputPixels[i];
                int lumInt = Clamp(lum, 0f, 255f);
                histogram[lumInt]++;
            }

            int[] cumulativeHistogram = new int[256];
            cumulativeHistogram[0] = histogram[0];
            for (int i = 1; i < 256; i++)
                cumulativeHistogram[i] = cumulativeHistogram[i - 1] + histogram[i];

            float scale = 255f / cumulativeHistogram[255];
            int[] equalizedMap = new int[256];
            for (int i = 0; i < 256; i++)
            {
                int val = (int)(cumulativeHistogram[i] * scale);
                equalizedMap[i] = (val > 255) ? 255 : val;
            }

            Parallel.For(0, heightImg,
                         new ParallelOptions { MaxDegreeOfParallelism = threadManager.GetThreadCount() },
                         y =>
                         {
                             for (int x = 0; x < widthImg; x++)
                             {
                                 int indexInput = y * inputStride + x * bytesPerPixel;
                                 int indexOutput = y * outputStride + x * bytesPerPixel;

                                 float lum = 0.2126f * inputPixels[indexInput + 2]
                                           + 0.7152f * inputPixels[indexInput + 1]
                                           + 0.0722f * inputPixels[indexInput];
                                 int lumInt = Clamp(lum, 0f, 255f);
                                 int newLum = equalizedMap[lumInt];

                                 float scaleFactor = (lum != 0) ? (newLum / lum) : 1f;

                                 float newRed = inputPixels[indexInput + 2] * scaleFactor;
                                 float newGreen = inputPixels[indexInput + 1] * scaleFactor;
                                 float newBlue = inputPixels[indexInput] * scaleFactor;

                                 byte finalRed = Clamp(newRed, 0f, 255f);
                                 byte finalGreen = Clamp(newGreen, 0f, 255f);
                                 byte finalBlue = Clamp(newBlue, 0f, 255f);

                                 outputPixels[indexOutput] = finalBlue;
                                 outputPixels[indexOutput + 1] = finalGreen;
                                 outputPixels[indexOutput + 2] = finalRed;
                             }
                         });

            Marshal.Copy(outputPixels, 0, outputData.Scan0, outputByteCount);
            output.UnlockBits(outputData);

            return output;
        }

        public Bitmap AddGlow(Bitmap input, float glowIntensity)
        {
            input = Ensure24bppRgb(input);

            Bitmap output = new Bitmap(input.Width, input.Height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, input.Width, input.Height);
            BitmapData inputData = input.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData outputData = output.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytesPerPixel = 3;
            int inputStride = inputData.Stride;
            int outputStride = outputData.Stride;
            int heightImg = input.Height;
            int widthImg = input.Width;
            int inputByteCount = inputStride * heightImg;
            int outputByteCount = outputStride * heightImg;

            byte[] inputPixels = new byte[inputByteCount];
            byte[] outputPixels = new byte[outputByteCount];

            Marshal.Copy(inputData.Scan0, inputPixels, 0, inputByteCount);
            input.UnlockBits(inputData);

            Parallel.For(0, heightImg,
                         new ParallelOptions { MaxDegreeOfParallelism = threadManager.GetThreadCount() },
                         y =>
                         {
                             for (int x = 0; x < widthImg; x++)
                             {
                                 int indexInput = y * inputStride + x * bytesPerPixel;
                                 int indexOutput = y * outputStride + x * bytesPerPixel;

                                 byte red = inputPixels[indexInput + 2];
                                 byte green = inputPixels[indexInput + 1];
                                 byte blue = inputPixels[indexInput];

                                 float brightness = (0.2126f * red + 0.7152f * green + 0.0722f * blue) / 255f;

                                 if (brightness > 0.6f)
                                 {
                                     float factor = 1.0f + glowIntensity;
                                     float newRed = red * factor;
                                     float newGreen = green * factor;
                                     float newBlue = blue * factor;

                                     byte finalRed = Clamp(newRed, 0f, 255f);
                                     byte finalGreen = Clamp(newGreen, 0f, 255f);
                                     byte finalBlue = Clamp(newBlue, 0f, 255f);

                                     outputPixels[indexOutput] = finalBlue;
                                     outputPixels[indexOutput + 1] = finalGreen;
                                     outputPixels[indexOutput + 2] = finalRed;
                                 }
                                 else
                                 {
                                     outputPixels[indexOutput] = blue;
                                     outputPixels[indexOutput + 1] = green;
                                     outputPixels[indexOutput + 2] = red;
                                 }
                             }
                         });

            Marshal.Copy(outputPixels, 0, outputData.Scan0, outputByteCount);
            output.UnlockBits(outputData);

            return output;
        }

        public Bitmap ApplyAdvancedNightVisionFilter(Bitmap input)
        {
            Bitmap heImage = HistogramEqualization(input);
            Bitmap nightVisionImage = ApplyNightVisionFilter(heImage);
            Bitmap finalImage = AddGlow(nightVisionImage, 0.0f);

            heImage.Dispose();
            nightVisionImage.Dispose();

            return finalImage;
        }

        public static class AsmLibrary
        {
            [DllImport("JaAsm.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "ApplyFilter")]
            public static extern void ApplyFilter(
                IntPtr pixelBuffer,
                int width,
                int height,
                int stride
            );
        }
    }
}