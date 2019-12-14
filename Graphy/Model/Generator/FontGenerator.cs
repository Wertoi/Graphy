using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using INFITF;
using DRAFTINGITF;
using MECMOD;
using HybridShapeTypeLib;
using Graphy.Model;
using Graphy.Model.CatiaShape;
using Graphy.Model.CatiaDocument;

namespace Graphy.Model.Generator
{
    public class FontGenerator : IGenerator
    {
        public event EventHandler<ProgressRateChangedEventArgs> ProgressRateChanged;
        public event EventHandler<ProgressRateChangedEventArgs> StepProgressRateChanged;

        public FontGenerator()
        {

        }

        private double _progressRate;
        private int _stepProgressRate;

        public double ProgressRate
        {
            get => _progressRate;
            set
            {
                if (value != ProgressRate)
                {
                    _progressRate = value;
                    ProgressRateChanged?.Invoke(this, new ProgressRateChangedEventArgs(ProgressRate));
                }
            }
        }

        public int StepProgressRate
        {
            get => _stepProgressRate;
            set
            {
                if (value != StepProgressRate)
                {
                    _stepProgressRate = value;
                    StepProgressRateChanged?.Invoke(this, new ProgressRateChangedEventArgs(StepProgressRate));
                }
            }
        }

        public void Generate(CatiaEnv catiaEnv, Font font, string savePath)
        {
            CatiaDrawingDocument catiaDrawingDocument = new CatiaDrawingDocument(catiaEnv.AddNewDocument(CatiaGenericDocument.CatiaDocumentFormat.Drawing));
            DrawingView drawingView = catiaDrawingDocument.DrawingDocument.Sheets.ActiveSheet.Views.ActiveView;

            CatiaPartDocument catiaPartDocument = new CatiaPartDocument(catiaEnv.AddNewDocument(CatiaGenericDocument.CatiaDocumentFormat.CATPart));

            // Create .dxf set
            HybridBody importSet = catiaPartDocument.PartDocument.Part.HybridBodies.Add();
            importSet.set_Name("Set_Import_.dxf");

            // Create assembly set
            HybridBody assemblySet = catiaPartDocument.PartDocument.Part.HybridBodies.Add();
            assemblySet.set_Name("Set_Font_Assembled");

            // Create disassembled set
            HybridBody disassemblySet = catiaPartDocument.PartDocument.Part.HybridBodies.Add();
            disassemblySet.set_Name("Set_Font_Disassembled");

            catiaPartDocument.PartDocument.Part.Update();

            HybridShapeFactory factory = (HybridShapeFactory)catiaPartDocument.PartDocument.Part.HybridShapeFactory;

            Selection selectionPart = catiaPartDocument.PartDocument.Selection;

            string tempFullPath = System.IO.Path.GetTempPath() + "CatiaWriterTempChar.dxf";

            ProgressRate = 0;

            foreach (char c in font.SupportedCharacterList)
            {
                DrawingText text = drawingView.Texts.Add(c.ToString(), 0, 0);
                text.AnchorPosition = CatTextAnchorPosition.catHalfCenter;
                text.SetFontName(0, 0, font.Name + " (TrueType)"); // We only work with TrueType font

                if (System.IO.File.Exists(tempFullPath))
                    System.IO.File.Delete(tempFullPath);


                catiaDrawingDocument.DrawingDocument.ExportData(tempFullPath, "dxf");
                drawingView.Texts.Remove(1);


                if (System.IO.File.Exists(tempFullPath))
                {
                    CatiaGenericDocument tempGenericDocument = catiaEnv.OpenDocument(new CatiaFile(tempFullPath));
                    CatiaDrawingDocument tempDrawingDocument = new CatiaDrawingDocument(catiaEnv)
                    {
                        DrawingDocument = (DrawingDocument)catiaEnv.Application.ActiveDocument
                        // Dunno why but the ligne below does not work.
                        //DrawingDocument = (DrawingDocument)tempGenericDocument.Document
                    };

                    Selection selection = tempDrawingDocument.DrawingDocument.Selection;

                    // Paste the dxf in the font part file
                    bool isPasteOK = false;

                    do
                    {
                        try
                        {
                            tempDrawingDocument.DrawingDocument.Activate();

                            selection.Clear();
                            selection.Add(tempDrawingDocument.DrawingDocument.Sheets.ActiveSheet.Views.ActiveView);
                            selection.Copy();

                            // Activate the font part file
                            catiaPartDocument.PartDocument.Activate();

                            selectionPart.Clear();
                            selectionPart.Add(importSet);
                            selectionPart.Paste();

                            isPasteOK = true;
                        }
                        catch (Exception)
                        {
                            isPasteOK = false;
                        }
                    }
                    while (!isPasteOK);


                    // Close the temp drawing
                    tempDrawingDocument.DrawingDocument.Close();

                    // Activate the font part file
                    catiaPartDocument.PartDocument.Activate();

                    // Rename the sketch
                    Sketch sketch = importSet.HybridSketches.Item(importSet.HybridSketches.Count);
                    sketch.set_Name(c.ToString());

                    // Create the reference of the sketch
                    Reference sketchRef = catiaPartDocument.PartDocument.Part.CreateReferenceFromObject(sketch);

                    // Create the assembly
                    HybridShapeAssemble assemblyShape = factory.AddNewJoin(sketchRef, sketchRef);
                    assemblyShape.SetConnex(false);
                    assemblyShape.RemoveElement(1);
                    assemblyShape.set_Name(c.ToString());
                    assemblyShape.Compute();
                    assemblySet.AppendHybridShape(assemblyShape);

                    // *** CREATION UNICODE CATEGORY SET & CHAR SET ***

                    HybridBody charSet = null;

                    // Check if unicode category set already exists
                    bool unicodeCategoryExists = false;
                    foreach (HybridBody unicodeCategoryset in disassemblySet.HybridBodies)
                    {
                        if (unicodeCategoryset.get_Name() == CharUnicodeInfo.GetUnicodeCategory(c).ToString())
                        {
                            unicodeCategoryExists = true;
                            charSet = unicodeCategoryset.HybridBodies.Add();
                            break;
                        }
                    }

                    if (!unicodeCategoryExists)
                    {
                        HybridBody unicodeCategorySet = disassemblySet.HybridBodies.Add();
                        unicodeCategorySet.set_Name(CharUnicodeInfo.GetUnicodeCategory(c).ToString());

                        charSet = unicodeCategorySet.HybridBodies.Add();
                    }

                    // *** END ***

                    // Get the reference of the assemblyShape
                    Reference assemblyShapeRef = catiaPartDocument.PartDocument.Part.CreateReferenceFromObject(assemblyShape);

                    // Create the main contourList
                    List<CatiaContour> catiaContourList = new List<CatiaContour>();
                    List<CatiaSurface> catiaSurfaceList = new List<CatiaSurface>();
                    List<CatiaContour> extContourList = new List<CatiaContour>();
                    Reference planeXYReference = catiaPartDocument.PartDocument.Part.CreateReferenceFromObject(catiaPartDocument.PartDocument.Part.OriginElements.PlaneXY);

                    if (factory.AddNewDatums(assemblyShapeRef).Length > 1)
                    {
                        // Disassemble the assembly into a list of contours
                        List<HybridShape> disassembledShapeList = new List<HybridShape>();
                        DisassembleCurve(catiaPartDocument.PartDocument, assemblyShape, disassembledShapeList);

                        // Smooth and store the list of contours into main contourList
                        foreach (HybridShape shape in disassembledShapeList)
                        {
                            catiaContourList.Add(new CatiaContour(catiaPartDocument.PartDocument, planeXYReference)
                            {
                                Shape = shape
                            });
                        }

                        // Compute the smooth shape and the inner surface area.
                        foreach (CatiaContour catiaContour in catiaContourList)
                        {
                            catiaContour.ComputeInnerSurfaceArea();
                        }

                        // Sort by inner surface area largest to smallest.
                        catiaContourList.Sort((x, y) => x.Area.CompareTo(y.Area));
                        catiaContourList.Reverse();

                        // Append the smoothed shape to the set.
                        charSet.AppendHybridShape(catiaContourList.First().SmoothedShape);

                        catiaSurfaceList.Add(new CatiaSurface(catiaPartDocument.PartDocument)
                        {
                            ExternalContour = catiaContourList.First()
                        });

                        // For each contour in the list of contours
                        for (int i = 1; i < catiaContourList.Count; i++)
                        {
                            // Append in the set the current contour
                            charSet.AppendHybridShape(catiaContourList[i].SmoothedShape);

                            // Get the list of points composing the current contour
                            List<CatiaPoint> pointList = new List<CatiaPoint>();
                            GetCurvePointList(pointList, catiaPartDocument.PartDocument, catiaContourList[i].SmoothedShape);

                            // Create a list to compare current contour with all longer contours than current one.
                            List<CatiaContour> tempContourList = new List<CatiaContour>();
                            for (int k = 0; k < i; k++)
                            {
                                tempContourList.Add(catiaContourList[k]);
                            }

                            int smallestParentContourIndex = -1;

                            // For each contour larger than current one, we check to see if the current contour is INT or EXT
                            for (int j = 0; j < tempContourList.Count; j++)
                            {
                                int nbOfPointInside = 0;

                                // For each point in the point list
                                foreach (CatiaPoint point in pointList)
                                {
                                    point.ComputePointShape();

                                    // Compute the number of intersection with the longest contour
                                    int nbOfIntersecX = GetNumberOfIntersection(point, tempContourList[j].ShapeReference, catiaPartDocument.PartDocument, factory, true);
                                    int nbOfIntersecY = GetNumberOfIntersection(point, tempContourList[j].ShapeReference, catiaPartDocument.PartDocument, factory, false);

                                    // If at least one of the number of intersection is an odd number we are IN the contour
                                    if (nbOfIntersecX % 2 != 0 || nbOfIntersecY % 2 != 0)
                                    {
                                        nbOfPointInside++;
                                    }
                                }

                                // If all the points are inside the contour, it's an interior contour
                                if (nbOfPointInside == pointList.Count)
                                    smallestParentContourIndex = j;
                            }

                            // >--- FILL THE SURFACE COLLECTION ---

                            // If smallestParentContourIndex is not modified, we have an external contour
                            if (smallestParentContourIndex == -1)
                            {
                                catiaSurfaceList.Add(new CatiaSurface(catiaPartDocument.PartDocument)
                                {
                                    ExternalContour = catiaContourList[i]
                                });
                            }
                            else
                            {
                                // Get the parent contour.
                                CatiaContour parentContour = tempContourList[smallestParentContourIndex];

                                // Try to find the parent contour in the list of surfaces' external contour.
                                CatiaSurface tempSurface = catiaSurfaceList.Find((surface) => surface.ExternalContour == parentContour);

                                // If the parent contour has been found, i-contour is internal.
                                if(tempSurface != null)
                                {
                                    tempSurface.InternalContourList.Add(catiaContourList[i]);
                                }
                                // If the parent contour is not found, i-contour is the external contour of a new surface.
                                else
                                {
                                    catiaSurfaceList.Add(new CatiaSurface(catiaPartDocument.PartDocument)
                                    {
                                        ExternalContour = catiaContourList[i]
                                    });
                                }
                            }

                            // >--- END ---
                        }

                        // >--- GIVE NAME TO CONTOURS ---
                        int extCount = 0;
                        foreach (CatiaSurface surface in catiaSurfaceList)
                        {
                            surface.ExternalContour.SmoothedShape.set_Name("Ext." + (extCount + 1).ToString());

                            for (int i = 0; i < surface.InternalContourList.Count; i++)
                            {
                                surface.InternalContourList[i].SmoothedShape.set_Name(surface.ExternalContour.SmoothedShape.get_Name() + "-Int." + (i + 1).ToString());
                            }

                            extCount++;
                        }
                        // >--- END ---



                        // >--- STORE ALL EXTERNAL CONTOURS FOR HEIGHT AND WIDTH COMPUTATION ---
                        foreach (CatiaSurface surface in catiaSurfaceList)
                        {
                            extContourList.Add(surface.ExternalContour);
                        }
                        // >--- END ---

                    }
                    else
                    {
                        HybridShapeCurveExplicit onlyExtContour = factory.AddNewCurveDatum(assemblyShapeRef);
                        onlyExtContour.Compute();

                        CatiaContour catiaContour = new CatiaContour(catiaPartDocument.PartDocument, planeXYReference)
                        {
                            Shape = onlyExtContour
                        };

                        catiaContour.ComputeSmoothedShape();
                        catiaContour.SmoothedShape.set_Name("Ext.1");
                        charSet.AppendHybridShape(catiaContour.SmoothedShape);

                        extContourList.Add(catiaContour);
                    }

                    charSet.set_Name(c.ToString() + " " + GetWidthAndHeight(catiaPartDocument.PartDocument, extContourList, planeXYReference));

                    // >--- DELETE IMPORTED SKETCH AND ASSEMBLY SHAPE ---
                    Selection partDocumentSelection = catiaPartDocument.PartDocument.Selection;
                    partDocumentSelection.Clear();
                    partDocumentSelection.Add(assemblyShape);
                    partDocumentSelection.Add(sketch);

                    partDocumentSelection.Delete();
                    // >--- END ---
                }

                ProgressRate += 1 / (double)font.SupportedCharacterList.Length;
            }

            catiaDrawingDocument.DrawingDocument.Close();

            catiaPartDocument.PartDocument.SaveAs(savePath + GeneratedFont.GetFileNameFormat() + font.Name);
        }

