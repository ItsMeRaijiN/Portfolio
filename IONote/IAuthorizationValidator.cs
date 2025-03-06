using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoNote.AuthorizationModels
{
    public interface IAuthorizationValidator
    {
        bool IsPasswordSame(string password1, string password2);
        bool IsCredentialsNoneEmpty(string username, string password);
        bool IsPasswordValid(string password);
    }

}
