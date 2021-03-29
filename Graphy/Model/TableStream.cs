using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphy.CsvStream;
using Graphy.Enum;

namespace Graphy.Model
{
    public class TableStream
    {
        public TableStream()
        {

        }

        private const int PART_NAME_INDEX = 0;
        private const int MARKING_NAME_INDEX = 1;
        private const int MARKING_IS_TEXT_INDEX = 2;
        private const int MARKING_TEXT_INDEX = 3;
        private const int MARKING_IS_BOLD_INDEX = 4;
        private const int MARKING_IS_ITALIC_INDEX = 5;
        private const int MARKING_IS_UNDERLINE_INDEX = 6;
        private const int MARKING_IS_STRIKE_THROUGH_INDEX = 7;
        private const int MARKING_FONT_FAMILY_INDEX = 8;
        private const int MARKING_ICON_PATH_DATA_INDEX = 9;
        private const int MARKING_HEIGHT_INDEX = 10;
        private const int MARKING_EXTRUSION_HEIGHT_INDEX = 11;
        private const int MARKING_HORIZONTAL_ALIGNMENT_INDEX = 12;
        private const int MARKING_VERTICAL_ALIGNMENT_INDEX = 13;
        private const int MARKING_PROJECTION_SURFACE_NAME_INDEX = 14;
        private const int MARKING_TRACKING_CURVE_NAME_INDEX = 15;
        private const int MARKING_REFERENCE_POINT_NAME_INDEX = 16;
        private const int MARKING_REFERENCE_AXIS_SYSTEME_NAME_INDEX = 17;

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

