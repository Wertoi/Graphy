using HybridShapeTypeLib;
using INFITF;
using MECMOD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Graphy.Model.CatiaShape
{
    public class CatiaContour : CatiaGenericShape
    {
        public CatiaContour(PartDocument partDocument, Reference supportReference) : base(partDocument)
        {
            SupportReference = supportReference;
        }

        private Reference _supportReference;
        private HybridShape _smoothedShape;
        private Reference _smoothedShapeReference;
        private bool _isSmoothedShapeComputed = false;
        private double _area;
        private PathGeometry _pathGeometry;

        // Modify property Shape
        new public HybridShape Shape
        {
            get => _shape;
            set
            {
                _shape = value;
                ShapeReference = PartDocument.Part.CreateReferenceFromObject(Shape);

                IsSmoothedShapeComputed = false;
            }
        }

        public HybridShape SmoothedShape
        {
            get => _smoothedShape;
            set
            {
                _smoothedShape = value;
                SmoothedShapeReference = PartDocument.Part.CreateReferenceFromObject(SmoothedShape);
            }
        }

        public Reference SmoothedShapeReference { get => _smoothedShapeReference; set => _smoothedShapeReference = value; }
        public Reference SupportReference { get => _supportReference; set => _supportReference = value; }
        public double Area { get => _area; set => _area = value; }
        public PathGeometry PathGeometry { get => _pathGeometry; set => _pathGeometry = value; }
        public bool IsSmoothedShapeComputed { get => _isSmoothedShapeComputed; set => _isSmoothedShapeComputed = value; }

        public void ComputeSmoothedShape()
        {
            // Smooth the sub curve
            HybridShapeCurveSmooth smoothSubCurve = HybridShapeFactory.AddNewCurveSmooth(ShapeReference);
            smoothSubCurve.CorrectionMode = 3;
            smoothSubCurve.SetMaximumDeviation(0.01);
            smoothSubCurve.TopologySimplificationActivity = true;
            smoothSubCurve.MaximumDeviationActivity = true;
            smoothSubCurve.Support = SupportReference;
            smoothSubCurve.Compute();

            // Create a curve without historic to avoid huge data storage
            Reference smoothSubCurveRef = PartDocument.Part.CreateReferenceFromObject(smoothSubCurve);
            HybridShapeCurveExplicit smoothSubCurveWithoutHistoric = HybridShapeFactory.AddNewCurveDatum(smoothSubCurveRef);
            smoothSubCurveWithoutHistoric.Compute();

            SmoothedShape = (HybridShape)smoothSubCurveWithoutHistoric;

            IsSmoothedShapeComputed = true;
        }


        public void DrawContour(HybridBody set)
        {
            List<Reference> segmentRefList = new List<Reference>();

            for(int i = 0; i < PathGeometry.Figures.First().Segments.Count; i++)
            {
                PathSegment segment = PathGeometry.Figures.First().Segments[i];

                // *** If the segment is a multi lines segment ***
                if (segment.GetType() == typeof(PolyLineSegment))
                {
                    // Create a list of point
                    List<CatiaPoint> tempPointList = new List<CatiaPoint>();

                    // For each point in the multi lines segment
                    foreach (System.Windows.Point point in ((PolyLineSegment)segment).Points)
                    {
                        // Get the point and store it in the list
                        tempPointList.Add(new CatiaPoint(PartDocument, point.X, -point.Y, 0));
                        tempPointList.Last().ComputePointShape();
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
            ComputeSmoothedShape();

            set.AppendHybridShape(SmoothedShape);
        }


        public void Scale(double scaleRatio, Reference scaleCenterPointRef)
        {
            HybridShapeScaling scaledShape;
            if (IsSmoothedShapeComputed)
                scaledShape = HybridShapeFactory.AddNewHybridScaling(SmoothedShapeReference, scaleCenterPointRef, scaleRatio);
            else
                scaledShape = HybridShapeFactory.AddNewHybridScaling(ShapeReference, scaleCenterPointRef, scaleRatio);

            scaledShape.Compute();
            Shape = (HybridShape)scaledShape;
        }

        public void Move(Reference referenceAxisRef, Reference targetAxisRef)
        {
            HybridShapeAxisToAxis movedShape;
            if (IsSmoothedShapeComputed)
                movedShape = HybridShapeFactory.AddNewAxisToAxis(SmoothedShapeReference, referenceAxisRef, targetAxisRef);
            else
                movedShape = HybridShapeFactory.AddNewAxisToAxis(ShapeReference, referenceAxisRef, targetAxisRef);

            movedShape.Compute();
            Shape = (HybridShape)movedShape;
        }

        public void Project(Reference projectionSurfaceRef)
        {
            HybridShapeProject projectedShape;
            if (IsSmoothedShapeComputed)
                projectedShape = HybridShapeFactory.AddNewProject(SmoothedShapeReference, projectionSurfaceRef);
            else
                projectedShape = HybridShapeFactory.AddNewProject(ShapeReference, projectionSurfaceRef);

            projectedShape.Compute();
            Shape = (HybridShape)projectedShape;
        }


        public CatiaContour Copy()
        {
            CatiaContour copyContour = new CatiaContour(PartDocument, SupportReference);

            if(ShapeReference != null)
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
