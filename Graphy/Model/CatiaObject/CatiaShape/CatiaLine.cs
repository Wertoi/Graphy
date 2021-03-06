using HybridShapeTypeLib;
using INFITF;
using MECMOD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Graphy.Model.CatiaObject.CatiaShape
{
    public class CatiaLine : CatiaGenericShape
    {
        /// <summary>
        /// Create the normal line to a surface at a defined point.
        /// </summary>
        /// <param name="partDocument"></param>
        /// <param name="surface"></param>
        /// <param name="point"></param>
        /// <param name="beginOffset"></param>
        /// <param name="endOffset"></param>
        /// <param name="direction"></param>
        public CatiaLine(PartDocument partDocument, CatiaSurface surface, CatiaPoint point,
            double beginOffset = 0, double endOffset = 10, bool direction = false) : base(partDocument)
        {
            HybridShapeLineNormal tempShape = HybridShapeFactory.AddNewLineNormal(surface.ShapeReference, point.ShapeReference, beginOffset, endOffset, direction);
            tempShape.Compute();

            // Compute the direction
            tempShape.GetDirection(LineDirection);

            // Store the shape
            Shape = (HybridShape)tempShape;
        }

        /// <summary>
        /// Create the line angle to a spline at de define point on a plane.
        /// </summary>
        /// <param name="partDocument"></param>
        /// <param name="curve"></param>
        /// <param name="supportPlaneRef"></param>
        /// <param name="point"></param>
        public CatiaLine(PartDocument partDocument, CatiaCurve curve, Reference supportPlaneRef, CatiaPoint point, double angle = 90,
            double beginOffset = 0, double endOffset = 10, bool direction = false) : base(partDocument)
        {
            HybridShapeLineAngle tempShape = HybridShapeFactory.AddNewLineAngle(curve.ShapeReference, supportPlaneRef, point.ShapeReference, false,
                beginOffset, endOffset, angle, direction);
            tempShape.Compute();

            // Compute the direction
            tempShape.GetDirection(LineDirection);
            
            // Store the shape
            Shape = (HybridShape)tempShape;
        }


        public CatiaLine(PartDocument partDocument, CatiaCurve spline, CatiaPoint point,
            double beginOffset = 0, double endOffset = 10, bool direction = false) : base(partDocument)
        {
            HybridShapeLineTangency tempShape = HybridShapeFactory.AddNewLineTangency(spline.ShapeReference, point.ShapeReference,
                beginOffset, endOffset, direction);
            tempShape.Compute();

            // Compute the direction
            tempShape.GetDirection(LineDirection);

            // Store the shape
            Shape = (HybridShape)tempShape;
        }

        private object[] _lineDirection = new object[3];

        public object[] LineDirection { get => _lineDirection; set => _lineDirection = value; }
    }
}
