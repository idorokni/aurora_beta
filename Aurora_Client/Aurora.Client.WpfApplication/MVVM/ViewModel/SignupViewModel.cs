using Aurora.Client.WpfApplication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using System.Windows.Data;
using Aurora.Client.Communication.Managers;
using Aurora.Client.Communication.Codes;
using Aurora.Client.Communication.DataStruct;
using Aurora.Client.Communication.Infrustructure;
using SharedData.Model;

namespace Aurora.Client.WpfApplication.MVVM.ViewModel
{
    internal class SignupViewModel : ObservableObject
    {
        private string _email = "";
        private string _username = "";
        private string _errorString = "";
        private string _password = "";
        private string _confirmPassword = "";
        private Visibility _errorVisibility = Visibility.Collapsed;  

        public string Email { get { return _email; } set { _email = value; OnPropertyChanged(); } }
        public string Username { get { return _username; } set { _username = value; OnPropertyChanged(); } }
        public string ErrorString { get { return _errorString; } set { _errorString = value; OnPropertyChanged(); } }
        public string Password { get { return _password; } set { _password = value; OnPropertyChanged(); } }
        public string ConfirmPassword { get { return _confirmPassword; } set { _confirmPassword = value; OnPropertyChanged(); } }
        public Visibility ErrorVisibility { get { return _errorVisibility; } set {  _errorVisibility = value; OnPropertyChanged(); } }

        public RelayCommand SubmitSignupCommand { get; set; }
        public RelayCommand SwitchToSignin { get; set; }

        public SignupViewModel()
        {
            SubmitSignupCommand = new RelayCommand(async o =>
            {
                var serverResponse = await AuthenticationManager.Instance.SignupToServer(_username, _password, _email);
                if(serverResponse.code == ResponseCode.TOKEN_SIGNUP_FAILED)
                {
                    ErrorString = serverResponse.message;
                    ErrorVisibility = Visibility.Visible;
                }
                else
                {
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenConnectData>(serverResponse.message);

                    var tuple = await SocialMediaManager.Instance.GetRecentPost(0);
                    var onlineUsers = await SocialMediaManager.Instance.GetOnlineUsersAsync();
                    MainViewModel.Instance.CurrentView = new HomeViewModel(data.Data, new DefultViewPostModel
                    {
                        PostID = tuple.Item2,
                        Image = Convert.FromBase64String(tuple.Item3),
                        UserID = tuple.Item1
                    }, onlineUsers);
                }
            }, o => _confirmPassword != string.Empty && _password != string.Empty && _confirmPassword == _password);

            SwitchToSignin = new RelayCommand(o =>
            {
                MainViewModel.Instance.CurrentView = new LoginViewModel();
            });
        }
    }
}
