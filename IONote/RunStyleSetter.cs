using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoNote.NoteModels
{
    internal class RunStyleSetter
    {
        public static int SetStyle(string input)
        {
            if (input.StartsWith(@"<@") | input.StartsWith("</@")) { return 100; }
            else if (input.StartsWith("<LI>")) { return 10; }
            else if (input.StartsWith("<BI>")) { return 9; }
            else if (input.StartsWith("<B>")) { return 8; }
            else if (input.StartsWith("<I>")) { return 7; }
            else if (input.StartsWith("<H6>")) { return 6; }
            else if (input.StartsWith("<H5>")) { return 5; }
            else if (input.StartsWith("<H4>")) { return 4; }
            else if (input.StartsWith("<H3>")) { return 3; }
            else if (input.StartsWith("<H2>")) { return 2; }
            else if (input.StartsWith("<H1>")) { return 1; }
            return 0;
        }
    }
}
