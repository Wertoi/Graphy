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
            IconCollection.Add(new Icon()
            {
                Name = "Link_ButtonIcon",
                PathData = "M22,22H2v-2h20V22z M10,2H7v16h3V2z M17,8h-3v10h3V8z"
            });

            IconCollection.Add(new Icon()
            {
                Name = "Refresh_ButtonIcon",
                PathData = "M22,2v2H2V2H22z M7,22h3V6H7V22z M14,16h3V6h-3V16z"
            });

            SelectedIcon = IconCollection.First();

            Add_ButtonIconCommand = new RelayCommand(() => IconCollection.Add(new Icon()));
            Delete_ButtonIconCommand = new RelayCommand<Icon>((icon) => IconCollection.Remove(icon));
            CopyIconCommand = new RelayCommand<Icon>((icon) => IconCollection.Add(new Icon()
            {
                Name = icon.Name,
                PathData = icon.PathData
            }));
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


        private RelayCommand _Add_ButtonIconCommand;
        public RelayCommand Add_ButtonIconCommand { get => _Add_ButtonIconCommand; set => _Add_ButtonIconCommand = value; }


        private RelayCommand<Icon> _Delete_ButtonIconCommand;
        public RelayCommand<Icon> Delete_ButtonIconCommand { get => _Delete_ButtonIconCommand; set => _Delete_ButtonIconCommand = value; }


        private RelayCommand<Icon> _copyIconCommand;
        public RelayCommand<Icon> CopyIconCommand { get => _copyIconCommand; set => _copyIconCommand = value; }


        private RelayCommand<Icon> _drawIconCommand;
        public RelayCommand<Icon> DrawIconCommand { get => _drawIconCommand; set => _drawIconCommand = value; }

        private void DrawIconCommandAction(Icon icon)
        {
            MessengerInstance.Send<Icon>(icon, InputDataToken.SelectedIconChanged);
        }
    }
}
