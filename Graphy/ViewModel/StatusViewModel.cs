using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using INFITF;

namespace Graphy.ViewModel
{
    public class StatusViewModel : ViewModelBase
    {
        // CONSTRUCTOR
        public StatusViewModel()
        {
            // MESSENGER REGISTRATION

            // CATIA TOKEN
            MessengerInstance.Register<string>(this, Enum.CatiaToken.Close, (str) => CatiaApplicationIsClose(str));
            MessengerInstance.Register<string>(this, Enum.CatiaToken.IncorrectLengthUnit, (catiaLengthUnit) => IncorrectLengthUnit(catiaLengthUnit));

            // INPUT DATA TOKEN
            MessengerInstance.Register<string>(this, Enum.InputDataToken.SelectionIncorrect, (dataTypeName) => SelectionIncorrect(dataTypeName));

            // PROCESS TOKEN
            MessengerInstance.Register<string>(this, Enum.ProcessToken.Failed, (msg) => ProcessFailed(msg));
            MessengerInstance.Register<double>(this, Enum.ProcessToken.Refresh, (rate) => { ProgressRate = rate; });
            MessengerInstance.Register<string>(this, Enum.ProcessToken.Started, (msg) => ProcessStarted(msg));
            MessengerInstance.Register<bool>(this, Enum.ProcessToken.Finished, (closeDirectly) => ProcessFinished(closeDirectly));

            // FONT TOKEN


            // SETTING TOKEN
            MessengerInstance.Register<string>(this, Enum.SettingToken.SettingFileReadingFailed, (msg) => SettingFileReadingFailed(msg));
            MessengerInstance.Register<string>(this, Enum.SettingToken.SettingFileWritingFailed, (msg) => SettingFileWritingFailed(msg));
            MessengerInstance.Register<string>(this, Enum.SettingToken.LicenceFileReadingFailed, (msg) => LicenceFileReadingFailed(msg));

            // *** END MESSENGER REGISTRATION ***

            // Commands initialization
            ResetExceptionCommand = new RelayCommand(ResetExceptionCommandAction);
            TerminateCommand = new RelayCommand(TerminateCommandAction);
            ResetCatiaExceptionCommand = new RelayCommand(ResetCatiaExceptionCommandAction);
        }


        // ATTRIBUTS
        private bool _isOneStateActivated;
        private bool _isInProgress = false;
        private bool _isFinished = false;
        private bool _isCatiaExceptionRaised = false;
        private bool _isGeneralExceptionRaised = false;
        private string _exceptionMessage = "";
        private string _processMessage = "";
        private double _progressRate = 0;


        public bool IsOneStateActivated
        {
            get => _isOneStateActivated;
            set
            {
                Set(() => IsOneStateActivated, ref _isOneStateActivated, value);
            }
        }

        public bool IsInProgress
        {
            get => _isInProgress;
            set
            {
                Set(() => IsInProgress, ref _isInProgress, value);
            }
        }

        public bool IsFinished
        {
            get => _isFinished;
            set
            {
                Set(() => IsFinished, ref _isFinished, value);     
            }
        }

        public bool IsCatiaExceptionRaised
        {
            get => _isCatiaExceptionRaised;
            set
            {
                Set(() => IsCatiaExceptionRaised, ref _isCatiaExceptionRaised, value);
            }
        }

        public bool IsGeneralExceptionRaised
        {
            get => _isGeneralExceptionRaised;
            set
            {
                Set(() => IsGeneralExceptionRaised, ref _isGeneralExceptionRaised, value);
            }
        }


        public string ExceptionMessage
        {
            get => _exceptionMessage;
            set
            {
                Set(() => ExceptionMessage, ref _exceptionMessage, value);
            }
        }

        public string ProcessMessage
        {
            get => _processMessage;
            set
            {
                Set(() => ProcessMessage, ref _processMessage, value);
            }
        }

        public double ProgressRate
        {
            get => _progressRate;
            set
            {
                Set(() => ProgressRate, ref _progressRate, value);
            }
        }


        // COMMANDS

        // RESET EXCEPTIONS COMMAND
        private RelayCommand _resetExceptionCommand;
        public RelayCommand ResetExceptionCommand { get => _resetExceptionCommand; set => _resetExceptionCommand = value; }

