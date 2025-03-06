using System.Text.RegularExpressions;

namespace IoNote.AuthorizationModels
{
    class PasswordReminderModel : IAuthorizationValidator
    {
        public bool IsPasswordSame(string password1, string password2) => password1 == password2;

        public bool IsCredentialsNoneEmpty(string username, string password)
            => !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);

        public bool IsPasswordValid(string password)
        {
            string pattern = @"^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
            return Regex.IsMatch(password, pattern);
        }

        public bool IsUsernameNoneEmpty(string username) => !string.IsNullOrEmpty(username);

        public bool IsAnswerNoneEmpty(string answer) => !string.IsNullOrEmpty(answer);
    }
}
