using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace IoNote.NoteModels
{
    internal class ImageConverter
    {
        public static byte[] ConvertImageToBinary(Canvas canvas)
        {
            try
            {
                RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                    (int)canvas.ActualWidth, (int)canvas.ActualHeight, 96d, 96d, PixelFormats.Pbgra32);

                canvas.Measure(new Size((int)canvas.ActualWidth, (int)canvas.ActualHeight));
                canvas.Arrange(new Rect(new Size((int)canvas.ActualWidth, (int)canvas.ActualHeight)));

                renderBitmap.Render(canvas);

                using (MemoryStream stream = new MemoryStream())
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                    encoder.Save(stream);
                    return stream.ToArray();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static BitmapImage ConvertBinaryToImage(byte[] blob)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(blob))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static byte[] ConvertImageExternalToBinary(BitmapImage image)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(stream);
                    return stream.ToArray();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
