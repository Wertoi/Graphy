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
        }

        // PRIVATE CONST
        private const string USER_PREFERENCE_FILE_NAME = "USER_PREFERENCES.txt";
        private const string LICENCE_FILE_NAME = "LICENSES.txt";

        // PRIVATE ATTRIBUTS


        // PUBLIC ATTRIBUTS
        private Language _selectedLanguage;

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



        // COMMANDS

        private RelayCommand _showLicenceCommand;
        public RelayCommand ShowLicenceCommand { get => _showLicenceCommand; set => _showLicenceCommand = value; }

        private void ShowLicenceCommandAction()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;

            if (System.IO.File.Exists(path + LICENCE_FILE_NAME))
            {
                try
                {
                    System.Diagnostics.Process.Start(path + LICENCE_FILE_NAME);
                }
                catch (Exception ex)
                {
                    MessengerInstance.Send(ex.Message, Enum.SettingToken.LicenceFileReadingFailed);
                }
            }
            else
            {
                MessengerInstance.Send("License file should be in the .exe directory.", Enum.SettingToken.LicenceFileReadingFailed);
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
