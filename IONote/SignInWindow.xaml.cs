using System.Windows;
using System.Windows.Controls;
using IoNote.AuthorizationModels;
using IoNote.NoteModels;
using IoNote.NotePages;

namespace IoNote.AuthorizationPages
{
    public partial class SignInWindow : Window
    {
        // Prywatne pole dla SignInModel
        private readonly SignInModel _signInModel;

        public SignInWindow()
        {
            InitializeComponent();

            // Inicjalizacja obiektu SignInModel
            _signInModel = new SignInModel();

            ErrorLabel.Visibility = Visibility.Hidden;
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            SignUpWindow signUpWindow = new SignUpWindow();
            signUpWindow.Show();
            this.Close();
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            string password = PasswordBox.Visibility == Visibility.Visible
                ? PasswordBox.Password
                : PasswordTextBox.Text;

            // Użycie obiektu _signInModel
            bool isCredNoneEmpty = _signInModel.IsCredentialsNoneEmpty(username: UsernameTextBox.Text, password: password);
            bool isUserValid = DatabaseManager.CheckUser(username: UsernameTextBox.Text, password: password);

            if (isCredNoneEmpty && isUserValid)
            {
                SignedInUser User = new SignedInUser();

                DatabaseManager.LogInUser(username: UsernameTextBox.Text, user:  ref User);
                StartWindow startWindow = new StartWindow(User);
                startWindow.Show();
                this.Close();
            }
            else
            {
                ErrorLabel.Visibility = Visibility.Visible;
            }
        }

        private void RemindPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            PasswordReminderWindow passwordReminderWindow = new PasswordReminderWindow();
            passwordReminderWindow.Show();
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
    }
}
