using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
