using HybridShapeTypeLib;
using INFITF;
using MECMOD;
using SPATypeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphy.Model.CatiaShape
{
    public class CatiaGenericShape
    {
        // ENUMS
        public enum ShapeType
        {
            Unknown = 0,
            Point = 1,
            Curve = 2,
            Line = 3,
            Circle = 4,
            Surface = 5,
            Plane = 6,
            Solid = 7,
            AxisSystem = 12
        };

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

        public static ShapeType GetShapeType(HybridShapeFactory hybridShapeFactory, Reference shapeReference)
        {
            return (ShapeType)hybridShapeFactory.GetGeometricalFeatureType(shapeReference);
        }

        public static HybridShape CopyShape(Reference shapeReference,  HybridShapeFactory hybridShapeFactory)
        {
            HybridShape shapeCopy = (HybridShape)hybridShapeFactory.AddNewCurveDatum(shapeReference);
            shapeCopy.Compute();

            return shapeCopy;
        }

        // EQUALS OVERRIDE
        public override bool Equals(object obj)
        {
            // Is null?
            if (Object.ReferenceEquals(null, obj))
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

        // 
        public class InvalidShapeException : Exception
        {
            public InvalidShapeException()
            {
            }

            public InvalidShapeException(string message)
                : base(message)
            {
            }

            public InvalidShapeException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }
    }
}
