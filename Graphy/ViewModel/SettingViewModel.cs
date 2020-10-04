using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using INFITF;
using GalaSoft.MvvmLight.Command;
using System.IO;
using Graphy.Enum;
using System.ComponentModel;
using System.Globalization;
using Graphy.Model;
using Graphy.Model.Generator;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Graphy.Model.CatiaShape;

namespace Graphy.ViewModel
{
    public class SettingViewModel : ViewModelBase
    {
        public SettingViewModel()
        {
            // COMMANDS INITIALIZATION
            ShowLicenceCommand = new RelayCommand(ShowLicenceCommandAction);

            // MESSENGER REGISTRATION
            MessengerInstance.Register<List<SelectableFont>>(this, Enum.FontToken.FavoriteFontListChanged, (collection) => SaveFavoriteFontCollection(collection));

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
        private VerticalAlignment _verticalAlignment;

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

        public VerticalAlignment VerticalAlignment
        {
            get => _verticalAlignment;
            set
            {
                Set(() => VerticalAlignment, ref _verticalAlignment, value);

                if (!_isReadingUserPreferenceFlag)
                {
                    Properties.Settings.Default.VerticalAlignment = (int)VerticalAlignment;
                    Properties.Settings.Default.Save();
                }

                MessengerInstance.Send(VerticalAlignment, Enum.SettingToken.VerticalAlignmentChanged);
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
            VerticalAlignment = (VerticalAlignment)Properties.Settings.Default.VerticalAlignment;

            MessengerInstance.Send(Properties.Settings.Default.FavoriteFontCollection, Enum.SettingToken.UserPreferencesChanged);

            _isReadingUserPreferenceFlag = false;
        }


        private void SaveFavoriteFontCollection(List<SelectableFont> favoriteFontCollection)
        {
            System.Collections.Specialized.StringCollection stringCollection = new System.Collections.Specialized.StringCollection();
            foreach (SelectableFont font in favoriteFontCollection)
            {
                stringCollection.Add(font.FontFamily.Source);
            }

            Properties.Settings.Default.FavoriteFontCollection = stringCollection;
            Properties.Settings.Default.Save();
        }

    }
}
