using GalaSoft.MvvmLight;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Graphy.Model
{
    public class Icon : ObservableObject
    {
        public Icon()
        {
            
        }

        private string _name;
        private string _pathData;

        public string Name
        {
            get => _name;
            set
            {
                Set(() => Name, ref _name, value);
            }
        }

        public string PathData
        {
            get => _pathData;
            set
            {
                Set(() => PathData, ref _pathData, value);
            }
        }

        public static Icon Default()
        {
            return new Icon()
            {
                Name = "Default_Icon",
                PathData = "M19,19H5V5H19M19,3H5A2,2 0 0,0 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V5A2,2 0 0,0 19,3M13.96,12.29L11.21,15.83L9.25,13.47L6.5,17H17.5L13.96,12.29Z"
            };
        }


    }
}
