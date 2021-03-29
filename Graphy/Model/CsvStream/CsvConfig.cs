using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Graphy.CsvStream
{
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class CsvConfig : ObservableObject
    {
        public CsvConfig()
        {

        }

        public CsvConfig(char delimiter, string newLineMark, char quotationMark, char commentMark)
        {
            Delimiter = delimiter;
            NewLineMark = newLineMark;
            QuotationMark = quotationMark;
            CommentMark = commentMark;
        }

        private char _delimiter;
        private string _newLineMark;
        private char _quotationMark;
        private char _commentMark;

        public char Delimiter
        {
            get => _delimiter;
            set
            {
                Set(() => Delimiter, ref _delimiter, value);
            }
        }

        public string NewLineMark
        {
            get => _newLineMark;
            set
            {
                Set(() => NewLineMark, ref _newLineMark, value);
            }
        }

        public char QuotationMark
        {
            get => _quotationMark;
            set
            {
                Set(() => QuotationMark, ref _quotationMark, value);
            }
        }

        public char CommentMark
        {
            get => _commentMark;
            set
            {
                Set(() => CommentMark, ref _commentMark, value);
            }
        }


        public static CsvConfig Default
        {
            get { return new CsvConfig(';', "\r\n", '\"', '#'); }
        }

    }
}
