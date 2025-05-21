using Aurora.Client.WpfApplication.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Text;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using Aurora.Client.Communication.DataStruct;
using Aurora.Client.Communication.Codes;
using Aurora.Client.Communication.Infrustructure;
using Aurora.Client.Communication.Managers;

namespace Aurora.Client.WpfApplication.MVVM.ViewModel
{
    internal class EditProfileViewModel : ObservableObject
    {
        private UserData _user;
        private string _email;
        private string _bio;
        private string _birthday;
        private Visibility _defultImageVisibility;
        private Visibility _customImageVisibility;
        private string _imgaePath;
        private byte[] _startingImage;

        public string ChangeEmail { get { return _email; } set { _email = value; OnPropertyChanged(); } }
        public string ChangeBio { get { return _bio; } set { _bio = value; OnPropertyChanged(); } }
        public string ChangeBirthday { get { return _birthday; } set { _birthday = value; OnPropertyChanged(); } }

        public UserData User { get { return _user; } set { _user = value; OnPropertyChanged(); } }
        public Visibility DefultImageVisibility { get { return _defultImageVisibility; } set { _defultImageVisibility = value; OnPropertyChanged(); } }
        public Visibility CustomImageVisibility { get { return _customImageVisibility; } set { _customImageVisibility = value; OnPropertyChanged(); } }
        public string ImagePath { get { return _imgaePath; } set { _imgaePath = value; OnPropertyChanged(); } }
        public byte[] StartingImage { get { return _startingImage; } set { _startingImage = value; OnPropertyChanged(); } }

        public RelayCommand ChooseImageCommand { get; set; }
        public RelayCommand SubmitChangesCommand { get; set; }

        public EditProfileViewModel(UserData user)
        {
            User = user;
            StartingImage = (User.ProfilePicture == null || User.ProfilePicture.Length == 0)
                ? File.ReadAllBytes("Images/default_user_image.png")
                : Convert.FromBase64String(User.ProfilePicture);

            DefultImageVisibility = Visibility.Visible;
            CustomImageVisibility = Visibility.Collapsed;
            ChooseImageCommand = new RelayCommand(o =>
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                bool? success = fileDialog.ShowDialog();
                if (success == true)
                {
                    ImagePath = fileDialog.FileName;
                    CustomImageVisibility = Visibility.Visible;
                    DefultImageVisibility = Visibility.Collapsed;
                }
            });

            SubmitChangesCommand = new RelayCommand(async o =>
            {
                try
                {
                    if (await SocialMediaManager.Instance.UpdateUser(ChangeBio, User.Username, ChangeBirthday, ChangeEmail, User.Followers, User.Following, User.JoinDate, ImagePath))
                    {
                        User.Bio = ChangeBio;
                        User.Birthday = ChangeBirthday;
                        User.Email = ChangeEmail;
                        User.ProfilePicture = Convert.ToBase64String(await File.ReadAllBytesAsync(ImagePath));
                        MainViewModel.Instance.CurrentView = HomeViewModel.Instance;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            });
        }
    }
}
