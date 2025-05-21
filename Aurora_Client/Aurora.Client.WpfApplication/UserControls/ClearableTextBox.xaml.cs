using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for ClearableTextBox.xaml
    /// </summary>
    public partial class ClearableTextBox : UserControl, INotifyPropertyChanged
    {
        public ClearableTextBox()
        {
            InitializeComponent();
        }

        private string _placeHolder;
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(ClearableTextBox), new PropertyMetadata(string.Empty));

        public event PropertyChangedEventHandler?  PropertyChanged;

        public string PlaceHolder { get { return _placeHolder; } set { _placeHolder = value; OnPropertyChanged(nameof(PlaceHolder)); } }
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); OnPropertyChanged(nameof(Text)); }
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void txtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtInput.Text))
            {
                tbPlaceHolder.Visibility = Visibility.Visible;
            }
            else
            {
                tbPlaceHolder.Visibility = Visibility.Hidden;
            }
        }
    }
}
