using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using Graphy.Model;

namespace Graphy
{
    public class DesignTableReader : ObservableObject
    {
        public DesignTableReader()
        {

        }

        private string _exceptionMessage;

        private Excel.Application _excelApplication;
        private Excel.Workbook _excelWorkbook;
        private Excel.Worksheet _excelWorksheet;

        private bool _isOpen = false;

        public string ExceptionMessage
        {
            get => _exceptionMessage;
            set
            {
                Set(() => ExceptionMessage, ref _exceptionMessage, value);
            }
        }

        public bool OpenDesignTable(string designTableFullPath)
        {
            try
            {
                _excelApplication = new Excel.Application();
                _excelWorkbook = _excelApplication.Workbooks.Open(designTableFullPath);
                _excelWorksheet = (Excel.Worksheet)_excelWorkbook.ActiveSheet;

                _isOpen = true;

                return true;
            }
            catch (Exception ex)
            {
                ExceptionMessage = ex.Message;
                return false;
            }
        }

        public void CloseDesignTable()
        {
            _excelWorkbook.Close();

            _isOpen = false;
        }

        /// <summary>
        /// Load the design table parameters. The Excel file should be open first with the "OpenDesignTable" function.
        /// </summary>
        /// <param name="parameterCollection">Parameter Collection which will be filled.</param>
        public void LoadDesignTable(ICollection<DesignTableParameter> parameterCollection)
        {
            if (_isOpen)
            {
                parameterCollection.Clear();
                parameterCollection.Add(DesignTableParameter.NoLinkParameter());

                try
                {
                    foreach (Excel.Range cell in (Excel.Range)_excelWorksheet.UsedRange)
                    {
                        // Première colonne : Liste des pièces
                        if (cell.Column > 1)
                        {
                            // Première ligne : En-têtes des colonnes
                            if (cell.Row == 1 && cell.Value != null)
                            {
                                string headerName = Convert.ToString(cell.Value);
                                parameterCollection.Add(new DesignTableParameter()
                                {
                                    Name = headerName,
                                    ColumnIndex = cell.Column
                                });
                            }

                            // Deuxième ligne : On récupère la première valeur de la colonne pour tester son type
                            if (cell.Row == 2 && cell.Value != null)
                            {
                                string firstCellValue = Convert.ToString(cell.Value);
                                DesignTableParameter.ParameterType columnCategory = DesignTableParameter.GetParameterCategory(firstCellValue);

                                foreach (DesignTableParameter parameter in parameterCollection)
                                {
                                    if (parameter.ColumnIndex == cell.Column)
                                    {
                                        parameter.Type = columnCategory;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CloseDesignTable();
                    throw new Exception(ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// Load the part list written in the design table Excel file. The Excel file should be open first with the "OpenDesignTable" function.
        /// </summary>
        /// <param name="partList">Part list which will be filled.</param>
        public void GetPartList(ICollection<string> partList)
        {
            partList.Clear();

            try
            {
                foreach(Excel.Range cell in ((Excel.Range)_excelWorksheet.UsedRange.Columns[1]).Rows)
                {
                    // Première colonne : Liste des pièces
                    if (cell.Row > 1 && cell.Value != null)
                    {
                        string partName = Convert.ToString(cell.Value);
                        partList.Add(partName);
                    }
                }
            }
            catch (Exception ex)
            {
                CloseDesignTable();
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// Returns the row associated to the part number. The Excel file should be open first with the "OpenDesignTable" function.
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns></returns>
        public int GetRow(string partNumber)
        {
            try
            {
                foreach (Excel.Range cell in ((Excel.Range)_excelWorksheet.Columns[1]).Rows)
                {
                    // Première colonne : Liste des pièces
                    if (cell.Row > 1 && cell.Value == partNumber)
                    {
                        return cell.Row;
                    }
                }

                CloseDesignTable();
                throw new Exception("Part Number not found");
            }
            catch (Exception ex)
            {
                CloseDesignTable();
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// Returns the part number value for the specified column. The Excel file should be open first with the "OpenDesignTable" function.
        /// </summary>
        /// <param name="partNumber">Value to retreive part number.</param>
        /// <param name="column">Column of the parameter</param>
        /// <returns></returns>
        public object GetValue(int row, int column)
        {
            try
            {
                return _excelWorksheet.Cells[row, column].Value;
            }
            catch (Exception ex)
            {
                CloseDesignTable();
                throw new Exception(ex.Message, ex);
            }
        }

    }
}
