using HybridShapeTypeLib;
using INFITF;
using MECMOD;
using SPATypeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphy.Model.CatiaObject.CatiaShape
{
    public class CatiaGenericShape
    {
        // CONSTRUCTOR
        public CatiaGenericShape(PartDocument partDocument)
        {
            PartDocument = partDocument;
        }

        // PRIVATE CONST ATTRIBUTS
        private const string SPAWORKBENCH_NAME = "SPAWorkbench";

        // ATTRIBUTS
        private PartDocument _partDocument;
        protected HybridShape _shape;
        private Reference _shapeReference;
        private SPAWorkbench _SPAWorkbench;
        private HybridShapeFactory _hybridShapeFactory;

        // PROPERTIES
        public PartDocument PartDocument
        {
            get => _partDocument;
            set
            {
                _partDocument = value;
                SPAWorkbench = (SPAWorkbench)PartDocument.GetWorkbench(SPAWORKBENCH_NAME);
                HybridShapeFactory = (HybridShapeFactory)PartDocument.Part.HybridShapeFactory;
            }
        }

        public HybridShape Shape
        {
            get => _shape;
            set
            {
                _shape = value;
                ShapeReference = PartDocument.Part.CreateReferenceFromObject(Shape);
            }
        }

        public Reference ShapeReference { get => _shapeReference; set => _shapeReference = value; }
        public SPAWorkbench SPAWorkbench { get => _SPAWorkbench; set => _SPAWorkbench = value; }
        public HybridShapeFactory HybridShapeFactory { get => _hybridShapeFactory; set => _hybridShapeFactory = value; }


        // EQUALS OVERRIDE
        public override bool Equals(object obj)
        {
            // Is null?
            if (obj is null)
            {
                return false;
            }

            // Is the same object?
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            // Is the same type?
            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            CatiaGenericShape genericshape = (CatiaGenericShape)obj;
            return (Shape == genericshape.Shape);
        }

        // GETHASHCODE OVERRIDE
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
