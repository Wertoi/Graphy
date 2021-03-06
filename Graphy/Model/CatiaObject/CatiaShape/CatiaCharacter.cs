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
        public CatiaCharacter(PartDocument partDocument, char value) : base(partDocument)
        {
            Value = value;
            SurfaceList = new List<CatiaSurface>();
        }

        private char _value;
        private bool _isSpaceCharacter = false;


        public bool IsSpaceCharacter { get => _isSpaceCharacter; set => _isSpaceCharacter = value; }
        public char Value
        {
            get => _value;
            set
            {
                _value = value;

                if (Value == ' ')
                {
                    //Value = '_';
                    IsSpaceCharacter = true;
                }
            }
        }


        public new CatiaCharacter Clone()
        {
            CatiaCharacter copyCharacter = new CatiaCharacter(PartDocument, Value)
            {
                PathGeometry = PathGeometry.Clone(),
                IsSpaceCharacter = IsSpaceCharacter
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
            return (Value == catiaCharacter.Value && IsSpaceCharacter == catiaCharacter.IsSpaceCharacter);
        }

        // GETHASHCODE OVERRIDE
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
