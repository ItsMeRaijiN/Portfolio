using IoNote.AuthorizationModels;
using IoNote.NoteModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace IoNote.NotePages
{
    public partial class PaintWindow : Window
    {
        private bool isDrawing = false;
        private string activeTool = null;
        private Point previousPoint;
        private Brush currentBrush = Brushes.Black;
        private double strokeThickness = 2;
        private int noteId;
        private string activeImageName = null;
        private WriteableBitmap canvasSnapshot;
        private Stack<List<UIElement>> undoStack = new Stack<List<UIElement>>();
        private List<UIElement> currentDrawing = new List<UIElement>();
        private List<Line> currentStrokeLines = new List<Line>();

        private enum BackgroundType
        {
            Empty,
            Lines,
            Grid
        }

        private BackgroundType currentBackground = BackgroundType.Empty;
        private const double GRID_SIZE = 20;

        public PaintWindow(int noteId)
        {
            InitializeComponent();
            UpdateToolState();
            UpdateUndoButtonState();
            this.noteId = noteId;
        }

        private void backgroundButton_Click(object sender, RoutedEventArgs e)
        {
            Window dialog = new Window
            {
                Title = "Wybierz tło",
                Width = 250,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            StackPanel stackPanel = new StackPanel { Margin = new Thickness(10) };

            RadioButton emptyButton = new RadioButton
            {
                Content = "Puste tło",
                Margin = new Thickness(0, 5, 0, 5),
                IsChecked = currentBackground == BackgroundType.Empty
            };

            RadioButton linesButton = new RadioButton
            {
                Content = "Linie poziome",
                Margin = new Thickness(0, 5, 0, 5),
                IsChecked = currentBackground == BackgroundType.Lines
            };

            RadioButton gridButton = new RadioButton
            {
                Content = "Kratka",
                Margin = new Thickness(0, 5, 0, 5),
                IsChecked = currentBackground == BackgroundType.Grid
            };

            Button okButton = new Button
            {
                Content = "OK",
                Width = 80,
                Height = 30,
                Margin = new Thickness(0, 20, 0, 0)
            };

            okButton.Click += (s, args) =>
            {
                if (emptyButton.IsChecked == true)
                    currentBackground = BackgroundType.Empty;
                else if (linesButton.IsChecked == true)
                    currentBackground = BackgroundType.Lines;
                else if (gridButton.IsChecked == true)
                    currentBackground = BackgroundType.Grid;

                UpdateBackground();
                dialog.DialogResult = true;
            };

            stackPanel.Children.Add(emptyButton);
            stackPanel.Children.Add(linesButton);
            stackPanel.Children.Add(gridButton);
            stackPanel.Children.Add(okButton);

            dialog.Content = stackPanel;
            dialog.ShowDialog();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (activeTool == null) return;

            if (activeTool == "fill")
            {
                Point clickPoint = e.GetPosition(PaintCanvas);
                FloodFill(clickPoint);
            }
            else
            {
                SaveCanvasState();
                isDrawing = true;
                previousPoint = e.GetPosition(PaintCanvas);
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (activeTool == null || !isDrawing) return;

            Point currentPoint = e.GetPosition(PaintCanvas);

            Line line = new Line
            {
                Stroke = currentBrush,
                StrokeThickness = strokeThickness,
                X1 = previousPoint.X,
                Y1 = previousPoint.Y,
                X2 = currentPoint.X,
                Y2 = currentPoint.Y
            };

            PaintCanvas.Children.Add(line);
            previousPoint = currentPoint;
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            isDrawing = false;
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Background is SolidColorBrush brush)
            {
                currentBrush = brush;
                selectedColorBox.Background = brush;
            }
        }

        private bool ColorsAreClose(Color c1, Color c2, int tolerance = 10)
        {
            return Math.Abs(c1.R - c2.R) <= tolerance &&
                   Math.Abs(c1.G - c2.G) <= tolerance &&
                   Math.Abs(c1.B - c2.B) <= tolerance;
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCanvasState();
            PaintCanvas.Children.Clear();
            UpdateBackground();
        }

        private void drawButton_Click(object sender, RoutedEventArgs e)
        {
            if (activeTool == "draw")
            {
                activeTool = null;
            }
            else
            {
                activeTool = "draw";
                currentBrush = selectedColorBox.Background;
            }
            UpdateToolState();
        }

        private void DrawGrid()
        {
            double canvasHeight = PaintCanvas.ActualHeight;
            double canvasWidth = PaintCanvas.ActualWidth;

            for (double y = GRID_SIZE; y < canvasHeight; y += GRID_SIZE)
            {
                Line line = new Line
                {
                    Stroke = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
                    StrokeThickness = 0.5,
                    X1 = 0,
                    Y1 = y,
                    X2 = canvasWidth,
                    Y2 = y,
                    Tag = "background"
                };
                PaintCanvas.Children.Add(line);
            }

            for (double x = GRID_SIZE; x < canvasWidth; x += GRID_SIZE)
            {
                Line line = new Line
                {
                    Stroke = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
                    StrokeThickness = 0.5,
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = canvasHeight,
                    Tag = "background"
                };
                PaintCanvas.Children.Add(line);
            }
        }

        private void DrawLines()
        {
            double canvasHeight = PaintCanvas.ActualHeight;
            double canvasWidth = PaintCanvas.ActualWidth;

            for (double y = GRID_SIZE; y < canvasHeight; y += GRID_SIZE)
            {
                Line line = new Line
                {
                    Stroke = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
                    StrokeThickness = 0.5,
                    X1 = 0,
                    Y1 = y,
                    X2 = canvasWidth,
                    Y2 = y,
                    Tag = "background"
                };
                PaintCanvas.Children.Add(line);
            }
        }

        private void editExistImageButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> imageNames = DatabaseManager.GetImagesName(noteId);

            if (imageNames == null || !imageNames.Any())
            {
                MessageBox.Show("Brak dostępnych zdjęć do edycji.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string selectedImageName = ShowImageSelectionDialog(imageNames);

            if (string.IsNullOrWhiteSpace(selectedImageName))
            {
                MessageBox.Show("Nie wybrano żadnego obrazu.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            byte[] imageBlob = DatabaseManager.GetImage(noteId, selectedImageName);

            if (imageBlob == null)
            {
                MessageBox.Show("Nie udało się pobrać wybranego obrazu.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            BitmapImage bitmap = ImageConverter.ConvertBinaryToImage(imageBlob);

            if (bitmap == null)
            {
                MessageBox.Show("Nie udało się wczytać obrazu.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            PaintCanvas.Children.Clear();
            Image imageControl = new Image
            {
                Source = bitmap,
                Width = PaintCanvas.ActualWidth,
                Height = PaintCanvas.ActualHeight,
                Stretch = Stretch.Uniform
            };
            PaintCanvas.Children.Add(imageControl);

            activeImageName = selectedImageName;
        }

        private void eraseButton_Click(object sender, RoutedEventArgs e)
        {
            if (activeTool == "erase")
            {
                activeTool = null;
            }
            else
            {
                activeTool = "erase";
                currentBrush = Brushes.White;
            }
            UpdateToolState();
        }

        private void exportButton_Click(object sender, RoutedEventArgs e)
        {
            string baseDirectory = Directory.GetCurrentDirectory();
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                InitialDirectory = baseDirectory,
                Filter = "PNG Image|*.png",
                DefaultExt = "png"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                    (int)PaintCanvas.ActualWidth,
                    (int)PaintCanvas.ActualHeight,
                    96d, 96d,
                    PixelFormats.Pbgra32);

                PaintCanvas.Measure(new Size((int)PaintCanvas.ActualWidth, (int)PaintCanvas.ActualHeight));
                PaintCanvas.Arrange(new Rect(new Size((int)PaintCanvas.ActualWidth, (int)PaintCanvas.ActualHeight)));
                renderBitmap.Render(PaintCanvas);

                using (FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                    encoder.Save(fileStream);
                }

                MessageBox.Show("Eksport zakończony sukcesem.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void fillButton_Click(object sender, RoutedEventArgs e)
        {
            if (activeTool == "fill")
            {
                activeTool = null;
            }
            else
            {
                activeTool = "fill";
                currentBrush = selectedColorBox.Background;
            }
            UpdateToolState();
        }

        public class FillGroup : System.Windows.Controls.Canvas
        {
            public FillGroup()
            {
                IsHitTestVisible = false;
            }
        }

        private void FloodFill(Point startPoint)
        {
            SaveCanvasState();

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)PaintCanvas.ActualWidth,
                (int)PaintCanvas.ActualHeight,
                96d, 96d,
                PixelFormats.Pbgra32);

            renderBitmap.Render(PaintCanvas);

            canvasSnapshot = new WriteableBitmap(renderBitmap);

            Color fillColor = ((SolidColorBrush)currentBrush).Color;

            int stride = canvasSnapshot.BackBufferStride;
            byte[] pixels = new byte[stride * canvasSnapshot.PixelHeight];
            canvasSnapshot.CopyPixels(pixels, stride, 0);

            int x = (int)startPoint.X;
            int y = (int)startPoint.Y;

            if (x < 0 || x >= canvasSnapshot.PixelWidth || y < 0 || y >= canvasSnapshot.PixelHeight)
                return;

            int pos = y * stride + x * 4;
            Color targetColor = Color.FromRgb(pixels[pos + 2], pixels[pos + 1], pixels[pos]);

            if (targetColor == fillColor)
                return;

            FillGroup fillGroup = new FillGroup();
            Canvas.SetLeft(fillGroup, 0);
            Canvas.SetTop(fillGroup, 0);

            Stack<Point> pixels_to_check = new Stack<Point>();
            pixels_to_check.Push(startPoint);

            HashSet<string> processed = new HashSet<string>();

            while (pixels_to_check.Count > 0)
            {
                Point current = pixels_to_check.Pop();
                x = (int)current.X;
                y = (int)current.Y;

                if (x < 0 || x >= canvasSnapshot.PixelWidth || y < 0 || y >= canvasSnapshot.PixelHeight)
                    continue;

                string key = $"{x},{y}";
                if (processed.Contains(key))
                    continue;

                processed.Add(key);

                pos = y * stride + x * 4;
                Color currentColor = Color.FromRgb(pixels[pos + 2], pixels[pos + 1], pixels[pos]);

                if (ColorsAreClose(currentColor, targetColor))
                {
                    Rectangle rect = new Rectangle
                    {
                        Width = 1,
                        Height = 1,
                        Fill = currentBrush
                    };
                    Canvas.SetLeft(rect, x);
                    Canvas.SetTop(rect, y);
                    fillGroup.Children.Add(rect);

                    pixels_to_check.Push(new Point(x + 1, y));
                    pixels_to_check.Push(new Point(x - 1, y));
                    pixels_to_check.Push(new Point(x, y + 1));
                    pixels_to_check.Push(new Point(x, y - 1));
                }
            }

            PaintCanvas.Children.Add(fillGroup);
        }

        private void insertButton_Click(object sender, RoutedEventArgs e)
        {
            byte[] imageBlob = ImageConverter.ConvertImageToBinary(PaintCanvas);
            if (imageBlob == null)
            {
                MessageBox.Show("Nie udało się przekonwertować obrazu.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string imageName = Microsoft.VisualBasic.Interaction.InputBox(
                "Wprowadź nazwę obrazu:",
                "Dodawanie obrazu",
                "Nowy obraz");

            if (string.IsNullOrWhiteSpace(imageName))
            {
                MessageBox.Show("Nazwa obrazu nie może być pusta.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (DatabaseManager.CheckImage(noteId, imageName))
            {
                MessageBox.Show("Obraz o tej nazwie już istnieje dla notatki.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool success = DatabaseManager.AddImage(imageBlob, noteId, imageName);
            if (success)
            {
                MessageBox.Show("Obraz został pomyślnie dodany do bazy danych.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Nie udało się dodać obrazu do bazy danych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            this.Close();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(activeImageName))
            {
                MessageBox.Show("Nie można zapisać, ponieważ nie edytujesz istniejącego obrazu.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            byte[] imageBlob = ImageConverter.ConvertImageToBinary(PaintCanvas);

            if (DatabaseManager.ChangeImage(noteId, activeImageName, imageBlob))
            {
                MessageBox.Show("Obraz został pomyślnie zapisany.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Nie udało się zapisać obrazu.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveCanvasState()
        {
            var currentState = new List<UIElement>();
            foreach (UIElement element in PaintCanvas.Children)
            {
                if (element is Line line)
                {
                    currentState.Add(new Line
                    {
                        X1 = line.X1,
                        Y1 = line.Y1,
                        X2 = line.X2,
                        Y2 = line.Y2,
                        Stroke = line.Stroke,
                        StrokeThickness = line.StrokeThickness,
                        Tag = line.Tag
                    });
                }
                else if (element is FillGroup fillGroup)
                {
                    var newGroup = new FillGroup();
                    foreach (UIElement fillElement in fillGroup.Children)
                    {
                        if (fillElement is Rectangle rect)
                        {
                            var newRect = new Rectangle
                            {
                                Width = rect.Width,
                                Height = rect.Height,
                                Fill = rect.Fill,
                            };
                            Canvas.SetLeft(newRect, Canvas.GetLeft(rect));
                            Canvas.SetTop(newRect, Canvas.GetTop(rect));
                            newGroup.Children.Add(newRect);
                        }
                    }
                    Canvas.SetLeft(newGroup, Canvas.GetLeft(fillGroup));
                    Canvas.SetTop(newGroup, Canvas.GetTop(fillGroup));
                    currentState.Add(newGroup);
                }
            }
            undoStack.Push(currentState);
            UpdateUndoButtonState();
        }

        private string ShowImageSelectionDialog(List<string> imageNames)
        {
            Window dialog = new Window
            {
                Title = "Wybierz zdjęcie do edycji",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            StackPanel stackPanel = new StackPanel { Margin = new Thickness(10) };

            ComboBox comboBox = new ComboBox
            {
                ItemsSource = imageNames,
                SelectedIndex = 0,
                Margin = new Thickness(0, 0, 0, 10)
            };

            Button okButton = new Button
            {
                Content = "OK",
                Width = 80,
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            okButton.Click += (sender, e) => dialog.DialogResult = true;

            stackPanel.Children.Add(comboBox);
            stackPanel.Children.Add(okButton);
            dialog.Content = stackPanel;

            if (dialog.ShowDialog() == true)
            {
                return comboBox.SelectedItem as string;
            }
            return null;
        }

        private void sizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sizeLabel == null) return;
            strokeThickness = sizeSlider.Value;
            sizeLabel.Content = $"Rozmiar: {strokeThickness:F0}";
        }

        private void undoButton_Click(object sender, RoutedEventArgs e)
        {
            if (undoStack.Count > 0)
            {
                PaintCanvas.Children.Clear();
                var previousState = undoStack.Pop();
                foreach (var element in previousState)
                {
                    PaintCanvas.Children.Add(element);
                }
                UpdateUndoButtonState();
                UpdateBackground();
            }
        }

        private void UpdateBackground()
        {
            var existingElements = PaintCanvas.Children.OfType<UIElement>()
                .Where(x => !(x is Line && ((Line)x).Tag as string == "background"))
                .ToList();

            PaintCanvas.Children.Clear();

            switch (currentBackground)
            {
                case BackgroundType.Lines:
                    DrawLines();
                    break;
                case BackgroundType.Grid:
                    DrawGrid();
                    break;
            }

            foreach (var element in existingElements)
            {
                PaintCanvas.Children.Add(element);
            }
        }

        private void UpdateToolState()
        {
            drawButton.Background = Brushes.LightGray;
            eraseButton.Background = Brushes.LightGray;
            fillButton.Background = Brushes.LightGray;

            if (activeTool == "draw")
            {
                drawButton.Background = Brushes.LightGreen;
            }
            else if (activeTool == "erase")
            {
                eraseButton.Background = Brushes.LightGreen;
            }
            else if (activeTool == "fill")
            {
                fillButton.Background = Brushes.LightGreen;
            }
        }

        private void UpdateUndoButtonState()
        {
            undoButton.IsEnabled = undoStack.Count > 0;
        }

    }
}