using System;
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
    /// Logique d'interaction pour StateView.xaml
    /// </summary>
    public partial class StateView : UserControl
    {
        public StateView()
        {
            // To remove
            //System.Windows.MessageBox.Show("Initialisation StatusView.");

            InitializeComponent();

            // To remove
            //System.Windows.MessageBox.Show("Fin initialisation StatusView.");
        }

        public event EventHandler QuitButtonClicked;
        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            QuitButtonClicked?.Invoke(sender, new EventArgs());
        }
    }
}
