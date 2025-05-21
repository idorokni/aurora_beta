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

namespace Aurora.Client.WpfApplication.UserControls
{
    /// <summary>
    /// Interaction logic for UserChatItem.xaml
    /// </summary>
    public partial class UserChatItem : UserControl
    {
        public UserChatItem()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty SelectUserCommandProperty =
    DependencyProperty.Register(nameof(SelectUserCommand), typeof(ICommand), typeof(UserChatItem), new PropertyMetadata(null));

        public ICommand SelectUserCommand
        {
            get => (ICommand)GetValue(SelectUserCommandProperty);
            set => SetValue(SelectUserCommandProperty, value);
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectUserCommand?.CanExecute(DataContext) == true)
                SelectUserCommand.Execute(DataContext);
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectUserCommand?.CanExecute(DataContext) == true)
                SelectUserCommand.Execute(DataContext);
        }
    }
}
