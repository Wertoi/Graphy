using HybridShapeTypeLib;
using MECMOD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphy.Model.CatiaShape
{
    public class CatiaPoint : CatiaGenericShape
    {
        public CatiaPoint(PartDocument partDocument) : base(partDocument)
        {

        }

        public CatiaPoint(PartDocument partDocument, double x, double y, double z) : base(partDocument)
        {
            X = x;
            Y = y;
            Z = z;
        }

        private double _x;
        private double _y;
        private double _z;

        public double X { get => _x; set => _x = value; }
        public double Y { get => _y; set => _y = value; }
        public double Z { get => _z; set => _z = value; }


        new public HybridShape Shape
        {
            get => _shape;
            set
            {
                _shape = value;
                ShapeReference = PartDocument.Part.CreateReferenceFromObject(Shape);

                // Retrieves the point coordinates
                SPATypeLib.Measurable measurable = SPAWorkbench.GetMeasurable(ShapeReference);

                object[] pointCoordinate = new object[3];
                measurable.GetPoint(pointCoordinate);

                X = (double)pointCoordinate[0];
                Y = (double)pointCoordinate[1];
                Z = (double)pointCoordinate[2];
            }
        }

        public void ComputePointShape()
        {
            HybridShape tempShape = (HybridShape)HybridShapeFactory.AddNewPointCoord(X, Y, Z);
            tempShape.Compute();
            Shape = tempShape;
        }

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

            CatiaPoint point = (CatiaPoint)obj;
            return (X == point.X) && (Y == point.Y) && (Z == point.Z);
        }

        // GETHASHCODE OVERRIDE
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
