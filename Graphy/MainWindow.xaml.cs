using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Graphy
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.NavigationService.Navigated += NavigationService_Navigated;

            SimpleMarkingRadioButton.IsChecked = true;
        }

        private void NavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            if (MainFrame.NavigationService.Content.GetType() == typeof(View.SimpleMarkingPage))
            {
                SimpleMarkingRadioButton.IsChecked = true;
                return;
            }

            if (MainFrame.NavigationService.Content.GetType() == typeof(View.ComplexMarkingPage))
            {
                ComplexMarkingRadioButton.IsChecked = true;
                return;
            }

            if (MainFrame.NavigationService.Content.GetType() == typeof(View.IconPage))
            {
                IconRadioButton.IsChecked = true;
                return;
            }

            if (MainFrame.NavigationService.Content.GetType() == typeof(View.SettingPage))
            {
                SettingRadioButton.IsChecked = true;
                return;
            }
        }

        private void SimpleMarkingRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if(MainFrame != null)
                MainFrame.Navigate(new View.SimpleMarkingPage());
        }

        private void ComplexMarkingRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (MainFrame != null)
                MainFrame.Navigate(new View.ComplexMarkingPage());
        }


        private void IconRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (MainFrame != null)
                MainFrame.Navigate(new View.IconPage());
        }

        private void SettingRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (MainFrame != null)
                MainFrame.Navigate(new View.SettingPage());
        }
    }
}
