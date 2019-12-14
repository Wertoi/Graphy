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
        private double _area;
        private System.Windows.Media.PathGeometry _pathGeometry;

        // Modify property Shape
        new public HybridShape Shape
        {
            get => _shape;
            set
            {
                _shape = value;
                ShapeReference = PartDocument.Part.CreateReferenceFromObject(Shape);

                if (GetShapeType(HybridShapeFactory, ShapeReference) == ShapeType.Curve || GetShapeType(HybridShapeFactory, ShapeReference) == ShapeType.Circle)
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
                }
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

        public void ComputeSmoothedShape()
        {
            // Smooth the sub curve
            HybridShapeCurveSmooth smoothSubCurve = HybridShapeFactory.AddNewCurveSmooth(ShapeReference);
            smoothSubCurve.CorrectionMode = 3;
            smoothSubCurve.SetMaximumDeviation(0.001);
            smoothSubCurve.TopologySimplificationActivity = true;
            smoothSubCurve.MaximumDeviationActivity = true;
            smoothSubCurve.Support = SupportReference;
            smoothSubCurve.Compute();

            // Create a curve without historic to avoid huge data storage
            Reference smoothSubCurveRef = PartDocument.Part.CreateReferenceFromObject(smoothSubCurve);
            HybridShapeCurveExplicit smoothSubCurveWithoutHistoric = HybridShapeFactory.AddNewCurveDatum(smoothSubCurveRef);
            smoothSubCurveWithoutHistoric.Compute();

            SmoothedShape = (HybridShape)smoothSubCurveWithoutHistoric;
        }

        public void ComputeInnerSurfaceArea()
        {
            if(SmoothedShape == null)
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
