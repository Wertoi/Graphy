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

            // INPUT DATA TOKEN
            MessengerInstance.Register<string>(this, Enum.InputDataToken.SelectionIncorrect, (dataTypeName) => SelectionIncorrect(dataTypeName));

            // PROCESS TOKEN
            MessengerInstance.Register<string>(this, Enum.ProcessToken.Failed, (msg) => ProcessFailed(msg));
            MessengerInstance.Register<double>(this, Enum.ProcessToken.Refresh, (rate) => { ProgressRate = rate; });
            MessengerInstance.Register<string>(this, Enum.ProcessToken.Started, (msg) => ProcessStarted(msg));
            MessengerInstance.Register<bool>(this, Enum.ProcessToken.Finished, (closeDirectly) => ProcessFinished(closeDirectly));

            // FONT TOKEN


            // SETTING TOKEN
            MessengerInstance.Register<string>(this, Enum.SettingToken.LicenceFileReadingFailed, (msg) => LicenceFileReadingFailed(msg));

            // *** END MESSENGER REGISTRATION ***

            // Commands initialization
            ResetExceptionCommand = new RelayCommand(ResetExceptionCommandAction);
            TerminateCommand = new RelayCommand(TerminateCommandAction);
        }


        // ATTRIBUTS
        private bool _isOneStateActivated;
        private bool _isInProgress = false;
        private bool _isFinished = false;
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
            ExceptionMessage = "";

            if (IsInProgress)
                IsInProgress = false;

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
                }
                else
                {
                    if (IsGeneralExceptionRaised)
                    {
                        IsInProgress = false;
                    }
                }

                if (IsFinished || IsGeneralExceptionRaised || IsInProgress)
                    IsOneStateActivated = true;
                else
                    IsOneStateActivated = false;
            }
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
