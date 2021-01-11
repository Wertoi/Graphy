using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Graphy.Model
{
    public class TableReader
    {
        public TableReader()
        {

        }

        public static bool TryRead(string fullPath, ICollection<MarkablePart> markablePartList)
        {
            if (!File.Exists(fullPath))
                return false;

            try
            {
                using (StreamReader reader = new StreamReader(fullPath))
                {
                    while (!reader.EndOfStream)
                    {
                        string[] splittedLine = reader.ReadLine().Split(';');

                        if (splittedLine.Count() > 0)
                        {
                            string partName = splittedLine[0];
                            MarkablePart tempPart = markablePartList.ToList().Find((part) => part.PartName == partName);

                            if (tempPart == null)
                            {
                                tempPart = new MarkablePart()
                                {
                                    PartName = partName
                                };

                                markablePartList.Add(tempPart);
                            }


                            string markingDataName = splittedLine[1];
                            MarkingData tempMarkingData = tempPart.MarkingDataCollection.ToList().Find((markingData) => markingData.Name == markingDataName);

                            if (tempMarkingData == null)
                            {
                                tempMarkingData = new MarkingData();
                                tempMarkingData.Name = markingDataName;
                                tempMarkingData.IsText = Convert.ToBoolean(Convert.ToInt32(splittedLine[2]));
                                tempMarkingData.Text = splittedLine[3];
                                tempMarkingData.IsBold = Convert.ToBoolean(Convert.ToInt32(splittedLine[4]));
                                tempMarkingData.IsItalic = Convert.ToBoolean(Convert.ToInt32(splittedLine[5]));
                                tempMarkingData.FontFamily = new FontFamily(splittedLine[6]);
                                tempMarkingData.Icon = new Icon()
                                {
                                    PathData = splittedLine[7]
                                };

                                tempMarkingData.MarkingHeight = double.Parse(splittedLine[8]);
                                tempMarkingData.ExtrusionHeight = double.Parse(splittedLine[9]);
                                tempMarkingData.ProjectionSurfaceName = splittedLine[10];
                                tempMarkingData.TrackingCurveName = splittedLine[11];
                                tempMarkingData.StartPointName = splittedLine[12];
                                tempMarkingData.AxisSystemName = splittedLine[13];

                                tempPart.MarkingDataCollection.Add(tempMarkingData);
                            }

                        }
                    }

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
