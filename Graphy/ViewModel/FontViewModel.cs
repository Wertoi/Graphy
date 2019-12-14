using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Graphy.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Media;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graphy.Model.Generator;

namespace Graphy.ViewModel
{
    public class FontViewModel : ViewModelBase
    {
        // CONSTRUCTOR
        public FontViewModel()
        {
            // INITILIAZE COLLECTIONS
            GeneratedFontCollection = new ObservableCollection<GeneratedFont>();
            ClassicFontCollection = new ObservableCollection<Font>();

            // INITIALIZE PRIVATE ATTRIBUT
            _computedFontCollection = new List<Font>();

            // MESSENGER REGISTRATION
            MessengerInstance.Register<string>(this, Enum.SettingToken.GeneratedFontDirectoryPathChanged, (path) => GeneratedFontDirectoryChanged(path));
            MessengerInstance.Register<List<Font>>(this, Enum.SettingToken.ComputedFontCollectionChanged, (computedFontCollection) => ComputedFontCollectionChanged(computedFontCollection));

            // COMMANDS INITIALIZATION
            ComputeCharacterListCommand = new RelayCommand<Font>((selectedFont) => ComputeCharacterListCommandAction(selectedFont));
            GenerateFontCommand = new RelayCommand<Font>((selectedFont) => GenerateFontCommandAction(selectedFont));

            // LOAD COLLECTIONS
            LoadClassicLoadCollection();
        }

        // OBSERVABLE ATTRIBUTS
        private ObservableCollection<GeneratedFont> _generatedFontCollection;
        private GeneratedFont _selectedGeneratedFont;
        private bool _isGeneratedFontCollectionEmpty = true;

        private ObservableCollection<Font> _classicFontCollection;
        private Font _selectedclassicFont;
        private bool _isClassicFontCollectionEmpty = true;

        // PRIVATE ATTRIBUTS
        private string _generatedFontDirectoryPath;
        private List<Font> _computedFontCollection;


        public ObservableCollection<GeneratedFont> GeneratedFontCollection
        {
            get => _generatedFontCollection;
            set
            {
                Set(() => GeneratedFontCollection, ref _generatedFontCollection, value);
            }
        }

        public GeneratedFont SelectedGeneratedFont
        {
            get => _selectedGeneratedFont;
            set
            {
                Set(() => SelectedGeneratedFont, ref _selectedGeneratedFont, value);
                MessengerInstance.Send(SelectedGeneratedFont, Enum.InputDataToken.SelectedFontChanged);
            }
        }

        public bool IsGeneratedFontCollectionEmpty
        {
            get => _isGeneratedFontCollectionEmpty;
            set
            {
                Set(() => IsGeneratedFontCollectionEmpty, ref _isGeneratedFontCollectionEmpty, value);
            }
        }

        public ObservableCollection<Font> ClassicFontCollection
        {
            get => _classicFontCollection;
            set
            {
                Set(() => ClassicFontCollection, ref _classicFontCollection, value);
            }
        }


        public Font SelectedClassicFont
        {
            get => _selectedclassicFont;
            set
            {
                Set(() => SelectedClassicFont, ref _selectedclassicFont, value);
            }
        }

        public bool IsClassicFontCollectionEmpty
        {
            get => _isClassicFontCollectionEmpty;
            set
            {
                Set(() => IsClassicFontCollectionEmpty, ref _isClassicFontCollectionEmpty, value);
            }
        }


        // COMMANDS
        private RelayCommand<Font> _computeCharacterListCommand;
        public RelayCommand<Font> ComputeCharacterListCommand { get => _computeCharacterListCommand; set => _computeCharacterListCommand = value; }

        private async void ComputeCharacterListCommandAction(Font selectedFont)
        {
            SupportedCharGenerator supportedCharGenerator = new SupportedCharGenerator();
            supportedCharGenerator.ProgressRateChanged += SupportedCharGenerator_ProgressRateChanged;

            MessengerInstance.Send("Récupération de la liste des caractères supportés.", Enum.ProcessToken.Started);

            await Task.Run(() =>
            {
                _computedFontCollection.Add(new Font(selectedFont.FontFamily, supportedCharGenerator.GenerateSupportedCharacters(selectedFont)));

                AddSupportedCharacterToCollection(true, true);

                MessengerInstance.Send(_computedFontCollection, Enum.FontToken.SupportedCharacterComputed);

                MessengerInstance.Send("Liste des caractères supportés récupérée.", Enum.ProcessToken.Finished);
            });
        }

