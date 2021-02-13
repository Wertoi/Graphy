using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphy.Model
{
    public class MarkablePart : ObservableObject
    {
        public MarkablePart()
        {
            CatiaPart = new CatiaPart();
            MarkingDataCollection = new ObservableCollection<MarkingData>();
            //MarkingDataCollection.CollectionChanged += MarkingDataCollection_CollectionChanged;
        }

        public MarkablePart(CatiaPart part)
        {
            CatiaPart = part;
            MarkingDataCollection = new ObservableCollection<MarkingData>();
        }

        /*private void MarkingDataCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (MarkingData markingData in e.NewItems)
                    markingData.PropertyChanged += MarkingData_PropertyChanged;

            if (e.OldItems != null)
                foreach (MarkingData markingData in e.OldItems)
                    markingData.PropertyChanged -= MarkingData_PropertyChanged;
        }

        private void MarkingData_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsOK")
                CanGenerate = CheckIfCanGenerate();
        }*/

        private string _partName;
        private CatiaPart _catiaPart;
        private ObservableCollection<MarkingData> _markingDataCollection;
        private int _numberOfMarkingDataOK;
        private bool _canGenerate = false;
        private bool _isSelected = false;
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

                if (System.IO.File.Exists(CatiaPart.FileFullPath))
                {
                    PartName = CatiaPart.Name;
                    HasFile = true;
                }
                else
                    HasFile = false;

            }
        }


        public ObservableCollection<MarkingData> MarkingDataCollection
        {
            get => _markingDataCollection;
            set
            {
                Set(() => MarkingDataCollection, ref _markingDataCollection, value);
            }
        }

        public bool CanGenerate
        {
            get => _canGenerate;
            set
            {
                Set(() => CanGenerate, ref _canGenerate, value);
            }
        }

        public int NumberOfMarkingDataOK
        {
            get => _numberOfMarkingDataOK;
            set
            {
                Set(() => NumberOfMarkingDataOK, ref _numberOfMarkingDataOK, value);
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
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
    }
}
