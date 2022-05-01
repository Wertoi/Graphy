using Graphy.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphy.Enum
{
    public enum ImportMode
    {
        [LocalizationKey("ImportMode_Replace")]
        ReplaceCollection = 0,

        [LocalizationKey("ImportMode_Add")]
        AddToCollection = 1
    }
}

