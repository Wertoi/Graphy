using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using INFITF;

namespace Graphy.ViewModel
{
    public class ProcessViewModel : ViewModelBase
    {
        // CONSTRUCTOR
        public ProcessViewModel()
        {
            // MESSENGER REGISTRATION

            // PROCESS TOKEN
            MessengerInstance.Register<string>(this, Enum.ProcessToken.Failed, (msg) => ProcessFailed(msg));

            MessengerInstance.Register<(double progressRate, int currentStep)>(this, Enum.ProcessToken.Refresh,
                (progressInfo) => ProcessRefresh(progressInfo.progressRate, progressInfo.currentStep));

            MessengerInstance.Register<object>(this, Enum.ProcessToken.SimpleStarted, (_) => ProcessSimpleStarted());
            MessengerInstance.Register<int>(this, Enum.ProcessToken.ComplexStarted, (maximumStep) => ProcessComplexStarted(maximumStep));

            MessengerInstance.Register<string>(this, Enum.ProcessToken.Finished, (msg) => ProcessFinished(msg));

            // SETTING TOKEN
            MessengerInstance.Register<string>(this, Enum.SettingToken.LicenceFileReadingFailed, (msg) => LicenceFileReadingFailed(msg));

            // *** END MESSENGER REGISTRATION ***



            // Commands initialization
            ResetExceptionCommand = new RelayCommand(ResetExceptionCommandAction);
        }


        // ATTRIBUTS
        private bool _isOneStateActivated;
        private bool _isInProgress = false;
        private double _progressRate = 0;
        private bool _isProgressRateAvailable = false;
        private int _currentStep;
        private int _maximumStep;

        private bool _isInformationRaised = false;
        private bool _isExceptionRaised = false;
        private string _informationMessage = "";
        private string _exceptionMessage = "";


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

        public bool IsInformationRaised
        {
            get => _isInformationRaised;
            set
            {
                Set(() => IsInformationRaised, ref _isInformationRaised, value);
            }
        }

        public bool IsExceptionRaised
        {
            get => _isExceptionRaised;
            set
            {
                Set(() => IsExceptionRaised, ref _isExceptionRaised, value);
            }
        }


        public string InformationMessage
        {
            get => _informationMessage;
            set
            {
                Set(() => InformationMessage, ref _informationMessage, value);
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

        public double ProgressRate
        {
            get => _progressRate;
            set
            {
                Set(() => ProgressRate, ref _progressRate, value);
            }
        }

        public bool IsProgressRateAvailable
        {
            get => _isProgressRateAvailable;
            set
            {
                Set(() => IsProgressRateAvailable, ref _isProgressRateAvailable, value);
            }
        }

        public int MaximumStep
        {
            get => _maximumStep;
            set
            {
                Set(() => MaximumStep, ref _maximumStep, value);
            }
        }

        public int CurrentStep
        {
            get => _currentStep;
            set
            {
                Set(() => CurrentStep, ref _currentStep, value);
            }
        }


        // COMMANDS

        // RESET EXCEPTIONS COMMAND
        private RelayCommand _resetExceptionCommand;
        public RelayCommand ResetExceptionCommand { get => _resetExceptionCommand; set => _resetExceptionCommand = value; }

        private void ResetExceptionCommandAction()
        {
            IsInformationRaised = false;
            IsExceptionRaised = false;
            InformationMessage = "";
            ExceptionMessage = "";

            if (IsInProgress)
                IsInProgress = false;

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
                if (IsExceptionRaised)
                {
                    IsInProgress = false;
                    IsInformationRaised = false;
                }

                if (IsInformationRaised || IsExceptionRaised || IsInProgress)
                    IsOneStateActivated = true;
                else
                    IsOneStateActivated = false;
            }
        }

        private void ProcessSimpleStarted()
        {
            IsInProgress = true;
            IsProgressRateAvailable = false;

            ManageStates();
        }

        private void ProcessComplexStarted(int maximumStep)
        {
            IsInProgress = true;
            IsProgressRateAvailable = true;
            ProgressRate = 0;
            MaximumStep = maximumStep;


            ManageStates();
        }

        private void ProcessRefresh(double progressRate, int currentStep)
        {
            ProgressRate = progressRate;
            CurrentStep = currentStep;
        }

        private void ProcessFinished(string msg)
        {
            IsInformationRaised = true;
            IsInProgress = false;
            InformationMessage = msg;

            ManageStates();
        }

        private void ProcessFailed(string msg)
        {
            IsExceptionRaised = true;
            ExceptionMessage = msg;

            ManageStates();
        }

        private void LicenceFileReadingFailed(string msg)
        {
            IsExceptionRaised = true;
            ExceptionMessage = "An error occured during licenses file opening.\r\n\r\nError report: \r\n" + msg;

            ManageStates();
        }

    }
}
