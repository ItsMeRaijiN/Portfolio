using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoNote.DatabaseModels;

namespace IoNote.NoteModels
{
    public class OpenNote : note
    {
        public OpenNote(note note) 
        {
            this.noteid= note.noteid;
            this.name= note.name; 
            this.content= note.content;
            this.background= note.background;
            this.folderid= note.folderid;
        }
    }
}
