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


        public static void GenerateTemplate(CsvConfig csvConfig, MarkablePart markablePart)
        {
            CsvWriter writer = new CsvWriter(csvConfig);
            List<string> headerList = new List<string>()
            { "PartName",
                "MarkingName",
                "IsText",
                "Text",
                "IsBold",
                "IsItalic",
                "IsUnderline",
                "IsStrikeThrough",
                "FontFamily",
                "IconPathData",
                "MarkingHeight",
                "ExtrusionHeight",
                "HorizontalAlignment",
                "VerticalAlignment",
                "Surface",
                "Curve",
                "Point",
                "AxisSystem"};
            writer.AddRow(headerList, true);

            List<string> exampleList = new List<string>()
            { markablePart.PartName,
                markablePart.MarkingData.Name,
                markablePart.MarkingData.IsText.ToString(),
                markablePart.MarkingData.Text,
                markablePart.MarkingData.IsBold.ToString(),
                markablePart.MarkingData.IsItalic.ToString(),
                markablePart.MarkingData.IsUnderline.ToString(),
                markablePart.MarkingData.IsStrikeThrough.ToString(),
                markablePart.MarkingData.FontFamily.Source,
                markablePart.MarkingData.Icon.PathData,
                markablePart.MarkingData.MarkingHeight.ToString(),
                markablePart.MarkingData.ExtrusionHeight.ToString(),
                markablePart.MarkingData.HorizontalAlignment.ToString(),
                markablePart.MarkingData.VerticalAlignment.ToString(),
                markablePart.MarkingData.ProjectionSurfaceName,
                markablePart.MarkingData.TrackingCurveName,
                markablePart.MarkingData.ReferencePointName,
                markablePart.MarkingData.AxisSystemName };
            writer.AddRow(exampleList);

            //writer.Write();
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
                        MarkablePart tempMarkablePart = new MarkablePart();
                        MarkingData defaultMarkingData = MarkingData.Default();

                        // Assign PartName
                        string partNameValue = row[PART_NAME_INDEX];
                        if (partNameValue != "")
                            tempMarkablePart.PartName = partNameValue;
                        else
                            tempMarkablePart.HasFile = false;


                        // Assign Marking data Name
                        string markingDataNameValue = row[MARKING_NAME_INDEX];
                        if (markingDataNameValue != "")
                            tempMarkablePart.MarkingData.Name = markingDataNameValue;
                        else
                        {
                            tempMarkablePart.MarkingData.Name = defaultMarkingData.Name;
                            tempMarkablePart.MarkingData.Logs += GetWarningLog(row[MARKING_NAME_INDEX], "Name", typeof(string));
                            tempMarkablePart.MarkingData.WarningNumber++;
                        }


                        // Assign IsText
                        if (TryParseToBoolean(row[MARKING_IS_TEXT_INDEX], out bool isTextValue))
                            tempMarkablePart.MarkingData.IsText = isTextValue;
                        else
                        {
                            tempMarkablePart.MarkingData.IsText = defaultMarkingData.IsText;
                            tempMarkablePart.MarkingData.Logs += GetWarningLog(row[MARKING_IS_TEXT_INDEX], "IsText", typeof(bool));
                            tempMarkablePart.MarkingData.WarningNumber++;
                        }


                        #region Text assignation area

                        // Assign Text
                        string textValue = row[MARKING_TEXT_INDEX];
                        if(textValue != "")
                            tempMarkablePart.MarkingData.Text = textValue;
                        else if(tempMarkablePart.MarkingData.IsText)
                        {
                            tempMarkablePart.MarkingData.Text = defaultMarkingData.Text;
                            tempMarkablePart.MarkingData.Logs += GetWarningLog(row[MARKING_TEXT_INDEX], "Text", typeof(string));
                            tempMarkablePart.MarkingData.WarningNumber++;
                        }


                        // Assign IsBold
                        if (TryParseToBoolean(row[MARKING_IS_BOLD_INDEX], out bool isBoldValue))
                            tempMarkablePart.MarkingData.IsBold = isBoldValue;
                        else if(tempMarkablePart.MarkingData.IsText)
                        {
                            tempMarkablePart.MarkingData.IsBold = defaultMarkingData.IsBold;
                            tempMarkablePart.MarkingData.Logs += GetWarningLog(row[MARKING_IS_BOLD_INDEX], "IsBold", typeof(bool));
                            tempMarkablePart.MarkingData.WarningNumber++;
                        }


                        // Assign IsItalic
                        if (TryParseToBoolean(row[MARKING_IS_ITALIC_INDEX], out bool isItalicValue))
                            tempMarkablePart.MarkingData.IsItalic = isItalicValue;
                        else if (tempMarkablePart.MarkingData.IsText)
                        {
                            tempMarkablePart.MarkingData.IsItalic = defaultMarkingData.IsItalic;
                            tempMarkablePart.MarkingData.Logs += GetWarningLog(row[MARKING_IS_ITALIC_INDEX], "IsItalic", typeof(bool));
                            tempMarkablePart.MarkingData.WarningNumber++;
                        }


                        // Assign IsUnderline
                        if (TryParseToBoolean(row[MARKING_IS_UNDERLINE_INDEX], out bool isUnderlineValue))
                            tempMarkablePart.MarkingData.IsUnderline = isUnderlineValue;
                        else if (tempMarkablePart.MarkingData.IsText)
                        {
                            tempMarkablePart.MarkingData.IsUnderline = defaultMarkingData.IsUnderline;
                            tempMarkablePart.MarkingData.Logs += GetWarningLog(row[MARKING_IS_UNDERLINE_INDEX], "IsUnderline", typeof(bool));
                            tempMarkablePart.MarkingData.WarningNumber++;
                        }


                        // Assign IsStrikeThrough
                        if (TryParseToBoolean(row[MARKING_IS_STRIKE_THROUGH_INDEX], out bool isStrikeThroughValue))
                            tempMarkablePart.MarkingData.IsStrikeThrough = isStrikeThroughValue;
                        else if (tempMarkablePart.MarkingData.IsText)
                        {
                            tempMarkablePart.MarkingData.IsStrikeThrough = defaultMarkingData.IsStrikeThrough;
                            tempMarkablePart.MarkingData.Logs += GetWarningLog(row[MARKING_IS_STRIKE_THROUGH_INDEX], "IsStrikeThrough", typeof(bool));
                            tempMarkablePart.MarkingData.WarningNumber++;
                        }


                        // Assign FontFamily
                        if (TryParseToFontFamily(row[MARKING_FONT_FAMILY_INDEX], out FontFamily fontFamilyValue))
                            tempMarkablePart.MarkingData.FontFamily = fontFamilyValue;
                        else if (tempMarkablePart.MarkingData.IsText)
                        {
                            tempMarkablePart.MarkingData.FontFamily = defaultMarkingData.FontFamily;
                            tempMarkablePart.MarkingData.Logs += GetWarningLog(row[MARKING_FONT_FAMILY_INDEX], "FontFamily", typeof(string));
                            tempMarkablePart.MarkingData.WarningNumber++;
                        }

                        #endregion


                        // Assign Icon
                        if (TryParseToIcon(row[MARKING_ICON_PATH_DATA_INDEX], out Icon iconValue))
                            tempMarkablePart.MarkingData.Icon.PathData = row[MARKING_ICON_PATH_DATA_INDEX];
                        else if (!tempMarkablePart.MarkingData.IsText)
                        {
                            tempMarkablePart.MarkingData.Icon = defaultMarkingData.Icon;
                            tempMarkablePart.MarkingData.Logs += GetWarningLog(row[MARKING_ICON_PATH_DATA_INDEX], "Icon", typeof(System.Windows.Media.Geometry));
                            tempMarkablePart.MarkingData.WarningNumber++;
                        }


                        // Assign MarkingHeight
                        if (TryParseToDouble(row[MARKING_HEIGHT_INDEX], out double markingHeightValue))
                            tempMarkablePart.MarkingData.MarkingHeight = markingHeightValue;
                        else
                        {
                            tempMarkablePart.MarkingData.MarkingHeight = defaultMarkingData.MarkingHeight;
                            tempMarkablePart.MarkingData.Logs += GetWarningLog(row[MARKING_HEIGHT_INDEX], "MarkingHeight", typeof(double));
                            tempMarkablePart.MarkingData.WarningNumber++;
                        }


                        // Assign ExtrusionHeight
                        if (TryParseToDouble(row[MARKING_EXTRUSION_HEIGHT_INDEX], out double extrusionHeightValue))
                            tempMarkablePart.MarkingData.ExtrusionHeight = extrusionHeightValue;
                        else
                        {
                            tempMarkablePart.MarkingData.ExtrusionHeight = defaultMarkingData.ExtrusionHeight;
                            tempMarkablePart.MarkingData.Logs += GetWarningLog(row[MARKING_EXTRUSION_HEIGHT_INDEX], "ExtrusionHeight", typeof(double));
                            tempMarkablePart.MarkingData.WarningNumber++;
                        }


                        // Assign Horizontal alignement
                        if (TryParseToHorizontalAlignment(row[MARKING_HORIZONTAL_ALIGNMENT_INDEX], out HorizontalAlignment horizontalAlignmentValue))
                            tempMarkablePart.MarkingData.HorizontalAlignment = horizontalAlignmentValue;
                        else
                        {
                            tempMarkablePart.MarkingData.HorizontalAlignment = defaultMarkingData.HorizontalAlignment;
                            tempMarkablePart.MarkingData.Logs += GetWarningLog(row[MARKING_HORIZONTAL_ALIGNMENT_INDEX], "HorizontalAlignment", typeof(HorizontalAlignment));
                            tempMarkablePart.MarkingData.WarningNumber++;
                        }


                        // Assign Vertical alignement
                        if (TryParseToVerticalAlignment(row[MARKING_VERTICAL_ALIGNMENT_INDEX], out VerticalAlignment verticalAlignmentValue))
                            tempMarkablePart.MarkingData.VerticalAlignment = verticalAlignmentValue;
                        else
                        {
                            tempMarkablePart.MarkingData.VerticalAlignment = defaultMarkingData.VerticalAlignment;
                            tempMarkablePart.MarkingData.Logs += GetWarningLog(row[MARKING_VERTICAL_ALIGNMENT_INDEX], "VerticalAlignment", typeof(VerticalAlignment));
                            tempMarkablePart.MarkingData.WarningNumber++;
                        }



                        // Assign Projection surface name
                        string projectionSurfaceNameValue = row[MARKING_PROJECTION_SURFACE_NAME_INDEX];
                        if (projectionSurfaceNameValue != "")
                            tempMarkablePart.MarkingData.ProjectionSurfaceName = projectionSurfaceNameValue;
                        else
                        {
                            tempMarkablePart.MarkingData.ProjectionSurfaceName = MarkingData.NoMarkingData().ProjectionSurfaceName;
                            tempMarkablePart.MarkingData.Logs += GetWarningLog(row[MARKING_PROJECTION_SURFACE_NAME_INDEX], "ProjectionSurfaceName", typeof(string));
                            tempMarkablePart.MarkingData.WarningNumber++;
                        }


                        // Assign Tracking curve name
                        string trackingCurveNameValue = row[MARKING_TRACKING_CURVE_NAME_INDEX];
                        if (trackingCurveNameValue != "")
                            tempMarkablePart.MarkingData.TrackingCurveName = trackingCurveNameValue;
                        else
                        {
                            tempMarkablePart.MarkingData.TrackingCurveName = MarkingData.NoMarkingData().TrackingCurveName;
                            tempMarkablePart.MarkingData.Logs += GetWarningLog(row[MARKING_TRACKING_CURVE_NAME_INDEX], "TrackingCurveName", typeof(string));
                            tempMarkablePart.MarkingData.WarningNumber++;
                        }

                        // Assign Reference point name
                        string referencePointNameValue = row[MARKING_REFERENCE_POINT_NAME_INDEX];
                        if (referencePointNameValue != "")
                            tempMarkablePart.MarkingData.ReferencePointName = referencePointNameValue;
                        else
                        {
                            tempMarkablePart.MarkingData.ReferencePointName = MarkingData.NoMarkingData().ReferencePointName;
                            tempMarkablePart.MarkingData.Logs += GetWarningLog(row[MARKING_REFERENCE_POINT_NAME_INDEX], "ReferencePointName", typeof(string));
                            tempMarkablePart.MarkingData.WarningNumber++;
                        }

                        // Assign Axis system name
                        string axisSystemNameValue = row[MARKING_REFERENCE_AXIS_SYSTEME_NAME_INDEX];
                        if (axisSystemNameValue != "")
                            tempMarkablePart.MarkingData.AxisSystemName = axisSystemNameValue;
                        else
                        {
                            tempMarkablePart.MarkingData.AxisSystemName = MarkingData.NoMarkingData().AxisSystemName;
                            tempMarkablePart.MarkingData.Logs += GetWarningLog(row[MARKING_REFERENCE_AXIS_SYSTEME_NAME_INDEX], "AxisSystemName", typeof(string));
                            tempMarkablePart.MarkingData.WarningNumber++;
                        }

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


        private static bool TryParseToBoolean(string str, out bool result)
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

        private static bool TryParseToFontFamily(string str, out FontFamily result)
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


        private static bool TryParseToDouble(string str, out double result)
        {
            result = 0d;

            try
            {
                result = double.Parse(str, System.Globalization.NumberStyles.Any, CultureInfo.CurrentCulture);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool TryParseToIcon(string str, out Icon result)
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
            catch (Exception)
            {
                return false;
            }
        }

        private static bool TryParseToHorizontalAlignment(string str, out HorizontalAlignment result)
        {
            result = HorizontalAlignment.Left;

            try
            {
                result = (HorizontalAlignment)int.Parse(str);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        private static bool TryParseToVerticalAlignment(string str, out VerticalAlignment result)
        {
            result = VerticalAlignment.Bottom;

            try
            {
                result = (VerticalAlignment)int.Parse(str);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string GetWarningLog(string value, string field, Type targetType)
        {
            if (value == "")
                return "Warning: " + field + " value is empty. Default value assigned.";
            else
                return "Warning: retrieved " + field + " value cannot be parsed to " + targetType.Name + ". Default value assigned";
        }
    }

}

