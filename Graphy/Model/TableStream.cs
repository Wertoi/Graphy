using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphy.CsvStream;

namespace Graphy.Model
{
    public class TableStream
    {
        public TableStream()
        {

        }

        public static void GenerateTemplate()
        {
            CsvWriter writer = new CsvWriter();
            writer.AddRow(new string[] { writer.Config.CommentMark + "PartName", "MarkingName", "MarkingType" , "Text", "IsBold", "IsItalic", "IsUnderligned", "IsStrikeThrought",  "Font", "IconPath",
                "MarkingHeight", "ExtrusionHeight",
                "SurfaceName", "CurveName", "StartPointName", "AxisSystemName" });
            writer.AddRow(new string[] { "Part1", "Marking Test n°1", "1", "Hello World !", "0", "1", "0", "0", "Calibri", "N/A", (1.6).ToString(), "1", "Extraction.1", "Droite.1", "Point.1", "Système d'axes.1" });

            string csvTemplate = writer.Write();
            File.WriteAllText(Path.GetTempPath() + "Test.csv", csvTemplate);
        }

        public static bool TryRead(string fullPath, ICollection<(string partName, MarkingData markingData)> markablePartList)
        {
            if (!File.Exists(fullPath))
                return false;

            try
            {
                CsvReader reader = new CsvReader();
                string fileText = File.ReadAllText(fullPath);

                foreach (string[] row in reader.Read(fileText))
                {
                    // If the row is not a comment row
                    if (row.First().Substring(0, 1) != reader.Config.CommentMark.ToString())
                    {

                        // Get the part part name and try to find this part in the markable part collection
                        (string partName, MarkingData markingData) tempPart;
                        tempPart.partName = row.First();
                        tempPart.markingData = new MarkingData();

                        tempPart.markingData.Name = row[1];
                        tempPart.markingData.IsText = Convert.ToBoolean(Convert.ToInt32(row[2]));
                        tempPart.markingData.Text = row[3];
                        tempPart.markingData.IsBold = Convert.ToBoolean(Convert.ToInt32(row[4]));
                        tempPart.markingData.IsItalic = Convert.ToBoolean(Convert.ToInt32(row[5]));
                        tempPart.markingData.FontFamily = new FontFamily(row[6]);
                        tempPart.markingData.Icon = new Icon()
                        {
                            PathData = row[7]
                        };

                        tempPart.markingData.MarkingHeight = double.Parse(row[8]);
                        tempPart.markingData.ExtrusionHeight = double.Parse(row[9]);
                        tempPart.markingData.ProjectionSurfaceName = row[10];
                        tempPart.markingData.TrackingCurveName = row[11];
                        tempPart.markingData.StartPointName = row[12];
                        tempPart.markingData.AxisSystemName = row[13];

                        markablePartList.Add(tempPart);

                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

}

