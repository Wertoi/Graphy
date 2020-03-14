using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Graphy.Model;
using Graphy.Model.Generator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using INFITF;
using MECMOD;
using Graphy.Model.CatiaDocument;

namespace Graphy.ViewModel
{
    public class InputDataViewModel : ViewModelBase
    {
        // CONSTRUCTOR
        public InputDataViewModel()
        {
            MarkingData = new MarkingData("Hello World !", 1.6, 0.1);

            MarkingData.PropertyChanged += MarkingData_PropertyChanged;

            // MESSENGER REGISTRATION
            MessengerInstance.Register<SelectableFont>(this, Enum.InputDataToken.SelectedFontChanged, (font) => { MarkingData.Font = font; });
            MessengerInstance.Register<CatiaPartDocument>(this, Enum.CatiaToken.SelectedPartDocumentChanged, (partDoc) => { _selectedPartDocument = partDoc; CheckIfCanGenerate(); });

            MessengerInstance.Register<string>(this, Enum.DesignTableToken.DesignTableLoaded, (fullPath) => DesignTableLoaded(fullPath));
            MessengerInstance.Register<List<CatiaFile>>(this, Enum.DesignTableToken.SelectedPartCollectionChanged, (partList) => { _partList = partList; CheckIfCanGenerate(); });

            MessengerInstance.Register<double>(this, Enum.SettingToken.ToleranceFactorChanged, (toleranceFactor) => { _toleranceFactor = toleranceFactor; });
            MessengerInstance.Register<bool>(this, Enum.SettingToken.KeepHistoricChanged, (keepHistoric) => { _keepHistoric = keepHistoric; });
            MessengerInstance.Register<bool>(this, Enum.SettingToken.CreateVolumeChanged, (createVolume) => { _createVolume = createVolume; });

            // COMMANDS INITIALIZATION
            SelectTrackingCurveCommand = new RelayCommand(SelectTrackingCurveCommandAction);
            SelectStartPointCommand = new RelayCommand(SelectStartPointCommandAction);
            SelectProjectionSurfaceCommand = new RelayCommand(SelectionProjectionSurfaceCommandAction);
            SelectLocalAxisSystemCommand = new RelayCommand(SelectLocalAxisSystemCommandAction);
            GenerateCommand = new RelayCommand(GenerateCommandAction);

            _partList = new List<CatiaFile>();
        }


        #region ATTRIBUTS

        // OBSERVABLE ATTRIBUTS
        private MarkingData _markingData;
        private bool _canGenerate;
        private bool _isDesignTableActivated = false;

        // PRIVATE ATTRIBUTS
        private CatiaPartDocument _selectedPartDocument;
        private string _designTableFullPath;
        private List<CatiaFile> _partList;
        private double _toleranceFactor;
        private bool _keepHistoric;
        private bool _createVolume;

        private DesignTableParameter _tempTextParameter;
        private DesignTableParameter _tempCharacterHeightParameter;
        private DesignTableParameter _tempExtrusionHeightParameter;

