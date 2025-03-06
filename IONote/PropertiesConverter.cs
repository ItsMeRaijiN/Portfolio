using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IoNote.NoteModels
{
    internal class PropertiesConverter
    {
        private string toConvert = "";

        public PropertiesConverter(string input)
        {
            toConvert = input;
        }
        private void MakeSize()
        {
            string sizePattern = @"(##)(\d{1,2})([^#]+)(##)";
            string converted = Regex.Replace(toConvert, sizePattern, m => "<@S><" + m.Groups[2].Value+">" + m.Groups[3].Value +"</@S>");
            toConvert = converted;
        }
        private void MakeImage()
        {
            string imagePattern = @"(!)(\[)(.+?)(\])(\(DB\))";
            string converted = Regex.Replace(toConvert, imagePattern, m => " \r\n<@Im>" + m.Groups[3].Value + "</@Im>\r\n");
            toConvert = converted;
        }
        public void Convert()
        {
            MakeSize();
            MakeImage();
        }
        public string[] GetConverted()
        {
            Convert();
            //do testow polecam to w jednej linijce, bo wtedy ladnie koloruje
            Regex splitter = new Regex(@"(<@S>)(.+?)(</@S>)|(<@Im>)(.+?)(</@Im>)");
            string[] converted = splitter.Split(toConvert);
            return converted;
        }
    }
}
