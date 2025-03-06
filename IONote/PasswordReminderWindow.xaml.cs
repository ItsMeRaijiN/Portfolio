using System;
using System.Windows;
using System.Windows.Controls;
using IoNote.AuthorizationModels;

namespace IoNote.AuthorizationPages
{
    /// <summary>
    /// Interaction logic for PasswordReminderWindow.xaml
    /// </summary>
    public partial class PasswordReminderWindow : Window
    {
        // Pole prywatne dla PasswordReminderModel
        private readonly PasswordReminderModel _passwordReminderModel;

        public PasswordReminderWindow()
        {
            InitializeComponent();

            // Inicjalizacja obiektu PasswordReminderModel
            _passwordReminderModel = new PasswordReminderModel();

            UsernameLabel.Visibility = Visibility.Visible;
            UsernameTextBox.Visibility = Visibility.Visible;
            RemindPasswordButton.Visibility = Visibility.Visible;

            RemindPasswordLabel.Visibility = Visibility.Hidden;
            HelperQuestionLabel.Visibility = Visibility.Hidden;
            RemindPasswordQuestionLabel.Visibility = Visibility.Hidden;
            AnswerTextBox.Visibility = Visibility.Hidden;
            VerifyButton.Visibility = Visibility.Hidden;
            PasswordLabel.Visibility = Visibility.Hidden;
            PasswordBox.Visibility = Visibility.Hidden;
            PasswordTextBox.Visibility = Visibility.Hidden;
            ShowPasswordCheckBox.Visibility = Visibility.Hidden;
            RepeatPasswordLabel.Visibility = Visibility.Hidden;
            RepeatPasswordBox.Visibility = Visibility.Hidden;
            RepeatPasswordTextBox.Visibility = Visibility.Hidden;
            ShowRepeatPasswordCheckBox.Visibility = Visibility.Hidden;
            BackButton.Visibility = Visibility.Hidden;
            ChangePasswordButton.Visibility = Visibility.Hidden;
        }

        private void RemindPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            bool isCredNoneEmpty = _passwordReminderModel.IsUsernameNoneEmpty(username: UsernameTextBox.Text);
            bool isUserValid = DatabaseManager.CheckUser(username: UsernameTextBox.Text);

            if (isCredNoneEmpty && isUserValid)
            {
                RemindPasswordLabel.Visibility = Visibility.Visible;
                HelperQuestionLabel.Visibility = Visibility.Visible;
                RemindPasswordQuestionLabel.Visibility = Visibility.Visible;
                AnswerTextBox.Visibility = Visibility.Visible;
                VerifyButton.Visibility = Visibility.Visible;

                HelperQuestionLabel.Content = DatabaseManager.GetPasswordReminderQuestion(username: UsernameTextBox.Text);
            }
            else
            {
                MessageBox.Show("Nie ma takiego użytkownika w bazie.",
                                "Błąd",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            bool isAnswerNoneEmpty = _passwordReminderModel.IsAnswerNoneEmpty(answer: AnswerTextBox.Text);
            bool isAnswerValid = DatabaseManager.IsAnswerValid(username: UsernameTextBox.Text, answer: AnswerTextBox.Text);

            if (isAnswerNoneEmpty && isAnswerValid)
            {
                PasswordLabel.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Visible;
                ShowPasswordCheckBox.Visibility = Visibility.Visible;
                RepeatPasswordLabel.Visibility = Visibility.Visible;
                RepeatPasswordBox.Visibility = Visibility.Visible;
                ShowRepeatPasswordCheckBox.Visibility = Visibility.Visible;
                BackButton.Visibility = Visibility.Visible;
                ChangePasswordButton.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Odpowiedź nieprawidłowa.",
                                "Błąd",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void ShowPasswordCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                if (checkBox.Name == "ShowPasswordCheckBox")
                {
                    PasswordBox.Visibility = Visibility.Collapsed;
                    PasswordTextBox.Visibility = Visibility.Visible;
                    PasswordTextBox.Text = PasswordBox.Password;
                }
                else if (checkBox.Name == "ShowRepeatPasswordCheckBox")
                {
                    RepeatPasswordBox.Visibility = Visibility.Collapsed;
                    RepeatPasswordTextBox.Visibility = Visibility.Visible;
                    RepeatPasswordTextBox.Text = RepeatPasswordBox.Password;
                }
            }
        }

        private void ShowPasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                if (checkBox.Name == "ShowPasswordCheckBox")
                {
                    PasswordBox.Visibility = Visibility.Visible;
                    PasswordTextBox.Visibility = Visibility.Collapsed;
                    PasswordBox.Password = PasswordTextBox.Text;
                }
                else if (checkBox.Name == "ShowRepeatPasswordCheckBox")
                {
                    RepeatPasswordBox.Visibility = Visibility.Visible;
                    RepeatPasswordTextBox.Visibility = Visibility.Collapsed;
                    RepeatPasswordBox.Password = RepeatPasswordTextBox.Text;
                }
            }
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            bool isPasswordSame = _passwordReminderModel.IsPasswordSame(password1: PasswordBox.Password, password2: RepeatPasswordBox.Password);
            bool isPasswordValid = _passwordReminderModel.IsPasswordValid(password: PasswordBox.Password);

            if (isPasswordValid && isPasswordSame)
            {
                DatabaseManager.ChangePassword(username: UsernameTextBox.Text, password: PasswordBox.Password);
                MessageBox.Show("Hasło zostało zmienione.",
                                "Sukces",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                SignInWindow signInWindow = new SignInWindow();
                signInWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Hasło nie spełnia wymagań.",
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
    }
}
