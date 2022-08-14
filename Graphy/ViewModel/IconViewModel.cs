using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphy.Model;
using System.Runtime.InteropServices;
using GalaSoft.MvvmLight.Command;
using Graphy.Enum;
using System.IO;
using System.Collections.Specialized;

namespace Graphy.ViewModel
{
    public class IconViewModel : ViewModelBase
    {
        // CONSTRUCTOR
        public IconViewModel()
        {
            IconCollection = new ObservableCollection<Icon>();

            AddIconCommand = new RelayCommand(() => AddIconCommandAction());
            DeleteIconCommand = new RelayCommand<Icon>((icon) => DeleteIconCommandAction(icon));
            CopyIconCommand = new RelayCommand<Icon>((icon) => CopyIconCommandAction(icon));
            DrawIconCommand = new RelayCommand<Icon>((icon) => DrawIconCommandAction(icon));

            ExportIconCollectionCommand = new RelayCommand(() => ExportIconCollectionCommandAction());
            ImportIconCollectionCommand = new RelayCommand(() => ImportIconCollectionCommandAction());

            MessengerInstance.Register<StringCollection>(this, SettingToken.IconCollectionChanged, (collection) => LoadIconCollection(collection));
            MessengerInstance.Register<ImportMode>(this, SettingToken.ImportModeChanged, (importMode) => _selectedImportMode = importMode );
        }

        // PUBLIC ATTRIBUTS
        private ObservableCollection<Icon> _iconCollection;
        private Icon _selectedIcon;

        // PRIVATE ATTRIBUTS
        private ImportMode _selectedImportMode;

        public ObservableCollection<Icon> IconCollection
        {
            get => _iconCollection;
            set
            {
                Set(() => IconCollection, ref _iconCollection, value);
            }
        }

        public Icon SelectedIcon
        {
            get => _selectedIcon;
            set
            {
                Set(() => SelectedIcon, ref _selectedIcon, value);
            }
        }



        private RelayCommand _addIconCommand;
        public RelayCommand AddIconCommand { get => _addIconCommand; set => _addIconCommand = value; }

        private void AddIconCommandAction()
        {
            IconCollection.Add(Icon.Default());
            SelectedIcon = IconCollection.Last();

            MessengerInstance.Send(IconCollection.ToList(), Enum.IconToken.IconCollectionChanged);
            IconCollection.Last().PropertyChanged += Icon_PropertyChanged;
        }


        private RelayCommand<Icon> _deleteIconCommand;
        public RelayCommand<Icon> DeleteIconCommand { get => _deleteIconCommand; set => _deleteIconCommand = value; }

        private void DeleteIconCommandAction(Icon icon)
        {
            int index = IconCollection.IndexOf(icon);
            IconCollection.Remove(icon);
            SelectedIcon = IconCollection.ElementAtOrDefault(index - 1);

            MessengerInstance.Send(IconCollection.ToList(), Enum.IconToken.IconCollectionChanged);
        }


        private RelayCommand<Icon> _copyIconCommand;
        public RelayCommand<Icon> CopyIconCommand { get => _copyIconCommand; set => _copyIconCommand = value; }

        private void CopyIconCommandAction(Icon icon)
        {
            IconCollection.Add(new Icon()
            {
                Name = icon.Name,
                PathData = icon.PathData
            });

            SelectedIcon = IconCollection.Last();

            MessengerInstance.Send(IconCollection.ToList(), Enum.IconToken.IconCollectionChanged);
            IconCollection.Last().PropertyChanged += Icon_PropertyChanged;
        }


        private RelayCommand<Icon> _drawIconCommand;
        public RelayCommand<Icon> DrawIconCommand { get => _drawIconCommand; set => _drawIconCommand = value; }


        private void DrawIconCommandAction(Icon icon)
        {
            MessengerInstance.Send<Icon>(icon, IconToken.SelectedIconChanged);
        }


        private void LoadIconCollection(StringCollection collection)
        {
            IconCollection.Clear();

            if (collection == null || collection.Count == 0)
            {
                IconCollection.Add(Icon.Default());
                MessengerInstance.Send(IconCollection.ToList(), Enum.IconToken.IconCollectionChanged);
            }
            else
            {
                foreach (string str in collection)
                {
                    string[] separatedString = str.Split('\t');

                    Icon tempIcon = new Icon()
                    {
                        Name = separatedString.First(),
                        PathData = separatedString.Last()
                    };

                    IconCollection.Add(tempIcon);
                }
            }

            foreach (Icon icon in IconCollection)
            {
                icon.PropertyChanged += Icon_PropertyChanged;
            }

            SelectedIcon = IconCollection.First();
            DrawIconCommandAction(SelectedIcon);
        }


        private void Icon_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            MessengerInstance.Send(IconCollection.ToList(), Enum.IconToken.IconCollectionChanged);
        }


        private RelayCommand _exportIconCollectionCommand;
        public RelayCommand ExportIconCollectionCommand { get => _exportIconCollectionCommand; set => _exportIconCollectionCommand = value; }

        private void ExportIconCollectionCommandAction()
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Xml file (*.xml)|*.xml",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if ((bool)saveFileDialog.ShowDialog())
            {
                try
                {
                    System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<Icon>));
                    using (Stream exportStream = File.OpenWrite(saveFileDialog.FileName))
                    {
                        serializer.Serialize(exportStream, IconCollection.ToList());
                    }

                    MessengerInstance.Send("Icon collection export completed !", Enum.ProcessToken.Finished);
                }
                catch (Exception)
                {

                }
            }
        }


        private RelayCommand _importIconCollectionCommand;
        public RelayCommand ImportIconCollectionCommand { get => _importIconCollectionCommand; set => _importIconCollectionCommand = value; }

        private void ImportIconCollectionCommandAction()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = false,
                Filter = "Xml file (*.xml)|*.xml",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if ((bool)openFileDialog.ShowDialog())
            {
                try
                {
                    System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<Icon>));
                    using (Stream importStream = File.OpenRead(openFileDialog.FileName))
                    {
                        List<Icon> tempIconList = (List<Icon>)serializer.Deserialize(importStream);

                        if (_selectedImportMode == ImportMode.ReplaceCollection)
                            IconCollection.Clear();

                        foreach (Icon icon in tempIconList)
                        {
                            IconCollection.Add(icon);
                            icon.PropertyChanged += Icon_PropertyChanged;
                        }

                        MessengerInstance.Send(IconCollection.ToList(), Enum.IconToken.IconCollectionChanged);

                        if (IconCollection.Count > 0)
                            SelectedIcon = IconCollection.First();
                    }

                    MessengerInstance.Send<string>("Icon collection import completed !", Enum.ProcessToken.Finished);
                }
                catch (Exception)
                {

                }
            }
        }

    }
}
