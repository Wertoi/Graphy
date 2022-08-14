using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphy.Model;
using System.Collections.ObjectModel;
using System.Drawing.Text;
using GalaSoft.MvvmLight.Command;
using INFITF;
using MECMOD;
using Graphy.Enum;
using Graphy.Model.CatiaObject;
using Graphy.Model.CatiaObject.CatiaShape;


namespace Graphy.ViewModel
{
    public class SimpleMarkingViewModel : ViewModelBase
    {
        public SimpleMarkingViewModel()
        {
            FontFamilyCollection = new ObservableCollection<FontFamily>();
            MarkablePart = new MarkablePart
            {
                MarkingData = MarkingData.NoMarkingData()
            };

            MarkablePart.MarkingData.Text = MarkingData.Default().Text;

            // Fill the FontFamilyCollection with the installed fonts
            InstalledFontCollection installedFontCollection = new InstalledFontCollection();
            foreach (System.Drawing.FontFamily fontFamily in installedFontCollection.Families)
            {
                FontFamilyCollection.Add(new FontFamily(fontFamily.Name));
            }

            MarkablePart.MarkingData.FontFamily = FontFamilyCollection.ToList().Find((fontFamily) => fontFamily.Source == "Calibri");

            // MESSENGER REGISTRATION
            // From Catia
            MessengerInstance.Register<CatiaPart>(this, Enum.CatiaToken.SelectedPartChanged, (partDoc) => { MarkablePart.CatiaPart = partDoc; });

            // From settings
            MessengerInstance.Register<double>(this, Enum.SettingToken.ToleranceFactorChanged, (toleranceFactor) => { _toleranceFactor = toleranceFactor; });
            MessengerInstance.Register<bool>(this, Enum.SettingToken.KeepHistoricChanged, (keepHistoric) => { _keepHistoric = keepHistoric; });
            MessengerInstance.Register<bool>(this, Enum.SettingToken.CreateVolumeChanged, (createVolume) => { _createVolume = createVolume; });

            // From Icon
            MessengerInstance.Register<Icon>(this, Enum.IconToken.SelectedIconChanged, (icon) => { MarkablePart.MarkingData.Icon = icon; });


            // COMMANDS INITIALIZATION
            SelectTrackingCurveCommand = new RelayCommand(SelectTrackingCurveCommandAction);
            SelectStartPointCommand = new RelayCommand(SelectStartPointCommandAction);
            SelectProjectionSurfaceCommand = new RelayCommand(SelectProjectionSurfaceCommandAction);
            SelectAxisSystemCommand = new RelayCommand(SelectAxisSystemCommandAction);
            GenerateAxisSystemCommand = new RelayCommand(GenerateAxisSystemCommandAction);

            GenerateCommand = new RelayCommand(GenerateCommandAction);
        }

        // PUBLIC ATTRIBUTS
        private ObservableCollection<FontFamily> _fontFamilyCollection;
        private MarkablePart _markablePart;

        // PRIVATE ATTRIBUTS
        private double _toleranceFactor;
        private bool _keepHistoric;
        private bool _createVolume;

        public ObservableCollection<FontFamily> FontFamilyCollection
        {
            get => _fontFamilyCollection;
            set
            {
                Set(() => FontFamilyCollection, ref _fontFamilyCollection, value);
            }
        }

        public MarkablePart MarkablePart
        {
            get => _markablePart;
            set
            {
                Set(() => MarkablePart, ref _markablePart, value);
            }
        }

        #region SHAPE SELECTION

        #region Select Tracking Curve Command
        private RelayCommand _selectTrackingCurveCommand;
        public RelayCommand SelectTrackingCurveCommand { get => _selectTrackingCurveCommand; set => _selectTrackingCurveCommand = value; }

        private void SelectTrackingCurveCommandAction()
        {
            bool selectionStatus = TrySelectShape(ShapeType.Spline, out string shapeName);
            if (selectionStatus)
                MarkablePart.MarkingData.TrackingCurveName = shapeName;
        }
        #endregion


        #region Select Start Point Command
        private RelayCommand _selectStartPointCommand;
        public RelayCommand SelectStartPointCommand { get => _selectStartPointCommand; set => _selectStartPointCommand = value; }

        private void SelectStartPointCommandAction()
        {
            bool selectionStatus = TrySelectShape(ShapeType.Point, out string shapeName);
            if (selectionStatus)
                MarkablePart.MarkingData.ReferencePointName = shapeName;
        }
        #endregion


        #region Select Projection Surface Command
        private RelayCommand _selectProjectionSurfaceCommand;
        public RelayCommand SelectProjectionSurfaceCommand { get => _selectProjectionSurfaceCommand; set => _selectProjectionSurfaceCommand = value; }

        private void SelectProjectionSurfaceCommandAction()
        {

            bool selectionStatus = TrySelectShape(ShapeType.Surface, out string shapeName);
            if (selectionStatus)
                MarkablePart.MarkingData.ProjectionSurfaceName = shapeName;
        }
        #endregion


        #region Select Axis System Command
        private RelayCommand _selectAxisSystemCommand;
        public RelayCommand SelectAxisSystemCommand { get => _selectAxisSystemCommand; set => _selectAxisSystemCommand = value; }

        private void SelectAxisSystemCommandAction()
        {
            bool selectionStatus = TrySelectShape(ShapeType.AxisSystem, out string axisName);
            if (selectionStatus)
                MarkablePart.MarkingData.AxisSystemName = axisName;
        }

        private RelayCommand _generateAxisSystemCommand;
        public RelayCommand GenerateAxisSystemCommand { get => _generateAxisSystemCommand; set => _generateAxisSystemCommand = value; }

