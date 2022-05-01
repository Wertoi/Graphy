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
    /// Logique d'interaction pour ComplexMarkingPage.xaml
    /// </summary>
    public partial class ComplexMarkingPage : Page
    {
        public ComplexMarkingPage()
        {
            InitializeComponent();
        }

        private void MarkablePartViewButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new MarkablePartPage());
        }



        public int MouseOverItemIndex
        {
            get { return (int)GetValue(MouseOverItemIndexProperty); }
            set { SetValue(MouseOverItemIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseOverItemIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseOverItemIndexProperty =
            DependencyProperty.Register("MouseOverItemIndex", typeof(int), typeof(ComplexMarkingPage), new PropertyMetadata(0));


        private void MarkablePartListView_MouseMove(object sender, MouseEventArgs e)
        {
            var item = VisualTreeHelper.HitTest(MarkablePartListView, Mouse.GetPosition(MarkablePartListView)).VisualHit;

            // find ListViewItem (or null)
            while (item != null && !(item is ListViewItem))
                item = VisualTreeHelper.GetParent(item);

            if (item != null)
            {
                MouseOverItemIndex = MarkablePartListView.Items.IndexOf(((ListViewItem)item).DataContext);
            }
        }
    }
}
