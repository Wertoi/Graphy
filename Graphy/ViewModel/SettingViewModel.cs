﻿using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Command;
using Graphy.Enum;
using System.Globalization;
using Graphy.Model;
using Graphy.CsvStream;

namespace Graphy.ViewModel
{
    public class SettingViewModel : ViewModelBase
    {
        public SettingViewModel()
        {
            // COMMANDS INITIALIZATION
            ShowLicenceCommand = new RelayCommand(ShowLicenceCommandAction);

            // MESSENGER REGISTRATION
            MessengerInstance.Register<List<Icon>>(this, Enum.IconToken.IconCollectionChanged, (collection) => SaveIconCollection(collection));

            // INITIALIZE SETTINGS
            InitializeLanguage();
            ReadUserPreference();
        }



        // PRIVATE CONST
        private const string LICENCE_LINK = "https://github.com/Wertoi/Graphy/blob/master/LICENSE";

        // PRIVATE ATTRIBUTS
        private bool _isReadingUserPreferenceFlag;

        // PUBLIC ATTRIBUTS
        private Language _selectedLanguage;
        private double _toleranceFactor;
        private bool _keepHistoric;
        private bool _createVolume;
        private CsvConfig _csvConfig;
        private ImportMode _selectedImportMode;
        private HorizontalAxisSystemPosition _horizontalAxisSystemPosition;

        public Language SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                Set(() => SelectedLanguage, ref _selectedLanguage, value);

                switch (value)
                {
                    case Language.French:
                        TranslationSource.Instance.CurrentCulture = new CultureInfo("fr-FR");
                        break;

                    case Language.English:
                        TranslationSource.Instance.CurrentCulture = new CultureInfo("en-US");
                        break;

                    default:
                        TranslationSource.Instance.CurrentCulture = new CultureInfo("en-US");
                        break;
                }
            }
        }

        public double ToleranceFactor
        {
            get => _toleranceFactor;
            set
            {
                Set(() => ToleranceFactor, ref _toleranceFactor, value);

                if(!_isReadingUserPreferenceFlag)
                {
                    Properties.Settings.Default.ToleranceFactor = ToleranceFactor;
                    Properties.Settings.Default.Save();
                }

                MessengerInstance.Send(ToleranceFactor, Enum.SettingToken.ToleranceFactorChanged);
            }
        }

        public bool KeepHistoric
        {
            get => _keepHistoric;
            set
            {
                Set(() => KeepHistoric, ref _keepHistoric, value);

                if (!_isReadingUserPreferenceFlag)
                {
                    Properties.Settings.Default.KeepHistoric = KeepHistoric;
                    Properties.Settings.Default.Save();
                }

                MessengerInstance.Send(KeepHistoric, Enum.SettingToken.KeepHistoricChanged);
            }
        }

        public bool CreateVolume
        {
            get => _createVolume;
            set
            {
                Set(() => CreateVolume, ref _createVolume, value);

                if (!_isReadingUserPreferenceFlag)
                {
                    Properties.Settings.Default.CreateVolume = CreateVolume;
                    Properties.Settings.Default.Save();
                }

                MessengerInstance.Send(CreateVolume, Enum.SettingToken.CreateVolumeChanged);
            }
        }

        public CsvConfig CsvConfig
        {
            get => _csvConfig;
            set
            {
                Set(() => CsvConfig, ref _csvConfig, value);
            }
        }


        private void CsvConfig_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!_isReadingUserPreferenceFlag)
            {
                Properties.Settings.Default.CsvConfig = CsvConfig;
                Properties.Settings.Default.Save();
            }

            MessengerInstance.Send(CsvConfig, Enum.SettingToken.CsvConfigChanged);
        }


        public ImportMode SelectedImportMode
        {
            get => _selectedImportMode;
            set
            {
                Set(() => SelectedImportMode, ref _selectedImportMode, value);

                if (!_isReadingUserPreferenceFlag)
                {
                    Properties.Settings.Default.ImportMode = (int)SelectedImportMode;
                    Properties.Settings.Default.Save();
                }

                MessengerInstance.Send(SelectedImportMode, Enum.SettingToken.ImportModeChanged);
            }
        }


        public HorizontalAxisSystemPosition HorizontalAxisSystemPosition
        {
            get => _horizontalAxisSystemPosition;
            set
            {
                Set(() => HorizontalAxisSystemPosition, ref _horizontalAxisSystemPosition, value);

                if(!_isReadingUserPreferenceFlag)
                {
                    Properties.Settings.Default.HorizontalAxisSystemPosition = (int)HorizontalAxisSystemPosition;
                    Properties.Settings.Default.Save();
                }

                MessengerInstance.Send<HorizontalAxisSystemPosition>(HorizontalAxisSystemPosition, Enum.SettingToken.HorizontalAxisSystemPositionChanged);
            }
        }

        // COMMANDS

        private RelayCommand _showLicenceCommand;
        public RelayCommand ShowLicenceCommand { get => _showLicenceCommand; set => _showLicenceCommand = value; }

        private void ShowLicenceCommandAction()
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(LICENCE_LINK));
            }
            catch(Exception ex)
            {
                MessengerInstance.Send(ex.Message, Enum.SettingToken.LicenceFileReadingFailed);
            }
        }


        // METHODS

        /// <summary>
        /// 
        /// </summary>
        public void InitializeLanguage()
        {
            // Initialize language
            if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name.Contains("FR"))
            {
                SelectedLanguage = Language.French;
            }
            else
            {
                SelectedLanguage = Language.English;
            }
        }

        private void ReadUserPreference()
        {
            _isReadingUserPreferenceFlag = true;

            ToleranceFactor = Properties.Settings.Default.ToleranceFactor;
            KeepHistoric = Properties.Settings.Default.KeepHistoric;
            CreateVolume = Properties.Settings.Default.CreateVolume;
            SelectedImportMode = (ImportMode)Properties.Settings.Default.ImportMode;
            HorizontalAxisSystemPosition = (HorizontalAxisSystemPosition)Properties.Settings.Default.HorizontalAxisSystemPosition;

            if(Properties.Settings.Default.CsvConfig == null)
            {
                CsvConfig = CsvConfig.Default;
                Properties.Settings.Default.CsvConfig = CsvConfig;
                Properties.Settings.Default.Save();
            }
            else
                CsvConfig = Properties.Settings.Default.CsvConfig;

            CsvConfig.PropertyChanged += CsvConfig_PropertyChanged;

            MessengerInstance.Send(Properties.Settings.Default.IconCollection, Enum.SettingToken.IconCollectionChanged);

            _isReadingUserPreferenceFlag = false;
        }


        private void SaveIconCollection(List<Icon> iconCollection)
        {
            System.Collections.Specialized.StringCollection stringCollection = new System.Collections.Specialized.StringCollection();
            foreach(Icon icon in iconCollection)
            {
                stringCollection.Add(icon.Name + '\t' + icon.PathData);
            }

            Properties.Settings.Default.IconCollection = stringCollection;
            Properties.Settings.Default.Save();
        }

    }
}
