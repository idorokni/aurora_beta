using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
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

    public partial class SignupView : UserControl
    {
        public SignupView()
        {
            InitializeComponent();
        }

        private void textUsername_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtUsername.Focus();
            textUsername.Visibility = Visibility.Collapsed;
        }

        private void txtUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtUsername.Text == string.Empty)
            {
                textUsername.Visibility = Visibility.Visible;
            }
        }

        private void textEmail_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtEmail.Focus();
            textEmail.Visibility = Visibility.Collapsed;
        }

        private void txtEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            if(txtEmail.Text == string.Empty)
            {
                textEmail.Visibility = Visibility.Visible;
            }
        }

        private void textPassword_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtPassword.FocusPasswordBox();
            textPassword.Visibility = Visibility.Collapsed;
        }

        private void txtPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if(txtPassword.passwordBox.Password == string.Empty)
            {
                textPassword.Visibility = Visibility.Visible;
            }
        }

        private void textConfirmPassword_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtConfirmPassword.FocusPasswordBox();
            textConfirmPassword.Visibility = Visibility.Collapsed;
        }

        private void txtConfirmPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if(txtConfirmPassword.passwordBox.Password == string.Empty)
            {
                textConfirmPassword.Visibility = Visibility.Visible;
            }
        }
    }
}
