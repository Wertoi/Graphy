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
            _fontCollection = new Collection<SelectableFont>();

            foreach (System.Drawing.FontFamily fontFamily in installedFontCollection.Families)
            {
                if(fontFamily.Name == DEFAULT_FONT)
                    _fontCollection.Add(new SelectableFont(new System.Windows.Media.FontFamily(fontFamily.Name), true));
                else
                    _fontCollection.Add(new SelectableFont(new System.Windows.Media.FontFamily(fontFamily.Name), false));

                _fontCollection.Last().PropertyChanged += FontCollection_PropertyChanged;
            }

            // FONT COLLECTION INITIALIZATION
            FontCollectionViewSource = new CollectionViewSource();
            FontCollectionViewSource.Filter += FontCollection_Filter;
            FontCollectionViewSource.Source = _fontCollection;
            SelectedFont = _fontCollection.First();

            // MESSENGER REGISTRATION
            MessengerInstance.Register<StringCollection>(this, Enum.SettingToken.FavoriteFontCollectionChanged, (selectedFontList) => ReadUserPreferences(selectedFontList));


            // COMMANDS INITIALIZATION
            ComputeCharacterListCommand = new RelayCommand<SelectableFont>((selectedFont) => ComputeCharacterListCommandAction(selectedFont));
            SortByNameCommand = new RelayCommand<bool>((isAscending) => SortByNameCommandAction(isAscending));
            SortByFavoriteCommand = new RelayCommand(() => SortByFavoriteCommandAction());
            SendSelectedFontCommand = new RelayCommand<SelectableFont>((selectedFont) => SendSelectedFontCommandAction(selectedFont));
            ClearSearchCommand = new RelayCommand(() => { SearchText = "";  });
        }

        // CONSTANTS
        private const string DEFAULT_FONT = "Monospac821 BT";

        // PRIVATE ATTRIBUTS
        private Collection<SelectableFont> _fontCollection;

        // OBSERVABLE ATTRIBUTS
        private CollectionViewSource _fontCollectionViewSource;
        private SelectableFont _selectedFont;
        private string _searchText;

        public CollectionViewSource FontCollectionViewSource
        {
            get => _fontCollectionViewSource;
            set
            {
                Set(() => FontCollectionViewSource, ref _fontCollectionViewSource, value);
            }
        }


        public SelectableFont SelectedFont
        {
            get => _selectedFont;
            set
            {
                Set(() => SelectedFont, ref _selectedFont, value);
            }
        }


        public string SearchText
        {
            get => _searchText;
            set
            {
                Set(() => SearchText, ref _searchText, value);
                FontCollectionViewSource.View.Refresh();

                if(!FontCollectionViewSource.View.IsEmpty)
                    FontCollectionViewSource.View.MoveCurrentToFirst();
            }
        }



        // COMMANDS
        private RelayCommand<SelectableFont> _computeCharacterListCommand;
        public RelayCommand<SelectableFont> ComputeCharacterListCommand { get => _computeCharacterListCommand; set => _computeCharacterListCommand = value; }

        private async void ComputeCharacterListCommandAction(SelectableFont selectedFont)
        {
            MessengerInstance.Send<object>(null, Enum.ProcessToken.SimpleStarted);

            await Task.Run(() =>
            {
                selectedFont.ComputeSupportedCharacterList();

                MessengerInstance.Send<object>(null, Enum.ProcessToken.Finished);

            });
        }


        private RelayCommand<SelectableFont> _sendSelectedFontCommand;
        public RelayCommand<SelectableFont> SendSelectedFontCommand { get => _sendSelectedFontCommand; set => _sendSelectedFontCommand = value; }

        private void SendSelectedFontCommandAction(SelectableFont selectedFont)
        {
            if(selectedFont != null)
            {
                if (!selectedFont.IsCalculated)
                    ComputeCharacterListCommandAction(selectedFont);

                SelectedFont = selectedFont;
                SearchText = selectedFont.FontFamily.Source;
            }
        }



        private RelayCommand<bool> _sortByNameCommand;
        public RelayCommand<bool> SortByNameCommand { get => _sortByNameCommand; set => _sortByNameCommand = value; }

        private void SortByNameCommandAction(bool isAscending)
        {
            FontCollectionViewSource.SortDescriptions.Clear();
            if(isAscending)
                FontCollectionViewSource.View.SortDescriptions.Add(new SortDescription("FontFamily.Source", ListSortDirection.Ascending));
            else
                FontCollectionViewSource.View.SortDescriptions.Add(new SortDescription("FontFamily.Source", ListSortDirection.Descending));
        }



        private RelayCommand _sortByFavoriteCommand;
        public RelayCommand SortByFavoriteCommand { get => _sortByFavoriteCommand; set => _sortByFavoriteCommand = value; }

        private void SortByFavoriteCommandAction()
        {
            FontCollectionViewSource.SortDescriptions.Clear();
            FontCollectionViewSource.View.SortDescriptions.Add(new SortDescription("IsSelected", ListSortDirection.Descending));
            FontCollectionViewSource.View.SortDescriptions.Add(new SortDescription("FontFamily.Source", ListSortDirection.Ascending));
        }


        private RelayCommand _clearSearchCommand;
        public RelayCommand ClearSearchCommand { get => _clearSearchCommand; set => _clearSearchCommand = value; }

        // EVENTS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FontCollection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                List<SelectableFont> tempFontCollection = new List<SelectableFont>();
                foreach(SelectableFont font in _fontCollection)
                {
                    if (font.IsSelected)
                        tempFontCollection.Add(font);
                }

                MessengerInstance.Send<List<SelectableFont>>(tempFontCollection, Enum.FontToken.FavoriteFontCollectionChanged);
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
            foreach (string fontName in favoriteFontNameCollection)
            {
                foreach (SelectableFont font in _fontCollection)
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
