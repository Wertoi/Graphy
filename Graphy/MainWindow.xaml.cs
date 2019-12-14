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

        private void StateView_QuitButtonClicked(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SelectionButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void SettingView_BackButtonClicked(object sender, EventArgs e)
        {
            SettingView.Visibility = Visibility.Collapsed;
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            SettingView.Visibility = Visibility.Visible;
        }

        private void DesignTableView_BackButtonClicked(object sender, EventArgs e)
        {
            DesignTableView.Visibility = Visibility.Collapsed;
        }

        private void DesignTableButton_Click(object sender, RoutedEventArgs e)
        {
            DesignTableView.Visibility = Visibility.Visible;
        }

        private void SettingView_ShowAddNewFont(object sender, EventArgs e)
        {
            NewFontView.Visibility = Visibility.Visible;
        }

        private void NewFontView_BackButtonClicked(object sender, EventArgs e)
        {
            NewFontView.Visibility = Visibility.Collapsed;
        }
    }
}
