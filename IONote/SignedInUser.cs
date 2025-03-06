using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoNote.DatabaseModels;

namespace IoNote.NoteModels
{
    public class SignedInUser : user
    {
        public SignedInUser(user user)
        {
            this.userid = user.userid;
            this.username = user.username;
            this.password = user.password;
            this.question = user.question;
            this.answer = user.answer;
        }

        public SignedInUser() 
        {
            this.userid = 0;
            this.username = string.Empty;
            this.password = string.Empty;
            this.question = string.Empty;
            this.answer = string.Empty;
        }
    }
}
