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
    public class CatiaCurve : CatiaGenericShape
    {
        public CatiaCurve(PartDocument partDocument, CatiaSurface supportSurface) : base(partDocument)
        {
            SupportSurface = supportSurface;
        }


        /// <summary>
        /// Generate a curve from a list of points on a support surface.
        /// </summary>
        /// <param name="partDocument"></param>
        /// <param name="pointList"></param>
        /// <param name="supportSurface"></param>
        public CatiaCurve(PartDocument partDocument, List<CatiaPoint> pointList, CatiaSurface supportSurface) : base(partDocument)
        {
            PointList = pointList;
            SupportSurface = supportSurface;

            HybridShapeSpline tempShape = HybridShapeFactory.AddNewSpline();
            tempShape.SetSupport(supportSurface.ShapeReference);
            tempShape.SetSplineType(0);
            tempShape.SetClosing(0);
            foreach (CatiaPoint point in pointList)
            {
                tempShape.AddPointWithConstraintExplicit(point.ShapeReference, null, -1d, 1, null, 0d);
            }

            tempShape.Compute();

            Shape = (HybridShapeSpline)tempShape;
        }

        public CatiaCurve(PartDocument partDocument, CatiaCurve curve, CatiaSurface supportSurface, double offset, bool offsetDirection) : base(partDocument)
        {
            SupportSurface = supportSurface;

            HybridShapeCurvePar tempCurve = HybridShapeFactory.AddNewCurvePar(curve.ShapeReference, supportSurface.ShapeReference, offset, offsetDirection, false);
            tempCurve.Compute();

            Shape = (HybridShape)tempCurve;
        }

        private List<CatiaPoint> _pointList;
        private CatiaSurface _supportSurface;

        public List<CatiaPoint> PointList { get => _pointList; set => _pointList = value; }
        public CatiaSurface SupportSurface { get => _supportSurface; set => _supportSurface = value; }

        public CatiaLine GetNaturalTangentLine(CatiaPoint point)
        {
            return new CatiaLine(PartDocument, this, point, 0, 10, false);
        }

        public CatiaLine GetNaturalRadialLine(CatiaPoint point)
        {
            HybridShapePlaneTangent tangentPlane = SupportSurface.GetTangentPlane(point);
            Reference tangentPlaneRef = PartDocument.Part.CreateReferenceFromObject(tangentPlane);

            return new CatiaLine(PartDocument, this, tangentPlaneRef, point, 90, 0, 10, false);
        }
    }
}
