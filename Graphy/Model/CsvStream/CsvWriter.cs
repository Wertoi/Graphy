using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphy.CsvStream
{
    public class CsvWriter
    {
        private CsvConfig _config;
        private StringBuilder _csvContents;

        public CsvConfig Config { get => _config; set => _config = value; }

        public CsvWriter(CsvConfig config = null)
        {
            if (config == null)
                _config = CsvConfig.Default;
            else
                _config = config;

            _csvContents = new StringBuilder();
        }

        public void AddRow(IEnumerable<string> cells)
        {
            int i = 0;
            foreach (string cell in cells)
            {
                _csvContents.Append(ParseCell(cell));
                _csvContents.Append(_config.Delimiter);

                i++;
            }

            _csvContents.Length--; // remove last delimiter
            _csvContents.Append(_config.NewLineMark);
        }

        private string ParseCell(string cell)
        {
            // cells cannot be multi-line
            cell = cell.Replace("\r", "");
            cell = cell.Replace("\n", "");

            if (!NeedsToBeEscaped(cell))
                return cell;

            // double every quotation mark
            cell = cell.Replace(_config.QuotationMark.ToString(), string.Format("{0}{0}", _config.QuotationMark));

            // add quotation marks at the beginning and at the end
            cell = _config.QuotationMark + cell + _config.QuotationMark;

            return cell;
        }

        private bool NeedsToBeEscaped(string cell)
        {
            if (cell.Contains(_config.QuotationMark.ToString()))
                return true;

            if (cell.Contains(_config.Delimiter.ToString()))
                return true;

            return false;
        }

        public string Write()
        {
            return _csvContents.ToString();
        }
    }
}
