using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphy.CsvStream
{
    public class CsvConfig
    {
        public char Delimiter { get; private set; }
        public string NewLineMark { get; private set; }
        public char QuotationMark { get; private set; }
        public char CommentMark { get; private set; }

        public CsvConfig(char delimiter, string newLineMark, char quotationMark, char commentMark)
        {
            Delimiter = delimiter;
            NewLineMark = newLineMark;
            QuotationMark = quotationMark;
            CommentMark = commentMark;
        }


        public static CsvConfig Default
        {
            get { return new CsvConfig(';', "\r\n", '\"', '#'); }
        }

    }
}
