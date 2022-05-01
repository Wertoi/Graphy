using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphy.Model.CatiaObject;

namespace Graphy.Model
{
    public class MarkablePart : ObservableObject
    {
        public MarkablePart()
        {
            CatiaPart = new CatiaPart();
            MarkingData = new MarkingData();
        }


        public MarkablePart(CatiaPart part)
        {
            CatiaPart = part;
            MarkingData = MarkingData.Default();
        }

        private string _partName;
        private CatiaPart _catiaPart;
        private MarkingData _markingData;
        private bool _isSelected = false;
        private bool _isSelectable = false;
        private bool _hasFile = false;


        public string PartName
        {
            get => _partName;
            set
            {
                Set(() => PartName, ref _partName, value);
            }
        }

        public CatiaPart CatiaPart
        {
            get => _catiaPart;
            set
            {
                Set(() => CatiaPart, ref _catiaPart, value);

                if (CatiaPart != null && System.IO.File.Exists(CatiaPart.FileFullPath))
                {
                    PartName = CatiaPart.Name;
                    HasFile = true;
                }
                else
                    HasFile = false;

            }
        }


        public MarkingData MarkingData
        {
            get => _markingData;
            set
            {
                Set(() => MarkingData, ref _markingData, value);
            }
        }


        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (!IsSelectable)
                    Set(() => IsSelected, ref _isSelected, false);
                else
                    Set(() => IsSelected, ref _isSelected, value);
            }
        }

        public bool HasFile
        {
            get => _hasFile;
            set
            {
                Set(() => HasFile, ref _hasFile, value);
            }
        }


        public bool IsSelectable
        {
            get => _isSelectable;
            set
            {
                Set(() => IsSelectable, ref _isSelectable, value);
            }
        }
    }
}
