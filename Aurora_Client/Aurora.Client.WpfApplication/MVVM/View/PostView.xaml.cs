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

namespace Aurora.Client.WpfApplication.MVVM.View
{
    /// <summary>
    /// Interaction logic for PostView.xaml
    /// </summary>
    public partial class PostView : UserControl
    {
        public PostView()
        {
            InitializeComponent();
        }

        private void textComment_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtComment.Focus();
            textComment.Visibility = Visibility.Collapsed;
        }

        private void txtComment_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtComment.Text== string.Empty)
            {
                textComment.Visibility = Visibility.Visible;
            }
        }
    }
}