        private void SupportedCharGenerator_ProgressRateChanged(object sender, ProgressRateChangedEventArgs e)
        {
            MessengerInstance.Send(e.ProgressRate * 100, Enum.ProcessToken.Refresh);
        }


        // COMMANDS

        private RelayCommand<Font> _generateFontCommand;
        public RelayCommand<Font> GenerateFontCommand { get => _generateFontCommand; set => _generateFontCommand = value; }

        private void GenerateFontCommandAction(Font selectedFont)
        {
            MessengerInstance.Send(selectedFont, Enum.FontToken.GenerateNewFont);
        }



        // PRIVATE METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        private void GeneratedFontDirectoryChanged(string path)
        {
            _generatedFontDirectoryPath = path;

            // RESET FONT COLLECTION
            GeneratedFontCollection.Clear();

            // LOAD CUSTOM FONTS AND MERGE THEM WITH CATIA FONT
            if (System.IO.Directory.Exists(_generatedFontDirectoryPath))
            {
                string[] fileList = Directory.GetFiles(_generatedFontDirectoryPath, "*.CATPart");
                foreach (string file in fileList)
                {
                    GeneratedFont tempFont = new GeneratedFont()
                    {
                        FontFamily = GeneratedFont.GetFontFamilyFromName(file),
                        GeneratedFileFullPath = file,
                    };

                    if (tempFont.IsGeneratedFileOK)
                    {
                        GeneratedFontCollection.Add(tempFont);
                    }
                }
            }

            // SELECT THE FIRST ELEMENT OF FONT COLLECTION IF NOT EMPTY
            if (GeneratedFontCollection.Count > 0)
            {
                IsGeneratedFontCollectionEmpty = false;
                SelectedGeneratedFont = GeneratedFontCollection.First();
            }
            else
                IsGeneratedFontCollectionEmpty = true;

            // ADD SUPPORTED CHARACTERS
            AddSupportedCharacterToCollection(false, true);
        }


        private void LoadClassicLoadCollection()
        {
            System.Drawing.Text.InstalledFontCollection installedFontCollection = new System.Drawing.Text.InstalledFontCollection();

            foreach (System.Drawing.FontFamily fontFamily in installedFontCollection.Families)
            {
                Font tempFont = new Font() { FontFamily = new FontFamily(fontFamily.Name) };

                if (!ClassicFontCollection.Any(font => font.Name == tempFont.Name))
                    ClassicFontCollection.Add(tempFont);
            }

            // SELECT THE FIRST ELEMENT OF CLASSIC FONT COLLECTION IF NOT EMPTY
            if (ClassicFontCollection.Count > 0)
            {
                IsClassicFontCollectionEmpty = false;
                SelectedClassicFont = ClassicFontCollection.First();
            }
            else
                IsClassicFontCollectionEmpty = true;

            // ADD SUPPORTED CHARACTERS
            AddSupportedCharacterToCollection(true, false);
        }


        private void ComputedFontCollectionChanged(List<Font> computedFontCollection)
        {
            _computedFontCollection = computedFontCollection;

            AddSupportedCharacterToCollection(true, true);
        }


        private void AddSupportedCharacterToCollection(bool ToClassicFontCollection, bool ToGeneratedFontCollection)
        {
            if(ToClassicFontCollection)
            {
                foreach (Font font in ClassicFontCollection)
                {
                    // Reset the supported character list in order to avoid datas which does not exist anymore.
                    font.SupportedCharacterList = "";

                    foreach (Font computedFont in _computedFontCollection)
                    {
                        if (computedFont.Name == font.Name)
                        {
                            font.SupportedCharacterList = computedFont.SupportedCharacterList;
                            break;
                        }
                    }
                }
            }

            if(ToGeneratedFontCollection)
            {
                foreach (GeneratedFont generatedFont in GeneratedFontCollection)
                {
                    // Reset the supported character list in order to avoid datas which does not exist anymore.
                    generatedFont.SupportedCharacterList = "";

                    foreach (Font computedFont in _computedFontCollection)
                    {
                        if (computedFont.Name == generatedFont.Name)
                        {
                            generatedFont.SupportedCharacterList = computedFont.SupportedCharacterList;
                            break;
                        }
                    }
                }
            }
        }
    }
}
