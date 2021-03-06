using INFITF;
using MECMOD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphy.Model.CatiaObject.CatiaShape
{
    public class CatiaAxisSystem
    {
        public CatiaAxisSystem(PartDocument partDocument)
        {
            PartDocument = partDocument;
        }

        public CatiaAxisSystem(PartDocument partDocument, CatiaPoint point, CatiaCurve curve, CatiaSurface surface,
            bool xDirection, bool yDirection, bool zDirection, HybridBody workingSet)
        {
            PartDocument = partDocument;

            // Create the natural normal line of the surface passing by the point
            CatiaLine naturalNormalLine = surface.GetNaturalNormalLine(point);

            // Create the natural tangent line of the curve passing by the point
            CatiaLine naturalTangentLine = curve.GetNaturalTangentLine(point);

            // Create the natural radial line of the curve passing by the point
            CatiaLine naturalRadialLine = curve.GetNaturalRadialLine(point);

            // Create the axis system
            partDocument.Part.InWorkObject = workingSet;
            AxisSystem tempAxisSystem = partDocument.Part.AxisSystems.Add();

            // Assign the origin point
            tempAxisSystem.OriginType = CATAxisSystemOriginType.catAxisSystemOriginByPoint;
            tempAxisSystem.OriginPoint = point.ShapeReference;

            // Assign X direction
            if (xDirection)
                tempAxisSystem.XAxisType = CATAxisSystemAxisType.catAxisSystemAxisSameDirection;
            else
                tempAxisSystem.XAxisType = CATAxisSystemAxisType.catAxisSystemAxisOppositeDirection;

            tempAxisSystem.XAxisDirection = naturalTangentLine.ShapeReference;

            // Assign Y direction
            if (yDirection)
                tempAxisSystem.YAxisType = CATAxisSystemAxisType.catAxisSystemAxisSameDirection;
            else
                tempAxisSystem.YAxisType = CATAxisSystemAxisType.catAxisSystemAxisOppositeDirection;

            tempAxisSystem.YAxisDirection = naturalRadialLine.ShapeReference;

            // Assign Z direction
            if (zDirection)
                tempAxisSystem.ZAxisType = CATAxisSystemAxisType.catAxisSystemAxisSameDirection;
            else
                tempAxisSystem.ZAxisType = CATAxisSystemAxisType.catAxisSystemAxisOppositeDirection;

            tempAxisSystem.ZAxisDirection = naturalNormalLine.ShapeReference;

            // Update
            partDocument.Part.UpdateObject(tempAxisSystem);

            // Assign Shape
            System = tempAxisSystem;
        }

        // ATTRIBUTS
        private PartDocument _partDocument;
        private AxisSystem _system;
        private Reference _systemReference;

        private object[] _xDirection = new object[3];
        private object[] _yDirection = new object[3];
        private object[] _zDirection = new object[3];

        public object[] XDirection { get => _xDirection; set => _xDirection = value; }
        public object[] YDirection { get => _yDirection; set => _yDirection = value; }
        public object[] ZDirection { get => _zDirection; set => _zDirection = value; }


        public AxisSystem System
        {
            get => _system;
            set
            {
                _system = value;
                SystemReference = PartDocument.Part.CreateReferenceFromObject(System);

                // Retrieves directions
                System.GetXAxis(XDirection);
                System.GetYAxis(YDirection);
                System.GetZAxis(ZDirection);
            }

        }

        public Reference SystemReference { get => _systemReference; set => _systemReference = value; }
        public PartDocument PartDocument { get => _partDocument; set => _partDocument = value; }

        public static CatiaAxisSystem GetOriginAxisSystem(PartDocument partDocument, HybridBody originSet)
        {
            partDocument.Part.InWorkObject = originSet;
            AxisSystem tempOriginAxisSystem = partDocument.Part.AxisSystems.Add();
            tempOriginAxisSystem.set_Name("Origin axis system");

            CatiaAxisSystem originAxisSystem = new CatiaAxisSystem(partDocument)
            {
                System = tempOriginAxisSystem
            };

            return originAxisSystem;
        }
    }
}
