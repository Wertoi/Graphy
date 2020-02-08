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

        private double _perimeter;
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

               /* if (GetShapeType(HybridShapeFactory, ShapeReference) == ShapeType.Curve || GetShapeType(HybridShapeFactory, ShapeReference) == ShapeType.Circle)
                {
                    // Check if the contour is closed
                    if (IsContourClosed(PartDocument, HybridShapeFactory, ShapeReference, SupportReference))
                    {
                        SPATypeLib.Measurable measurable = SPAWorkbench.GetMeasurable(ShapeReference);

                        // Retrieves the contour perimeter
                        Perimeter = measurable.Length;
                    }
                    else
                    {
                        throw new ContourNotClosed("Contour is not closed.");
                    }
                }
                else
                {
                    throw new InvalidShapeException("Shape must be a curve.");
                }*/
            }
        }

        public double Perimeter { get => _perimeter; set => _perimeter = value; }

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

        public void ComputeInnerSurfaceArea()
        {
            if(!IsSmoothedShapeComputed)
            {
                ComputeSmoothedShape();
            }

            HybridShape tempShape = CopyShape(SmoothedShapeReference, HybridShapeFactory);
            Reference tempShapeReference = PartDocument.Part.CreateReferenceFromObject(tempShape);

            // Create a filled contour from tempShape
            HybridShapeFill filledContour = HybridShapeFactory.AddNewFill();
            filledContour.AddBound(tempShapeReference);
            filledContour.AddSupportAtBound(tempShapeReference, SupportReference);
            filledContour.Compute();

            Reference filledContourReference = PartDocument.Part.CreateReferenceFromObject(filledContour);

            SPATypeLib.Measurable measurable = SPAWorkbench.GetMeasurable(filledContourReference);

            Area = measurable.Area;
        }


        public static bool IsContourClosed(PartDocument partDocument, HybridShapeFactory hybridShapeFactory, Reference shapeReference, Reference supportReference)
        {
            HybridShape tempShape = CopyShape(shapeReference, hybridShapeFactory);
            Reference tempShapeReference = partDocument.Part.CreateReferenceFromObject(tempShape);

            try
            {
                // Create a filled contour from tempShape
                HybridShapeFill filledContour = hybridShapeFactory.AddNewFill();
                filledContour.AddBound(tempShapeReference);
                filledContour.AddSupportAtBound(tempShapeReference, supportReference);
                filledContour.Compute();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }


        public void DrawContour(HybridBody set)
        {
            List<Reference> lineList = new List<Reference>();

            foreach (PathSegment segment in PathGeometry.Figures.First().Segments)
            {
                if (segment.GetType() == typeof(PolyLineSegment))
                {
                    List<CatiaPoint> tempPointList = new List<CatiaPoint>();
                    foreach (System.Windows.Point point in ((PolyLineSegment)segment).Points)
                    {
                        tempPointList.Add(new CatiaPoint(PartDocument, point.X, -point.Y, 0));
                        tempPointList.Last().ComputePointShape();
                    }


                    for (int i = 0; i < tempPointList.Count() - 1; i++)
                    {
                        HybridShape line = HybridShapeFactory.AddNewLinePtPt(tempPointList[i].ShapeReference, tempPointList[i + 1].ShapeReference);

                        line.Compute();
                        lineList.Add(PartDocument.Part.CreateReferenceFromObject(line));
                    }

                    HybridShapeLinePtPt lastLine = HybridShapeFactory.AddNewLinePtPt(tempPointList.Last().ShapeReference, tempPointList.First().ShapeReference);
                    lastLine.Compute();
                    lineList.Add(PartDocument.Part.CreateReferenceFromObject(lastLine));
                }
                /*else
                {
                    if (segment.GetType() == typeof(PolyBezierSegment))
                    {
                        HybridShapeSpline curve = HybridShapeFactory.AddNewSpline();
                        foreach (System.Windows.Point point in ((PolyBezierSegment)segment).Points)
                        {
                            pointList.Add(new CatiaPoint(PartDocument, point.X, point.Y, 0));
                            pointList.Last().ComputePointShape();

                            pointReferenceList.Add(pointShapeRef);

                            curve.AddPoint(pointList.Last().ShapeReference);
                            curve.Compute();
                        }

                        tempSet.AppendHybridShape(curve);
                    }
                    else
                    {
                        if (segment.GetType() == typeof(System.Windows.Media.LineSegment))
                        {
                            Reference startPointShapeRef;
                            if (pointReferenceList.Count != 0)
                            {
                                startPointShapeRef = pointReferenceList.Last();
                            }
                            else
                            {
                                System.Windows.Point startPoint = character.PathGeometry.Figures.First().StartPoint;
                                HybridShape startPointShape = hybridShapeFactory.AddNewPointCoord(startPoint.X, startPoint.Y, 0);
                                startPointShape.Compute();
                                startPointShapeRef = partDocument.Part.CreateReferenceFromObject(startPointShape);

                                pointReferenceList.Add(startPointShapeRef);
                            }

                            System.Windows.Point point = ((System.Windows.Media.LineSegment)segment).Point;
                            HybridShape pointShape = hybridShapeFactory.AddNewPointCoord(point.X, point.Y, 0);
                            pointShape.Compute();
                            Reference pointShapeRef = partDocument.Part.CreateReferenceFromObject(pointShape);

                            pointReferenceList.Add(pointShapeRef);

                            HybridShape line = hybridShapeFactory.AddNewLinePtPt(startPointShapeRef, pointShapeRef);
                            lineList.Add(partDocument.Part.CreateReferenceFromObject(line));
                        }
                    }
                }*/
            }

            HybridShapeAssemble assemblyShape = HybridShapeFactory.AddNewJoin(lineList.First(), lineList[1]);
            for (int i = 2; i < lineList.Count; i++)
            {
                assemblyShape.AddElement(lineList[i]);
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


        public class ContourNotClosed : Exception
        {
            public ContourNotClosed()
            {
            }

            public ContourNotClosed(string message)
                : base(message)
            {
            }

            public ContourNotClosed(string message, Exception inner)
                : base(message, inner)
            {
            }
        }
    }
}
