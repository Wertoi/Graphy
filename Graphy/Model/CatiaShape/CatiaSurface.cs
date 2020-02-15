using HybridShapeTypeLib;
using INFITF;
using MECMOD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphy.Model.CatiaShape
{
    public class CatiaSurface : CatiaGenericShape
    {
        public CatiaSurface(PartDocument partDocument) : base(partDocument)
        {
            InternalContourList = new List<CatiaContour>();
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
            CatiaSurface copySurface = new CatiaSurface(PartDocument);
            copySurface.ExternalContour = ExternalContour.Copy();
            foreach(CatiaContour internalContour in InternalContourList)
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




        public void ComputeSurface(Reference projectionSurfaceRef)
        {
            // Initialize splitOrientation
            int splitOrientation = 1;

            // Create exterior split surface
            HybridShapeSplit exteriorSplitSurface;

            HybridShapeSplit tempSplit = HybridShapeFactory.AddNewHybridSplit(projectionSurfaceRef, ExternalContour.ShapeReference, splitOrientation);
            tempSplit.Compute();
            Reference tempSplitRef = PartDocument.Part.CreateReferenceFromObject(tempSplit);


            if (!IsSplitOrientationOK(PartDocument, tempSplitRef, ExternalContour.ShapeReference, HybridShapeFactory, projectionSurfaceRef))
            {
                exteriorSplitSurface = HybridShapeFactory.AddNewHybridSplit(projectionSurfaceRef, ExternalContour.ShapeReference, -splitOrientation);
            }
            else
            {
                exteriorSplitSurface = HybridShapeFactory.AddNewHybridSplit(projectionSurfaceRef, ExternalContour.ShapeReference, splitOrientation);
                splitOrientation = -1;
            }

            exteriorSplitSurface.Compute();


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
                Reference exteriorSplitSurfaceRef = PartDocument.Part.CreateReferenceFromObject(exteriorSplitSurface);
                Shape = HybridShapeFactory.AddNewHybridSplit(exteriorSplitSurfaceRef, internalContourListAssembledRef, -splitOrientation);
                Shape.Compute();

                // Hide
                HybridShapeFactory.GSMVisibility(internalContourListAssembledRef, 0);
                HybridShapeFactory.GSMVisibility(exteriorSplitSurfaceRef, 0);
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
        /// <param name="characterContourRef">Reference of the contour which split the surface.</param>
        /// <param name="factory">HybridShapeFactory of the working part.</param>
        /// <param name="supportRef">Reference of the surface before split.</param>
        /// <returns></returns>
        private bool IsSplitOrientationOK(PartDocument partDocument, Reference cutSurfaceRef, Reference characterContourRef, HybridShapeFactory factory, Reference supportRef)
        {
            SPATypeLib.Measurable measurable = SPAWorkbench.GetMeasurable(cutSurfaceRef);
            double cutArea = measurable.Area;

            // Create a filled contour
            HybridShapeFill filledContour = factory.AddNewFill();
            filledContour.AddBound(characterContourRef);
            filledContour.AddSupportAtBound(characterContourRef, supportRef);
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
