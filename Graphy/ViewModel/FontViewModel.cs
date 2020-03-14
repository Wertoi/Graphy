using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Media;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graphy.Model.Generator;
using System.Drawing;
using System.Drawing.Text;
using Graphy.Model;
using System.Windows.Data;
using System.Collections.Specialized;

namespace Graphy.ViewModel
{
    public class FontViewModel : ViewModelBase
    {
        // CONSTRUCTOR
        public FontViewModel()
        {
            // RETRIEVES THE WINDOWS FONT COLLECTION
            InstalledFontCollection installedFontCollection = new InstalledFontCollection();
            Collection<SelectableFont> tempFontCollection = new Collection<SelectableFont>();

            foreach (System.Drawing.FontFamily fontFamily in installedFontCollection.Families)
            {
                if(fontFamily.Name == DEFAULT_FONT)
                    tempFontCollection.Add(new SelectableFont(new System.Windows.Media.FontFamily(fontFamily.Name), true));
                else
                    tempFontCollection.Add(new SelectableFont(new System.Windows.Media.FontFamily(fontFamily.Name), false));

                tempFontCollection.Last().PropertyChanged += FontViewModel_PropertyChanged;
            }

            // FONT COLLECTION INITIALIZATION
            FontCollection = new CollectionViewSource();
            FontCollection.Filter += FontCollection_Filter;
            FontCollection.Source = tempFontCollection;

            // SETTINGS FONT COLLECTION INITIALIZATION
            SettingsFontCollection = new CollectionViewSource();
            SettingsFontCollection.Filter += SettingsFontCollection_Filter;
            SettingsFontCollection.Source = tempFontCollection;

            SettingsSelectedFont = tempFontCollection.First();

            // END OF FONT COLLECTION INITIALIZATION

            // MESSENGER REGISTRATION
            MessengerInstance.Register<StringCollection>(this, Enum.SettingToken.UserPreferencesChanged, (selectedFontList) => ReadUserPreferences(selectedFontList));


            // COMMANDS INITIALIZATION
            ComputeCharacterListCommand = new RelayCommand<SelectableFont>((selectedFont) => ComputeCharacterListCommandAction(selectedFont));
            SortByNameCommand = new RelayCommand<bool>((isAscending) => SortByNameCommandAction(isAscending));
            SortByFavoriteCommand = new RelayCommand(() => SortByFavoriteCommandAction());
            SendSelectedFontCommand = new RelayCommand<SelectableFont>((selectedFont) => SendSelectedFontCommandAction(selectedFont));
            ClearSearchCommand = new RelayCommand(() => { SearchText = "";  });
        }

        // CONSTANTS
        private const string DEFAULT_FONT = "Monospac821 BT";

        // ATTRIBUTS
        private CollectionViewSource _fontCollection;
        private CollectionViewSource _settingsFontCollection;
        private SelectableFont _selectedFont;
        private SelectableFont _settingsSelectedFont;
        private string _searchText;

        public CollectionViewSource FontCollection
        {
            get => _fontCollection;
            set
            {
                Set(() => FontCollection, ref _fontCollection, value);
            }
        }


        public CollectionViewSource SettingsFontCollection
        {
            get => _settingsFontCollection;
            set
            {
                Set(() => SettingsFontCollection, ref _settingsFontCollection, value);
            }
        }


        public SelectableFont SelectedFont
        {
            get => _selectedFont;
            set
            {
                Set(() => SelectedFont, ref _selectedFont, value);

                MessengerInstance.Send<SelectableFont>(SelectedFont, Enum.InputDataToken.SelectedFontChanged);
            }
        }

        public SelectableFont SettingsSelectedFont
        {
            get => _settingsSelectedFont;
            set
            {
                Set(() => SettingsSelectedFont, ref _settingsSelectedFont, value);
            }
        }


        public string SearchText
        {
            get => _searchText;
            set
            {
                Set(() => SearchText, ref _searchText, value);
                SettingsFontCollection.View.Refresh();

                if(!SettingsFontCollection.View.IsEmpty)
                    SettingsFontCollection.View.MoveCurrentToFirst();
            }
        }



        // COMMANDS
        private RelayCommand<SelectableFont> _computeCharacterListCommand;
        public RelayCommand<SelectableFont> ComputeCharacterListCommand { get => _computeCharacterListCommand; set => _computeCharacterListCommand = value; }