        private void GenerateAxisSystemCommandAction()
        {
            PartDocument partDocument = MarkablePart.CatiaPart.PartDocument;

            CatiaSurface supportSurface = CatiaSurface.GetCatiaSurface(partDocument, MarkablePart.MarkingData.ProjectionSurfaceName);

            CatiaAxisSystem catiaAxisSystem = new CatiaAxisSystem(partDocument,
                CatiaPoint.GetCatiaPoint(partDocument, MarkablePart.MarkingData.ReferencePointName),
                CatiaCurve.GetCatiaCurve(partDocument, MarkablePart.MarkingData.TrackingCurveName, supportSurface),
                supportSurface,
                true, true, true);

            string axisSystemName = "Marking_Reference_AxisSystem";
            string axisSystemNameCount = "";
            Selection selection = MarkablePart.CatiaPart.PartDocument.Selection;
            int count = 0;
            do
            {
                count++;
                axisSystemNameCount = axisSystemName + count;
                selection.Clear();
                selection.Search("Name=" + axisSystemNameCount + ",all");

            } while (selection.Count != 0);

            catiaAxisSystem.System.set_Name(axisSystemNameCount);
            MarkablePart.MarkingData.AxisSystemName = axisSystemNameCount;

        }
        #endregion

        private enum ShapeType
        {
            Point,
            Spline,
            Surface,
            AxisSystem
        }

        private bool TrySelectShape(ShapeType shapeType, out string shapeName)
        {
            shapeName = "";

            MessengerInstance.Send<CatiaPart>(MarkablePart.CatiaPart, Enum.CatiaToken.Refresh);

            if (MarkablePart.CatiaPart is null)
                return false;

            // DEFINE THE CATIA SELECTION FILTER BASED ON THE SHAPE TYPE
            object filterStr;
            switch (shapeType)
            {
                case ShapeType.Point:
                    filterStr = new object[1] { "ZeroDim" };
                    break;

                case ShapeType.Spline:
                    filterStr = new object[1] { "MonoDim" };
                    break;

                case ShapeType.Surface:
                    filterStr = new object[1] { "BiDim" };
                    break;

                case ShapeType.AxisSystem:
                    filterStr = new object[1] { "AxisSystem" };
                    break;

                default:
                    filterStr = new object[1] { "ZeroDim" };
                    break;
            }

            // CREATE THE CATIA SELECTOR
            Selection selection = MarkablePart.CatiaPart.PartDocument.Selection;
            selection.Clear();
            string selectionStatus = selection.SelectElement2((Array)filterStr, "", false);

            // IF THE SELECTION IS CANCEL OR OTHER ABORT.
            if (selectionStatus == "Cancel" || selectionStatus == "Undo" || selectionStatus == "Redo")
                return false;

            // OTHERWISE, RETRIEVE THE NAME OF THE SELECTION BASED ON THE SHAPE TYPE
            try
            {
                string tempShapeName;
                switch (shapeType)
                {
                    case ShapeType.AxisSystem:
                        tempShapeName = ((AxisSystem)selection.Item(1).Value).get_Name();
                        break;

                    default:
                        tempShapeName = ((HybridShape)selection.Item(1).Value).get_Name();
                        break;
                }

                // CHECK IF THE NAME IS UNIQUE IN THE CATPART.
                selection.Clear();
                selection.Search("Name=" + tempShapeName + ",all");

                // If the name is non-unique, return false
                if (selection.Count != 1)
                {
                    MessengerInstance.Send("", Enum.InputDataToken.SelectionIncorrect);
                    return false;
                }
                else
                {
                    shapeName = tempShapeName;
                    return true;
                }
            }
            catch (Exception)
            {
                MessengerInstance.Send("", Enum.InputDataToken.SelectionIncorrect);
                return false;
            }

        }

        #endregion


        // GENERATE COMMAND
        private RelayCommand _generateCommand;
        public RelayCommand GenerateCommand { get => _generateCommand; set => _generateCommand = value; }

        private async void GenerateCommandAction()
        {
            // STORE THE PREVIOUS CATIA PART DOCUMENT
            CatiaPart previousCatiaPartDocument = MarkablePart.CatiaPart;

            // DO A REFRESH OF CATIA TO PREVENT ANY ISSUE
            MessengerInstance.Send<CatiaPart>(MarkablePart.CatiaPart, Enum.CatiaToken.Refresh);

            // IF ACTUAL DOCUMENT IS DIFFERENT FROM PREVIOUS ONE, STOP THE PROCESS
            if (MarkablePart.CatiaPart.FileFullPath != previousCatiaPartDocument.FileFullPath)
            {
                MessengerInstance.Send("Part file has changed.", Enum.ProcessToken.Failed);
                return;
            }

            MessengerInstance.Send<int>(1, Enum.ProcessToken.ComplexStarted);

            await Task.Run(() =>
            {
                MarkingGenerator markingGenerator = new MarkingGenerator();
                markingGenerator.ProgressUpdated += MarkingGenerator_ProgressUpdated;

                // Marking name is not assigned by user.
                MarkablePart.MarkingData.Name = MarkablePart.MarkingData.IsText ? MarkablePart.MarkingData.Text : MarkablePart.MarkingData.Icon.Name;

                try
                {
                    markingGenerator.Run(MarkablePart.CatiaPart, MarkablePart.MarkingData, new List<CatiaCharacter>(),
                        _toleranceFactor, _keepHistoric, _createVolume);

                    MessengerInstance.Send<object>(null, Enum.ProcessToken.Finished);
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

    }
}
