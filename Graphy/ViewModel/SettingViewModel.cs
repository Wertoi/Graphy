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
            SelectGeneratedFontDirectoryPathCommand = new RelayCommand(SelectGeneratedFontDirectoryPathCommandAction);
            ShowLicenceCommand = new RelayCommand(ShowLicenceCommandAction);

            // MESSENGER REGISTRATION
            MessengerInstance.Register<CatiaEnv>(this, Enum.CatiaToken.Open, (catiaEnv) => { _catiaEnv = catiaEnv; });
            MessengerInstance.Register<Font>(this, Enum.FontToken.GenerateNewFont, (newFont) => GenerateFont(newFont));
            MessengerInstance.Register<List<Font>>(this, Enum.FontToken.SupportedCharacterComputed, (computedFontCollection) => WriteSupportedCharacterFile(computedFontCollection));

            // INITIALIZE SETTINGS
            InitializeSettings();
        }

        // PRIVATE CONST
        private const string SETTING_FILE_NAME = "SETTINGS.txt";
        private const string DEFAULT_FONT_DIRECTORY_NAME = "CATIA_FONT";
        private const string LICENCE_FILE_NAME = "LICENSES.txt";
        private const string SUPPORTED_CHARACTERS_FILE_NAME = "SUPPORTED_CHARACTERS.txt";

        // PRIVATE ATTRIBUTS
        private CatiaEnv _catiaEnv;

        // PUBLIC ATTRIBUTS
        private string _generatedFontDirectoryPath;
        //private string _classicFontDirectoryPath;
        private Language _selectedLanguage;
        


        public string GeneratedFontDirectoryPath
        {
            get => _generatedFontDirectoryPath;
            set
            {
                Set(() => GeneratedFontDirectoryPath, ref _generatedFontDirectoryPath, value);

                MessengerInstance.Send(GeneratedFontDirectoryPath, Enum.SettingToken.GeneratedFontDirectoryPathChanged);
            }
        }


        /*public string ClassicFontDirectoryPath
        {
            get => _classicFontDirectoryPath;
            set
            {
                Set(() => ClassicFontDirectoryPath, ref _classicFontDirectoryPath, value);

                MessengerInstance.Send(ClassicFontDirectoryPath, Enum.SettingToken.ClassicFontDirectoryPathChanged);
            }
        }*/


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
        private RelayCommand _selectGeneratedFontDirectoryPathCommand;
        public RelayCommand SelectGeneratedFontDirectoryPathCommand { get => _selectGeneratedFontDirectoryPathCommand; set => _selectGeneratedFontDirectoryPathCommand = value; }

        private void SelectGeneratedFontDirectoryPathCommandAction()
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = GeneratedFontDirectoryPath;

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                GeneratedFontDirectoryPath = folderBrowserDialog.SelectedPath;

                WriteSettingFile();
            }
        }


        /*private RelayCommand _selectClassicFontDirectoryPathCommand;
        public RelayCommand SelectClassicFontDirectoryPathCommand { get => _selectClassicFontDirectoryPathCommand; set => _selectClassicFontDirectoryPathCommand = value; }

        private void SelectClassicFontDirectoryPathCommandAction()
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = ClassicFontDirectoryPath;

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ClassicFontDirectoryPath = folderBrowserDialog.SelectedPath;

                WriteSettingFile();
            }
        }*/


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

        private void FontGenerator_ProgressRateChanged(object sender, ProgressRateChangedEventArgs e)
        {
            MessengerInstance.Send(e.ProgressRate * 100, Enum.ProcessToken.Refresh);
        }

        /// <summary>
        /// 
        /// </summary>
        public void InitializeSettings()
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

            ReadSettingFile();

            ReadSupportedCharacterFile();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ReadSettingFile()
        {
            // Initialize font directory path
            if (System.IO.File.Exists(App.GetExeDirectory() + Path.DirectorySeparatorChar + SETTING_FILE_NAME))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(App.GetExeDirectory() + Path.DirectorySeparatorChar + SETTING_FILE_NAME))
                    {
                        string fontDirectoryPathLine = sr.ReadLine();
                        GeneratedFontDirectoryPath = fontDirectoryPathLine.Split('\t')[1];
                    }
                }
                catch (Exception e)
                {
                    MessengerInstance.Send<string>(e.Message, Enum.SettingToken.SettingFileReadingFailed);
                }
            }
            else
            {
                MessengerInstance.Send<string>("Settings file not found.", Enum.SettingToken.SettingFileReadingFailed);

                // Create the setting file
                FileStream fileStream = System.IO.File.Create(App.GetExeDirectory() + Path.DirectorySeparatorChar + SETTING_FILE_NAME);
                fileStream.Close();

                // Generated Font Default value
                string defaultFontFolderPath = App.GetExeDirectory() + Path.DirectorySeparatorChar + DEFAULT_FONT_DIRECTORY_NAME;
                System.IO.Directory.CreateDirectory(defaultFontFolderPath);
                GeneratedFontDirectoryPath = defaultFontFolderPath;

                WriteSettingFile();
            }
        }

        private void WriteSettingFile()
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(App.GetExeDirectory() + Path.DirectorySeparatorChar + SETTING_FILE_NAME, false))
                {
                    sw.WriteLine("GENERATED_FONT_DIRECTORY_PATH" + "\t" + GeneratedFontDirectoryPath);
                }
            }
            catch (IOException e)
            {
                MessengerInstance.Send(e.Message, Enum.SettingToken.SettingFileWritingFailed);
            }
        }


        /// <summary>
        /// Read the supported sharacters file from exe location.
        /// Then store datas in the private supported char collection.
        /// Then 
        /// </summary>
        private void ReadSupportedCharacterFile()
        {
            if (System.IO.File.Exists(App.GetExeDirectory() + Path.DirectorySeparatorChar + SUPPORTED_CHARACTERS_FILE_NAME))
            {
                try
                {
                    List<Font> computedFontCollection = new List<Font>();
                    using (StreamReader sr = new StreamReader(App.GetExeDirectory() + Path.DirectorySeparatorChar + SUPPORTED_CHARACTERS_FILE_NAME))
                    {
                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine();

                            if (line.Split('\t').Count() == 1)
                                computedFontCollection.Last().SupportedCharacterList += line;
                            else
                            {
                                computedFontCollection.Add(new Font()
                                {
                                    Name = line.Split('\t')[0],
                                    SupportedCharacterList = line.Split('\t')[1]
                                });
                            }
                        }
                    }

                    // SEND DATA TO OTHER VIEW MODEL
                    MessengerInstance.Send(computedFontCollection, Enum.SettingToken.ComputedFontCollectionChanged);
                }
                catch (IOException e)
                {
                    MessengerInstance.Send(e.Message, Enum.SettingToken.SettingFileWritingFailed);
                }
            }
            else // CREATE THE SUPPORTED CHARACTERS FILE
            {
                FileStream fileStream = System.IO.File.Create(App.GetExeDirectory() + Path.DirectorySeparatorChar + SUPPORTED_CHARACTERS_FILE_NAME);
                fileStream.Close();
            }
        }

        private void WriteSupportedCharacterFile(List<Font> computedFontCollection)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(App.GetExeDirectory() + Path.DirectorySeparatorChar + SUPPORTED_CHARACTERS_FILE_NAME, false))
                {
                    foreach(Font supportedCharacter in computedFontCollection)
                    {
                        sw.WriteLine(supportedCharacter.Name + "\t" + supportedCharacter.SupportedCharacterList);
                    }
                }
            }
            catch (IOException e)
            {
                MessengerInstance.Send(e.Message, Enum.SettingToken.SettingFileWritingFailed);
            }
        }

        private async void GenerateFont(Font font)
        {
            MessengerInstance.Send<object>(null, Enum.CatiaToken.Refresh);

            if (_catiaEnv != null)
            {
                MessengerInstance.Send("Génération de la police : " + font.Name , Enum.ProcessToken.Started);
                await Task.Run(() =>
                {
                    try
                    {
                        FontGenerator fontGenerator = new FontGenerator();
                        fontGenerator.ProgressRateChanged += FontGenerator_ProgressRateChanged;
                        fontGenerator.Generate(_catiaEnv, font, GeneratedFontDirectoryPath + Path.DirectorySeparatorChar); // !!!!
                        MessengerInstance.Send("Génération du fichier font terminée !", Enum.ProcessToken.Finished);
                    }
                    catch (Exception ex)
                    {
                        _catiaEnv.Application.Visible = true;
                        MessengerInstance.Send(ex.Message + "\r\n" + ex.Source + "\r\n" + ex.TargetSite, Enum.ProcessToken.Failed);
                    }
                });
            }
        }
    }
}