        private async void ComputeCharacterListCommandAction(SelectableFont selectedFont)
        {
            MessengerInstance.Send("Génération du marquage.", Enum.ProcessToken.Started);

            await Task.Run(() =>
            {

                SupportedCharGenerator supportedCharGenerator = new SupportedCharGenerator();
                supportedCharGenerator.ProgressRateChanged += SupportedCharGenerator_ProgressRateChanged; ;

                try
                {
                    selectedFont.SupportedCharacterList = supportedCharGenerator.ComputeSupportedCharacterList(selectedFont.FontFamily);


                    MessengerInstance.Send<bool>(true, Enum.ProcessToken.Finished);
                }
                catch (Exception ex)
                {
                    MessengerInstance.Send(ex.Message, Enum.ProcessToken.Failed);
                }

            });
        }


        private RelayCommand<SelectableFont> _sendSelectedFontCommand;
        public RelayCommand<SelectableFont> SendSelectedFontCommand { get => _sendSelectedFontCommand; set => _sendSelectedFontCommand = value; }

        private void SendSelectedFontCommandAction(SelectableFont selectedFont)
        {
            if(!selectedFont.IsCalculated)
                ComputeCharacterListCommandAction(selectedFont);

            SearchText = selectedFont.FontFamily.Source;
        }

        private void SupportedCharGenerator_ProgressRateChanged(object sender, ProgressRateChangedEventArgs e)
        {
            MessengerInstance.Send(e.ProgressRate * 100, Enum.ProcessToken.Refresh);
        }



        private RelayCommand<bool> _sortByNameCommand;
        public RelayCommand<bool> SortByNameCommand { get => _sortByNameCommand; set => _sortByNameCommand = value; }

        private void SortByNameCommandAction(bool isAscending)
        {
            SettingsFontCollection.SortDescriptions.Clear();
            if(isAscending)
                SettingsFontCollection.View.SortDescriptions.Add(new SortDescription("FontFamily.Source", ListSortDirection.Ascending));
            else
                SettingsFontCollection.View.SortDescriptions.Add(new SortDescription("FontFamily.Source", ListSortDirection.Descending));
        }


        private RelayCommand _sortByFavoriteCommand;
        public RelayCommand SortByFavoriteCommand { get => _sortByFavoriteCommand; set => _sortByFavoriteCommand = value; }

        private void SortByFavoriteCommandAction()
        {
            SettingsFontCollection.SortDescriptions.Clear();
            SettingsFontCollection.View.SortDescriptions.Add(new SortDescription("IsSelected", ListSortDirection.Descending));
        }

        private RelayCommand _clearSearchCommand;
        public RelayCommand ClearSearchCommand { get => _clearSearchCommand; set => _clearSearchCommand = value; }

        // EVENTS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FontViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                SelectableFont oldSelectedFontFamily = (SelectableFont)FontCollection.View.CurrentItem;
                FontCollection.View.Refresh();

                if (!FontCollection.View.Contains(oldSelectedFontFamily))
                    FontCollection.View.MoveCurrentToFirst();

                MessengerInstance.Send<List<SelectableFont>>(FontCollection.View.Cast<SelectableFont>().ToList(), Enum.FontToken.FavoriteFontListChanged);
            }
        }

        /// <summary>
        /// Filter the Font Collection depending on property IsSelected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FontCollection_Filter(object sender, FilterEventArgs e)
        {
            SelectableFont font = (SelectableFont)e.Item;
            if (font == null)
                e.Accepted = false;
            else
                e.Accepted = font.IsSelected ? true : false;
        }

        /// <summary>
        /// Filter the Settings Font Collection function of the Search Text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsFontCollection_Filter(object sender, FilterEventArgs e)
        {
            SelectableFont font = (SelectableFont)e.Item;
            if (font == null)
                e.Accepted = false;
            else
            {
                if (SearchText != "" && SearchText != null)
                    e.Accepted = font.FontFamily.Source.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ? true : false;
                else
                    e.Accepted = true;
            }
        }


        // PRIVATE METHODS
        private void ReadUserPreferences(StringCollection favoriteFontNameCollection)
        {
            Collection<SelectableFont> tempFontCollection = (Collection<SelectableFont>)FontCollection.Source;
            foreach (string fontName in favoriteFontNameCollection)
            {
                foreach (SelectableFont font in tempFontCollection)
                {
                    if (font.FontFamily.Source == fontName)
                    {
                        font.IsSelected = true;
                        break;
                    }
                }
            }
        }

    }
}
