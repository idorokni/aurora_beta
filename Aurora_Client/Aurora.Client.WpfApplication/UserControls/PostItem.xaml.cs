using Aurora.Client.Communication;
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
    /// Interaction logic for PostItem.xaml
    /// </summary>
    public partial class PostItem : UserControl
    {
        public PostItem()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty CommandProperty =
    DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(PostItem), new PropertyMetadata(null));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }


        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Command?.CanExecute(DataContext) == true)
                Command.Execute(DataContext);
        }


    }
}
