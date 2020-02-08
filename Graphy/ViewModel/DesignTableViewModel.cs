using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphy.Model;
using GalaSoft.MvvmLight.Command;
using System.ComponentModel;

namespace Graphy.ViewModel
{
    public class DesignTableViewModel : ViewModelBase
    {
        public DesignTableViewModel()
        {
            ParameterCollection = new ObservableCollection<DesignTableParameter>();
            PartCollection = new ObservableCollection<CatiaFile>();

            SelectCatalogPartFolderCommand = new RelayCommand(SelectCatalogPartFolderCommandAction);
            SelectDesignTableCommand = new RelayCommand(SelectDesignTableCommandAction);
            SendSelectedPartCollectionCommand = new RelayCommand(SendSelectedPartCollectionCommandAction);
            SelectAllPartCommand = new RelayCommand(SelectAllPartCommandAction);
            UnselectAllPartCommand = new RelayCommand(UnselectAllPartCommandAction);
        }

        private string _fullPath = "No design table selected";
        private string _partFolderPath = "No Catia part folder selected";
        private bool _isDesignTableSelected = false;
        private bool _isPartFolderSelected = false;
        private ObservableCollection<DesignTableParameter> _parameterCollection;
        private ObservableCollection<CatiaFile> _partCollection;

        public string FullPath
        {
            get => _fullPath;
            set
            {
                Set(() => FullPath, ref _fullPath, value);

                // If the selected file exists
                if (System.IO.File.Exists(FullPath))
                {
                    // Set the IsDesignTableSelected flag to True
                    IsDesignTableSelected = true;
                }
            }
        }

        public string PartFolderPath
        {
            get => _partFolderPath;
            set
            {
                Set(() => PartFolderPath, ref _partFolderPath, value);

                // If the selected folder exists
                if (System.IO.Directory.Exists(PartFolderPath))
                {
                    // Set the IsPartFolderSelected flag to True
                    IsPartFolderSelected = true;

                    // Load the catia file collection
                    LoadCatiaFileCollection(PartFolderPath);
                }
            }
        }



        public bool IsDesignTableSelected
        {
            get => _isDesignTableSelected;
            set
            {
                Set(() => IsDesignTableSelected, ref _isDesignTableSelected, value);
            }
        }

        public bool IsPartFolderSelected
        {
            get => _isPartFolderSelected;
            set
            {
                Set(() => IsPartFolderSelected, ref _isPartFolderSelected, value);
            }
        }

        public ObservableCollection<DesignTableParameter> ParameterCollection
        {
            get => _parameterCollection;
            set
            {
                Set(() => ParameterCollection, ref _parameterCollection, value);
            }
        }

        public ObservableCollection<CatiaFile> PartCollection
        {
            get => _partCollection;
            set
            {
                Set(() => PartCollection, ref _partCollection, value);
            }
        }

        // *** COMMANDS *** //

        // SELECT DESIGN TABLE COMMAND
        private RelayCommand _selectDesignTableCommand;
        public RelayCommand SelectDesignTableCommand { get => _selectDesignTableCommand; set => _selectDesignTableCommand = value; }

