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
    /// Logique d'interaction pour IconView.xaml
    /// </summary>
    public partial class IconView : UserControl
    {
        public IconView()
        {
            InitializeComponent();
        }

        public event EventHandler BackButtonClicked;
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            BackButtonClicked?.Invoke(sender, new EventArgs());
        }
    }
}
