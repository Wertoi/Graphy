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

                    bool flag = false;
                    foreach (MarkablePart part in MarkablePartCollection)
                    {
                        if (part.PartName == tempCatiaPart.Name)
                        {
                            part.CatiaPart = tempCatiaPart;
                            flag = true;
                        }
                    }

                    if (!flag)
                    {
                        MarkablePart newMarkablePart = new MarkablePart(tempCatiaPart)
                        {
                            PartName = tempCatiaPart.Name
                        };
                        MarkablePartCollection.Add(newMarkablePart);
                    }
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


        private RelayCommand _generateTableTemplateCommand;
        public RelayCommand GenerateTableTemplateCommand { get => _generateTableTemplateCommand; set => _generateTableTemplateCommand = value; }
        private void GenerateTableTemplateCommandAction()
        {
            ExcelTableStream.GenerateTemplateTable();
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

            LoadCatiaFileCollection(PartFolderPath);

            bool _ = TableReader.TryRead(TableFullPath, MarkablePartCollection);
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
