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
        private bool _isInternalContourListEmpty;
        private double _area;
        private Reference _internalContourListAssembledReference;


        public CatiaContour ExternalContour { get => _externalContour; set => _externalContour = value; }
        public List<CatiaContour> InternalContourList
        {
            get => _internalContourList;
            set
            {
                _internalContourList = value;

                IsInternalContourListEmpty = InternalContourList.Count > 0 ? false : true;
            }
        }
        public bool IsInternalContourListEmpty { get => _isInternalContourListEmpty; set => _isInternalContourListEmpty = value; }

        new public HybridShape Shape
        {
            get => _shape;
            set
            {
                _shape = value;
                ShapeReference = PartDocument.Part.CreateReferenceFromObject(Shape);

                if (GetShapeType(HybridShapeFactory, ShapeReference) == ShapeType.Surface)
                {
                    // Retrieves the surface area
                    SPATypeLib.Measurable measurable = SPAWorkbench.GetMeasurable(ShapeReference);

                    Area = measurable.Area;
                }
                else
                {
                    throw new InvalidShapeException("Shape must be a surface.");
                }
            }
        }

        public double Area { get => _area; set => _area = value; }
        public Reference InternalContourListAssembledReference { get => _internalContourListAssembledReference; set => _internalContourListAssembledReference = value; }
    }
}