        public MarkingData MarkingData
        {
            get => _markingData;
            set
            {
                Set(() => MarkingData, ref _markingData, value);
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

        public bool IsDesignTableActivated
        {
            get => _isDesignTableActivated;
            set
            {
                Set(() => IsDesignTableActivated, ref _isDesignTableActivated, value);

                if (!IsDesignTableActivated)
                {
                    _tempTextParameter = MarkingData.Text.LinkedParameter;
                    _tempCharacterHeightParameter = MarkingData.CharacterHeight.LinkedParameter;
                    _tempExtrusionHeightParameter = MarkingData.ExtrusionHeight.LinkedParameter;

                    MarkingData.Text.LinkedParameter = DesignTableParameter.NoLinkParameter();
                    MarkingData.CharacterHeight.LinkedParameter = DesignTableParameter.NoLinkParameter();
                    MarkingData.ExtrusionHeight.LinkedParameter = DesignTableParameter.NoLinkParameter();
                }
                else if (_tempTextParameter != null && _tempCharacterHeightParameter != null && _tempExtrusionHeightParameter != null)
                {
                    MarkingData.Text.LinkedParameter = _tempTextParameter;
                    MarkingData.CharacterHeight.LinkedParameter = _tempCharacterHeightParameter;
                    MarkingData.ExtrusionHeight.LinkedParameter = _tempExtrusionHeightParameter;
                }

                CheckIfCanGenerate();
            }
        }

        #endregion


        #region COMMANDS

        // TRACKING CURVE SELECTION COMMAND
        private RelayCommand _selectTrackingCurveCommand;
        public RelayCommand SelectTrackingCurveCommand { get => _selectTrackingCurveCommand; set => _selectTrackingCurveCommand = value; }

        private void SelectTrackingCurveCommandAction()
        {
            MessengerInstance.Send<object>(null, Enum.CatiaToken.Refresh);

            if (_selectedPartDocument != null)
            {
                object filterStr = new object[1] { "MonoDim" };

                Selection selection = _selectedPartDocument.PartDocument.Selection;
                selection.Clear();
                string selectionStatus = selection.SelectElement2((Array)filterStr, "Sélectionner dans l'arbre la courbe de suivi.", false);

                if (selectionStatus != "Cancel" && selectionStatus != "Undo" && selectionStatus != "Redo")
                {
                    try
                    {
                        string tempName = ((HybridShape)selection.Item(1).Value).get_Name();

                        if (!IsNameUnique(tempName))
                            MessengerInstance.Send("", Enum.InputDataToken.SelectionIncorrect);
                        else
                            MarkingData.TrackingCurveName = tempName;

                    }
                    catch (Exception)
                    {
                        MessengerInstance.Send("", Enum.InputDataToken.SelectionIncorrect);
                    }
                }
            }
        }


        // START POINT SELECTION COMMAND
        private RelayCommand _selectStartPointCommand;
        public RelayCommand SelectStartPointCommand { get => _selectStartPointCommand; set => _selectStartPointCommand = value; }

        private void SelectStartPointCommandAction()
        {
            MessengerInstance.Send<object>(null, Enum.CatiaToken.Refresh);

            if (_selectedPartDocument != null)
            {
                object filterStr = new object[1] { "ZeroDim" };

                Selection selection = _selectedPartDocument.PartDocument.Selection;
                selection.Clear();
                string selectionStatus = selection.SelectElement2((Array)filterStr, "Sélectionner dans l'arbre le point de départ.", false);

                if (selectionStatus != "Cancel" && selectionStatus != "Undo" && selectionStatus != "Redo")
                {
                    try
                    {
                        string tempName = ((HybridShape)selection.Item(1).Value).get_Name();

                        if (!IsNameUnique(tempName))
                            MessengerInstance.Send("", Enum.InputDataToken.SelectionIncorrect);
                        else
                            MarkingData.StartPointName = tempName;
                    }
                    catch (Exception)
                    {
                        MessengerInstance.Send("", Enum.InputDataToken.SelectionIncorrect);
                    }
                }
            }
        }


        // SUPPORT SURFACE SELECTION COMMAND
        private RelayCommand _selectProjectionSurfaceCommand;
        public RelayCommand SelectProjectionSurfaceCommand { get => _selectProjectionSurfaceCommand; set => _selectProjectionSurfaceCommand = value; }

        private void SelectionProjectionSurfaceCommandAction()
        {
            MessengerInstance.Send<object>(null, Enum.CatiaToken.Refresh);

            if (_selectedPartDocument != null)
            {
                object filterStr = new object[1] { "BiDim" };

                Selection selection = _selectedPartDocument.PartDocument.Selection;
                selection.Clear();
                string selectionStatus = selection.SelectElement2((Array)filterStr, "Sélectionner dans l'arbre la surface support.", false);

                if (selectionStatus != "Cancel" && selectionStatus != "Undo" && selectionStatus != "Redo")
                {
                    try
                    {
                        string tempName = ((HybridShape)selection.Item(1).Value).get_Name();

                        if (!IsNameUnique(tempName))
                            MessengerInstance.Send("", Enum.InputDataToken.SelectionIncorrect);
                        else
                            MarkingData.ProjectionSurfaceName = tempName;

                    }
                    catch (Exception)
                    {
                        MessengerInstance.Send("", Enum.InputDataToken.SelectionIncorrect);
                    }
                }
            }
        }


        // LOCAL AXIS SYSTEM SELECTION COMMAND
        private RelayCommand _selectLocalAxisSystemCommand;
        public RelayCommand SelectLocalAxisSystemCommand { get => _selectLocalAxisSystemCommand; set => _selectLocalAxisSystemCommand = value; }

        private void SelectLocalAxisSystemCommandAction()
        {
            MessengerInstance.Send<object>(null, Enum.CatiaToken.Refresh);

            if (_selectedPartDocument != null)
            {
                object filterStr = new object[1] { "AxisSystem" };

                Selection selection = _selectedPartDocument.PartDocument.Selection;
                selection.Clear();
                string selectionStatus = selection.SelectElement2((Array)filterStr, "Sélectionner dans l'arbre le repère local.", false);

                if (selectionStatus != "Cancel" && selectionStatus != "Undo" && selectionStatus != "Redo")
                {
                    try
                    {
                        string tempName = ((AxisSystem)selection.Item(1).Value).get_Name();

                        if (!IsNameUnique(tempName))
                            MessengerInstance.Send("", Enum.InputDataToken.SelectionIncorrect);
                        else
                            MarkingData.AxisSystemName = tempName;

                    }
                    catch (Exception)
                    {
                        MessengerInstance.Send("", Enum.InputDataToken.SelectionIncorrect);
                    }
                }
            }
        }


        // GENERATE COMMAND
        private RelayCommand _generateCommand;
        public RelayCommand GenerateCommand { get => _generateCommand; set => _generateCommand = value; }

        private async void GenerateCommandAction()
        {
            if (IsNameUnique(MarkingData.ProjectionSurfaceName) && IsNameUnique(MarkingData.StartPointName) && IsNameUnique(MarkingData.TrackingCurveName) && IsNameUnique(MarkingData.AxisSystemName))
            {
                if(IsDesignTableActivated)
                    MessengerInstance.Send<int>(_partList.Count, Enum.ProcessToken.ComplexStarted);
                else
                    MessengerInstance.Send<int>(1, Enum.ProcessToken.ComplexStarted);

                await Task.Run(() =>
                {
                    MarkingGenerator markingGenerator = new MarkingGenerator();
                    markingGenerator.ProgressUpdated += MarkingGenerator_ProgressUpdated; ;

                    try
                    {

                        // If the design table mode is selected
                        if (IsDesignTableActivated)
                        {
                            markingGenerator.RunForCatalogPart(_selectedPartDocument.CatiaEnv, MarkingData, _designTableFullPath, _partList, _toleranceFactor, _keepHistoric, _createVolume);
                        }
                        else
                        {
                            markingGenerator.Run(_selectedPartDocument, MarkingData, new List<Model.CatiaShape.CatiaCharacter>(), _toleranceFactor, _keepHistoric, _createVolume);
                        }

                        MessengerInstance.Send<object>(null, Enum.ProcessToken.Finished);
                    }
                    catch (Exception ex)
                    {
                        MessengerInstance.Send(ex.Message, Enum.ProcessToken.Failed);
                    }

                });

            }
            else
                MessengerInstance.Send("", Enum.InputDataToken.SelectionIncorrect);
        }

        private void MarkingGenerator_ProgressUpdated(object sender, (double progressRate, int currentStep) e)
        {
            MessengerInstance.Send<(double, int)>((e.progressRate * 100, e.currentStep), Enum.ProcessToken.Refresh);
        }


        #endregion


        #region EVENTS

        private void MarkingData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CheckIfCanGenerate();
        }


        private void DesignTable_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CheckIfCanGenerate();
        }

