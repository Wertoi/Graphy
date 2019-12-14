using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using INFITF;
using MECMOD;

namespace Graphy.Model
{
    public class Contour
    {
        public enum ContourType
        {
            Ext,
            Int,
            ExtInt
        }

        public enum ContourStatus
        {
            FontFileOriginal,
            Imported
        }

        public Contour()
        {

        }

        public Contour(HybridShape shape, ContourStatus status)
        {
            switch(status)
            {
                case ContourStatus.FontFileOriginal:
                    FontFileContourShape = shape;
                    break;

                case ContourStatus.Imported:
                    ImportedContourShape = shape;
                    break;
            }
        }

        private HybridShape _fontFileContourShape;
        private HybridShape _importedContourShape;
        private Reference _modifiedContourRef;
        private double _area;
        private ContourType _type;
        private Reference _contourRef;
        private double _scaledPerimeter;
        private string _parentContourName;

        public HybridShape FontFileContourShape { get => _fontFileContourShape; set => _fontFileContourShape = value; }
        public double Area { get => _area; set => _area = value; }
        public ContourType Type { get => _type; set => _type = value; }
        public Reference ContourRef { get => _contourRef; set => _contourRef = value; }
        public Reference ModifiedContourRef { get => _modifiedContourRef; set => _modifiedContourRef = value; }
        public double ScaledPerimeter { get => _scaledPerimeter; set => _scaledPerimeter = value; }
        public HybridShape ImportedContourShape { get => _importedContourShape; set => _importedContourShape = value; }
        public string ParentContourName { get => _parentContourName; set => _parentContourName = value; }

        public string Name(ContourStatus status)
        {
            switch(status)
            {
                case ContourStatus.FontFileOriginal:
                    return FontFileContourShape.get_Name();

                case ContourStatus.Imported:
                    return ImportedContourShape.get_Name();

                default:
                    return FontFileContourShape.get_Name();
            }
        }

        public Contour DeepCopy(PartDocument partDocument, HybridShapeTypeLib.HybridShapeFactory factory)
        {
            Contour contourCopy = (Contour)this.MemberwiseClone();

            // Attributs à copier.
            contourCopy.FontFileContourShape = (HybridShape)factory.AddNewCurveDatum(ContourRef);
            contourCopy.FontFileContourShape.Compute();

            contourCopy.ContourRef = partDocument.Part.CreateReferenceFromObject(contourCopy.FontFileContourShape);

            return contourCopy;
        }


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

            Contour contour = (Contour)obj;
            return (FontFileContourShape == contour.FontFileContourShape);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