        private void ResetExceptionCommandAction()
        {
            IsGeneralExceptionRaised = false;
            IsCatiaExceptionRaised = false;
            ExceptionMessage = "";

            if (IsInProgress)
                IsInProgress = false;

            ManageStates();
        }

        // REFRESH CATIA EXCEPTION COMMAND
        private RelayCommand _resetCatiaExceptionCommand;
        public RelayCommand ResetCatiaExceptionCommand { get => _resetCatiaExceptionCommand; set => _resetCatiaExceptionCommand = value; }

        private void ResetCatiaExceptionCommandAction()
        {
            IsGeneralExceptionRaised = false;
            IsCatiaExceptionRaised = false;
            ExceptionMessage = "";

            if (IsInProgress)
                IsInProgress = false;

            MessengerInstance.Send<Model.CatiaDocument.CatiaPartDocument>(null, Enum.CatiaToken.Refresh);

            ManageStates();
        }

        // TERMINATE COMMAND
        private RelayCommand _terminateCommand;
        public RelayCommand TerminateCommand { get => _terminateCommand; set => _terminateCommand = value; }

        private void TerminateCommandAction()
        {
            IsFinished = false;

            ManageStates();
        }


        // METHODS

        /// <summary>
        /// Manages priorities between states.
        /// </summary>
        private void ManageStates()
        {
            if (IsInDesignMode)
                IsOneStateActivated = false;
            else
            {
                if (IsFinished)
                {
                    IsInProgress = false;
                    IsGeneralExceptionRaised = false;
                    IsCatiaExceptionRaised = false;
                }
                else
                {
                    if (IsCatiaExceptionRaised)
                    {
                        IsInProgress = false;
                        IsGeneralExceptionRaised = false;
                    }
                    else if (IsGeneralExceptionRaised)
                    {
                        IsInProgress = false;
                    }
                }

                if (IsFinished || IsCatiaExceptionRaised || IsGeneralExceptionRaised || IsInProgress)
                    IsOneStateActivated = true;
                else
                    IsOneStateActivated = false;
            }
        }


        private void IncorrectLengthUnit(string catiaLengthUnit)
        {
            IsCatiaExceptionRaised = true;
            ExceptionMessage = "Catia length unit must be the millimeter (mm).\r\nActual length unit: " + catiaLengthUnit;

            ManageStates();
        }

        private void CatiaApplicationIsClose(string errorMsg)
        {
            IsCatiaExceptionRaised = true;
            ExceptionMessage = "No Catia application has been detected.\r\n\r\nError message:\r\n" + errorMsg;

            ManageStates();
        }

        private void SelectionIncorrect(string dataTypeName)
        {
            IsGeneralExceptionRaised = true;
            ExceptionMessage = "Incorrect selection.\r\nSelection must have an unique name (avoid spaces and special characters).\r\nSelection from volumes are forbidden.";

            ManageStates();
        }

        private void ProcessStarted(string msg)
        {
            IsInProgress = true;
            ProgressRate = 0;
            ProcessMessage = msg;

            ManageStates();
        }

        private void ProcessFinished(bool closeDirectly)
        {
            IsInProgress = false;

            if (!closeDirectly)
                IsFinished = true;

            ManageStates();
        }

        private void ProcessFailed(string msg)
        {
            IsGeneralExceptionRaised = true;
            ExceptionMessage = "Process error. Input datas must be reviewed.\r\n\r\nError report: \r\n" + msg;

            ManageStates();
        }

        private void SettingFileReadingFailed(string msg)
        {
            IsGeneralExceptionRaised = true;
            ExceptionMessage = "An error occured during parameters loading.\r\nParameters will be reset.\r\n\r\nError report:\r\n" + msg;

            ManageStates();
        }

        private void SettingFileWritingFailed(string msg)
        {
            IsGeneralExceptionRaised = true;
            ExceptionMessage = "An error occured during parameters saving.\r\n\r\nError report: \r\n" + msg;

            ManageStates();
        }

        private void LicenceFileReadingFailed(string msg)
        {
            IsGeneralExceptionRaised = true;
            ExceptionMessage = "An error occured during licenses file opening.\r\n\r\nError report: \r\n" + msg;

            ManageStates();
        }
    }
}
