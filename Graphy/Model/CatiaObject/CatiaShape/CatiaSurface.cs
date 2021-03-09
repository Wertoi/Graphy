using HybridShapeTypeLib;
using INFITF;
using MECMOD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphy.Model.CatiaObject.CatiaShape
{
    public class CatiaSurface : CatiaGenericShape
    {
        public CatiaSurface(PartDocument partDocument) : base(partDocument)
        {
            InternalContourList = new List<CatiaContour>();
        }

        public CatiaSurface(PartDocument partDocument, CatiaCurve curve, CatiaSurface supportSurface, double height) : base(partDocument)
        {
            InternalContourList = new List<CatiaContour>();

            HybridShapeSweepLine tempShape = HybridShapeFactory.AddNewSweepLine(curve.ShapeReference);
            tempShape.FirstGuideSurf = supportSurface.ShapeReference;
            tempShape.Mode = 4;
            tempShape.SetFirstLengthLaw(height / 2, 0d, 0);
            tempShape.SetSecondLengthLaw(height / 2, 0d, 0);
            tempShape.SetAngle(1, 0d);
            tempShape.SolutionNo = 0;
            tempShape.SmoothActivity = false;
            tempShape.GuideDeviationActivity = false;
            tempShape.SetbackValue = 0.02;
            tempShape.FillTwistedAreas = 1;
            tempShape.Compute();

            Shape = (HybridShape)tempShape;
        }


        private CatiaContour _externalContour;
        private List<CatiaContour> _internalContourList;

        public CatiaContour ExternalContour { get => _externalContour; set => _externalContour = value; }
        public List<CatiaContour> InternalContourList
        {
            get => _internalContourList;
            set => _internalContourList = value;
        }


        public CatiaSurface Copy()
        {
            CatiaSurface copySurface = new CatiaSurface(PartDocument)
            {
                ExternalContour = ExternalContour.Copy()
            };

            foreach (CatiaContour internalContour in InternalContourList)
            {
                copySurface.InternalContourList.Add(internalContour.Copy());
            }

            if(ShapeReference != null)
            {
                HybridShape copyShape = (HybridShape)HybridShapeFactory.AddNewSurfaceDatum(ShapeReference);
                copyShape.Compute();

                copySurface.Shape = copyShape;
            }

            return copySurface;
        }

        public HybridShapePlaneTangent GetTangentPlane(CatiaPoint point)
        {
            HybridShapePlaneTangent tangentPlane = HybridShapeFactory.AddNewPlaneTangent(ShapeReference, point.ShapeReference);
            tangentPlane.Compute();

            return tangentPlane;
        }

        public CatiaLine GetNaturalNormalLine(CatiaPoint point)
        {
            return new CatiaLine(PartDocument, this, point, 0, 10, false);
        }

        public CatiaSurface GetSurfaceWithoutLink()
        {
            HybridShapeSurfaceExplicit tempSurface = HybridShapeFactory.AddNewSurfaceDatum(ShapeReference);
            tempSurface.Compute();

            CatiaSurface surfaceWithoutLink = new CatiaSurface(PartDocument)
            {
                Shape = tempSurface
            };

            return surfaceWithoutLink;
        }

        public void ComputeSurface(Reference projectionSurfaceRef)
        {
            // Initialize splitOrientation
            int splitOrientation = 1;

            // Try a split
            HybridShapeSplit exteriorSplitSurface = HybridShapeFactory.AddNewHybridSplit(projectionSurfaceRef, ExternalContour.ShapeReference, splitOrientation);
            exteriorSplitSurface.Compute();
            Reference exteriorSplitSurfaceRef = PartDocument.Part.CreateReferenceFromObject(exteriorSplitSurface);

            // Check the split, if NOK invert the split
            if (!IsSplitOrientationOK(PartDocument, exteriorSplitSurfaceRef, ExternalContour.ShapeReference, HybridShapeFactory, projectionSurfaceRef))
            {
                exteriorSplitSurface.Orientation = -splitOrientation;
                exteriorSplitSurface.Compute();
            }
            else
                splitOrientation = -1;

            exteriorSplitSurface.Compute();

            // Create interior splits
            if (InternalContourList.Count > 0)
            {
                Reference internalContourListAssembledRef;

                // Assemble interior contours
                if (InternalContourList.Count > 1)
                {
                    HybridShapeAssemble interiorContourAssy = HybridShapeFactory.AddNewJoin(InternalContourList[0].ShapeReference, InternalContourList[1].ShapeReference);
                    interiorContourAssy.SetConnex(false);
                    foreach (CatiaContour intContour in InternalContourList)
                    {
                        interiorContourAssy.AddElement(intContour.ShapeReference);
                    }

                    interiorContourAssy.Compute();
                    internalContourListAssembledRef = PartDocument.Part.CreateReferenceFromObject(interiorContourAssy);
                }
                else
                {
                    internalContourListAssembledRef = InternalContourList[0].ShapeReference;
                }


                // Cut surface result by interior contours
                Shape = HybridShapeFactory.AddNewHybridSplit(exteriorSplitSurfaceRef, internalContourListAssembledRef, -splitOrientation);
                Shape.Compute();

            }
            else
            {
                Shape = exteriorSplitSurface;
            }

        }


        /// <summary>
        /// Check if the orientation of the split should be inverted by comparing split surface area and the character filled surface area.
        /// </summary>
        /// <param name="partDocument">Part of the working document.</param>
        /// <param name="cutSurfaceRef">Reference of the splited surface.</param>
        /// <param name="contourRef">Reference of the contour which split the surface.</param>
        /// <param name="factory">HybridShapeFactory of the working part.</param>
        /// <param name="supportRef">Reference of the surface before split.</param>
        /// <returns></returns>
        private bool IsSplitOrientationOK(PartDocument partDocument, Reference cutSurfaceRef, Reference contourRef, HybridShapeFactory factory, Reference supportRef)
        {
            SPATypeLib.Measurable measurable = SPAWorkbench.GetMeasurable(cutSurfaceRef);
            double cutArea = measurable.Area;

            // Create a filled contour
            HybridShapeFill filledContour = factory.AddNewFill();
            filledContour.AddBound(contourRef);
            filledContour.AddSupportAtBound(contourRef, supportRef);
            filledContour.Compute();

            Reference filledContourRef = partDocument.Part.CreateReferenceFromObject(filledContour);

            SPATypeLib.Measurable measurable2 = SPAWorkbench.GetMeasurable(filledContourRef);
            double charArea = measurable2.Area;

            // If the difference between areas are equal with a marge of 10% => OK
            if (Math.Abs(cutArea - charArea) < 0.1 * charArea)
                return true;
            else
                return false;
        }
    }
}
