using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphy.Attribute
{
    public class LocalizationKeyAttribute : System.Attribute
    {
        public string LocKey { get; }

        public LocalizationKeyAttribute(string locKey)
        {
            this.LocKey = locKey;
        }
    }
}
