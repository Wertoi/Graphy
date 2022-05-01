using HybridShapeTypeLib;
using INFITF;
using MECMOD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Graphy.Model.CatiaObject.CatiaShape
{
    public class CatiaContour : CatiaGenericShape
    {
        public CatiaContour(PartDocument partDocument, Reference supportReference) : base(partDocument)
        {
            SupportReference = supportReference;
        }

        private Reference _supportReference;
        private PathGeometry _pathGeometry;

        // Modify property Shape
        new public HybridShape Shape
        {
            get => _shape;
            set
            {
                _shape = value;
                ShapeReference = PartDocument.Part.CreateReferenceFromObject(Shape);
            }
        }

        public Reference SupportReference { get => _supportReference; set => _supportReference = value; }
        public PathGeometry PathGeometry { get => _pathGeometry; set => _pathGeometry = value; }


        public void DrawContour(double xCorrectif)
        {
            List<Reference> segmentRefList = new List<Reference>();

            for (int i = 0; i < PathGeometry.Figures.First().Segments.Count; i++)
            {
                PathSegment segment = PathGeometry.Figures.First().Segments[i];

                // *** If the segment is a multi lines segment ***
                if (segment.GetType() == typeof(PolyLineSegment))
                {
                    // Create a list of point
                    List<CatiaPoint> tempPointList = new List<CatiaPoint>();
                    System.Windows.Point startPoint = PathGeometry.Figures.First().StartPoint;
                    tempPointList.Add(new CatiaPoint(PartDocument, startPoint.X - xCorrectif, -startPoint.Y, 0));

                    // For each point in the multi lines segment
                    foreach (System.Windows.Point point in ((PolyLineSegment)segment).Points)
                    {
                        // Get the point and store it in the list.
                        if(point != startPoint)
                        {
                            tempPointList.Add(new CatiaPoint(PartDocument, point.X - xCorrectif, -point.Y, 0));
                        }
                    }

                    // For each point in the list
                    for (int j = 0; j < tempPointList.Count() - 1; j++)
                    {
                        // Create the line between point i and point i+1 and add it to the segment List
                        HybridShape line = HybridShapeFactory.AddNewLinePtPt(tempPointList[j].ShapeReference, tempPointList[j + 1].ShapeReference);
                        line.Compute();
                        segmentRefList.Add(PartDocument.Part.CreateReferenceFromObject(line));
                    }

                    // Create the line between last point and first point to close the contour and add it to the segment list
                    HybridShapeLinePtPt lastLine = HybridShapeFactory.AddNewLinePtPt(tempPointList.Last().ShapeReference, tempPointList.First().ShapeReference);
                    lastLine.Compute();
                    segmentRefList.Add(PartDocument.Part.CreateReferenceFromObject(lastLine));
                }

                /* RETEX: ToleranceType.Relative when getting the PathGeometry seems to only generate PolyLineSegment.
                 * I keep the other segment type generations for information.
                else
                {
                    // *** If the semgnet is a Bezier curve ***
                    if (segment.GetType() == typeof(PolyBezierSegment))
                    {
                        // Create a spline
                        HybridShapeSpline spline = HybridShapeFactory.AddNewSpline();
                        
                        // Create a list of point
                        List<CatiaPoint> tempPointList = new List<CatiaPoint>();

                        // For each point in the multi lines segment
                        foreach (System.Windows.Point point in ((PolyBezierSegment)segment).Points)
                        {
                            // Get the point and store it in the list
                            tempPointList.Add(new CatiaPoint(PartDocument, point.X, -point.Y, 0));
                            tempPointList.Last().ComputePointShape();

                            // Add the point to the spline
                            spline.AddPoint(tempPointList.Last().ShapeReference);
                            spline.Compute();
                        }

                        spline.SetClosing(1);

                        // Add the spline to the segment list
                        segmentRefList.Add(PartDocument.Part.CreateReferenceFromObject(spline));
                    }
                    else
                    {
                        // *** If the segment is a simple line ***
                        if (segment.GetType() == typeof(LineSegment))
                        {
                            CatiaPoint startPoint = new CatiaPoint(PartDocument);
                            if(i != 0)
                            {
                                PathSegment previousSegment = PathGeometry.Figures.First().Segments[i - 1];
                                if (previousSegment.GetType() == typeof(PolyLineSegment))
                                {
                                    System.Windows.Point polyLineSegmentLastPoint = ((PolyLineSegment)previousSegment).Points.Last();
                                    startPoint.X = polyLineSegmentLastPoint.X;
                                    startPoint.Y = -polyLineSegmentLastPoint.Y;
                                    startPoint.Z = 0;
                                    startPoint.ComputePointShape();
                                }
                                else
                                {
                                    if(previousSegment.GetType() == typeof(PolyBezierSegment))
                                    {
                                        System.Windows.Point polyBezierSegmentLastPoint = ((PolyBezierSegment)previousSegment).Points.Last();
                                        startPoint.X = polyBezierSegmentLastPoint.X;
                                        startPoint.Y = -polyBezierSegmentLastPoint.Y;
                                        startPoint.Z = 0;
                                        startPoint.ComputePointShape();
                                    }
                                    else if(segment.GetType() == typeof(LineSegment))
                                    {
                                        System.Windows.Point lineSegmentPoint = ((LineSegment)previousSegment).Point;
                                        startPoint.X = lineSegmentPoint.X;
                                        startPoint.Y = -lineSegmentPoint.Y;
                                        startPoint.Z = 0;
                                        startPoint.ComputePointShape();
                                    }
                                }
                                    
                            }
                            else
                            {
                                System.Windows.Point segmentStartPoint = PathGeometry.Figures.First().StartPoint;
                                startPoint.X = segmentStartPoint.X;
                                startPoint.Y = -segmentStartPoint.Y;
                                startPoint.Z = 0;
                                startPoint.ComputePointShape();
                            }

                            System.Windows.Point point = ((LineSegment)segment).Point;
                            CatiaPoint lastPoint = new CatiaPoint(PartDocument, point.X, -point.Y, 0);
                            lastPoint.ComputePointShape();

                            // Create the line between last point and first point to close the contour and add it to the segment list
                            HybridShapeLinePtPt line = HybridShapeFactory.AddNewLinePtPt(startPoint.ShapeReference, lastPoint.ShapeReference);
                            line.Compute();
                            segmentRefList.Add(PartDocument.Part.CreateReferenceFromObject(line));

                        }
                    }
                }*/
            }

            HybridShapeAssemble assemblyShape = HybridShapeFactory.AddNewJoin(segmentRefList.First(), segmentRefList[1]);
            for (int i = 2; i < segmentRefList.Count; i++)
            {
                assemblyShape.AddElement(segmentRefList[i]);
            }

            assemblyShape.Compute();

            Shape = assemblyShape;
        }

        public void Translate(double xValue, double yValue, double zValue, CatiaAxisSystem axisSystem, bool appendInSet, HybridBody set = null)
        {
            HybridShapeTranslate translatedShape = HybridShapeFactory.AddNewEmptyTranslate();
            translatedShape.ElemToTranslate = ShapeReference;
            translatedShape.VectorType = 2;
            translatedShape.RefAxisSystem = axisSystem.SystemReference;

            // I do not know why but coordinate values are multiplied by 1000 by Catia so we must divide it by 1000.
            translatedShape.CoordXValue = xValue / 1000;
            translatedShape.CoordYValue = yValue / 1000;
            translatedShape.CoordZValue = zValue / 1000;
            translatedShape.VolumeResult = false;

            translatedShape.Compute();
            Shape = (HybridShape)translatedShape;

            // Add to set and hide it.
            if (appendInSet)
            {
                set.AppendHybridShape(Shape);
                HybridShapeFactory.GSMVisibility(ShapeReference, 0);
            }
        }

        public void Scale(double scaleRatio, CatiaPoint scaleCenterPoint, bool appendInSet, HybridBody set = null)
        {
            HybridShapeScaling scaledShape = HybridShapeFactory.AddNewHybridScaling(ShapeReference, scaleCenterPoint.ShapeReference, scaleRatio);
            scaledShape.Compute();
            Shape = (HybridShape)scaledShape;

            // Add to set and hide it.
            if (appendInSet)
            {
                set.AppendHybridShape(Shape);
                HybridShapeFactory.GSMVisibility(ShapeReference, 0);
            }
        }

        public void Move(CatiaAxisSystem referenceAxisSystem, CatiaAxisSystem targetAxisSystemRef, bool appendInSet, HybridBody set = null)
        {
            HybridShapeAxisToAxis movedShape = HybridShapeFactory.AddNewAxisToAxis(ShapeReference, referenceAxisSystem.SystemReference, targetAxisSystemRef.SystemReference);
            movedShape.Compute();
            Shape = (HybridShape)movedShape;

            // Add to set and hide it.
            if (appendInSet)
            {
                set.AppendHybridShape(Shape);
                HybridShapeFactory.GSMVisibility(ShapeReference, 0);
            }
        }

        public void Project(CatiaSurface projectionSurface, bool appendInSet, HybridBody set = null)
        {
            HybridShapeProject projectedShape = HybridShapeFactory.AddNewProject(ShapeReference, projectionSurface.ShapeReference);
            projectedShape.Compute();
            Shape = (HybridShape)projectedShape;

            // Add to set and hide it.
            if (appendInSet)
            {
                set.AppendHybridShape(Shape);
                HybridShapeFactory.GSMVisibility(ShapeReference, 0);
            }
        }


        public CatiaContour Copy()
        {
            CatiaContour copyContour = new CatiaContour(PartDocument, SupportReference);

            if (ShapeReference != null)
            {
                HybridShape copyShape = (HybridShape)HybridShapeFactory.AddNewCurveDatum(ShapeReference);
                copyShape.Compute();

                copyContour.Shape = copyShape;
            }

            copyContour.PathGeometry = PathGeometry.Clone();

            return copyContour;
        }

    }
}