        private async void SelectDesignTableCommandAction()
        {
            // Call a dialog box to select the design table file
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Excel design table (*.xls;*.xlsx;*.xlsm)|*.xls;*.xlsx;*.xlsm";

            // If user has selected a file
            if ((bool)openFileDialog.ShowDialog())
            {
                // Store the design table full path
                FullPath = openFileDialog.FileName;

                // Create a temp parameter collection
                List<DesignTableParameter> tempParameterCollection = new List<DesignTableParameter>();

                // Create a design table reader
                DesignTableReader designTableReader = new DesignTableReader();
                designTableReader.PropertyChanged += DesignTableReader_PropertyChanged;

                // Update the status view
                MessengerInstance.Send("Lecture de la table de paramétrage.", Enum.ProcessToken.Started);

                // Necessary to update the progress bar view correctly
                await Task.Run(() =>
                {
                    try
                    {
                        // If design table file is open
                        if (designTableReader.OpenDesignTable(FullPath))
                        {
                            // Load the design table
                            designTableReader.LoadDesignTable(tempParameterCollection);

                            // If we have found at least one parameter, we send the design table full path
                            if (tempParameterCollection.Count > 0)
                            {
                                MessengerInstance.Send(FullPath, Enum.DesignTableToken.DesignTableLoaded);
                            }

                            // Close the design table
                            designTableReader.CloseDesignTable();

                            // Update the status view
                            MessengerInstance.Send<bool>(true, Enum.ProcessToken.Finished);
                        }
                        else
                        {
                            // If the design table cannot be open, update the status view
                            MessengerInstance.Send(designTableReader.ExceptionMessage, Enum.ProcessToken.Failed);
                        }
                    }
                    catch (Exception ex)
                    {
                        // If exception, update the status view
                        MessengerInstance.Send(ex.Message, Enum.ProcessToken.Failed);
                    }
                });

                // Update the parameter observable collection with the temp list
                ParameterCollection.Clear();
                foreach (DesignTableParameter parameter in tempParameterCollection)
                {
                    ParameterCollection.Add(parameter);
                }
            }
        }

        private void DesignTableReader_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            MessengerInstance.Send(((DesignTableReader)sender).ProgressRate * 100, Enum.ProcessToken.Refresh);
        }

        // SELECT CATALOG PART FOLDER COMMAND
        private RelayCommand _selectCatalogPartFolderCommand;
        public RelayCommand SelectCatalogPartFolderCommand { get => _selectCatalogPartFolderCommand; set => _selectCatalogPartFolderCommand = value; }

        private void SelectCatalogPartFolderCommandAction()
        {
            // Call a dialog box to select the folder
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();

            // If user has selected a folder
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Store the part folder path
                PartFolderPath = folderBrowserDialog.SelectedPath;

                // Set the flag IsPartFolderSelected to true
                IsPartFolderSelected = true;
            }
        }


        // SEND SELECTED PART COLLECTION COMMAND
        private RelayCommand _sendSelectedPartCollectionCommand;
        public RelayCommand SendSelectedPartCollectionCommand { get => _sendSelectedPartCollectionCommand; set => _sendSelectedPartCollectionCommand = value; }

        private void SendSelectedPartCollectionCommandAction()
        {
            // Create the collection to send
            List<CatiaFile> selectedPartcollection = new List<CatiaFile>();

            foreach (CatiaFile part in PartCollection)
            {
                if (part.IsSelected)
                    selectedPartcollection.Add(part);
            }

            MessengerInstance.Send<List<CatiaFile>>(selectedPartcollection, Enum.DesignTableToken.SelectedPartCollectionChanged);
        }


        // SELECT ALL PARTS COMMAND
        private RelayCommand _selectAllPartCommand;
        public RelayCommand SelectAllPartCommand { get => _selectAllPartCommand; set => _selectAllPartCommand = value; }

        private void SelectAllPartCommandAction()
        {
            foreach(CatiaFile part in PartCollection)
            {
                part.IsSelected = true;
            }
        }


        // UNSELECT ALL PARTS COMMAND
        private RelayCommand _unselectAllPartCommand;
        public RelayCommand UnselectAllPartCommand { get => _unselectAllPartCommand; set => _unselectAllPartCommand = value; }

        private void UnselectAllPartCommandAction()
        {
            foreach(CatiaFile part in PartCollection)
            {
                part.IsSelected = false;
            }
        }


        // *** PRIVATE METHODS *** //

        private void LoadCatiaFileCollection(string folderPath)
        {
            // For each file in the part directory
            foreach (string fileFullPath in System.IO.Directory.GetFiles(folderPath))
            {
                CatiaFile tempCatiaFile = new CatiaFile(fileFullPath);

                if (tempCatiaFile.GetDocumentFormat() == Model.CatiaDocument.CatiaGenericDocument.CatiaDocumentFormat.CATPart)
                {
                    PartCollection.Add(tempCatiaFile);
                }
            }
        }
    }
}
