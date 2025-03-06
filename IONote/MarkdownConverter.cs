using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IoNote.NoteModels
{
    class MarkdownConverter
    {
        private string toConvert = "";
        public MarkdownConverter(string input) 
        { 
            toConvert = input;
        }
        private void MakeItalic ()
        {
                string italicPattern = @"([^\\]\*)([^*]+)([^\\]\*)";
                string converted =Regex.Replace(toConvert, italicPattern, m => "<@Italic><I>" + m.Groups[2].Value + "</@It>");      
                toConvert = converted;
        }      
        private void MakeBold()
        {
            string italicPattern = @"([^\\]\*{2})([^*]+)([^\\]\*{2})";
            string converted = Regex.Replace(toConvert, italicPattern, m => "<@Bold><B>" + m.Groups[2].Value + "</@Bo>");
            toConvert = converted;
        }
        private void MakeBoldItalic()
        {
            string italicPattern = @"([^\\]\*{3,})([^*]+)([^\\]\*{3,})";
            string converted = Regex.Replace(toConvert, italicPattern, m => "<@BoldItalic><BI>" + m.Groups[2].Value + "</@BoIt>");
            toConvert = converted;
        }
        private void MakeHeader1()
        {
            string header1Pattern = @"(\r\n#{1} )([^#*\r\n]+)(\r\n)";
            string converted = Regex.Replace(toConvert, header1Pattern, m => " <@Header1><H1>" + m.Groups[2].Value + "</@H1>");
            toConvert = converted;
        }
        private void MakeHeader2()
        {
            string header1Pattern = @"(\r\n#{2} )([^#*\r\n]+)(\r\n)";
            string converted = Regex.Replace(toConvert, header1Pattern, m => " <@Header2><H2>" + m.Groups[2].Value + "</@H2>");
            toConvert = converted;
        }
        private void MakeHeader3()
        {
            string header1Pattern = @"(\r\n#{3} )([^#*\r\n]+)(\r\n)";
            string converted = Regex.Replace(toConvert, header1Pattern, m => " <@Header3><H3>" + m.Groups[2].Value + "</@H3>");
            toConvert = converted;
        }
        private void MakeHeader4()
        {
            string header1Pattern = @"(\r\n#{4} )([^#*\r\n]+)(\r\n)";
            string converted = Regex.Replace(toConvert, header1Pattern, m => " <@Header4><H4>" + m.Groups[2].Value + "</@H4>");
            toConvert = converted;
        }
        private void MakeHeader5()
        {
            string header1Pattern = @"(\r\n#{5} )([^#*\r\n]+)(\r\n)";
            string converted = Regex.Replace(toConvert, header1Pattern, m => " <@Header5><H5>" + m.Groups[2].Value + "</@H5>");
            toConvert = converted;
        }
        private void MakeHeader6()
        {
            string header1Pattern = @"(\r\n#{6} )([^#*\r\n]+)(\r\n)";
            string converted = Regex.Replace(toConvert, header1Pattern, m => " <@Header6><H6>" + m.Groups[2].Value + "</@H6>");
            toConvert = converted;
        }

        private void MakeUnorderedList()
        {
            string listElementPattern = @"(\r\n)([\+\-] [^\r\n]+)";

            string converted = Regex.Replace(toConvert, listElementPattern, m => "<@ListItem><LI>" + m.Groups[2].Value + "</@LI>");
            toConvert = converted;
        }

        public void Convert()
        {
            MakeHeader1();
            MakeHeader2();
            MakeHeader3();
            MakeHeader4();
            MakeHeader5();
            MakeHeader6();
            MakeBoldItalic();
            MakeBold();
            MakeItalic();
            MakeUnorderedList();
        }
       
        public string[] GetConverted()
        {
            Convert();
            //do testow polecam to w jednej linijce, bo wtedy ladnie koloruje
            Regex splitter = new Regex(@"(<@BoldItalic>)(.+?)(</@BoIt>)|(<@Italic>)(.+?)(</@It>)|(<@Bold>)(.+?)(</@Bo>)|(<@Header1>)(.+?)(</@H1>)" +
                @"|(<@Header2>)(.+?)(</@H2>)|(<@Header3>)(.+?)(</@H3>)|(<@Header4>)(.+?)(</@H4>)|(<@Header5>)(.+?)(</@H5>)|(<@Header6>)(.+?)(</@H6>)" +
                @"|(<@ListItem>)(.+?)(</@LI>)");
            string[] converted = splitter.Split(toConvert);
            return converted;
        }
    }
}
