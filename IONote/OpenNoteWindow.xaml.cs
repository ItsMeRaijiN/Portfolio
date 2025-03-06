using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using IoNote.AuthorizationModels;
using IoNote.AuthorizationPages;
using IoNote.DatabaseModels;
using IoNote.NoteModels;
using Microsoft.Win32;
using System.Text.Json;
//using System.Drawing;
using System.Drawing.Imaging;

namespace IoNote.NotePages
{
    /// <summary>
    /// Interaction logic for OpenNoteWindow.xaml
    /// </summary>
    public partial class OpenNoteWindow : Window
    {

        private SignedInUser User { get; set; }
        private OpenNote Note { get; set; }

        private int noteId;

        public OpenNoteWindow(SignedInUser user, note note)
        {
            this.User = user;
            this.Note = new OpenNote(note);
            this.noteId = note.noteid;
            InitializeComponent();
            if (Note.content != null)
            {
                TextRange textRange = new TextRange(EditBox.Document.ContentStart, EditBox.Document.ContentEnd);
                textRange.Text = Note.content;
            }

        }

        private void viewButton_Click(object sender = null, RoutedEventArgs e = null)
        {
            TextRange textRange = new TextRange(EditBox.Document.ContentStart, EditBox.Document.ContentEnd);
            string content = textRange.Text;

            
            PropertiesConverter paragraphConverter = new PropertiesConverter(content);
            string[] contentSplitParagraph = paragraphConverter.GetConverted();
            FlowDocument document = new FlowDocument();
            bool sizeToProcess = false;
            bool imageToProcess = false;
            foreach (string paragraphString in contentSplitParagraph)
            {
                string paragraphStringCopy = paragraphString;
                Paragraph paragraph = new Paragraph();
                if (paragraphStringCopy.StartsWith("<@S>")) 
                {
                    sizeToProcess = true;
                    continue;
                }
                else if (sizeToProcess) 
                {
                    int sizeIndex=paragraphStringCopy.IndexOf(">");
                    string sizeString = paragraphStringCopy.Substring(1, sizeIndex-1);
                    int size = int.Parse(sizeString);
                    paragraph.FontSize=size;
                    paragraphStringCopy=paragraphStringCopy.Remove(0, sizeIndex+1);
                    sizeToProcess = false;
                }
                else if(paragraphStringCopy.StartsWith("<@Im>"))
                {
                    imageToProcess= true;
                    continue;
                }
                else if(imageToProcess)
                {
                    byte[] imageBlob = DatabaseManager.GetImage(noteId, paragraphStringCopy);
                    if (imageBlob != null)
                    {
                        BitmapImage bitmap = ImageConverter.ConvertBinaryToImage(imageBlob);
                        Image newImage = new Image();
                        newImage.Width = 150;
                        newImage.Height = 150;
                        newImage.Source = bitmap;
                        BlockUIContainer buc = new BlockUIContainer(newImage);
                        document.Blocks.Add(buc);
                    }
                    else 
                    {
                        MessageBoxResult result = MessageBox.Show(
                            $"Nie znaleziono obrazu {paragraphStringCopy}. Sprawdź, czy nazwa obrazu jest prawidłow oraz czy nie został usunięty z galerii.",
                            "Błąd",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        return;
                    }
                    imageToProcess = false;
                    continue;
                }
                else if (paragraphStringCopy.StartsWith("</@>"))
                { continue; }

                MarkdownConverter converter = new MarkdownConverter(paragraphStringCopy);
                string[] contentSplit = converter.GetConverted();

                foreach (string run in contentSplit)
                {
                    int matchCase = RunStyleSetter.SetStyle(run);
                    switch (matchCase)
                    {
                        case 0:
                            paragraph.Inlines.Add(run);
                            break;

                        case 1:
                            string h1Run = run.Replace("<H1>", "\r\n");
                            h1Run += ("\r\n");
                            Inline h1Line = (new Bold(new Run(h1Run)));
                            h1Line.FontSize = 36;
                            paragraph.Inlines.Add(h1Line);
                            break;
                        case 2:
                            string h2Run = run.Replace("<H2>", "\r\n");
                            h2Run += ("\r\n");
                            Inline h2Line = (new Bold(new Run(h2Run)));
                            h2Line.FontSize = 32;
                            paragraph.Inlines.Add(h2Line);
                            break;
                        case 3:
                            string h3Run = run.Replace("<H3>", "\r\n");
                            h3Run += ("\r\n");
                            Inline h3Line = (new Bold(new Run(h3Run)));
                            h3Line.FontSize = 28;
                            paragraph.Inlines.Add(h3Line);
                            break;
                        case 4:
                            string h4Run = run.Replace("<H4>", "\r\n");
                            h4Run += ("\r\n");
                            Inline h4Line = (new Bold(new Run(h4Run)));
                            h4Line.FontSize = 24;
                            paragraph.Inlines.Add(h4Line);
                            break;
                        case 5:
                            string h5Run = run.Replace("<H5>", "\r\n");
                            h5Run += ("\r\n");
                            Inline h5Line = (new Bold(new Run(h5Run)));
                            h5Line.FontSize = 20;
                            paragraph.Inlines.Add(h5Line);
                            break;
                        case 6:
                            string h6Run = run.Replace("<H6>", "\r\n");
                            h6Run += ("\r\n");
                            Inline h6Line = (new Bold(new Run(h6Run)));
                            h6Line.FontSize = 16;
                            paragraph.Inlines.Add(h6Line);
                            break;
                        case 7:
                            string itRun = run.Replace("<I>", "");
                            paragraph.Inlines.Add(new Italic(new Run(itRun)));
                            break;
                        case 8:
                            string boRun = run.Replace("<B>", "");
                            paragraph.Inlines.Add(new Bold(new Run(boRun)));
                            break;
                        case 9:
                            string boItRun = run.Replace("<BI>", "");
                            paragraph.Inlines.Add(new Bold(new Italic(new Run(boItRun))));
                            break;
                        case 10:
                            string listItemText = run.Replace("<LI>", "\r\n");
                            listItemText = listItemText.Replace("- ", " • ").Replace("+ ", " • ");
                            paragraph.Inlines.Add(listItemText);
                            break;

                        case 100:
                            break;
                    }
                }
                if (paragraph.Inlines.Count == 0) 
                { continue; }
                document.Blocks.Add(paragraph);
                
            }
            PreviewDoc.Document = document;
        }

        private void returnButton_Click(object sender, RoutedEventArgs e)
        {
            var canSave = MessageBox.Show("czy zapisać?", "Zapisz", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (canSave == MessageBoxResult.Yes)
            {
                saveButton_Click();
            }
            
            StartWindow startWindow = new StartWindow(User);
            startWindow.Show();
            this.Close();
        }

        private void saveButton_Click(object sender = null, RoutedEventArgs e = null)
        {
            TextRange textRange = new TextRange(EditBox.Document.ContentStart, EditBox.Document.ContentEnd);
            this.Note.content = textRange.Text;
            

            bool success = DatabaseManager.ChangeContent(
               noteId: Note.noteid,
               content: this.Note.content,
               folderId : Note.folderid
           );

            if (success)
            {
                MessageBox.Show("Zapis zakończony sukcesem.",
                                "Sukces",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
               
            }
        }

        private void exportButton_Click(object sender, RoutedEventArgs e)
        {
            var canSave = MessageBox.Show("czy zapisać i eksportować?", "Zapisz", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(canSave == MessageBoxResult.No)
            {
                return;
            }
            saveButton_Click();

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Wybierz lokalizację do zapisania folderu notatki",
                FileName = Note.name,
                Filter = "Folder|*.this.directory",
                CustomPlaces = new List<FileDialogCustomPlace>()
            };

            if (dialog.ShowDialog() == true)
            {
                string selectedPath = System.IO.Path.GetDirectoryName(dialog.FileName);
                string noteFolderPath = System.IO.Path.Combine(selectedPath, Note.name);

                try
                {
                    if (Directory.Exists(noteFolderPath))
                    {
                        var result = MessageBox.Show(
                            "Folder o tej nazwie już istnieje. Czy chcesz nadpisać jego zawartość?",
                            "Folder istnieje",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning
                        );

                        if (result == MessageBoxResult.No)
                        {
                            return;
                        }
                        // Jeśli użytkownik wybrał Tak, usuwamy istniejący folder i jego zawartość
                        Directory.Delete(noteFolderPath, true);
                    }

                    Directory.CreateDirectory(noteFolderPath);

                    List<image> images = DatabaseManager.GetImagesByNoteId(noteId);
                    if (images.Count == 0)
                    {
                        MessageBox.Show("Brak zdjęć do eksportu.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var noteJsonString = ImportExportHandler.SerializeNoteToJSON(Note, images);
                    string jsonFilePath = System.IO.Path.Combine(noteFolderPath, $"{Note.name}.json");
                    bool isJsonWritten = ImportExportHandler.WriteJSONFile(jsonFilePath, noteJsonString);

                    if (!isJsonWritten)
                    {
                        MessageBox.Show("Nie udało się przekonwertować pliku notatki na plik JSON.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    foreach (var image in images)
                    {
                        string imagePath = System.IO.Path.Combine(noteFolderPath, $"{image.name}.png");
                        File.WriteAllBytes(imagePath, image.data);
                    }

                    TextRange textRange = new TextRange(EditBox.Document.ContentStart, EditBox.Document.ContentEnd);
                    string notePath = System.IO.Path.Combine(noteFolderPath, $"{Note.name}.md");
                    File.WriteAllText(notePath, textRange.Text);

                    MessageBox.Show("Notatka oraz zdjęcia zostały poprawnie eksportowane.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Wystąpił błąd podczas eksportu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void drawButton_Click(object sender, RoutedEventArgs e)
        {
            PaintWindow paintWindow = new PaintWindow(Note.noteid);
            paintWindow.Show();
        }

        private void addImageDesktop_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files(*.BMP;*.JPG;*PNG)|*.BMP;*.JPG;*.PNG|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                Uri fileUri = new Uri(openFileDialog.FileName);
                Image newImage = new Image();
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.UriSource = fileUri;
                bi.EndInit();
                newImage.Source = bi;

                byte[] imageBlob = ImageConverter.ConvertImageExternalToBinary(bi);
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

                if (DatabaseManager.CheckImage(noteId, imageName)) // Sprawdzanie, czy obraz o takiej nazwie już istnieje
                {
                    MessageBox.Show("Obraz o tej nazwie już istnieje dla notatki.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Dodanie obrazu do bazy danych
                bool success = DatabaseManager.AddImage(imageBlob, noteId, imageName);
                if (success)
                {
                    MessageBox.Show("Obraz został pomyślnie dodany do bazy danych.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);

                }
                else
                {
                    MessageBox.Show("Nie udało się dodać obrazu do bazy danych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                string toEditBox = $"\r\n ![{imageName}](DB)";
                EditBox.AppendText(toEditBox);
            }
        }

        private void addImageDB_Click(object sender, RoutedEventArgs e)
        {
            // Pobierz nazwy zdjęć przypisanych do bieżącej notatki
            List<string> imageNames = DatabaseManager.GetImagesName(noteId);

            if (imageNames == null || !imageNames.Any())
            {
                MessageBox.Show("Brak dostępnych zdjęć do edycji.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Wyświetl okno dialogowe z listą nazw zdjęć
            string selectedImageName = ShowImageSelectionDialog(imageNames, "Wybierz obraz do wstawienia");

            if (string.IsNullOrWhiteSpace(selectedImageName))
            {
                MessageBox.Show("Nie wybrano żadnego obrazu.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            string toEditBox = $"\r\n ![{selectedImageName}](DB)";
            EditBox.AppendText(toEditBox);

        }

        private string ShowImageSelectionDialog(List<string> imageNames, string title)
        {
            // Tworzenie okna dialogowego
            Window dialog = new Window
            {
                Title = title,
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            StackPanel stackPanel = new StackPanel { Margin = new Thickness(10) };

            // Tworzenie ComboBox
            ComboBox comboBox = new ComboBox
            {
                ItemsSource = imageNames,
                SelectedIndex = 0,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Tworzenie przycisku OK
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

            // Wyświetlenie okna dialogowego
            if (dialog.ShowDialog() == true)
            {
                return comboBox.SelectedItem as string;
            }
            return null; // Jeśli użytkownik zamknie okno bez wyboru
        }

        private void DeleteImageDB_Click(object sender, RoutedEventArgs e)
        {
            List<string> imageNames = DatabaseManager.GetImagesName(noteId);

            if (imageNames == null || !imageNames.Any())
            {
                MessageBox.Show("Brak dostępnych zdjęć do edycji.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Wyświetl okno dialogowe z listą nazw zdjęć
            string selectedImageName = ShowImageSelectionDialog(imageNames, "Wybierz obraz do usunięcia");

            if (string.IsNullOrWhiteSpace(selectedImageName))
            {
                MessageBox.Show("Nie wybrano żadnego obrazu.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            bool success = DatabaseManager.DeleteImage(noteId, selectedImageName);
            if (success)
            {
                MessageBox.Show("Obraz został pomyślnie usunięty z bazy danych.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                DeleteImageFromEditBox(selectedImageName);
            }
            else
            {
                MessageBox.Show("Nie udało się usunąć obrazu z bazy danych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteImageFromEditBox(string imageName)
        {
            string pattern = $@"!\[{imageName}\]\(DB\)";
            TextRange textRange = new TextRange(EditBox.Document.ContentStart, EditBox.Document.ContentEnd);
            string content = textRange.Text;

            // Remove all matches of the pattern
            string updatedContent = System.Text.RegularExpressions.Regex.Replace(content, pattern, string.Empty);

            // Update the EditBox content
            EditBox.Document.Blocks.Clear();
            EditBox.Document.Blocks.Add(new Paragraph(new Run(updatedContent)));

            viewButton_Click();
        }
    }
}