        #endregion


        private void CheckIfCanGenerate()
        {
            if (MarkingData.Font != null &&
                _selectedPartDocument != null &&
                MarkingData.ProjectionSurfaceName != MarkingData.DEFAULT_PROJECTION_SURFACE_NAME &&
                MarkingData.TrackingCurveName != MarkingData.DEFAULT_TRACKING_CURVE_NAME &&
                MarkingData.StartPointName != MarkingData.DEFAULT_START_POINT_NAME &&
                MarkingData.AxisSystemName != MarkingData.DEFAULT_AXIS_SYSTEM_NAME)
            {
                if (IsDesignTableActivated)
                {
                    if (MarkingData.Text.IsLinkOn || (!MarkingData.Text.IsLinkOn && MarkingData.Text.Value != "") &&
                        MarkingData.CharacterHeight.IsLinkOn || (!MarkingData.CharacterHeight.IsLinkOn && MarkingData.CharacterHeight.Value > 0) &&
                        MarkingData.ExtrusionHeight.IsLinkOn || (!MarkingData.ExtrusionHeight.IsLinkOn && MarkingData.ExtrusionHeight.Value != 0) &&
                        System.IO.File.Exists(_designTableFullPath) && _partList.Count > 0)
                    {
                        CanGenerate = true;
                    }
                    else
                    {
                        CanGenerate = false;
                    }
                }
                else
                {
                    if (MarkingData.Text.Value != "" && MarkingData.CharacterHeight.Value > 0 && MarkingData.ExtrusionHeight.Value != 0)
                    {
                        CanGenerate = true;
                    }
                    else
                    {
                        CanGenerate = false;
                    }
                }
            }
            else
            {
                CanGenerate = false;
            }
        }


        private void DesignTableLoaded(string designTableFullPath)
        {
            MarkingData.Text.LinkedParameter = DesignTableParameter.NoLinkParameter();
            MarkingData.ExtrusionHeight.LinkedParameter = DesignTableParameter.NoLinkParameter();
            MarkingData.CharacterHeight.LinkedParameter = DesignTableParameter.NoLinkParameter();

            _designTableFullPath = designTableFullPath;

            CheckIfCanGenerate();
        }

        private bool IsNameUnique(string name)
        {
            Selection selection = _selectedPartDocument.PartDocument.Selection;
            selection.Clear();
            selection.Search("Name=" + name + ",all");

            return selection.Count == 1 ? true : false;
        }
    }
}
