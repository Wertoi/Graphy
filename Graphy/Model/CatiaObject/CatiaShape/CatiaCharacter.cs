using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using HybridShapeTypeLib;
using MECMOD;

namespace Graphy.Model.CatiaObject.CatiaShape
{
    public class CatiaCharacter : CatiaDrawableShape
    {
        public CatiaCharacter(PartDocument partDocument, char value, FontFamily fontFamily, bool isBold, bool isItalic) : base(partDocument)
        {
            Value = value;
            FontFamily = fontFamily;
            IsBold = isBold;
            IsItalic = isItalic;
            SurfaceList = new List<CatiaSurface>();
        }

        private char _value;
        private bool _isSpaceCharacter = false;
        private FontFamily _fontFamily;
        private bool _isBold;
        private bool _isItalic;

        public char Value
        {
            get => _value;
            set
            {
                _value = value;

                IsSpaceCharacter = Value == ' ';
            }
        }
        public bool IsSpaceCharacter { get => _isSpaceCharacter; set => _isSpaceCharacter = value; }
        public bool IsBold { get => _isBold; set => _isBold = value; }
        public bool IsItalic { get => _isItalic; set => _isItalic = value; }
        public FontFamily FontFamily { get => _fontFamily; set => _fontFamily = value; }

        public void ComputeGeometry(FontFamily fontFamily, double toleranceFactor)
        {
            PathGeometry = fontFamily.GetCharacterGeometry(Value, toleranceFactor, IsBold, IsItalic);
        }

        public new CatiaCharacter Clone()
        {
            CatiaCharacter copyCharacter = new CatiaCharacter(PartDocument, Value, FontFamily, IsBold, IsItalic)
            {
                PathGeometry = PathGeometry.Clone()
            };

            foreach (CatiaSurface surface in SurfaceList)
            {
                copyCharacter.SurfaceList.Add(surface.Copy());
            }

            return copyCharacter;
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

            CatiaCharacter catiaCharacter = (CatiaCharacter)obj;
            return (Value == catiaCharacter.Value && FontFamily == catiaCharacter.FontFamily &&
                IsBold == catiaCharacter.IsBold && IsItalic == catiaCharacter.IsItalic);
        }

        // GETHASHCODE OVERRIDE
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
