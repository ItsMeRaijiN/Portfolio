namespace IoNote.AuthorizationModels
{
    class SignInModel : IAuthorizationValidator
    {
        public bool IsPasswordSame(string password1, string password2) => throw new NotImplementedException();
        public bool IsCredentialsNoneEmpty(string username, string password)
            => !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
        public bool IsPasswordValid(string password) => throw new NotImplementedException();
    }
}
