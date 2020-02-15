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

namespace Graphy.ViewModel
{
    public class SettingViewModel : ViewModelBase
    {
        public SettingViewModel()
        {
            // COMMANDS INITIALIZATION
            ShowLicenceCommand = new RelayCommand(ShowLicenceCommandAction);

            // MESSENGER REGISTRATION
            MessengerInstance.Register<ICollectionView>(this, Enum.FontToken.FavoriteFontListChanged, (collection) => WriteUserPreference(collection));

            // INITIALIZE SETTINGS
            InitializeLanguage();
            ReadUserPreference();

            MarkingDataSettings = new MarkingData.MarkingDataSettings();
            MarkingDataSettings.PropertyChanged += MarkingDataSetting_PropertyChanged;

            MarkingDataSettings.ToleranceFactor = 0.001;
            MarkingDataSettings.KeepHistory = true;
            MarkingDataSettings.CreateVolume = true;
        }

        private void MarkingDataSetting_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            MessengerInstance.Send<MarkingData.MarkingDataSettings>(MarkingDataSettings, Enum.SettingToken.MarkingDateSettingChange);
        }

        // PRIVATE CONST
        private const string USER_PREFERENCE_FILE_NAME = "USER_PREFERENCES.txt";
        private const string LICENCE_LINK = "https://github.com/Wertoi/Graphy/blob/master/LICENSE";

        // PRIVATE ATTRIBUTS


        // PUBLIC ATTRIBUTS
        private Language _selectedLanguage;
        private MarkingData.MarkingDataSettings _markingDataSettings;

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

        public MarkingData.MarkingDataSettings MarkingDataSettings
        {
            get => _markingDataSettings;
            set
            {
                Set(() => MarkingDataSettings, ref _markingDataSettings, value);
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
            string userPreferenceFullPath = App.GetExeDirectory() + Path.DirectorySeparatorChar + USER_PREFERENCE_FILE_NAME;
            if (System.IO.File.Exists(userPreferenceFullPath))
            {
                try
                {
                    List<string> selectedFontList = new List<string>();

                    using (StreamReader sr = new StreamReader(userPreferenceFullPath))
                    {
                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine();

                            selectedFontList.Add(line);
                        }
                    }

                    MessengerInstance.Send<List<string>>(selectedFontList, Enum.SettingToken.UserPreferencesChanged);
                }
                catch (Exception e)
                {
                    MessengerInstance.Send<string>(e.Message, Enum.SettingToken.SettingFileReadingFailed);
                }
            }
            else
            {
                System.Windows.Data.CollectionView fontCollection = new System.Windows.Data.CollectionView(new List<SelectableFont>()
                {
                    new SelectableFont(new FontFamily("Monospac821 BT"))
                }) ;
                WriteUserPreference(fontCollection);
            }
        }


        private void WriteUserPreference(ICollectionView collection)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(App.GetExeDirectory() + Path.DirectorySeparatorChar + USER_PREFERENCE_FILE_NAME, false))
                {
                    foreach (SelectableFont font in collection)
                    {
                        if (font.IsSelected)
                            sw.WriteLine(font.FontFamily.Source);
                    }
                }
            }
            catch (IOException e)
            {
                MessengerInstance.Send(e.Message, Enum.SettingToken.SettingFileWritingFailed);
            }
        }


    }
}
