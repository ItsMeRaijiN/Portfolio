using System.Text.RegularExpressions;

namespace IoNote.AuthorizationModels
{
    class SignUpModel : IAuthorizationValidator
    {
        public bool IsPasswordSame(string password1, string password2) => password1 == password2;

        public bool IsCredentialsNoneEmpty(string username, string password, string question, string answer)
            => !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) &&
               !string.IsNullOrEmpty(question) && !string.IsNullOrEmpty(answer);

        public bool IsCredentialsNoneEmpty(string username, string password)
            => !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);

        public bool IsPasswordValid(string password)
        {
            string pattern = @"^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
            return Regex.IsMatch(password, pattern);
        }
    }
}
