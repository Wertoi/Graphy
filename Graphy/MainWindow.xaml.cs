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
        }

        private void SelectionButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }



        // ***** SETTING VIEW MANAGEMENT *****

        private void SettingView_BackButtonClicked(object sender, EventArgs e)
        {
            SettingView.Visibility = Visibility.Collapsed;
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            SettingView.Visibility = Visibility.Visible;
        }

        // ***** END OF SETTING VIEW MANAGEMENT *****




        // ***** DESIGN TABLE VIEW MANAGEMENT *****

        private void DesignTableView_BackButtonClicked(object sender, EventArgs e)
        {
            DesignTableView.Visibility = Visibility.Collapsed;
        }

        private void DesignTableButton_Click(object sender, RoutedEventArgs e)
        {
            DesignTableView.Visibility = Visibility.Visible;
        }

        // ***** END OF DESIGN TABLE VIEW MANAGEMENT *****




        // ***** FONT VIEW MANAGEMENT *****

        private void FontView_BackButtonClicked(object sender, EventArgs e)
        {
            FontView.Visibility = Visibility.Collapsed;
        }

        private void FontButton_Click(object sender, RoutedEventArgs e)
        {
            FontView.Visibility = Visibility.Visible;
        }

        // ***** END OF FONT VIEW MANAGEMENT *****



        private void IconButton_Click(object sender, RoutedEventArgs e)
        {
            IconView.Visibility = Visibility.Visible;
        }

        private void IconView_BackButtonClicked(object sender, EventArgs e)
        {
            IconView.Visibility = Visibility.Collapsed;
        }
    }
}
