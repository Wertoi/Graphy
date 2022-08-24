using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Graphy.Model;
using Graphy.Model.CatiaObject;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            GenerateCommand = new RelayCommand(GenerateCommandAction);
            SelectTableCommand = new RelayCommand(SelectTableCommandAction);
            GenerateTableTemplateCommand = new RelayCommand(GenerateTableTemplateCommandAction);
            SelectMarkablePartCommand = new RelayCommand<int>((index) => SelectMarkablePartCommandAction(index));
            SelectAllPartCommand = new RelayCommand(SelectAllPartCommandAction);
            UnselectAllPartCommand = new RelayCommand(UnselectAllPartCommandAction);

            LoadDataCommand = new RelayCommand(LoadDataCommandAction);

            // From settings
            MessengerInstance.Register<double>(this, Enum.SettingToken.ToleranceFactorChanged, (toleranceFactor) => { _toleranceFactor = toleranceFactor; });
            MessengerInstance.Register<bool>(this, Enum.SettingToken.KeepHistoricChanged, (keepHistoric) => { _keepHistoric = keepHistoric; });
            MessengerInstance.Register<bool>(this, Enum.SettingToken.CreateVolumeChanged, (createVolume) => { _createVolume = createVolume; });
            MessengerInstance.Register<CsvStream.CsvConfig>(this, Enum.SettingToken.CsvConfigChanged, (csvConfig) => { _csvConfig = csvConfig; });
            MessengerInstance.Register<Enum.HorizontalAxisSystemPosition>(this, Enum.SettingToken.HorizontalAxisSystemPositionChanged, (horizontalAxisSystemPosition) => { _horizontalAxisSystemPosition = horizontalAxisSystemPosition; });

            // From Catia
            MessengerInstance.Register<CatiaEnv>(this, Enum.CatiaToken.CatieEnvChanged, (catiaEnv) => { _catiaEnv = catiaEnv; });
        }

        public const string DEFAULT_TABLE_FULLPATH = "No table selected";
        public const string DEFAULT_PART_FOLDER_PATH = "No folder selected";

        private string _tableFullPath;
        private string _partFolderPath;
        private ObservableCollection<MarkablePart> _markablePartCollection;
        private MarkablePart _selectedMarkablePart;

        // PRIVATE ATTRIBUTS
        private double _toleranceFactor;
        private bool _keepHistoric;
        private bool _createVolume;
        private Enum.HorizontalAxisSystemPosition _horizontalAxisSystemPosition;
        CatiaEnv _catiaEnv;
        private CsvStream.CsvConfig _csvConfig;

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



        private RelayCommand _generateCommand;
        public RelayCommand GenerateCommand { get => _generateCommand; set => _generateCommand = value; }
        private async void GenerateCommandAction()
        {
            // Create a list with only selected markings
            List<MarkablePart> selectedMarkablePartList = new List<MarkablePart>();

            foreach (MarkablePart markablePart in MarkablePartCollection)
            {
                if (markablePart.IsSelected)
                    selectedMarkablePartList.Add(markablePart);
            }

            MessengerInstance.Send<int>(selectedMarkablePartList.Count, Enum.ProcessToken.ComplexStarted);

            await Task.Run(() =>
            {
                MarkingGenerator markingGenerator = new MarkingGenerator();
                
                markingGenerator.ProgressUpdated += MarkingGenerator_ProgressUpdated;

                try
                {
                    markingGenerator.RunForCollection(_catiaEnv, selectedMarkablePartList, _toleranceFactor, _keepHistoric, _createVolume, _horizontalAxisSystemPosition);

                    MessengerInstance.Send<string>("Complex drawing process complete !", Enum.ProcessToken.Finished);
                }
                catch (Exception ex)
                {
                    MessengerInstance.Send(ex.Message, Enum.ProcessToken.Failed);
                    return;
                }

            });
        }

        private void MarkingGenerator_ProgressUpdated(object sender, (double progressRate, int currentStep) e)
        {
            MessengerInstance.Send<(double, int)>((e.progressRate * 100, e.currentStep), Enum.ProcessToken.Refresh);
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


        private RelayCommand _generateTableTemplateCommand;
        public RelayCommand GenerateTableTemplateCommand { get => _generateTableTemplateCommand; set => _generateTableTemplateCommand = value; }

        private void GenerateTableTemplateCommandAction()
        {
            MarkablePart tempMarkablePart = new MarkablePart();
            tempMarkablePart.MarkingData = MarkingData.Default();

            // Call a dialog box to select the folder
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();

            // If user has selected a folder
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Store the part folder path
                TableStream.GenerateTemplate(_csvConfig, tempMarkablePart, folderBrowserDialog.SelectedPath + System.IO.Path.DirectorySeparatorChar + "Graphy_Complex_Marking_Template.csv");
            }
        }


        private RelayCommand _loadDataCommand;
        public RelayCommand LoadDataCommand { get => _loadDataCommand; set => _loadDataCommand = value; }

        private void LoadDataCommandAction()
        {
            // Clear the markable part collection
            MarkablePartCollection.Clear();

            // Try to read the marking datas in the table.
            List<MarkablePart> markablePartListFromCSV = new List<MarkablePart>();

            // If we manage to read the CSV file
            if (TableStream.TryRead(TableFullPath, markablePartListFromCSV, _csvConfig))
            {
                // Add all the markable part from the CSV file in the Markable part collection
                foreach(MarkablePart markablePartFromCSV in markablePartListFromCSV)
                {
                    MarkablePartCollection.Add(markablePartFromCSV);
                    if (markablePartFromCSV.HasFile && System.IO.File.Exists(markablePartFromCSV.CatiaPart.FileFullPath))
                        markablePartFromCSV.IsSelectable = true;
                }

                MessengerInstance.Send<string>("Loading complete.\r\n" + markablePartListFromCSV.Count + " markings have been found.", Enum.ProcessToken.Finished);
            }
            else
            {
                MessengerInstance.Send<string>("An unexpected error occured during CSV table reading.\r\nReview table.", Enum.ProcessToken.Failed);
            }
        }


        private RelayCommand<int> _selectMarkablePartCommand;
        public RelayCommand<int> SelectMarkablePartCommand { get => _selectMarkablePartCommand; set => _selectMarkablePartCommand = value; }
        private void SelectMarkablePartCommandAction(int markablePartIndex)
        {
            SelectedMarkablePart = MarkablePartCollection[markablePartIndex];
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