        private string GetWidthAndHeight(PartDocument partDocument, List<CatiaContour> extContourList, Reference supportReference)
        {
            List<CatiaPoint> pointList = new List<CatiaPoint>();

            HybridBody tempSet = partDocument.Part.HybridBodies.Add();

            foreach (CatiaContour contour in extContourList)
            {
                HybridShape tempShape = CatiaGenericShape.CopyShape(contour.ShapeReference, (HybridShapeFactory)partDocument.Part.HybridShapeFactory);
                tempSet.AppendHybridShape(tempShape);
                GetCurvePointList(pointList, partDocument, tempShape);
            }

            HybridShapeFactory factory = (HybridShapeFactory)partDocument.Part.HybridShapeFactory;
            Reference tempSetRef = partDocument.Part.CreateReferenceFromObject(tempSet);
            factory.DeleteObjectForDatum(tempSetRef);

            return "W" + ((GetXWidth(pointList) * 1000).ToString()).Substring(0, 4) + " H" + ((GetYWidth(pointList) * 1000).ToString()).Substring(0, 4);
        }

        /// <summary>
        /// Get the maximum width on the X axis from a point list.
        /// </summary>
        /// <param name="pointList"></param>
        /// <returns></returns>
        private double GetXWidth(List<CatiaPoint> pointList)
        {
            if (pointList.Count != 0)
            {
                double minXValue = 0;
                double maxXvalue = 0;

                foreach (CatiaPoint point in pointList)
                {
                    if (point.X > maxXvalue)
                        maxXvalue = point.X;

                    if (point.X < minXValue)
                        minXValue = point.X;
                }

                return maxXvalue - minXValue;
            }
            else
                return 0;
        }


