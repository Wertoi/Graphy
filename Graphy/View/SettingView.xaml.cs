﻿using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Graphy.View
{
    /// <summary>
    /// Logique d'interaction pour SettingView.xaml
    /// </summary>
    public partial class SettingView : UserControl
    {
        public SettingView()
        {
            // To remove
            //System.Windows.MessageBox.Show("Initialisation SettingView.");

            InitializeComponent();
            
            // To remove
            //System.Windows.MessageBox.Show("Fin initialisation SettingView.");
        }

        public event EventHandler BackButtonClicked;
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            BackButtonClicked?.Invoke(sender, new EventArgs());
        }

        public event EventHandler ShowAddNewFont;
        private void AddNewFontButton_Click(object sender, RoutedEventArgs e)
        {
            ShowAddNewFont?.Invoke(sender, new EventArgs());
        }
    }
}