using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace Graphy.Model
{
    public class ExcelTableStream
    {
        public ExcelTableStream(string fullPath)
        {
            _excelApplication = new Excel.Application();
            _excelWorkbook = _excelApplication.Workbooks.Open(fullPath);
        }

        private const string TABLE_NAME = "MarkingTable";
        private const string TABLE_PART_NUMBER_HEADER_NAME = "PartNumber";
        private const string TABLE_MARKING_NAME_HEADER_NAME = "Name";
        private const string TABLE_IS_TEXT_HEADER_NAME = "Is Text";
        private const string TABLE_TEXT_HEADER_NAME = "Text";
        private const string TABLE_IS_BOLD_HEADER_NAME = "Is Bold";
        private const string TABLE_IS_ITALIC_HEADER_NAME = "Is Italic";
        private const string TABLE_FONT_HEADER_NAME = "Font";
        private const string TABLE_MARKING_HEIGHT_HEADER_NAME = "Marking height";
        private const string TABLE_EXTRUSION_HEIGHT_HEADER_NAME = "Extrusion height";
        private const string TABLE_ICON_PATH_HEADER_NAME = "Icon path";
        private const string TABLE_SURFACE_HEADER_NAME = "Surface name";
        private const string TABLE_CURVE_HEADER_NAME = "Curve name";
        private const string TABLE_START_POINT_HEADER_NAME = "Start point name";
        private const string TABLE_AXIS_SYSTEM_HEADER_NAME = "Axis system name";

        private Excel.Application _excelApplication;
        private Excel.Workbook _excelWorkbook;

        private bool _isOpen = false;


        public void Read()
        {

        }



        public static void GenerateTemplateTable()
        {
            try
            {
                Excel.Application application = new Excel.Application();
                Excel.Workbook workbook;

                application.SheetsInNewWorkbook = 2;
                application.Visible = true;
                workbook = (Excel.Workbook)(application.Workbooks.Add(Missing.Value));

                Excel.Worksheet mainSheet = (Excel.Worksheet)workbook.Sheets[1];
                Excel.Worksheet infoSheet = (Excel.Worksheet)workbook.Sheets[2];

                // DISCLAIMER
                mainSheet.Cells[1, 1].Value = "// ********************************************************* //";
                mainSheet.Cells[3, 1].Value = "// ****** THIS SHEET HAS BEEN AUTOMATICALLY GENERATED. ***** //";
                mainSheet.Cells[5, 1].Value = "// ****** PLEASE DO NOT MODIFY THE TEMPLATE MANUALLY.  ***** //";
                mainSheet.Cells[7, 1].Value = "// ********************************************************* //";

                // HEADERS
                mainSheet.Cells[9, 1].Value = TABLE_PART_NUMBER_HEADER_NAME;
                mainSheet.Cells[9, 2].Value = TABLE_MARKING_NAME_HEADER_NAME;
                mainSheet.Cells[9, 3].Value = TABLE_IS_TEXT_HEADER_NAME;
                mainSheet.Cells[9, 4].Value = TABLE_TEXT_HEADER_NAME;
                mainSheet.Cells[9, 5].Value = TABLE_IS_BOLD_HEADER_NAME;
                mainSheet.Cells[9, 6].Value = TABLE_IS_ITALIC_HEADER_NAME;
                mainSheet.Cells[9, 7].Value = TABLE_FONT_HEADER_NAME;
                mainSheet.Cells[9, 8].Value = TABLE_ICON_PATH_HEADER_NAME;
                mainSheet.Cells[9, 9].Value = TABLE_MARKING_HEIGHT_HEADER_NAME;
                mainSheet.Cells[9, 10].Value = TABLE_EXTRUSION_HEIGHT_HEADER_NAME;
                mainSheet.Cells[9, 11].Value = TABLE_SURFACE_HEADER_NAME;
                mainSheet.Cells[9, 12].Value = TABLE_CURVE_HEADER_NAME;
                mainSheet.Cells[9, 13].Value = TABLE_START_POINT_HEADER_NAME;
                mainSheet.Cells[9, 14].Value = TABLE_AXIS_SYSTEM_HEADER_NAME;

                // CREATE TABLE
                Excel.Range sourceRange2 = mainSheet.Range[mainSheet.Cells[9, 1], mainSheet.Cells[10, 14]];

                Excel.ListObject markingTable = (Excel.ListObject)mainSheet.ListObjects.AddEx(
                    SourceType: Excel.XlListObjectSourceType.xlSrcRange,
                    Source: sourceRange2,
                    XlListObjectHasHeaders: Excel.XlYesNoGuess.xlYes);

                markingTable.Name = "MarkingTable";

                // FONT LIST
                infoSheet.Cells[1, 1].Value = "Font";

                int count = 0;
                InstalledFontCollection installedFontCollection = new InstalledFontCollection();
                for (int i = 0; i < installedFontCollection.Families.Length; i++)
                {
                    infoSheet.Cells[2 + i, 1].Value = installedFontCollection.Families[i].Name;
                    count++;
                }

                CreateList(workbook, infoSheet, 1, count + 1, 1, 1, "FontTable", "FontList");
                AssignList(mainSheet,  10, 7, "FontList");


                infoSheet.Cells[1, 3].Value = "Bool";
                infoSheet.Cells[2, 3].Value = "0";
                infoSheet.Cells[3, 3].Value = "1";
                CreateList(workbook, infoSheet, 1, 3, 3, 3, "BoolTable", "BoolList");
                

                /*((Excel.Range)mainSheet.Cells[10, 7]).Validation.Add(Type: Excel.XlDVType.xlValidateList,
                    AlertStyle: Excel.XlDVAlertStyle.xlValidAlertStop,
                    Operator: Excel.XlFormatConditionOperator.xlBetween,
                    Formula1: "=FontList");
                ((Excel.Range)mainSheet.Cells[10, 7]).Validation.IgnoreBlank = true;
                ((Excel.Range)mainSheet.Cells[10, 7]).Validation.InCellDropdown = true;
                ((Excel.Range)mainSheet.Cells[10, 7]).Validation.ShowInput = true;*/


                // EXAMPLES
                mainSheet.Cells[10, 1].Value = "Part1";
                mainSheet.Cells[10, 2].Value = "1";
                mainSheet.Cells[10, 3].Value = "Hello World !";
                mainSheet.Cells[10, 4].Value = "Calibri";
                mainSheet.Cells[10, 5].Value = "1.6";
                mainSheet.Cells[10, 6].Value = "0.1";
                mainSheet.Cells[10, 7].Value = "null";
                mainSheet.Cells[10, 8].Value = "Surface.1";
                mainSheet.Cells[10, 9].Value = "Curve.1";
                mainSheet.Cells[10, 10].Value = "Point.1";
                mainSheet.Cells[10, 11].Value = "AxisSystem.1";

                mainSheet.Cells[11, 1].Value = "Part2";
                mainSheet.Cells[11, 2].Value = "1";
                mainSheet.Cells[11, 3].Value = "Icy le petit ourson <3";
                mainSheet.Cells[11, 4].Value = "Arial";
                mainSheet.Cells[11, 5].Value = "5";
                mainSheet.Cells[11, 6].Value = "-0.1";
                mainSheet.Cells[11, 7].Value = "null";
                mainSheet.Cells[11, 8].Value = "Surface.1";
                mainSheet.Cells[11, 9].Value = "Curve.1";
                mainSheet.Cells[11, 10].Value = "Point.1";
                mainSheet.Cells[11, 11].Value = "AxisSystem.1";

            }
            catch (Exception ex)
            {

            }
        }


        private static void CreateList(Excel.Workbook wb, Excel.Worksheet ws, int startRow, int endRow, int startColumn, int endColumn, string tableName, string listName)
        {
            Excel.Range sourceRange = ws.Range[ws.Cells[startRow, startColumn], ws.Cells[endRow, endColumn]];

            Excel.ListObject fontTable = (Excel.ListObject)ws.ListObjects.AddEx(
                SourceType: Excel.XlListObjectSourceType.xlSrcRange,
                Source: sourceRange,
                XlListObjectHasHeaders: Excel.XlYesNoGuess.xlYes);

            fontTable.Name = tableName;

            string formula = "=" + tableName + "[" + listName + "]";
            wb.Names.Add(Name: listName, RefersToR1C1: formula);
        }

        private static void AssignList(Excel.Worksheet ws, int row, int column, string listName)
        {
            ((Excel.Range)ws.Cells[row, column]).Validation.Add(Type: Excel.XlDVType.xlValidateList,
                    AlertStyle: Excel.XlDVAlertStyle.xlValidAlertStop,
                    Operator: Excel.XlFormatConditionOperator.xlBetween,
                    Formula1: "=" + listName);
            ((Excel.Range)ws.Cells[row, column]).Validation.IgnoreBlank = true;
            ((Excel.Range)ws.Cells[row, column]).Validation.InCellDropdown = true;
            ((Excel.Range)ws.Cells[row, column]).Validation.ShowInput = true;
        }
    }
}