        /// <summary>
        /// Get the maximum width on the Y axis from a point list.
        /// </summary>
        /// <param name="pointList"></param>
        /// <returns></returns>
        private static double GetYWidth(List<CatiaPoint> pointList)
        {
            double minYValue = 0;
            double maxYValue = 0;

            foreach (CatiaPoint point in pointList)
            {
                if (point.Y > maxYValue)
                    maxYValue = point.Y;

                if (point.Y < minYValue)
                    minYValue = point.Y;
            }

            return maxYValue - minYValue;
        }


        private static int GetNumberOfIntersection(CatiaPoint point, Reference comparedContourRef, PartDocument partDocument, HybridShapeFactory factory, bool direction)
        {
            OriginElements originElement = partDocument.Part.OriginElements;
            Reference normalPlaneRef;

            // Direction = true => direction is X axis
            // Direction = false => direction is Y axis
            if (direction)
                normalPlaneRef = partDocument.Part.CreateReferenceFromObject(originElement.PlaneYZ);
            else
                normalPlaneRef = partDocument.Part.CreateReferenceFromObject(originElement.PlaneZX);

            HybridShapeDirection directionIntersectionLine = factory.AddNewDirection(normalPlaneRef);
            directionIntersectionLine.Compute();

            HybridShapeLinePtDir intersectionLine = factory.AddNewLinePtDir(point.ShapeReference, directionIntersectionLine, 0, 10000, false);
            intersectionLine.Compute();
            Reference intersectionLineRef = partDocument.Part.CreateReferenceFromObject(intersectionLine);


            HybridShapeIntersection intersection = factory.AddNewIntersection(intersectionLineRef, comparedContourRef);
            intersection.Compute();
            Reference intersectionRef = partDocument.Part.CreateReferenceFromObject(intersection);

            if (MarkingGenerator.GetShapeType(partDocument.Part, intersectionRef) != MarkingGenerator.ShapeType.Point)
            {
                return 0;
            }
            else
            {
                try
                {
                    return factory.AddNewDatums(intersectionRef).Length;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }


        /// <summary>
        /// Disassemble a curve into subcurves.
        /// </summary>
        /// <param name="partDocument">PartDocument of the curve to disassemble.</param>
        /// <param name="curveShape">Curve to disassemble.</param>
        /// <param name="disassembledSubCurveList">List where are stored disassembled sub curves.</param>
        public void DisassembleCurve(PartDocument partDocument, HybridShape curveShape, List<HybridShape> disassembledSubCurveList)
        {
            HybridShapeFactory factory = (HybridShapeFactory)partDocument.Part.HybridShapeFactory;

            Selection selectedCurve = partDocument.Selection;
            selectedCurve.Clear();
            selectedCurve.Add(curveShape);
            selectedCurve.Search("Topology.CGMEdge,sel");

            for (int i = 1; i <= selectedCurve.Count; i++)
            {
                SelectedElement subCurveSelectedElement = selectedCurve.Item(i);

                string subCurveName = subCurveSelectedElement.Reference.get_Name().Substring(21, subCurveSelectedElement.Reference.get_Name().Length - 22);

                Reference subCurveRef = partDocument.Part.CreateReferenceFromBRepName(subCurveName, (AnyObject)subCurveSelectedElement.Value);
                HybridShapeCurveExplicit subCurve = factory.AddNewCurveDatum(subCurveRef);
                subCurve.Compute();

                disassembledSubCurveList.Add(subCurve);
            }
        }


        /// <summary>
        /// For each point in the curve, get coordinates and store them in pointList.
        /// </summary>
        /// <param name="pointList">Store the point of the list.</param>
        /// <param name="partDocument">PartDocument of the curve.</param>
        /// <param name="curveShape">Curve to extract the point list.</param>
        public static void GetCurvePointList(List<CatiaPoint> pointList, PartDocument partDocument, HybridShape shape)
        {
            Selection selectedCurve = partDocument.Selection;
            selectedCurve.Clear();
            selectedCurve.Add(shape);
            selectedCurve.Search("Topology.CGMEdge,sel");

            SPATypeLib.SPAWorkbench spaWorkbench = (SPATypeLib.SPAWorkbench)partDocument.GetWorkbench("SPAWorkbench");

            for (int i = 1; i < selectedCurve.Count + 1; i++)
            {
                SelectedElement subCurve = selectedCurve.Item(i);

                SPATypeLib.Measurable measurable = spaWorkbench.GetMeasurable(subCurve.Reference);

                object[] curvePointCoordinateArray = new object[9];
                measurable.GetPointsOnCurve(curvePointCoordinateArray);

                CatiaPoint firstPoint = new CatiaPoint(partDocument, (double)curvePointCoordinateArray.GetValue(0), (double)curvePointCoordinateArray.GetValue(1), (double)curvePointCoordinateArray.GetValue(2));

                pointList.Add(firstPoint);
            }
        }
    }
}
