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
    /// Logique d'interaction pour DesignTableView.xaml
    /// </summary>
    public partial class DesignTableView : UserControl
    {
        public DesignTableView()
        {
            // To remove
            //System.Windows.MessageBox.Show("Initialisation DesignTableView.");

            InitializeComponent();

            // To remove
            //System.Windows.MessageBox.Show("Fin initialisation DesignTableView.");
        }

        public event EventHandler BackButtonClicked;
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            BackButtonClicked?.Invoke(sender, new EventArgs());
        }


        /*private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            CatiaWriter.Model.DesignTableParameter parameter = e.Item as CatiaWriter.Model.DesignTableParameter;

            if (parameter.ParameterType == Model.ParameterType.NoType || parameter.ParameterType == Model.ParameterType.Number)
                e.Accepted = true;
            else
                e.Accepted = false;
        }*/
    }
}
