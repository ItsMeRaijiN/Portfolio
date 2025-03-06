using IoNote.AuthorizationModels;
using IoNote.AuthorizationPages;
using IoNote.DatabaseModels;
using IoNote.NoteModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace IoNote.NotePages
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public class CollectionItem
    {
        public string Emoji { get; set; } = string.Empty;
        public string? Name { get; set; } = string.Empty; // Wspólna nazwa  
        public bool IsFolder { get; set; } // True, jeśli to folder
        public bool IsBack { get; set; } = false; // True jeśli folder powrotu
        public folder? Folder { get; set; } // Obiekt folderu (jeśli folder)
        public note? Note { get; set; } // Obiekt notatki (jeśli notatka)
    }

    public partial class StartWindow : Window
    {
        private SignedInUser User { get; set; }

        private folder? CurrentFolder { get; set; }

        private ionoteContext dbContext = new();

        public ObservableCollection<CollectionItem> CollectionItems { get; set; } = new();

        private note movedNote = null;
        private CollectionItem cutItem = null; // Nowe pole do śledzenia wyciętego elementu

        public StartWindow(SignedInUser user)
        {
            this.User = user;
            InitializeComponent();
            usernameLabel.Text += user.username + "😊";

            FoldersListBox.ItemsSource = CollectionItems;
            CurrentFolder = null;
            LoadUserCollection(null);
        }

        private void LoadUserCollection(int? parentFolderID = null)
        {
            try
            {
                // Wyczyść listę
                CollectionItems.Clear();

                // Pobierz foldery użytkownika
                var userFolders = dbContext.folders
                    .Where(f => f.userid == User.userid && f.parentfolderid == parentFolderID)
                    .OrderBy(f => f.name)
                    .ToList();

                // Pobierz folder nadrzędny (jeśli istnieje)
                var parentFolder = dbContext.folders.Find(parentFolderID);
                if (parentFolderID == null)
                {
                    // Jeśli to jest folder Root (brak parentFolderID)
                    CurrentFolder = null;
                }
                else
                {
                    CollectionItems.Add(new CollectionItem
                    {
                        Emoji = "📂 ⬆️",
                        Name = null,
                        IsFolder = true,
                        Note = null,
                        Folder = parentFolder,
                        IsBack = true
                    }) ;
                    CurrentFolder = parentFolder;
                }

                // Dodaj wszystkie foldery użytkownika
                foreach (var folder in userFolders)
                {
                    CollectionItems.Add(new CollectionItem
                    {
                        Emoji = "📂",
                        Name = folder.name,
                        IsFolder = true,
                        Note = null,
                        Folder = folder,
                        IsBack = false
                    });
                }

                // Pobierz notatki użytkownika
                var userNotes = dbContext.notes
                    .Where(n => n.folderid == parentFolderID)
                    .OrderBy(n => n.name)
                    .ToList();

                foreach (var note in userNotes)
                {
                    CollectionItems.Add(new CollectionItem
                    {
                        Emoji = "📝",
                        Name = note.name,
                        IsFolder = false,
                        Folder = null,
                        Note = note,
                        IsBack = false
                    });
                }

                // Ustaw aktualną nazwę folderu
                CurrentFolderLabel.Text = parentFolderID == null ? "Root" : parentFolder?.name;
            }
            catch (Exception)
            {
                MessageBox.Show("Wystąpił błąd podczas odczytu kolekcji.",
                                "Błąd",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void FoldersListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Pobierz element, na który kliknięto
            var item = ItemsControl.ContainerFromElement(FoldersListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {
                item.IsSelected = true; // Ręczne zaznaczenie elementu
            }
        }

        private void FoldersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FoldersListBox.SelectedItem is CollectionItem selectedItem)
            {
                if (selectedItem.IsFolder)
                {

                    // Jeśli kliknięto folder nadrzędny (strzałka w górę)
                    if (selectedItem.IsBack)
                    {
                        // Przejdź do folderu nadrzędnego
                        LoadUserCollection(selectedItem.Folder?.parentfolderid);
                        FoldersListBox.SelectedItem = null;
                    }
                    else
                    {
                        // Jeśli kliknięto normalny folder, załaduj jego zawartość
                        LoadUserCollection(selectedItem.Folder?.folderid);
                        FoldersListBox.SelectedItem = null;

                    }
                }
                else
                {
                    // Jeśli kliknięto notatkę, otwórz ją w nowym oknie
                    OpenNoteWindow openNoteWindow = new OpenNoteWindow(User, selectedItem.Note);
                    openNoteWindow.Show();
                    this.Close();
                }
            }
        }

        private void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            ResetCutState();
            // Wyświetlenie okna dialogowego z potwierdzeniem usunięcia konta
            var result = MessageBox.Show("Czy na pewno chcesz usunąć swoje konto? Tej operacji nie można cofnąć.",
                                         "Potwierdzenie usunięcia konta",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // Próba usunięcia użytkownika
                bool isDeleted = DatabaseManager.DeleteUser(username: User.username);

                if (isDeleted)
                {
                    MessageBox.Show("Twoje konto zostało usunięte.",
                                    "Sukces",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    // Wylogowanie użytkownika i powrót do ekranu logowania
                    SignedInUser signedInUser = new SignedInUser();
                    DatabaseManager.LogOutUser(ref signedInUser);

                    SignInWindow signInWindow = new SignInWindow();
                    signInWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Wystąpił błąd podczas usuwania konta. Spróbuj ponownie.",
                                    "Błąd",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            ResetCutState();
            // Tworzenie zmiennej lokalnej dla użytkownika
            var user = this.User;

            // Wylogowanie użytkownika
            bool isLoggedOut = DatabaseManager.LogOutUser(ref user);

            if (isLoggedOut)
            {
                MessageBox.Show("Zostałeś wylogowany.",
                                "Wylogowanie",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                // Aktualizacja User
                this.User = user;

                // Przejście do okna logowania
                SignInWindow signInWindow = new SignInWindow();
                signInWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Wystąpił błąd podczas wylogowywania. Spróbuj ponownie.",
                                "Błąd",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            ResetCutState();
            // Pobierz nowe hasło od użytkownika
            string newPassword = Microsoft.VisualBasic.Interaction.InputBox(
                "Wprowadź nowe hasło:",
                "Zmiana hasła",
                "");

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show("Hasło nie może być puste.",
                                "Błąd",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            // Próba zmiany hasła w bazie danych
            bool isPasswordChanged = DatabaseManager.ChangePassword(User.username, newPassword);

            if (isPasswordChanged)
            {
                MessageBox.Show("Hasło zostało zmienione pomyślnie.",
                                "Sukces",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Wystąpił błąd podczas zmiany hasła. Spróbuj ponownie.",
                                "Błąd",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void AddFolderButton_Click(object sender, RoutedEventArgs e)
        {
            ResetCutState();
            // Pobierz nazwę folderu od użytkownika
            string folderName = Microsoft.VisualBasic.Interaction.InputBox(
                "Wprowadź nazwę folderu:",
                "Dodawanie folderu",
                "");

            // Sprawdź, czy nazwa folderu nie jest pusta
            if (string.IsNullOrWhiteSpace(folderName))
            {
                MessageBox.Show("Nazwa folderu nie może być pusta.",
                                "Błąd",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            // Próba dodania folderu do bazy danych
            bool isAdded = DatabaseManager.AddFolder(folderName, User.userid, CurrentFolder?.folderid);

            if (isAdded)
            {
                MessageBox.Show("Folder został dodany pomyślnie.",
                                "Sukces",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                // Odśwież listę folderów
                LoadUserCollection(CurrentFolder?.folderid);
            }
            else
            {
                MessageBox.Show("Wystąpił błąd podczas dodawania folderu. Spróbuj ponownie.",
                                "Błąd",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void AddNoteButton_Click(object sender, RoutedEventArgs e)
        {
            ResetCutState();
            // Pobierz tytuł i treść notatki od użytkownika
            string noteName = Microsoft.VisualBasic.Interaction.InputBox(
                "Wprowadź tytuł notatki:",
                "Dodawanie notatki",
                "");

            if (string.IsNullOrWhiteSpace(noteName))
            {
                MessageBox.Show("Tytuł notatki nie może być pusty.",
                                "Błąd",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            bool isAdded = DatabaseManager.AddNote(noteName, CurrentFolder?.folderid);

            if (isAdded)
            {
                MessageBox.Show("Notatka została dodany pomyślnie.",
                                "Sukces",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                // Odśwież listę
                LoadUserCollection(CurrentFolder?.folderid);
            }
            else
            {
                MessageBox.Show("Wystąpił błąd podczas dodawania notatki. Spróbuj ponownie.",
                                "Błąd",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void DeleteNoteButton_Click(object sender, RoutedEventArgs e)
        {
            ResetCutState();
            List<string?> noteNames = CollectionItems
                .Where(item => item.IsFolder == false)
                .Select(item => item.Name)
                .ToList();

            if (noteNames.Count == 0)
                return;

            string noteName = ShowSelectionDialog(noteNames, "Wybierz notatkę do usunięcia");

            if (string.IsNullOrWhiteSpace(noteName))
            {
                MessageBox.Show("Nie wybrano żadnej notatki.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int? noteId = CollectionItems
                .Where(item => item.Name == noteName)
                .Select(item => item.Note?.noteid)
                .FirstOrDefault();

            DeleteNote(noteId.Value);
        }

        private void DeleteFolderButton_Click(object sender, RoutedEventArgs e)
        {
            ResetCutState();
            List<string?> folderNames = CollectionItems
                .Where(item => item.IsFolder == true)
                .Select(item => item.Name)
                .ToList();

            if (folderNames.Count == 0)
                return;

            string folderName = ShowSelectionDialog(folderNames, "Wybierz folder do usunięcia");

            if (string.IsNullOrWhiteSpace(folderName))
            {
                MessageBox.Show("Nie wybrano żadnego folderu.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int? folderId = CollectionItems
                .Where(item => item.Name == folderName)
                .Select(item => item.Folder?.folderid)
                .FirstOrDefault();

            DeleteFolder(folderId.Value);
        }

        private bool DeleteNote(int noteId)
        {

            bool success = DatabaseManager.DeleteNote(noteId);
            if (success)
            {
                MessageBox.Show("Notatka została usunięta.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUserCollection(CurrentFolder?.folderid);
            }
            else
            {
                MessageBox.Show("Wystąpił błąd podczas usuwania notatki. Spróbuj ponownie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return success;
        }

        private bool DeleteFolder(int folderId)
        {
            bool success = DatabaseManager.DeleteFolder(folderId);
            if (success)
            {
                MessageBox.Show("Folder wraz z zawartością zostały usunięte.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUserCollection(CurrentFolder?.folderid);
            }
            else
            {
                MessageBox.Show("Wystąpił błąd podczas usuwania folderu. Spróbuj ponownie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return success;
        }

        private bool ImportNote(int? folderId)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                FilterIndex = 1,
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    MessageBox.Show("Nie wybrano pliku.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }

                bool success = DatabaseManager.ImportNoteFromJSON(filePath, User.userid, folderId);

                if (success)
                {
                    MessageBox.Show("Notatka została zaimportowana.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadUserCollection(CurrentFolder?.folderid);
                }
                else
                {
                    MessageBox.Show("Wystąpił błąd podczas importowania notatki. Spróbuj ponownie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return success;
            }
            return false;
        }

        private void ImportNoteButton_Click(object sender, RoutedEventArgs e)
        {
            ImportNote(CurrentFolder?.folderid);
        }

        private string ShowSelectionDialog(List<string> contentList, string title)
        {
            if (contentList.Count == 0)
            {
                MessageBox.Show("Brak elementów do wyboru.",
                                "Błąd",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return null;
            }
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
                ItemsSource = contentList,
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

        private void FoldersListBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(FoldersListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {
                var selectedItem = item.Content as CollectionItem;

                // Tworzymy nowe menu kontekstowe
                ContextMenu contextMenu = new ContextMenu();

                // Dodajemy opcje menu
                MenuItem openMenuItem = new MenuItem() { Header = "Otwórz" };
                openMenuItem.Click += (s, args) =>
                {
                    item.IsSelected = true;
                };

                MenuItem deleteMenuItem = new MenuItem() { Header = "Usuń" };
                deleteMenuItem.Click += (s, args) =>
                {
                    if (selectedItem.IsFolder == false && selectedItem.Note?.noteid != null)
                    {
                        DeleteNote(selectedItem.Note.noteid);
                    }
                    else if (selectedItem.IsFolder && selectedItem.Folder?.folderid != null)
                    {
                        DeleteFolder(selectedItem.Folder.folderid);
                    }
                };
                MenuItem importMenuItem = new MenuItem() { Header = "Importuj tutaj" };
                importMenuItem.Click += (s, args) => 
                {
                    if (selectedItem.IsFolder && selectedItem.Folder?.folderid != null)
                    {
                        ImportNote(selectedItem.Folder?.folderid);
                    }
                    else if (selectedItem.IsFolder == false && selectedItem.Note?.noteid != null)
                    {
                        MessageBox.Show("Nie można zaimportować notatki. Wybierz folder.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        ImportNote(CurrentFolder?.folderid);
                    }
                };

                MenuItem cutMenuItem = new MenuItem() { Header = "Wytnij" };
                cutMenuItem.Click += (s, args) =>
                {
                    if (selectedItem.IsFolder == false && selectedItem.Note?.noteid != null)
                    {
                        ResetCutState();
                        movedNote = selectedItem.Note;
                        // Wizualne oznaczenie wyciętej notatki (opcjonalne)
                        var item = selectedItem as CollectionItem;
                        if (item != null)
                        {
                            item.Emoji = "✂️";
                            FoldersListBox.Items.Refresh();
                        }
                    }
                    else if (selectedItem.IsFolder && selectedItem.Folder?.folderid != null)
                    {
                        ResetCutState();
                        MessageBox.Show("Nie można wyciąć folderu.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };

                // Dodajemy utworzone elementy do menu
                contextMenu.Items.Add(openMenuItem);
                contextMenu.Items.Add(deleteMenuItem);
                contextMenu.Items.Add(importMenuItem);
                contextMenu.Items.Add(cutMenuItem);
               

                // Pokazujemy menu w miejscu kliknięcia
                contextMenu.PlacementTarget = FoldersListBox;
                contextMenu.IsOpen = true;

                e.Handled = true;
            }
        }

        private void PasteNoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (movedNote != null)
            {
                // Najpierw zmieniamy folder notatki
                if (DatabaseManager.ChangeNoteFolder(movedNote.noteid, CurrentFolder?.folderid))
                {
                    // Odświeżamy widok
                    LoadUserCollection(CurrentFolder?.folderid);
                    ResetCutState();
                }
                else
                {
                    MessageBox.Show("Wystąpił błąd podczas przenoszenia notatki.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Brak notatki do wklejenia.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetCutState()
        {
            if (cutItem != null)
            {
                cutItem.Emoji = "📝"; // Przywróć oryginalną ikonkę
                FoldersListBox.Items.Refresh();
            }
            movedNote = null;
            cutItem = null;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            ResetCutState();
            base.OnClosing(e);
        }
    }

    
}
