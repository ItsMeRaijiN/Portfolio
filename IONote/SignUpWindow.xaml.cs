using System;
using System.Windows;
using IoNote.AuthorizationModels;

namespace IoNote.AuthorizationPages
{
    public partial class SignUpWindow : Window
    {
        // Prywatne pole dla SignUpModel
        private readonly SignUpModel _signUpModel;

        public SignUpWindow()
        {
            InitializeComponent();

            // Inicjalizacja obiektu SignUpModel
            _signUpModel = new SignUpModel();
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            string password = PasswordBox.Visibility == Visibility.Visible
                ? PasswordBox.Password
                : PasswordTextBox.Text;

            string repeatPassword = RepeatPasswordBox.Visibility == Visibility.Visible
                ? RepeatPasswordBox.Password
                : RepeatPasswordTextBox.Text;

            // Sprawdzenie czy hasło spełnia wymagania
            if (!_signUpModel.IsPasswordValid(password))
            {
                MessageBox.Show("Hasło musi mieć minimum 8 znaków, zawierać co najmniej 1 dużą literę, 1 cyfrę oraz 1 znak specjalny.",
                                "Błąd walidacji hasła",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            // Sprawdzenie czy hasła są takie same
            if (!_signUpModel.IsPasswordSame(password, repeatPassword))
            {
                MessageBox.Show("Hasła nie pasują do siebie.",
                                "Błąd",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            // Sprawdzenie czy dane są poprawne
            if (!_signUpModel.IsCredentialsNoneEmpty(UsernameTextBox.Text, password, PasswordReminderQuestionTextBox.Text, PasswordReminderAnswerTextBox.Text))
            {
                MessageBox.Show("Wszystkie pola muszą być wypełnione.",
                                "Błąd",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            // Próba dodania użytkownika do bazy
            bool success = DatabaseManager.AddUser(
                username: UsernameTextBox.Text,
                password: password,
                question: PasswordReminderQuestionTextBox.Text,
                answer: PasswordReminderAnswerTextBox.Text
            );

            if (success)
            {
                MessageBox.Show("Rejestracja zakończona sukcesem.",
                                "Sukces",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                SignInWindow signInWindow = new SignInWindow();
                signInWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Nie udało się zarejestrować użytkownika.",
                                "Błąd",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            SignInWindow signInWindow = new SignInWindow();
            signInWindow.Show();
            this.Close();
        }

        private void ShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            PasswordBox.Visibility = Visibility.Collapsed;
            PasswordTextBox.Visibility = Visibility.Visible;
            PasswordTextBox.Text = PasswordBox.Password;
            PasswordTextBox.Focus();
        }

        private void ShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordTextBox.Visibility = Visibility.Collapsed;
            PasswordBox.Visibility = Visibility.Visible;
            PasswordBox.Password = PasswordTextBox.Text;
            PasswordBox.Focus();
        }

        private void ShowRepeatPassword_Checked(object sender, RoutedEventArgs e)
        {
            RepeatPasswordBox.Visibility = Visibility.Collapsed;
            RepeatPasswordTextBox.Visibility = Visibility.Visible;
            RepeatPasswordTextBox.Text = RepeatPasswordBox.Password;
            RepeatPasswordTextBox.Focus();
        }

        private void ShowRepeatPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            RepeatPasswordTextBox.Visibility = Visibility.Collapsed;
            RepeatPasswordBox.Visibility = Visibility.Visible;
            RepeatPasswordBox.Password = RepeatPasswordTextBox.Text;
            RepeatPasswordBox.Focus();
        }
    }
}