        public static bool TryRead(string fullPath, ICollection<MarkablePart> markablePartList, CsvConfig csvConfig)
        {
            if (!File.Exists(fullPath))
                return false;

            try
            {
                CsvReader reader = new CsvReader(csvConfig);
                string fileText = File.ReadAllText(fullPath);

                foreach (string[] row in reader.Read(fileText))
                {
                    // If the row is not a comment row
                    if (row.First().Substring(0, 1) != reader.Config.CommentMark.ToString())
                    {
                        bool errorFlag = false;

                        MarkablePart tempMarkablePart = new MarkablePart();

                        // Assign PartName
                        string partNameValue = row[PART_NAME_INDEX];
                        if(partNameValue != "")
                            tempMarkablePart.PartName = row[PART_NAME_INDEX];
                        else
                        {
                            tempMarkablePart.PartName = "Part1";
                            errorFlag = true;
                            // Ecrire un commentaire du style : "PartName" value non-assigned. Default value assigned.
                            // Flag une erreur.
                        }

                        tempMarkablePart.MarkingData.Name = row[MARKING_NAME_INDEX];
                        tempMarkablePart.MarkingData.ProjectionSurfaceName = row[MARKING_PROJECTION_SURFACE_NAME_INDEX];
                        tempMarkablePart.MarkingData.TrackingCurveName = row[MARKING_TRACKING_CURVE_NAME_INDEX];
                        tempMarkablePart.MarkingData.ReferencePointName = row[MARKING_REFERENCE_POINT_NAME_INDEX];
                        tempMarkablePart.MarkingData.AxisSystemName = row[MARKING_REFERENCE_AXIS_SYSTEME_NAME_INDEX];
                        
                        // Assign IsText
                        if (TryConvertToBoolean(row[MARKING_IS_TEXT_INDEX], out bool isTextValue))
                            tempMarkablePart.MarkingData.IsText = isTextValue;
                        else
                        {
                            tempMarkablePart.MarkingData.IsText = MarkingData.Default().IsText;

                            if(tempMarkablePart.MarkingData.IsText)
                            {
                                tempMarkablePart.MarkingData.Comments += "Error: IsText value non-assigned or incoherent (" + isTextValue.ToString() + 
                                    "). Default value assigned\r\n";
                                errorFlag = true;
                            }
                            else
                                tempMarkablePart.MarkingData.Comments += "Warning: IsText value non-assigned or incoherent (" + isTextValue.ToString() + "" +
                                    "). Default value assigned\r\n";
                        }

                        // Assign Text
                        string textValue = row[MARKING_TEXT_INDEX];
                        if(textValue != "")
                            tempMarkablePart.MarkingData.Text = row[MARKING_TEXT_INDEX];
                        else
                        {
                            if(tempMarkablePart.MarkingData.IsText)
                            {
                                tempMarkablePart.MarkingData.Text = MarkingData.Default().Text;
                                tempMarkablePart.MarkingData.Comments += "Error: Text value empty. Default value assigned\r\n";
                                errorFlag = true;
                            }
                            else
                                tempMarkablePart.MarkingData.Comments += "Warning: Text value empty.\r\n";
                        }


                        // Assign IsBold
                        if (TryConvertToBoolean(row[MARKING_IS_BOLD_INDEX], out bool isBoldValue))
                            tempMarkablePart.MarkingData.IsBold = isBoldValue;
                        else
                        {
                            tempMarkablePart.MarkingData.IsBold = MarkingData.Default().IsBold;
                            tempMarkablePart.MarkingData.Comments += "Warning: IsBold value non-assigned or incoherent (" + isBoldValue.ToString() +
                                "). Default value assigned\r\n";
                            //errorFlag = true;
                        }


                        // Assign IsItalic
                        if (TryConvertToBoolean(row[MARKING_IS_ITALIC_INDEX], out bool isItalicValue))
                            tempMarkablePart.MarkingData.IsItalic = isItalicValue;
                        else
                        {
                            tempMarkablePart.MarkingData.IsItalic = MarkingData.Default().IsItalic;
                            tempMarkablePart.MarkingData.Comments += "Warning: IsItalic value non-assigned or incoherent (" + isItalicValue.ToString() +
                                "). Default value assigned\r\n";
                            //errorFlag = true;
                        }


                        // Assign IsUnderline
                        if (TryConvertToBoolean(row[MARKING_IS_UNDERLINE_INDEX], out bool isUnderlineValue))
                            tempMarkablePart.MarkingData.IsUnderline = isUnderlineValue;
                        else
                        {
                            tempMarkablePart.MarkingData.IsUnderline = MarkingData.Default().IsUnderline;
                            tempMarkablePart.MarkingData.Comments += "Warning: IsUnderline value non-assigned or incoherent (" + isUnderlineValue.ToString() +
                                "). Default value assigned\r\n";
                            //errorFlag = true;
                        }


                        // Assign IsStrikeThrough
                        if (TryConvertToBoolean(row[MARKING_IS_STRIKE_THROUGH_INDEX], out bool isStrikeThroughValue))
                            tempMarkablePart.MarkingData.IsStrikeThrough = isStrikeThroughValue;
                        else
                        {
                            tempMarkablePart.MarkingData.IsStrikeThrough = MarkingData.Default().IsStrikeThrough;
                            tempMarkablePart.MarkingData.Comments += "Warning: IsStrikeThrough value non-assigned or incoherent (" + isStrikeThroughValue.ToString() +
                                "). Default value assigned\r\n";
                            //errorFlag = true;
                        }


                        // Assign FontFamily
                        if (TryConvertToFontFamily(row[MARKING_FONT_FAMILY_INDEX], out FontFamily fontFamilyValue))
                            tempMarkablePart.MarkingData.FontFamily = fontFamilyValue;
                        else
                        {
                            if(tempMarkablePart.MarkingData.IsText)
                            {
                                tempMarkablePart.MarkingData.FontFamily = MarkingData.Default().FontFamily;
                                tempMarkablePart.MarkingData.Comments += "Error: FontFamily value non-assigned or incoherent (" + fontFamilyValue.Source +
                                    "). Default value assigned\r\n";
                                errorFlag = true;
                            }
                            else
                                tempMarkablePart.MarkingData.Comments += "Warning: FontFamily value non-assigned or incoherent (" + fontFamilyValue.Source +
                                    "). Default value assigned\r\n";
                        }


                        // Assign MarkingHeight
                        if (TryConvertToDouble(row[MARKING_HEIGHT_INDEX], out double markingHeightValue))
                        {
                            if(markingHeightValue > 0)
                                tempMarkablePart.MarkingData.MarkingHeight = markingHeightValue;
                            else
                            {
                                tempMarkablePart.MarkingData.MarkingHeight = MarkingData.Default().MarkingHeight;
                                tempMarkablePart.MarkingData.Comments += "Error: MarkingHeight must be > 0. Default value assigned\r\n";
                                errorFlag = true;
                            }
                        }
                        else
                        {
                            tempMarkablePart.MarkingData.MarkingHeight = MarkingData.Default().MarkingHeight;
                            tempMarkablePart.MarkingData.Comments += "Error: MarkingHeight value non-assigned or incoherent (" + markingHeightValue +
                                "). Default value assigned\r\n";
                            errorFlag = true;
                        }


                        // Assign ExtrusionHeight
                        if (TryConvertToDouble(row[MARKING_EXTRUSION_HEIGHT_INDEX], out double extrusionHeightValue))
                        {
                            if(extrusionHeightValue != 0)
                                tempMarkablePart.MarkingData.ExtrusionHeight = extrusionHeightValue;
                            else
                            {
                                tempMarkablePart.MarkingData.ExtrusionHeight = MarkingData.Default().ExtrusionHeight;
                                tempMarkablePart.MarkingData.Comments += "Warning: ExtrusionHeight must be different of 0. Default value assigned\r\n";
                            }
                        }
                        else
                        {
                            tempMarkablePart.MarkingData.ExtrusionHeight = MarkingData.Default().ExtrusionHeight;
                            tempMarkablePart.MarkingData.Comments += "Warning: ExtrusionHeight value non-assigned or incoherent. Default value assigned\r\n";
                        }


                        tempMarkablePart.MarkingData.HorizontalAlignment = (HorizontalAlignment)int.Parse(row[MARKING_HORIZONTAL_ALIGNMENT_INDEX]);

                        tempMarkablePart.MarkingData.VerticalAlignment = (VerticalAlignment)int.Parse(row[MARKING_VERTICAL_ALIGNMENT_INDEX]);

                        if (TryConvertToIcon(row[MARKING_ICON_PATH_DATA_INDEX], out Icon iconValue))
                            tempMarkablePart.MarkingData.Icon = iconValue;
                        else
                        {
                            if(tempMarkablePart.MarkingData.IsText)
                            {
                                tempMarkablePart.MarkingData.Comments += "Warning: IconPathData value non-assigned or incoherent. Default value assigned\r\n";
                            }
                            else
                            {
                                tempMarkablePart.MarkingData.Icon = MarkingData.Default().Icon;
                                tempMarkablePart.MarkingData.Comments += "Error: IconPathData value non-assigned or incoherent. Default value assigned\r\n";
                                errorFlag = true;
                            }
                        }

                        tempMarkablePart.MarkingData.IsOK = !errorFlag;

                        markablePartList.Add(tempMarkablePart);

                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        private static bool TryConvertToBoolean(string str, out bool result)
        {
            result = false;

            try
            {
                result = Convert.ToBoolean(Convert.ToInt32(str));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool TryConvertToFontFamily(string str, out FontFamily result)
        {
            result = new FontFamily(str);

            System.Drawing.Text.InstalledFontCollection installedFontCollection = new System.Drawing.Text.InstalledFontCollection();
            foreach (System.Drawing.FontFamily fontFamily in installedFontCollection.Families)
            {
                if (fontFamily.Name == str)
                {
                    result = new FontFamily(fontFamily.Name);
                    return true;
                }
            }

            return false;
        }


        private static bool TryConvertToDouble(string str, out double result)
        {
            result = 0d;

            try
            {
                result = double.Parse(str, System.Globalization.NumberStyles.Any, CultureInfo.CurrentCulture);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        private static bool TryConvertToIcon(string str, out Icon result)
        {
            result = new Icon()
            {
                PathData = str
            };

            try
            {
                System.Windows.Media.Geometry.Parse(result.PathData);
                return true;
            }
            catch(Exception)
            {
                return false;
            }

        }
    }

}

