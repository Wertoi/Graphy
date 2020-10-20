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

namespace Graphy.ViewModel
{
    public class IconViewModel : ViewModelBase
    {
        public IconViewModel()
        {
            IconCollection = new ObservableCollection<Icon>();

            IconCollection.Add(Icon.Default());
            SelectedIcon = IconCollection.First();
            DrawIconCommandAction(SelectedIcon);

            AddIconCommand = new RelayCommand(() => AddIconCommandAction());
            DeleteIconCommand = new RelayCommand<Icon>((icon) => DeleteIconCommandAction(icon));
            CopyIconCommand = new RelayCommand<Icon>((icon) => CopyIconCommandAction(icon));
            DrawIconCommand = new RelayCommand<Icon>((icon) => DrawIconCommandAction(icon));
        }
        
        private ObservableCollection<Icon> _iconCollection;
        private Icon _selectedIcon;

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
        }


        private RelayCommand<Icon> _deleteIconCommand;
        public RelayCommand<Icon> DeleteIconCommand { get => _deleteIconCommand; set => _deleteIconCommand = value; }

        private void DeleteIconCommandAction(Icon icon)
        {
            int index = IconCollection.IndexOf(icon);
            IconCollection.Remove(icon);
            SelectedIcon = IconCollection.ElementAtOrDefault(index - 1);
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
        }


        private RelayCommand<Icon> _drawIconCommand;
        public RelayCommand<Icon> DrawIconCommand { get => _drawIconCommand; set => _drawIconCommand = value; }

        private void DrawIconCommandAction(Icon icon)
        {
            MessengerInstance.Send<Icon>(icon, InputDataToken.SelectedIconChanged);
        }
    }
}
