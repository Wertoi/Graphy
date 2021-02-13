using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Graphy.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphy.ViewModel
{
    public class ComplexMarkingViewModel : ViewModelBase
    {
        public ComplexMarkingViewModel()
        {
            MarkablePartCollection = new ObservableCollection<MarkablePart>();

            TableFullPath = DEFAULT_TABLE_FULLPATH;
            PartFolderPath = DEFAULT_PART_FOLDER_PATH;

            GenerateTableTemplateCommand = new RelayCommand(GenerateTableTemplateCommandAction);
            SelectTableCommand = new RelayCommand(SelectTableCommandAction);
            SelectPartFolderCommand = new RelayCommand(SelectPartFolderCommandAction);
            SelectAllPartCommand = new RelayCommand(SelectAllPartCommandAction);
            UnselectAllPartCommand = new RelayCommand(UnselectAllPartCommandAction);

            LoadDataCommand = new RelayCommand(LoadDataCommandAction);

        }

        public const string DEFAULT_TABLE_FULLPATH = "No curve selected";
        public const string DEFAULT_PART_FOLDER_PATH = "No curve selected";

        private string _tableFullPath;
        private string _partFolderPath;
        private ObservableCollection<MarkablePart> _markablePartCollection;
        private bool _isMarkablePartCollectionEmpty = true;
        private MarkablePart _selectedMarkablePart;

        public string TableFullPath
        {
            get => _tableFullPath;
            set
            {
                Set(() => TableFullPath, ref _tableFullPath, value);
            }
        }

        public string PartFolderPath
        {
            get => _partFolderPath;
            set
            {
                Set(() => PartFolderPath, ref _partFolderPath, value);

                /*if(System.IO.Directory.Exists(PartFolderPath))
                    LoadCatiaFileCollection(PartFolderPath);*/
            }
        }

        private void LoadCatiaFileCollection(string folderPath)
        {
            // For each file in the part directory
            foreach (string fileFullPath in System.IO.Directory.GetFiles(folderPath))
            {
                if (CatiaPart.IsFormatOK(fileFullPath))
                {
                    CatiaPart tempCatiaPart = new CatiaPart(fileFullPath);

                    MarkablePart newMarkablePart = new MarkablePart(tempCatiaPart)
                    {
                        PartName = tempCatiaPart.Name
                    };
                    MarkablePartCollection.Add(newMarkablePart);
                }
            }
        }

        public ObservableCollection<MarkablePart> MarkablePartCollection
        {
            get => _markablePartCollection;
            set
            {
                Set(() => MarkablePartCollection, ref _markablePartCollection, value);
            }
        }

        public MarkablePart SelectedMarkablePart
        {
            get => _selectedMarkablePart;
            set
            {
                Set(() => SelectedMarkablePart, ref _selectedMarkablePart, value);
            }
        }

        public bool IsMarkablePartCollectionEmpty
        {
            get => _isMarkablePartCollectionEmpty;
            set
            {
                Set(() => IsMarkablePartCollectionEmpty, ref _isMarkablePartCollectionEmpty, value);
            }
        }


        private RelayCommand _generateTableTemplateCommand;
        public RelayCommand GenerateTableTemplateCommand { get => _generateTableTemplateCommand; set => _generateTableTemplateCommand = value; }
        private void GenerateTableTemplateCommandAction()
        {
            TableStream.GenerateTemplate();
        }



        private RelayCommand _selectTableCommand;
        public RelayCommand SelectTableCommand { get => _selectTableCommand; set => _selectTableCommand = value; }

        private void SelectTableCommandAction()
        {
            // Call a dialog box to select the design table file
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = false,
                Filter = "Excel design table (*.csv)|*.csv"
            };

            // If user has selected a file
            if ((bool)openFileDialog.ShowDialog())
            {
                // Store the table full path
                TableFullPath = openFileDialog.FileName;
            }
        }



        private RelayCommand _selectPartFolderCommand;
        public RelayCommand SelectPartFolderCommand { get => _selectPartFolderCommand; set => _selectPartFolderCommand = value; }

        private void SelectPartFolderCommandAction()
        {
            // Call a dialog box to select the folder
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();

            // If user has selected a folder
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Store the part folder path
                PartFolderPath = folderBrowserDialog.SelectedPath;
            }
        }



        private RelayCommand _loadDataCommand;
        public RelayCommand LoadDataCommand { get => _loadDataCommand; set => _loadDataCommand = value; }

        private void LoadDataCommandAction()
        {
            MarkablePartCollection.Clear();

            // If Part folder exists, get all the parts in it.
            if (System.IO.Directory.Exists(PartFolderPath))
                LoadCatiaFileCollection(PartFolderPath);

            // Try to read the marking datas in the table.
            List<(string, MarkingData)> markingDatas = new List<(string, MarkingData)>();

            // If we manage to read the CSV file
            if (TableStream.TryRead(TableFullPath, markingDatas))
            {

                // For each pair (Part name, Marking Data) in the datas got from the CSV file
                foreach ((string partName, MarkingData markingData) datas in markingDatas)
                {
                    // Define a flag
                    bool flag = false;


                    // For each markable part from Catia in the markable part collection
                    foreach (MarkablePart partFromCatia in MarkablePartCollection)
                    {

                        // If the part from Catia has the same name as the datas from the CSV file
                        if (partFromCatia.PartName == datas.partName)
                        {
                            // Add datas marking datas to the markable part
                            partFromCatia.MarkingDataCollection.Add(datas.markingData);
                            flag = true;
                            break;
                        }

                    }

                    // If the datas did not find a markable part, we create it.
                    if (!flag)
                    {
                        MarkablePart tempMarkablePart = new MarkablePart();
                        tempMarkablePart.PartName = datas.partName;
                        tempMarkablePart.MarkingDataCollection.Add(datas.markingData);
                        MarkablePartCollection.Add(tempMarkablePart);
                    }
                }
            }

            IsMarkablePartCollectionEmpty = MarkablePartCollection.Count == 0;
        }



        private RelayCommand _selectAllPartCommand;
        public RelayCommand SelectAllPartCommand { get => _selectAllPartCommand; set => _selectAllPartCommand = value; }

        private void SelectAllPartCommandAction()
        {
            foreach (MarkablePart part in MarkablePartCollection)
            {
                part.IsSelected = true;
            }
        }



        // UNSELECT ALL PARTS COMMAND
        private RelayCommand _unselectAllPartCommand;
        public RelayCommand UnselectAllPartCommand { get => _unselectAllPartCommand; set => _unselectAllPartCommand = value; }

        private void UnselectAllPartCommandAction()
        {
            foreach (MarkablePart part in MarkablePartCollection)
            {
                part.IsSelected = false;
            }
        }

    }
}
