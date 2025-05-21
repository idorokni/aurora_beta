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
    /// Interaction logic for UserChatsView.xaml
    /// </summary>
    public partial class UserChatsView : UserControl
    {
        public UserChatsView()
        {
            InitializeComponent();
        }

        private void txtMessage_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtMessage.Text == string.Empty)
            {
                textMessage.Visibility = Visibility.Visible;
            }
        }

        private void textMessage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtMessage.Focus();
            textMessage.Visibility = Visibility.Collapsed;
        }
    }
}
