using Graphy.Attribute;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphy.Model
{

    public class DesignTableParameter : ObservableObject
    {
        public enum ParameterType
        {
            [LocalizationKey("ParameterCategory_NoType")]
            NoType,

            [LocalizationKey("ParameterCategory_Text")]
            Text,

            [LocalizationKey("ParameterCategory_Number")]
            Number
        }

        public DesignTableParameter()
        {

        }

        public DesignTableParameter(string name, int columnIndex, ParameterType parameterType)
        {
            Name = name;
            ColumnIndex = columnIndex;
            Type = parameterType;
        }

        private string _name;
        private int _columnIndex;
        private ParameterType _type;

        public string Name
        {
            get => _name;
            set
            {
                Set(() => Name, ref _name, value);
            }
        }

        public int ColumnIndex
        {
            get => _columnIndex;
            set
            {
                Set(() => ColumnIndex, ref _columnIndex, value);
            }
        }

        public ParameterType Type
        {
            get => _type;
            set
            {
                Set(() => Type, ref _type, value);
            }
        }

        public static DesignTableParameter NoLinkParameter()
        {
            return new DesignTableParameter("Pas de lien.", 0, ParameterType.NoType);
        }

        public static ParameterType GetParameterCategory(string cellValue)
        {
            if (cellValue is null)
                return ParameterType.NoType;
            else
            {
                bool isNumeric = double.TryParse(cellValue, out _);

                if (isNumeric)
                    return ParameterType.Number;
                else
                    return ParameterType.Text;
            }
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

            DesignTableParameter designTableParameter = (DesignTableParameter)obj;
            return (Name == designTableParameter.Name) && (ColumnIndex == designTableParameter.ColumnIndex);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
