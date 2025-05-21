using Aurora.Client.Communication;
using SharedData.Model;
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
    /// Interaction logic for UserSearchResultItem.xaml
    /// </summary>
    public partial class UserSearchResultItem : UserControl
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(UserSearchResultItem), new PropertyMetadata(null));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public UserSearchResultItem()
        {
            InitializeComponent();
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Command != null && Command.CanExecute(null))
            {
                Command.Execute(DataContext as SearchModel);
            }
        }
    }
}
